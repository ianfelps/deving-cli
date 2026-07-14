using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using Deving.Cli.Commands.Encode;
using Deving.Cli.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;

namespace Deving.Cli.Commands.Scaffold;

public sealed class HttpCommand : AsyncCommand<HttpCommand.Settings>
{
    private static readonly HttpClient Client = new();

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<method>")]
        [Description("Método HTTP: get, post, put, patch, delete.")]
        public string Method { get; init; } = "get";

        [CommandArgument(1, "<url>")]
        [Description("URL de destino.")]
        public string Url { get; init; } = string.Empty;

        [CommandOption("-H|--header <HEADER>")]
        [Description("Header no formato 'Nome: valor' (repetível).")]
        public string[] Headers { get; init; } = [];

        [CommandOption("-j|--json <BODY>")]
        [Description("Corpo JSON (define Content-Type: application/json).")]
        public string? Json { get; init; }

        [CommandOption("-d|--data <BODY>")]
        [Description("Corpo bruto (text/plain).")]
        public string? Data { get; init; }

        [CommandOption("-i|--include")]
        [Description("Mostra também os headers da resposta.")]
        public bool Include { get; init; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        HttpMethod method;
        try { method = new HttpMethod(settings.Method.ToUpperInvariant()); }
        catch { ConsoleEx.Error($"Método inválido: {settings.Method}"); return 1; }

        if (!Uri.TryCreate(settings.Url, UriKind.Absolute, out var uri))
        {
            ConsoleEx.Error($"URL inválida: {settings.Url}");
            return 1;
        }

        using var request = new HttpRequestMessage(method, uri);

        if (settings.Json is not null)
            request.Content = new StringContent(settings.Json, Encoding.UTF8, "application/json");
        else if (settings.Data is not null)
            request.Content = new StringContent(settings.Data, Encoding.UTF8, "text/plain");

        foreach (var header in settings.Headers)
        {
            var idx = header.IndexOf(':');
            if (idx <= 0)
            {
                ConsoleEx.Warn($"Header ignorado (formato inválido): {header}");
                continue;
            }
            var name = header[..idx].Trim();
            var value = header[(idx + 1)..].Trim();
            if (!request.Headers.TryAddWithoutValidation(name, value))
                request.Content?.Headers.TryAddWithoutValidation(name, value);
        }

        var sw = Stopwatch.StartNew();
        HttpResponseMessage? response = null;
        Exception? error = null;
        await Theme.Status().StartAsync($"{method.Method} {uri.Host}…", async _ =>
        {
            try { response = await Client.SendAsync(request, cancellation); }
            catch (Exception ex) { error = ex; }
        });
        sw.Stop();

        if (error is not null || response is null)
        {
            ConsoleEx.Error($"Falha na requisição: {error?.Message}");
            return 1;
        }

        var statusColor = (int)response.StatusCode switch
        {
            >= 200 and < 300 => "green",
            >= 300 and < 400 => "yellow",
            _ => "red",
        };

        var meta = new List<IRenderable>
        {
            new Markup(
                $"[{statusColor}]{(int)response.StatusCode} {Markup.Escape(response.ReasonPhrase ?? string.Empty)}[/] " +
                $"[dim]({sw.ElapsedMilliseconds} ms)[/]"),
        };
        if (settings.Include)
        {
            meta.Add(Text.Empty);
            foreach (var h in response.Headers.Concat(response.Content.Headers))
                meta.Add(new Markup($"[{Theme.Accent}]{Markup.Escape(h.Key)}[/]: {Markup.Escape(string.Join(", ", h.Value))}"));
        }
        AnsiConsole.Write(Theme.Panel(new Rows(meta), $"{method.Method} {uri.Host}"));

        var body = await response.Content.ReadAsStringAsync(cancellation);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
        if (contentType.Contains("json") && body.Length > 0)
        {
            try { body = Codecs.FormatJson(body, minify: false); }
            catch { /* mantém o corpo original se não for JSON válido */ }
        }
        if (body.Length > 0)
        {
            AnsiConsole.Write(Theme.Rule("body"));
            ConsoleEx.Raw(body);
        }

        return response.IsSuccessStatusCode ? 0 : 1;
    }
}
