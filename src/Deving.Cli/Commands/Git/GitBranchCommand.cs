using System.ComponentModel;
using Deving.Cli.Infrastructure;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Git;

public sealed class GitBranchCommand : Command<GitBranchCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<type>")]
        [Description("Tipo da branch: feat, fix, chore, docs, refactor…")]
        public string Type { get; init; } = string.Empty;

        [CommandArgument(1, "<description>")]
        [Description("Descrição livre (será convertida para kebab-case).")]
        public string Description { get; init; } = string.Empty;

        [CommandOption("--no-switch")]
        [Description("Apenas cria a branch, sem trocar para ela.")]
        public bool NoSwitch { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var type = Slug.Kebab(settings.Type);
        var desc = Slug.Kebab(settings.Description);
        if (type.Length == 0 || desc.Length == 0)
        {
            ConsoleEx.Error("Tipo e descrição são obrigatórios.");
            return 1;
        }

        var name = $"{type}/{desc}";
        var result = settings.NoSwitch
            ? ProcessRunner.Git("branch", name)
            : ProcessRunner.Git("switch", "-c", name);

        if (!result.Success)
        {
            ConsoleEx.Error(result.Output);
            return 1;
        }

        ConsoleEx.Success($"Branch '{name}' {(settings.NoSwitch ? "criada" : "criada e ativada")}.");
        return 0;
    }
}
