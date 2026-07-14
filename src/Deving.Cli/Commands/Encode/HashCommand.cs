using System.ComponentModel;
using Deving.Cli.Infrastructure;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Encode;

public sealed class HashCommand : Command<HashCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[text]")]
        [Description("Texto a ser hasheado (ignorado se --file for usado).")]
        public string? Text { get; init; }

        [CommandOption("-a|--algo")]
        [Description("Algoritmo: md5, sha1, sha256 (default), sha384, sha512.")]
        [DefaultValue("sha256")]
        public string Algorithm { get; init; } = "sha256";

        [CommandOption("-f|--file")]
        [Description("Caminho de um arquivo para hashear seu conteúdo.")]
        public string? File { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        try
        {
            string result;
            if (!string.IsNullOrEmpty(settings.File))
            {
                if (!System.IO.File.Exists(settings.File))
                {
                    ConsoleEx.Error($"Arquivo não encontrado: {settings.File}");
                    return 1;
                }
                result = Codecs.Hash(settings.Algorithm, System.IO.File.ReadAllBytes(settings.File));
            }
            else if (settings.Text is not null)
            {
                result = Codecs.Hash(settings.Algorithm, settings.Text);
            }
            else
            {
                ConsoleEx.Error("Informe um texto ou --file.");
                return 1;
            }

            ConsoleEx.Raw(result);
            return 0;
        }
        catch (ArgumentException ex)
        {
            ConsoleEx.Error(ex.Message);
            return 1;
        }
    }
}
