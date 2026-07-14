using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Help;
using Spectre.Console.Rendering;

namespace Deving.Cli.Infrastructure;

/// <summary>
/// Help provider que agrupa os comandos de topo por categoria na seção COMMANDS,
/// em vez de listá-los numa única lista plana. As demais seções (usage, options,
/// examples, e o help de subcomandos) usam o comportamento padrão do Spectre.
/// </summary>
public sealed class CategorizedHelpProvider(ICommandAppSettings settings) : HelpProvider(settings)
{
    // Ordem das categorias e os comandos de topo que pertencem a cada uma.
    private static readonly (string Title, string[] Commands)[] Categories = CommandCatalog.Categories;

    public override IEnumerable<IRenderable> GetCommands(ICommandModel model, ICommandInfo? command)
    {
        // Só reagrupa o help da raiz. Como há um comando default (InfoCommand), o help
        // da raiz chega com `command` == DefaultCommand — tratamos esse caso como raiz.
        // Branches (ex.: `dev git --help`) seguem o padrão.
        var isRoot = command is null || ReferenceEquals(command, model.DefaultCommand);
        if (!isRoot)
            return base.GetCommands(model, command);

        var visible = model.Commands.Where(c => !c.IsHidden).ToList();
        if (visible.Count == 0)
            return base.GetCommands(model, command);

        var byName = visible.ToDictionary(c => c.Name, StringComparer.Ordinal);
        var result = new List<IRenderable> { Text.NewLine, new Markup($"[bold {Theme.Accent}]COMMANDS:[/]"), Text.NewLine };

        var shown = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (title, names) in Categories)
        {
            var items = names.Where(byName.ContainsKey).Select(n => byName[n]).ToList();
            if (items.Count == 0)
                continue;
            result.Add(new Markup($"[dim]  {title.EscapeMarkup()}[/]"));
            result.Add(Text.NewLine);
            result.Add(BuildGrid(items));
            foreach (var i in items)
                shown.Add(i.Name);
        }

        // Comandos não mapeados numa categoria caem em "Outros".
        var rest = visible.Where(c => !shown.Contains(c.Name)).ToList();
        if (rest.Count > 0)
        {
            result.Add(new Markup("[dim]  Outros[/]"));
            result.Add(Text.NewLine);
            result.Add(BuildGrid(rest));
        }

        return result;
    }

    private static Grid BuildGrid(IReadOnlyList<ICommandInfo> commands)
    {
        var grid = new Grid();
        grid.AddColumn(new GridColumn { Padding = new Padding(4, 0, 2, 0), NoWrap = true });
        grid.AddColumn(new GridColumn { Padding = new Padding(0, 0, 0, 0) });

        foreach (var c in commands)
        {
            var name = c.Name + (c.IsBranch ? " [dim]<command>[/]" : "");
            grid.AddRow(new Markup(name), new Markup((c.Description ?? string.Empty).EscapeMarkup()));
        }

        return grid;
    }
}
