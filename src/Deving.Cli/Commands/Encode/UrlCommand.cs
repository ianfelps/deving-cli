using System.ComponentModel;
using Deving.Cli.Infrastructure;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Encode;

public sealed class UrlCommand : Command<UrlCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<text>")]
        [Description("Texto a codificar (ou URL-encoded a decodificar com --decode).")]
        public string Text { get; init; } = string.Empty;

        [CommandOption("-d|--decode")]
        [Description("Decodifica em vez de codificar.")]
        public bool Decode { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var result = settings.Decode
            ? Uri.UnescapeDataString(settings.Text)
            : Uri.EscapeDataString(settings.Text);
        ConsoleEx.Raw(result);
        return 0;
    }
}
