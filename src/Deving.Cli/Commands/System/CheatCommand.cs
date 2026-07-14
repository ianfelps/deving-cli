using System.ComponentModel;
using Deving.Cli.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Productivity;

public sealed class CheatCommand : Command<CheatCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[topic]")]
        [Description("Tópico: git, docker, dotnet, tar. Omita para listar os tópicos.")]
        public string? Topic { get; init; }
    }

    private static readonly Dictionary<string, (string Title, (string Cmd, string Desc)[] Items)> Sheets = new(StringComparer.OrdinalIgnoreCase)
    {
        ["git"] = ("Git", new[]
        {
            ("git switch -c <branch>", "Cria e troca para uma nova branch"),
            ("git restore --staged <file>", "Remove arquivo do staging"),
            ("git reset --soft HEAD~1", "Desfaz último commit mantendo alterações"),
            ("git log --oneline --graph --all", "Histórico compacto em árvore"),
            ("git stash / git stash pop", "Guarda / restaura alterações temporárias"),
            ("git rebase -i HEAD~3", "Reescreve os últimos 3 commits"),
            ("git cherry-pick <hash>", "Aplica um commit específico na branch atual"),
        }),
        ["docker"] = ("Docker", new[]
        {
            ("docker ps -a", "Lista todos os contêineres"),
            ("docker compose up -d", "Sobe serviços em segundo plano"),
            ("docker exec -it <id> sh", "Abre shell dentro do contêiner"),
            ("docker logs -f <id>", "Acompanha logs em tempo real"),
            ("docker system prune -af", "Limpa imagens/contêineres não usados"),
            ("docker build -t <tag> .", "Constrói imagem a partir do Dockerfile"),
        }),
        ["dotnet"] = (".NET CLI", new[]
        {
            ("dotnet new <template>", "Cria projeto a partir de um template"),
            ("dotnet watch run", "Executa recompilando a cada alteração"),
            ("dotnet add package <pkg>", "Adiciona um pacote NuGet"),
            ("dotnet test", "Roda os testes da solução"),
            ("dotnet publish -c Release", "Publica build de produção"),
            ("dotnet tool install -g <id>", "Instala uma ferramenta global"),
        }),
        ["tar"] = ("tar", new[]
        {
            ("tar -czf a.tar.gz <dir>", "Compacta em gzip"),
            ("tar -xzf a.tar.gz", "Extrai um .tar.gz"),
            ("tar -tzf a.tar.gz", "Lista o conteúdo sem extrair"),
        }),
    };

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        if (string.IsNullOrWhiteSpace(settings.Topic))
        {
            ConsoleEx.Info("Tópicos disponíveis: " + string.Join(", ", Sheets.Keys.OrderBy(k => k)));
            return 0;
        }

        if (!Sheets.TryGetValue(settings.Topic, out var sheet))
        {
            ConsoleEx.Error($"Tópico '{settings.Topic}' não encontrado. Disponíveis: {string.Join(", ", Sheets.Keys.OrderBy(k => k))}");
            return 1;
        }

        var table = Theme.Table("Comando", "Descrição")
            .Title($"[bold {Theme.Accent}]{Markup.Escape(sheet.Title)}[/]");
        foreach (var (cmd, desc) in sheet.Items)
            table.AddRow($"[{Theme.Accent}]{Markup.Escape(cmd)}[/]", Markup.Escape(desc));
        AnsiConsole.Write(table);
        return 0;
    }
}
