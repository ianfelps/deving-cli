using Spectre.Console;
using Spectre.Console.Cli.Help;
using Spectre.Console.Rendering;

namespace Deving.Cli.Infrastructure;

/// <summary>
/// Paleta e fábricas de componentes visuais compartilhadas, para manter o mesmo
/// padrão do painel default (InfoCommand) em todas as telas: accent ciano, bordas
/// discretas e cabeçalhos destacados.
/// </summary>
public static class Theme
{
    /// <summary>Accent principal (ciano) — títulos, cabeçalhos e realces.</summary>
    public const string Accent = "#4fd8ff";

    /// <summary>Accent secundário (azul).</summary>
    public const string AccentDim = "#2f9cff";

    private static readonly Color BorderColor = Color.Grey39;

    /// <summary>Cor do accent como <see cref="Color"/> (mesma de <see cref="Accent"/>).</summary>
    public static readonly Color AccentColor = new(79, 216, 255);

    /// <summary>Estilo dos cabeçalhos de seção do help (accent em negrito).</summary>
    public static readonly Style HeaderStyle = new(AccentColor, decoration: Decoration.Bold);

    /// <summary>Tabela padrão: borda arredondada discreta e cabeçalhos em accent.</summary>
    public static Table Table(params string[] headers)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(BorderColor);

        foreach (var header in headers)
            table.AddColumn(new TableColumn($"[bold {Accent}]{header.EscapeMarkup()}[/]"));

        return table;
    }

    /// <summary>Painel arredondado (card) com header em accent e borda discreta.</summary>
    public static Panel Panel(IRenderable body, string header) =>
        new Panel(body)
        {
            Header = new PanelHeader($" [bold {Accent}]{header.EscapeMarkup()}[/] "),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(BorderColor),
            Padding = new Padding(1, 0, 1, 0),
        };

    /// <summary>Marcador de seção estilo Claude Code: ● accent + título em negrito.</summary>
    public static Markup Bullet(string title) =>
        new($"[bold {Accent}]●[/] [bold]{title.EscapeMarkup()}[/]");

    /// <summary>Spinner/status com spinner de pontos em accent, para operações lentas.</summary>
    public static Status Status() =>
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(new Style(AccentColor));

    /// <summary>Régua/seção padrão: título em accent, linha discreta, alinhada à esquerda.</summary>
    public static Rule Rule(string title) =>
        new Rule($"[bold {Accent}]{title.EscapeMarkup()}[/]")
            .LeftJustified()
            .RuleStyle(new Style(BorderColor));

    /// <summary>
    /// Estilos do help com cabeçalhos de seção (USAGE/OPTIONS/COMMANDS/…) em accent
    /// no lugar do amarelo padrão do Spectre. Parte do <see cref="HelpProviderStyle.Default"/>
    /// e sobrescreve só os headers, preservando o resto.
    /// </summary>
    public static HelpProviderStyle HelpStyles()
    {
        var s = HelpProviderStyle.Default;
        if (s.Description is not null) s.Description.Header = HeaderStyle;
        if (s.Usage is not null) s.Usage.Header = HeaderStyle;
        if (s.Examples is not null) s.Examples.Header = HeaderStyle;
        if (s.Arguments is not null) s.Arguments.Header = HeaderStyle;
        if (s.Commands is not null) s.Commands.Header = HeaderStyle;
        if (s.Options is not null) s.Options.Header = HeaderStyle;
        return s;
    }
}
