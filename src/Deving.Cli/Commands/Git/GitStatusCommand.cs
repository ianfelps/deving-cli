using Deving.Cli.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Git;

public sealed class GitStatusCommand : Command<GitStatusCommand.Settings>
{
    public sealed class Settings : CommandSettings;

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var status = ProcessRunner.Git("status", "--porcelain=v1", "--branch");
        if (!status.Success)
        {
            ConsoleEx.Error(status.Output.Length > 0 ? status.Output : "Não é um repositório git.");
            return 1;
        }

        var lines = status.StdOut.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var branchLine = lines.FirstOrDefault(l => l.StartsWith("##")) ?? "## (desconhecido)";
        var branch = branchLine[2..].Trim();
        var branchMarkup = new Markup($"[dim]branch[/]  {Markup.Escape(branch)}");

        var changes = lines.Where(l => !l.StartsWith("##")).ToList();
        if (changes.Count == 0)
        {
            AnsiConsole.Write(branchMarkup);
            AnsiConsole.WriteLine();
            ConsoleEx.Success("Working tree limpo.");
            return 0;
        }

        var table = new Table().Border(TableBorder.None);
        table.AddColumn(new TableColumn($"[bold {Theme.Accent}]estado[/]"));
        table.AddColumn(new TableColumn($"[bold {Theme.Accent}]arquivo[/]"));
        foreach (var line in changes)
        {
            var code = line.Length >= 2 ? line[..2] : line;
            var file = line.Length > 3 ? line[3..] : string.Empty;
            table.AddRow($"[{Theme.Accent}]{Markup.Escape(code.Trim())}[/]", Markup.Escape(file));
        }

        AnsiConsole.Write(Theme.Panel(new Rows(branchMarkup, Text.Empty, table), "git · status"));
        return 0;
    }
}
