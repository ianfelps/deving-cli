using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Deving.Cli.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Deving.Cli.Commands.Productivity;

public sealed class PortKillCommand : Command<PortKillCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<port>")]
        [Description("Porta TCP cujo processo será encerrado.")]
        public int Port { get; init; }

        [CommandOption("-f|--force")]
        [Description("Não pede confirmação antes de encerrar.")]
        public bool Force { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellation)
    {
        var pids = new List<int>();
        Theme.Status().Start($"procurando processo na porta {settings.Port}…", _ => pids = FindPids(settings.Port));
        if (pids.Count == 0)
        {
            ConsoleEx.Info($"Nenhum processo escutando na porta {settings.Port}.");
            return 0;
        }

        foreach (var pid in pids)
        {
            string name;
            try { name = Process.GetProcessById(pid).ProcessName; }
            catch { name = "(desconhecido)"; }

            if (!settings.Force &&
                !AnsiConsole.Confirm($"Encerrar PID {pid} ([{Theme.Accent}]{Markup.Escape(name)}[/]) na porta {settings.Port}?"))
            {
                continue;
            }

            try
            {
                Process.GetProcessById(pid).Kill(entireProcessTree: true);
                ConsoleEx.Success($"Processo {pid} ({name}) encerrado.");
            }
            catch (Exception ex)
            {
                ConsoleEx.Error($"Falha ao encerrar {pid}: {ex.Message}");
            }
        }
        return 0;
    }

    private static List<int> FindPids(int port)
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? FindPidsWindows(port)
            : FindPidsUnix(port);
    }

    private static List<int> FindPidsWindows(int port)
    {
        var result = ProcessRunner.Run("netstat", ["-ano", "-p", "tcp"]);
        var pids = new HashSet<int>();
        foreach (var line in result.StdOut.Split('\n'))
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            // Proto  LocalAddress  ForeignAddress  State  PID
            if (parts.Length >= 5 && parts[0].Equals("TCP", StringComparison.OrdinalIgnoreCase))
            {
                var local = parts[1];
                var colon = local.LastIndexOf(':');
                if (colon >= 0 && int.TryParse(local[(colon + 1)..], out var p) && p == port
                    && int.TryParse(parts[^1], out var pid) && pid > 0)
                {
                    pids.Add(pid);
                }
            }
        }
        return pids.ToList();
    }

    private static List<int> FindPidsUnix(int port)
    {
        var result = ProcessRunner.Run("lsof", ["-ti", $"tcp:{port}"]);
        return result.StdOut
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => int.TryParse(l.Trim(), out var pid) ? pid : 0)
            .Where(pid => pid > 0)
            .Distinct()
            .ToList();
    }
}
