using System.ComponentModel;
using Deving.Cli.Infrastructure;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Encode;

public sealed class Base64Command : Command<Base64Command.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<text>")]
        [Description("Texto a codificar (ou Base64 a decodificar com --decode).")]
        public string Text { get; init; } = string.Empty;

        [CommandOption("-d|--decode")]
        [Description("Decodifica em vez de codificar.")]
        public bool Decode { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        try
        {
            var result = settings.Decode
                ? Codecs.Base64Decode(settings.Text)
                : Codecs.Base64Encode(settings.Text);
            ConsoleEx.Raw(result);
            return 0;
        }
        catch (FormatException)
        {
            ConsoleEx.Error("Entrada Base64 inválida.");
            return 1;
        }
    }
}
