using System.Text.Json;

namespace Deving.Cli.Infrastructure;

/// <summary>
/// Persistência genérica de uma coleção em um arquivo JSON dentro de AppPaths.Root.
/// Uso: new JsonStore&lt;Snippet&gt;("snippets.json").
/// </summary>
public sealed class JsonStore<T>
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly string _path;

    public JsonStore(string fileName) => _path = AppPaths.DataFile(fileName);

    public List<T> Load()
    {
        if (!File.Exists(_path))
            return new List<T>();

        var json = File.ReadAllText(_path);
        if (string.IsNullOrWhiteSpace(json))
            return new List<T>();

        return JsonSerializer.Deserialize<List<T>>(json, Options) ?? new List<T>();
    }

    public void Save(List<T> items)
    {
        var json = JsonSerializer.Serialize(items, Options);
        File.WriteAllText(_path, json);
    }

    /// <summary>Carrega, aplica uma mutação na lista e salva.</summary>
    public void Update(Action<List<T>> mutate)
    {
        var items = Load();
        mutate(items);
        Save(items);
    }
}
