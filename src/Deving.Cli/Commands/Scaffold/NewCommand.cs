using System.ComponentModel;
using Deving.Cli.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Scaffold;

public sealed class NewCommand : Command<NewCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[template]")]
        [Description("Template: gitignore, editorconfig, dockerfile, readme. Omita para listar.")]
        public string? Template { get; init; }

        [CommandOption("-o|--output")]
        [Description("Nome/caminho do arquivo de saída (default depende do template).")]
        public string? Output { get; init; }

        [CommandOption("-f|--force")]
        [Description("Sobrescreve se o arquivo já existir.")]
        public bool Force { get; init; }
    }

    private static readonly Dictionary<string, (string FileName, string Content)> Templates =
        new(StringComparer.OrdinalIgnoreCase)
    {
        ["gitignore"] = (".gitignore", GitIgnore),
        ["editorconfig"] = (".editorconfig", EditorConfig),
        ["dockerfile"] = ("Dockerfile", Dockerfile),
        ["readme"] = ("README.md", Readme),
    };

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        if (string.IsNullOrWhiteSpace(settings.Template))
        {
            ConsoleEx.Info("Templates disponíveis: " + string.Join(", ", Templates.Keys.OrderBy(k => k)));
            return 0;
        }

        if (!Templates.TryGetValue(settings.Template, out var template))
        {
            ConsoleEx.Error($"Template '{settings.Template}' não encontrado. Disponíveis: {string.Join(", ", Templates.Keys.OrderBy(k => k))}");
            return 1;
        }

        var target = Path.GetFullPath(settings.Output ?? template.FileName);
        if (File.Exists(target) && !settings.Force)
        {
            ConsoleEx.Error($"Arquivo já existe: {target} (use --force para sobrescrever).");
            return 1;
        }

        File.WriteAllText(target, template.Content);
        ConsoleEx.Success($"Criado: {target}");
        return 0;
    }

    private const string GitIgnore = """
        # .NET
        bin/
        obj/
        *.user
        .vs/

        # Node
        node_modules/
        dist/

        # Env / segredos
        .env
        *.local

        # OS
        .DS_Store
        Thumbs.db
        """;

    private const string EditorConfig = """
        root = true

        [*]
        charset = utf-8
        end_of_line = lf
        insert_final_newline = true
        trim_trailing_whitespace = true
        indent_style = space
        indent_size = 4

        [*.{js,ts,json,yml,yaml,md}]
        indent_size = 2
        """;

    private const string Dockerfile = """
        FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
        WORKDIR /src
        COPY . .
        RUN dotnet publish -c Release -o /app

        FROM mcr.microsoft.com/dotnet/runtime:10.0
        WORKDIR /app
        COPY --from=build /app .
        ENTRYPOINT ["dotnet", "app.dll"]
        """;

    private const string Readme = """
        # Projeto

        Descrição curta do projeto.

        ## Requisitos

        - ...

        ## Como rodar

        ```bash
        # ...
        ```

        ## Licença

        MIT
        """;
}
