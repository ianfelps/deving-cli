using Deving.Cli.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Git;

public sealed class GitUndoCommand : Command<GitUndoCommand.Settings>
{
    public sealed class Settings : CommandSettings;

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var last = ProcessRunner.Git("log", "-1", "--oneline");
        if (!last.Success)
        {
            ConsoleEx.Error(last.Output.Length > 0 ? last.Output : "Nenhum commit para desfazer.");
            return 1;
        }

        AnsiConsole.MarkupLine($"Último commit: [{Theme.Accent}]{Markup.Escape(last.StdOut)}[/]");
        if (!AnsiConsole.Confirm("Desfazer este commit mantendo as alterações no working tree?"))
        {
            ConsoleEx.Info("Cancelado.");
            return 0;
        }

        var reset = ProcessRunner.Git("reset", "--soft", "HEAD~1");
        if (!reset.Success)
        {
            ConsoleEx.Error(reset.Output);
            return 1;
        }

        ConsoleEx.Success("Commit desfeito (alterações preservadas em staging).");
        return 0;
    }
}
