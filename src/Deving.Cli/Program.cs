using Deving.Cli.Commands;
using Deving.Cli.Commands.Encode;
using Deving.Cli.Commands.Git;
using Deving.Cli.Commands.Productivity;
using Deving.Cli.Commands.Scaffold;
using Deving.Cli.Infrastructure;
using Spectre.Console.Cli;

var app = new CommandApp<InfoCommand>();

app.Configure(config =>
{
    config.SetApplicationName("dev");
    config.SetApplicationVersion("0.2.0");
    config.SetHelpProvider(new CategorizedHelpProvider(config.Settings));
    config.Settings.HelpProviderStyles = Theme.HelpStyles();

    // ---- Encoding / utilitários ----
    config.AddCommand<Base64Command>("base64")
        .WithDescription("Codifica/decodifica Base64.")
        .WithExample("base64", "ola mundo")
        .WithExample("base64", "--decode", "b2xhIG11bmRv");
    config.AddCommand<UrlCommand>("url")
        .WithDescription("URL encode/decode.");
    config.AddCommand<JsonCommand>("json")
        .WithDescription("Formata, minifica ou valida JSON (arquivo ou stdin).");
    config.AddCommand<JwtCommand>("jwt")
        .WithDescription("Decodifica header e payload de um JWT.");
    config.AddCommand<HashCommand>("hash")
        .WithDescription("Gera hash (md5/sha1/sha256/sha384/sha512).");
    config.AddCommand<UuidCommand>("uuid")
        .WithDescription("Gera GUID(s) versão 4 ou 7.");
    config.AddCommand<TimeCommand>("time")
        .WithDescription("Converte entre epoch e ISO-8601.");

    // ---- Git ----
    config.AddBranch("git", git =>
    {
        git.SetDescription("Helpers de Git.");
        git.AddCommand<GitStatusCommand>("st")
            .WithDescription("Status resumido do repositório.");
        git.AddCommand<GitCommitCommand>("commit")
            .WithDescription("git add -A + commit (mensagem interativa se omitida).");
        git.AddCommand<GitBranchCommand>("branch")
            .WithDescription("Cria branch padronizada tipo/descricao-kebab.");
        git.AddCommand<GitCleanBranchesCommand>("clean-branches")
            .WithDescription("Remove branches locais já mergeadas.");
        git.AddCommand<GitUndoCommand>("undo")
            .WithDescription("Desfaz o último commit mantendo alterações.");
    });

    // ---- Produtividade / sistema ----
    config.AddBranch("port", port =>
    {
        port.SetDescription("Utilitários de portas TCP.");
        port.AddCommand<PortKillCommand>("kill")
            .WithDescription("Encerra o processo que ocupa uma porta.");
    });

    config.AddBranch("env", env =>
    {
        env.SetDescription("Leitura/escrita do .env do diretório atual.");
        env.AddCommand<EnvListCommand>("list").WithDescription("Lista as variáveis.");
        env.AddCommand<EnvGetCommand>("get").WithDescription("Lê uma variável.");
        env.AddCommand<EnvSetCommand>("set").WithDescription("Define uma variável.");
    });

    config.AddBranch("snippet", s =>
    {
        s.SetDescription("Gerenciador de snippets.");
        s.AddCommand<SnippetAddCommand>("add").WithDescription("Adiciona/atualiza um snippet.");
        s.AddCommand<SnippetListCommand>("list").WithDescription("Lista snippets.");
        s.AddCommand<SnippetGetCommand>("get").WithDescription("Mostra e copia um snippet.");
        s.AddCommand<SnippetRemoveCommand>("rm").WithDescription("Remove um snippet.");
    });

    config.AddBranch("note", n =>
    {
        n.SetDescription("Notas rápidas.");
        n.AddCommand<NoteAddCommand>("add").WithDescription("Adiciona uma nota.");
        n.AddCommand<NoteListCommand>("list").WithDescription("Lista notas.");
        n.AddCommand<NoteRemoveCommand>("rm").WithDescription("Remove uma nota.");
    });

    config.AddBranch("bookmark", b =>
    {
        b.SetDescription("Favoritos de diretório.");
        b.AddCommand<BookmarkAddCommand>("add").WithDescription("Adiciona um bookmark.");
        b.AddCommand<BookmarkListCommand>("list").WithDescription("Lista bookmarks.");
        b.AddCommand<BookmarkGoCommand>("go").WithDescription("Imprime o caminho (use com cd).");
        b.AddCommand<BookmarkRemoveCommand>("rm").WithDescription("Remove um bookmark.");
    });

    config.AddBranch("todo", t =>
    {
        t.SetDescription("Lista de tarefas local.");
        t.AddCommand<TodoAddCommand>("add").WithDescription("Adiciona uma tarefa.");
        t.AddCommand<TodoListCommand>("list").WithDescription("Lista tarefas.");
        t.AddCommand<TodoDoneCommand>("done").WithDescription("Marca tarefa como concluída.");
        t.AddCommand<TodoRemoveCommand>("rm").WithDescription("Remove uma tarefa.");
    });

    config.AddCommand<CheatCommand>("cheat")
        .WithDescription("Cheatsheets embutidas (git, docker, dotnet, tar).");

    // ---- Scaffolding / HTTP ----
    config.AddCommand<NewCommand>("new")
        .WithDescription("Gera arquivos de boilerplate a partir de templates.");
    config.AddCommand<HttpCommand>("http")
        .WithDescription("Cliente HTTP amigável (get/post/put/patch/delete).")
        .WithExample("http", "get", "https://httpbin.org/get");

#if DEBUG
    config.PropagateExceptions();
    config.ValidateExamples();
#endif
});

return app.Run(args);
