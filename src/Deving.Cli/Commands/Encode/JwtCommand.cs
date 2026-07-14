using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Deving.Cli.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Encode;

public sealed class JwtCommand : Command<JwtCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<token>")]
        [Description("JWT a ser decodificado (header.payload.signature).")]
        public string Token { get; init; } = string.Empty;
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var parts = settings.Token.Split('.');
        if (parts.Length < 2)
        {
            ConsoleEx.Error("Token inválido: esperado formato header.payload.signature.");
            return 1;
        }

        try
        {
            var header = DecodeSegment(parts[0]);
            var payload = DecodeSegment(parts[1]);

            AnsiConsole.Write(Theme.Rule("Header"));
            ConsoleEx.Raw(Codecs.FormatJson(header, minify: false));

            AnsiConsole.Write(Theme.Rule("Payload"));
            ConsoleEx.Raw(Codecs.FormatJson(payload, minify: false));

            PrintClaims(payload);
            return 0;
        }
        catch (Exception ex)
        {
            ConsoleEx.Error($"Falha ao decodificar: {ex.Message}");
            return 1;
        }
    }

    private static string DecodeSegment(string segment) =>
        Encoding.UTF8.GetString(Codecs.Base64UrlDecode(segment));

    private static void PrintClaims(string payloadJson)
    {
        using var doc = JsonDocument.Parse(payloadJson);
        var table = Theme.Table("Claim", "Valor", "Interpretação");

        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            var interp = string.Empty;
            if (prop.Name is "exp" or "iat" or "nbf" && prop.Value.TryGetInt64(out var epoch))
            {
                var dt = DateTimeOffset.FromUnixTimeSeconds(epoch);
                interp = dt.ToString("u");
                if (prop.Name == "exp")
                    interp += dt < DateTimeOffset.UtcNow ? " (expirado)" : " (válido)";
            }

            table.AddRow(
                Markup.Escape(prop.Name),
                Markup.Escape(prop.Value.ToString()),
                Markup.Escape(interp));
        }

        AnsiConsole.Write(Theme.Rule("Claims"));
        AnsiConsole.Write(table);
    }
}
