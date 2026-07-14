using System.ComponentModel;
using Deving.Cli.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Git;

public sealed class GitCommitCommand : Command<GitCommitCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-m|--message")]
        [Description("Mensagem do commit. Se omitida, será solicitada interativamente.")]
        public string? Message { get; init; }

        [CommandOption("-a|--all")]
        [Description("Faz 'git add -A' antes do commit (default: ligado).")]
        [DefaultValue(true)]
        public bool All { get; init; } = true;
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        if (!ProcessRunner.Git("rev-parse", "--is-inside-work-tree").Success)
        {
            ConsoleEx.Error("Não é um repositório git.");
            return 1;
        }

        var message = settings.Message;
        if (string.IsNullOrWhiteSpace(message))
        {
            message = AnsiConsole.Prompt(
                new TextPrompt<string>("Mensagem do commit:")
                    .Validate(m => string.IsNullOrWhiteSpace(m)
                        ? ValidationResult.Error("A mensagem não pode ser vazia.")
                        : ValidationResult.Success()));
        }

        if (settings.All)
        {
            var add = ProcessRunner.Git("add", "-A");
            if (!add.Success)
            {
                ConsoleEx.Error(add.Output);
                return 1;
            }
        }

        var commit = ProcessRunner.Git("commit", "-m", message!);
        if (!commit.Success)
        {
            ConsoleEx.Error(commit.Output);
            return 1;
        }

        AnsiConsole.WriteLine(commit.StdOut);
        ConsoleEx.Success("Commit criado.");
        return 0;
    }
}
