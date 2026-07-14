using Spectre.Console;

namespace Deving.Cli.Infrastructure;

/// <summary>Helpers de saída consistente no terminal via Spectre.Console.</summary>
public static class ConsoleEx
{
    public static void Success(string message) =>
        AnsiConsole.MarkupLine($"[green]✓[/] {Markup.Escape(message)}");

    public static void Info(string message) =>
        AnsiConsole.MarkupLine($"[blue]ℹ[/] {Markup.Escape(message)}");

    public static void Warn(string message) =>
        AnsiConsole.MarkupLine($"[yellow]![/] {Markup.Escape(message)}");

    public static void Error(string message) =>
        AnsiConsole.MarkupLine($"[red]✗[/] {Markup.Escape(message)}");

    /// <summary>Imprime texto cru (sem interpretar markup), útil para saída copiável.</summary>
    public static void Raw(string text) => AnsiConsole.WriteLine(text);
}
