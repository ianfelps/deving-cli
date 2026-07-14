using System.ComponentModel;
using Deving.Cli.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Productivity;

public sealed class TodoItem
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool Done { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

internal static class TodoStore
{
    public static readonly JsonStore<TodoItem> Store = new("todos.json");
}

public sealed class TodoAddCommand : Command<TodoAddCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<text>")]
        [Description("Descrição da tarefa.")]
        public string Text { get; init; } = string.Empty;
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var id = 0;
        TodoStore.Store.Update(list =>
        {
            id = list.Count == 0 ? 1 : list.Max(t => t.Id) + 1;
            list.Add(new TodoItem { Id = id, Text = settings.Text });
        });
        ConsoleEx.Success($"Tarefa #{id} adicionada.");
        return 0;
    }
}

public sealed class TodoListCommand : Command<TodoListCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("-a|--all")]
        [Description("Inclui tarefas concluídas.")]
        public bool All { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var todos = TodoStore.Store.Load().Where(t => settings.All || !t.Done).ToList();
        if (todos.Count == 0)
        {
            ConsoleEx.Info("Nenhuma tarefa.");
            return 0;
        }

        var table = Theme.Table("#", "✓", "Tarefa");
        foreach (var t in todos.OrderBy(t => t.Done).ThenBy(t => t.Id))
        {
            var check = t.Done ? "[green]✓[/]" : " ";
            var text = t.Done ? $"[strikethrough dim]{Markup.Escape(t.Text)}[/]" : Markup.Escape(t.Text);
            table.AddRow(t.Id.ToString(), check, text);
        }
        AnsiConsole.Write(table);
        return 0;
    }
}

public sealed class TodoDoneCommand : Command<TodoDoneCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<id>")]
        [Description("Id da tarefa a concluir.")]
        public int Id { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var found = false;
        TodoStore.Store.Update(list =>
        {
            var t = list.FirstOrDefault(x => x.Id == settings.Id);
            if (t is not null) { t.Done = true; found = true; }
        });
        if (!found)
        {
            ConsoleEx.Error($"Tarefa #{settings.Id} não encontrada.");
            return 1;
        }
        ConsoleEx.Success($"Tarefa #{settings.Id} concluída.");
        return 0;
    }
}

public sealed class TodoRemoveCommand : Command<TodoRemoveCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<id>")]
        [Description("Id da tarefa a remover.")]
        public int Id { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var removed = 0;
        TodoStore.Store.Update(list => removed = list.RemoveAll(t => t.Id == settings.Id));
        if (removed == 0)
        {
            ConsoleEx.Error($"Tarefa #{settings.Id} não encontrada.");
            return 1;
        }
        ConsoleEx.Success($"Tarefa #{settings.Id} removida.");
        return 0;
    }
}
