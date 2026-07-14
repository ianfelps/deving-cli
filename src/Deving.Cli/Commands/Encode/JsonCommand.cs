using System.ComponentModel;
using System.Text.Json;
using Deving.Cli.Infrastructure;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Encode;

public sealed class JsonCommand : Command<JsonCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[input]")]
        [Description("Caminho do arquivo JSON. Se omitido, lê do stdin.")]
        public string? Input { get; init; }

        [CommandOption("-m|--minify")]
        [Description("Minifica em vez de formatar.")]
        public bool Minify { get; init; }

        [CommandOption("--validate")]
        [Description("Apenas valida o JSON (não imprime o conteúdo).")]
        public bool ValidateOnly { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        string json;
        if (string.IsNullOrEmpty(settings.Input))
        {
            json = Console.In.ReadToEnd().TrimStart('﻿').Trim();
        }
        else if (File.Exists(settings.Input))
        {
            json = File.ReadAllText(settings.Input);
        }
        else
        {
            ConsoleEx.Error($"Arquivo não encontrado: {settings.Input}");
            return 1;
        }

        try
        {
            if (settings.ValidateOnly)
            {
                using var _ = JsonDocument.Parse(json);
                ConsoleEx.Success("JSON válido.");
                return 0;
            }

            ConsoleEx.Raw(Codecs.FormatJson(json, settings.Minify));
            return 0;
        }
        catch (JsonException ex)
        {
            ConsoleEx.Error($"JSON inválido: {ex.Message}");
            return 1;
        }
    }
}
