using System.Diagnostics;
using System.Text;

namespace Deving.Cli.Infrastructure;

public readonly record struct ProcessResult(int ExitCode, string StdOut, string StdErr)
{
    public bool Success => ExitCode == 0;
    public string Output => string.IsNullOrWhiteSpace(StdOut) ? StdErr : StdOut;
}

/// <summary>
/// Wrapper simples para executar processos externos (git, netstat, lsof…)
/// capturando stdout/stderr.
/// </summary>
public static class ProcessRunner
{
    public static ProcessResult Run(string fileName, IEnumerable<string> args, string? workingDirectory = null)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
        };

        foreach (var a in args)
            psi.ArgumentList.Add(a);

        try
        {
            using var process = Process.Start(psi);
            if (process is null)
                return new ProcessResult(-1, string.Empty, $"Não foi possível iniciar '{fileName}'.");

            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return new ProcessResult(process.ExitCode, stdout.TrimEnd(), stderr.TrimEnd());
        }
        catch (Exception ex)
        {
            return new ProcessResult(-1, string.Empty, ex.Message);
        }
    }

    public static ProcessResult Git(params string[] args) => Run("git", args);
}
