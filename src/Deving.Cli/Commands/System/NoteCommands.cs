using System.ComponentModel;
using Deving.Cli.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Productivity;

public sealed class Note
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

internal static class NoteStore
{
    public static readonly JsonStore<Note> Store = new("notes.json");
}

public sealed class NoteAddCommand : Command<NoteAddCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<text>")]
        [Description("Texto da nota.")]
        public string Text { get; init; } = string.Empty;
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var id = 0;
        NoteStore.Store.Update(list =>
        {
            id = list.Count == 0 ? 1 : list.Max(n => n.Id) + 1;
            list.Add(new Note { Id = id, Text = settings.Text });
        });
        ConsoleEx.Success($"Nota #{id} adicionada.");
        return 0;
    }
}

public sealed class NoteListCommand : Command<NoteListCommand.Settings>
{
    public sealed class Settings : CommandSettings;

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var notes = NoteStore.Store.Load();
        if (notes.Count == 0)
        {
            ConsoleEx.Info("Nenhuma nota.");
            return 0;
        }

        var table = Theme.Table("#", "Nota", "Quando");
        foreach (var n in notes.OrderBy(n => n.Id))
            table.AddRow(n.Id.ToString(), Markup.Escape(n.Text), n.CreatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
        AnsiConsole.Write(table);
        return 0;
    }
}

public sealed class NoteRemoveCommand : Command<NoteRemoveCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<id>")]
        [Description("Id da nota a remover.")]
        public int Id { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var removed = 0;
        NoteStore.Store.Update(list => removed = list.RemoveAll(n => n.Id == settings.Id));
        if (removed == 0)
        {
            ConsoleEx.Error($"Nota #{settings.Id} não encontrada.");
            return 1;
        }
        ConsoleEx.Success($"Nota #{settings.Id} removida.");
        return 0;
    }
}
