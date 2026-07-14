using System.Runtime.InteropServices;

namespace Deving.Cli.Infrastructure;

/// <summary>Copia texto para a área de transferência de forma multiplataforma.</summary>
public static class Clipboard
{
    public static bool TryCopy(string text)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return PipeTo("clip", [], text);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return PipeTo("pbcopy", [], text);

            // Linux: tenta wl-copy (Wayland) e depois xclip (X11).
            return PipeTo("wl-copy", [], text) || PipeTo("xclip", ["-selection", "clipboard"], text);
        }
        catch
        {
            return false;
        }
    }

    private static bool PipeTo(string exe, string[] args, string text)
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = exe,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            foreach (var a in args) psi.ArgumentList.Add(a);

            using var p = System.Diagnostics.Process.Start(psi);
            if (p is null) return false;
            p.StandardInput.Write(text);
            p.StandardInput.Close();
            p.WaitForExit();
            return p.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
