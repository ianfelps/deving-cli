using System.ComponentModel;
using Deving.Cli.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Encode;

public sealed class TimeCommand : Command<TimeCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[value]")]
        [Description("Epoch (segundos ou ms), data ISO-8601, ou 'now' (default).")]
        public string Value { get; init; } = "now";

        [CommandOption("--to")]
        [Description("Formato de saída: iso, epoch, ms, ou 'all' (default).")]
        [DefaultValue("all")]
        public string To { get; init; } = "all";
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        DateTimeOffset dt;

        if (settings.Value is "now")
        {
            dt = DateTimeOffset.UtcNow;
        }
        else if (long.TryParse(settings.Value, out var epoch))
        {
            dt = Codecs.EpochToDate(epoch);
        }
        else if (DateTimeOffset.TryParse(settings.Value, out var parsed))
        {
            dt = parsed.ToUniversalTime();
        }
        else
        {
            ConsoleEx.Error($"Não foi possível interpretar: {settings.Value}");
            return 1;
        }

        switch (settings.To.ToLowerInvariant())
        {
            case "iso":
                ConsoleEx.Raw(dt.ToString("o"));
                break;
            case "epoch":
                ConsoleEx.Raw(dt.ToUnixTimeSeconds().ToString());
                break;
            case "ms":
                ConsoleEx.Raw(dt.ToUnixTimeMilliseconds().ToString());
                break;
            default:
                var table = Theme.Table("Formato", "Valor");
                table.AddRow("ISO-8601 (UTC)", dt.ToString("o"));
                table.AddRow("Local", dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz"));
                table.AddRow("Epoch (s)", dt.ToUnixTimeSeconds().ToString());
                table.AddRow("Epoch (ms)", dt.ToUnixTimeMilliseconds().ToString());
                AnsiConsole.Write(table);
                break;
        }
        return 0;
    }
}
