using System.ComponentModel;
using Deving.Cli.Infrastructure;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Encode;

public sealed class UuidCommand : Command<UuidCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-c|--count")]
        [Description("Quantidade de GUIDs a gerar.")]
        [DefaultValue(1)]
        public int Count { get; init; } = 1;

        [CommandOption("-v|--version")]
        [Description("Versão: 4 (aleatório, default) ou 7 (ordenável por tempo).")]
        [DefaultValue(4)]
        public int Version { get; init; } = 4;

        public override Spectre.Console.ValidationResult Validate()
        {
            if (Count < 1)
                return Spectre.Console.ValidationResult.Error("--count deve ser >= 1.");
            if (Version is not (4 or 7))
                return Spectre.Console.ValidationResult.Error("--version deve ser 4 ou 7.");
            return Spectre.Console.ValidationResult.Success();
        }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        for (var i = 0; i < settings.Count; i++)
        {
            var guid = settings.Version == 7 ? Codecs.NewGuidV7() : Guid.NewGuid();
            ConsoleEx.Raw(guid.ToString());
        }
        return 0;
    }
}
