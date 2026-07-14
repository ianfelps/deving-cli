# deving-cli (`dev`)

Ferramenta de linha de comando em .NET que reúne utilitários do dia a dia de desenvolvimento
num único comando coeso: **encoding**, **helpers de Git**, **produtividade/sistema** e
**scaffolding/HTTP**.

Construída com [Spectre.Console.Cli](https://spectreconsole.net/) sobre **.NET 10**.

## Instalação

Como *dotnet global tool*, a partir do pacote gerado localmente:

```bash
dotnet pack -c Release
dotnet tool install -g --add-source ./src/Deving.Cli/nupkg Deving.Cli
```

Depois disso o comando `dev` fica disponível globalmente. Para atualizar:

```bash
dotnet pack -c Release
dotnet tool update -g --add-source ./src/Deving.Cli/nupkg Deving.Cli
```

Para desenvolvimento, sem instalar:

```bash
dotnet run --project src/Deving.Cli -- <comando>
```

## Comandos

### Encoding / utilitários

| Comando | Descrição |
|---|---|
| `dev base64 <texto> [-d]` | Codifica/decodifica Base64 |
| `dev url <texto> [-d]` | URL encode/decode |
| `dev json [arquivo] [--minify] [--validate]` | Formata/minifica/valida JSON (arquivo ou stdin) |
| `dev jwt <token>` | Decodifica header, payload e claims de um JWT |
| `dev hash <texto> [-a sha256] [-f arquivo]` | Hash md5/sha1/sha256/sha384/sha512 |
| `dev uuid [-c N] [-v 4\|7]` | Gera GUID(s) versão 4 ou 7 (ordenável) |
| `dev time [epoch\|iso\|now] [--to iso\|epoch\|ms]` | Converte entre epoch e ISO-8601 |

### Git

| Comando | Descrição |
|---|---|
| `dev git st` | Status resumido do repositório |
| `dev git commit [-m msg]` | `git add -A` + commit (mensagem interativa se omitida) |
| `dev git branch <tipo> <descrição>` | Cria branch padronizada `tipo/descricao-kebab` |
| `dev git clean-branches [--dry-run]` | Remove branches locais já mergeadas |
| `dev git undo` | Desfaz o último commit mantendo as alterações |

### Produtividade / sistema

| Comando | Descrição |
|---|---|
| `dev port kill <porta> [-f]` | Encerra o processo que ocupa uma porta TCP |
| `dev env list\|get <k>\|set <k> <v>` | Lê/edita o `.env` do diretório atual |
| `dev snippet add\|list\|get\|rm` | Gerenciador de snippets (copia p/ clipboard no `get`) |
| `dev note add\|list\|rm` | Notas rápidas persistentes |
| `dev todo add\|list\|done\|rm` | Lista de tarefas local |
| `dev bookmark add\|list\|go\|rm` | Favoritos de diretório |
| `dev cheat [tópico]` | Cheatsheets embutidas (git, docker, dotnet, tar) |

### Scaffolding / HTTP

| Comando | Descrição |
|---|---|
| `dev new [template] [-o arquivo]` | Gera boilerplate (gitignore, editorconfig, dockerfile, readme) |
| `dev http <método> <url> [-H h] [-j json] [-d data] [-i]` | Cliente HTTP amigável |

## Exemplos

```bash
dev base64 "ola mundo"                 # b2xhIG11bmRv
dev uuid -v 7 -c 3                      # 3 GUIDs v7 ordenáveis
echo '{ "a":1 }' | dev json --minify   # {"a":1}
dev jwt eyJhbGciOi...                   # tabela de claims
dev git branch feat "Login com Google" # cria feat/login-com-google
dev port kill 3000                      # mata quem ocupa a porta 3000
dev http get https://httpbin.org/get -i # status + headers + JSON formatado
cd "$(dev bookmark go proj)"            # pula para um diretório favorito
```

## Dados

Snippets, notas, todos e bookmarks são persistidos em JSON sob
`%APPDATA%/deving-cli/` (Windows) ou `~/.config/deving-cli/` (Linux/macOS).

## Desenvolvimento

```bash
dotnet build      # compila
dotnet test       # roda os testes (xUnit)
```

Estrutura:

- `src/Deving.Cli/` — aplicação (Program.cs monta a árvore de comandos)
  - `Infrastructure/` — utilitários compartilhados (JsonStore, ProcessRunner, ConsoleEx, …)
  - `Commands/{Encode,Git,System,Scaffold}/` — comandos agrupados por categoria
- `tests/Deving.Cli.Tests/` — testes de lógica pura

## Licença

Apache-2.0. Veja [LICENSE](LICENSE).
