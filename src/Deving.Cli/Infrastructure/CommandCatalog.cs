namespace Deving.Cli.Infrastructure;

/// <summary>
/// Catálogo compartilhado das categorias de comandos de topo. Fonte única usada tanto
/// pelo help categorizado quanto pelo painel default (InfoCommand), evitando duplicar
/// a lista de categorias em dois lugares.
/// </summary>
public static class CommandCatalog
{
    // Ordem das categorias e os comandos de topo que pertencem a cada uma.
    public static readonly (string Title, string[] Commands)[] Categories =
    [
        ("Encoding / utilitários", ["base64", "url", "json", "jwt", "hash", "uuid", "time"]),
        ("Git", ["git"]),
        ("Produtividade / sistema", ["port", "env", "snippet", "note", "bookmark", "todo", "cheat"]),
        ("Scaffolding / HTTP", ["new", "http"]),
    ];

    /// <summary>Total de comandos de topo listados nas categorias.</summary>
    public static int TotalCommands => Categories.Sum(c => c.Commands.Length);
}
