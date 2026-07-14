using System.ComponentModel;
using Deving.Cli.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Productivity;

public sealed class Bookmark
{
    public string Alias { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}

internal static class BookmarkStore
{
    public static readonly JsonStore<Bookmark> Store = new("bookmarks.json");
}

public sealed class BookmarkAddCommand : Command<BookmarkAddCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<alias>")]
        [Description("Apelido do diretório.")]
        public string Alias { get; init; } = string.Empty;

        [CommandArgument(1, "[path]")]
        [Description("Caminho (default: diretório atual).")]
        public string? Path { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var path = System.IO.Path.GetFullPath(settings.Path ?? Directory.GetCurrentDirectory());
        if (!Directory.Exists(path))
        {
            ConsoleEx.Error($"Diretório não existe: {path}");
            return 1;
        }

        BookmarkStore.Store.Update(list =>
        {
            list.RemoveAll(b => b.Alias == settings.Alias);
            list.Add(new Bookmark { Alias = settings.Alias, Path = path });
        });
        ConsoleEx.Success($"Bookmark '{settings.Alias}' → {path}");
        return 0;
    }
}

public sealed class BookmarkListCommand : Command<BookmarkListCommand.Settings>
{
    public sealed class Settings : CommandSettings;

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var bookmarks = BookmarkStore.Store.Load();
        if (bookmarks.Count == 0)
        {
            ConsoleEx.Info("Nenhum bookmark.");
            return 0;
        }

        var table = Theme.Table("Alias", "Caminho");
        foreach (var b in bookmarks.OrderBy(b => b.Alias))
            table.AddRow(Markup.Escape(b.Alias), Markup.Escape(b.Path));
        AnsiConsole.Write(table);
        return 0;
    }
}

public sealed class BookmarkGoCommand : Command<BookmarkGoCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<alias>")]
        [Description("Apelido do bookmark.")]
        public string Alias { get; init; } = string.Empty;
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var bookmark = BookmarkStore.Store.Load().FirstOrDefault(b => b.Alias == settings.Alias);
        if (bookmark is null)
        {
            ConsoleEx.Error($"Bookmark '{settings.Alias}' não encontrado.");
            return 1;
        }
        // Imprime só o caminho para uso com: cd $(dev bookmark go x)
        ConsoleEx.Raw(bookmark.Path);
        return 0;
    }
}

public sealed class BookmarkRemoveCommand : Command<BookmarkRemoveCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<alias>")]
        [Description("Apelido do bookmark a remover.")]
        public string Alias { get; init; } = string.Empty;
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var removed = 0;
        BookmarkStore.Store.Update(list => removed = list.RemoveAll(b => b.Alias == settings.Alias));
        if (removed == 0)
        {
            ConsoleEx.Error($"Bookmark '{settings.Alias}' não encontrado.");
            return 1;
        }
        ConsoleEx.Success($"Bookmark '{settings.Alias}' removido.");
        return 0;
    }
}
