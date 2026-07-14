namespace Deving.Cli.Infrastructure;

/// <summary>
/// Resolve os caminhos de armazenamento da ferramenta em %APPDATA%/deving-cli/
/// (ou o equivalente em Linux/macOS via SpecialFolder.ApplicationData).
/// </summary>
public static class AppPaths
{
    public static string Root
    {
        get
        {
            var baseDir = Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData,
                Environment.SpecialFolderOption.Create);
            var dir = Path.Combine(baseDir, "deving-cli");
            Directory.CreateDirectory(dir);
            return dir;
        }
    }

    /// <summary>Caminho completo de um arquivo de dados (ex.: "snippets.json").</summary>
    public static string DataFile(string fileName) => Path.Combine(Root, fileName);
}
