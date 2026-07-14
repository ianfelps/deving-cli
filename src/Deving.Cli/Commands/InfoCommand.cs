using System.Reflection;
using System.Runtime.InteropServices;
using Deving.Cli.Commands.Productivity;
using Deving.Cli.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;

namespace Deving.Cli.Commands;

/// <summary>
/// Comando default (executado ao rodar `dev` sem argumentos): mostra um painel de
/// identidade no estilo fastfetch — ASCII art à esquerda, infos chave/valor à direita
/// e uma barra de paleta de cores no rodapé.
/// </summary>
public sealed class InfoCommand : Command<InfoCommand.Settings>
{
    public sealed class Settings : CommandSettings;

    private const string Author = "ianfelps";

    // Logo "dev" em ANSI Shadow. Colorida linha a linha (ciano → azul) em BuildArt.
    private static readonly string[] Art =
    [
        @" ██████╗  ███████╗ ██╗   ██╗",
        @" ██╔══██╗ ██╔════╝ ██║   ██║",
        @" ██║  ██║ █████╗   ╚██╗ ██╔╝",
        @" ██║  ██║ ██╔══╝    ╚████╔╝ ",
        @" ██████╔╝ ███████╗   ╚██╔╝  ",
        @" ╚═════╝  ╚══════╝    ╚═╝   ",
    ];

    private static readonly string[] ArtColors =
        ["#5ff5ff", "#4fd8ff", "#3fbaff", "#2f9cff", "#1f7eff", "#1560ff"];

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var grid = new Grid();
        grid.AddColumn(new GridColumn { Padding = new Padding(1, 0, 3, 0), NoWrap = true });
        grid.AddColumn(new GridColumn { Padding = new Padding(0, 0, 0, 0) });
        grid.AddRow(BuildArt(), BuildInfo());

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($" [bold {Theme.Accent}]✻[/] [bold]deving-cli[/] [dim]· utilitários de dev[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.Write(grid);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($" [dim]▐[/] [dim]dica: rode[/] [{Theme.Accent}]dev --help[/] [dim]para ver todos os comandos[/]");
        AnsiConsole.WriteLine();
        return 0;
    }

    private static IRenderable BuildArt()
    {
        var lines = Art.Select((line, i) =>
            (IRenderable)new Markup($"[{ArtColors[i]}]{line.EscapeMarkup()}[/]"));
        return new Rows(lines);
    }

    private static IRenderable BuildInfo()
    {
        var lines = new List<IRenderable>
        {
            new Markup($"[bold #5ff5ff]dev[/][dim]@[/][bold]{Environment.MachineName.EscapeMarkup()}[/]"),
            Text.Empty,

            Theme.Bullet("tool"),
            Sub("versão", Version()),
            Sub("autor", Author),
            Sub("comandos", $"{CommandCatalog.TotalCommands} em {CommandCatalog.Categories.Length} categorias"),
            Text.Empty,

            Theme.Bullet("ambiente"),
            Sub("os", RuntimeInformation.OSDescription),
            Sub("dotnet", RuntimeInformation.FrameworkDescription),
            Sub("shell", DetectShell()),
            Sub("dir", CollapseHome(Directory.GetCurrentDirectory())),
        };

        var branch = GitBranch();
        if (branch is not null)
            lines.Add(Sub("git", branch));

        lines.Add(Text.Empty);
        lines.Add(Theme.Bullet("dados"));
        lines.Add(new Markup(
            $"  Notas [bold]{NoteStore.Store.Load().Count}[/]  " +
            $"Todos [bold]{TodoStore.Store.Load().Count}[/]  " +
            $"Snippets [bold]{SnippetStore.Store.Load().Count}[/]  " +
            $"Bookmarks [bold]{BookmarkStore.Store.Load().Count}[/]"));

        lines.Add(Text.Empty);
        lines.Add(Theme.Bullet("comandos"));
        foreach (var (title, commands) in CommandCatalog.Categories)
        {
            var shortTitle = title.Split(' ')[0];
            lines.Add(new Markup(
                $"  [dim]{shortTitle.PadRight(13).EscapeMarkup()}[/] " +
                $"{string.Join(" ", commands).EscapeMarkup()}"));
        }

        lines.Add(Text.Empty);
        lines.Add(BuildPalette());

        return new Rows(lines);
    }

    private static IRenderable Sub(string key, string value) =>
        new Markup($"  [dim]{key.PadRight(9).EscapeMarkup()}[/] {value.EscapeMarkup()}");

    private static IRenderable BuildPalette()
    {
        string[] colors = ["red", "green", "yellow", "blue", "magenta", "cyan", "white", "grey"];
        return new Markup(string.Concat(colors.Select(c => $"[{c}]███[/]")));
    }

    private static string Version()
    {
        var asm = Assembly.GetExecutingAssembly();
        var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(info))
        {
            // Remove metadados de build (ex.: "0.1.0+abc123").
            var plus = info.IndexOf('+');
            return plus >= 0 ? info[..plus] : info;
        }
        return asm.GetName().Version?.ToString(3) ?? "0.1.0";
    }

    private static string DetectShell()
    {
        var shell = Environment.GetEnvironmentVariable("SHELL");
        if (!string.IsNullOrWhiteSpace(shell))
            return Path.GetFileName(shell);

        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PSModulePath")))
            return "PowerShell";

        var comspec = Environment.GetEnvironmentVariable("ComSpec");
        return string.IsNullOrWhiteSpace(comspec) ? "?" : Path.GetFileName(comspec);
    }

    private static string CollapseHome(string path)
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrEmpty(home) && path.StartsWith(home, StringComparison.OrdinalIgnoreCase))
            return "~" + path[home.Length..];
        return path;
    }

    private static string? GitBranch()
    {
        var result = ProcessRunner.Git("rev-parse", "--abbrev-ref", "HEAD");
        if (!result.Success)
            return null;
        var branch = result.StdOut.Trim();
        return string.IsNullOrWhiteSpace(branch) ? null : branch;
    }
}
