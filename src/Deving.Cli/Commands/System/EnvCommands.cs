using System.ComponentModel;
using Deving.Cli.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Productivity;

/// <summary>Leitura/escrita simples de um arquivo .env no diretório atual.</summary>
internal static class EnvFile
{
    public static string Path => System.IO.Path.Combine(Directory.GetCurrentDirectory(), ".env");

    public static List<(string Key, string Value)> Read()
    {
        var list = new List<(string, string)>();
        if (!File.Exists(Path)) return list;

        foreach (var raw in File.ReadAllLines(Path))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            var eq = line.IndexOf('=');
            if (eq <= 0) continue;
            list.Add((line[..eq].Trim(), line[(eq + 1)..].Trim()));
        }
        return list;
    }

    public static void Set(string key, string value)
    {
        var lines = File.Exists(Path) ? File.ReadAllLines(Path).ToList() : new List<string>();
        var found = false;
        for (var i = 0; i < lines.Count; i++)
        {
            var t = lines[i].TrimStart();
            if (t.StartsWith('#')) continue;
            var eq = t.IndexOf('=');
            if (eq > 0 && t[..eq].Trim() == key)
            {
                lines[i] = $"{key}={value}";
                found = true;
                break;
            }
        }
        if (!found) lines.Add($"{key}={value}");
        File.WriteAllLines(Path, lines);
    }
}

public sealed class EnvListCommand : Command<EnvListCommand.Settings>
{
    public sealed class Settings : CommandSettings;

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var entries = EnvFile.Read();
        if (entries.Count == 0)
        {
            ConsoleEx.Info("Nenhuma variável em .env (ou arquivo inexistente).");
            return 0;
        }

        var table = Theme.Table("Chave", "Valor");
        foreach (var (k, v) in entries)
            table.AddRow(Markup.Escape(k), Markup.Escape(v));
        AnsiConsole.Write(table);
        return 0;
    }
}

public sealed class EnvGetCommand : Command<EnvGetCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<key>")]
        [Description("Nome da variável.")]
        public string Key { get; init; } = string.Empty;
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var match = EnvFile.Read().FirstOrDefault(e => e.Key == settings.Key);
        if (match.Key is null)
        {
            ConsoleEx.Error($"Variável '{settings.Key}' não encontrada.");
            return 1;
        }
        ConsoleEx.Raw(match.Value);
        return 0;
    }
}

public sealed class EnvSetCommand : Command<EnvSetCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<key>")]
        [Description("Nome da variável.")]
        public string Key { get; init; } = string.Empty;

        [CommandArgument(1, "<value>")]
        [Description("Valor a definir.")]
        public string Value { get; init; } = string.Empty;
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        EnvFile.Set(settings.Key, settings.Value);
        ConsoleEx.Success($"{settings.Key} definido em .env");
        return 0;
    }
}
