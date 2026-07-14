using System.ComponentModel;
using Deving.Cli.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;

namespace Deving.Cli.Commands.Git;

public sealed class GitCleanBranchesCommand : Command<GitCleanBranchesCommand.Settings>
{
    private static readonly string[] Protected = ["main", "master", "develop"];

    public sealed class Settings : CommandSettings
    {
        [CommandOption("--dry-run")]
        [Description("Apenas lista as branches que seriam removidas.")]
        public bool DryRun { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var current = ProcessRunner.Git("rev-parse", "--abbrev-ref", "HEAD");
        if (!current.Success)
        {
            ConsoleEx.Error("Não é um repositório git.");
            return 1;
        }
        var currentBranch = current.StdOut.Trim();

        var merged = default(ProcessResult);
        Theme.Status().Start("lendo branches mergeadas…", _ => merged = ProcessRunner.Git("branch", "--merged"));
        if (!merged.Success)
        {
            ConsoleEx.Error(merged.Output);
            return 1;
        }

        var candidates = merged.StdOut
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Replace("*", string.Empty).Trim())
            .Where(b => b.Length > 0)
            .Where(b => !Protected.Contains(b))
            .Where(b => b != currentBranch)
            .ToList();

        if (candidates.Count == 0)
        {
            ConsoleEx.Success("Nenhuma branch mergeada para remover.");
            return 0;
        }

        var list = candidates.Select(b => (IRenderable)new Markup($"[{Theme.Accent}]{Markup.Escape(b)}[/]"));
        AnsiConsole.Write(Theme.Panel(new Rows(list), $"branches mergeadas ({candidates.Count})"));

        if (settings.DryRun)
        {
            ConsoleEx.Info("--dry-run: nada foi removido.");
            return 0;
        }

        if (!AnsiConsole.Confirm($"Remover {candidates.Count} branch(es)?"))
        {
            ConsoleEx.Info("Cancelado.");
            return 0;
        }

        var removed = 0;
        foreach (var b in candidates)
        {
            var del = ProcessRunner.Git("branch", "-d", b);
            if (del.Success)
            {
                removed++;
            }
            else
            {
                ConsoleEx.Warn($"Falha ao remover '{b}': {del.Output}");
            }
        }

        ConsoleEx.Success($"{removed} branch(es) removida(s).");
        return 0;
    }
}
