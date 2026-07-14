using System.ComponentModel;
using Deving.Cli.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Productivity;

public sealed class Snippet
{
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

internal static class SnippetStore
{
    public static readonly JsonStore<Snippet> Store = new("snippets.json");
}

public sealed class SnippetAddCommand : Command<SnippetAddCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<name>")]
        [Description("Nome do snippet.")]
        public string Name { get; init; } = string.Empty;

        [CommandArgument(1, "<content>")]
        [Description("Conteúdo do snippet.")]
        public string Content { get; init; } = string.Empty;
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        SnippetStore.Store.Update(list =>
        {
            list.RemoveAll(s => s.Name == settings.Name);
            list.Add(new Snippet { Name = settings.Name, Content = settings.Content });
        });
        ConsoleEx.Success($"Snippet '{settings.Name}' salvo.");
        return 0;
    }
}

public sealed class SnippetListCommand : Command<SnippetListCommand.Settings>
{
    public sealed class Settings : CommandSettings;

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var snippets = SnippetStore.Store.Load();
        if (snippets.Count == 0)
        {
            ConsoleEx.Info("Nenhum snippet salvo.");
            return 0;
        }

        var table = Theme.Table("Nome", "Prévia");
        foreach (var s in snippets.OrderBy(s => s.Name))
        {
            var preview = s.Content.Length > 60 ? s.Content[..60] + "…" : s.Content;
            table.AddRow(Markup.Escape(s.Name), Markup.Escape(preview.ReplaceLineEndings(" ")));
        }
        AnsiConsole.Write(table);
        return 0;
    }
}

public sealed class SnippetGetCommand : Command<SnippetGetCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<name>")]
        [Description("Nome do snippet.")]
        public string Name { get; init; } = string.Empty;

        [CommandOption("--no-copy")]
        [Description("Não copia para a área de transferência.")]
        public bool NoCopy { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var snippet = SnippetStore.Store.Load().FirstOrDefault(s => s.Name == settings.Name);
        if (snippet is null)
        {
            ConsoleEx.Error($"Snippet '{settings.Name}' não encontrado.");
            return 1;
        }

        ConsoleEx.Raw(snippet.Content);
        if (!settings.NoCopy && Clipboard.TryCopy(snippet.Content))
            ConsoleEx.Info("(copiado para a área de transferência)");
        return 0;
    }
}

public sealed class SnippetRemoveCommand : Command<SnippetRemoveCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<name>")]
        [Description("Nome do snippet a remover.")]
        public string Name { get; init; } = string.Empty;
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var removed = 0;
        SnippetStore.Store.Update(list => removed = list.RemoveAll(s => s.Name == settings.Name));
        if (removed == 0)
        {
            ConsoleEx.Error($"Snippet '{settings.Name}' não encontrado.");
            return 1;
        }
        ConsoleEx.Success($"Snippet '{settings.Name}' removido.");
        return 0;
    }
}
