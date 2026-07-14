using System.Globalization;
using System.Text;

namespace Deving.Cli.Infrastructure;

public static class Slug
{
    /// <summary>Converte um texto livre em kebab-case ascii (para nomes de branch, arquivos…).</summary>
    public static string Kebab(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove acentos.
        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var ascii = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();

        var result = new StringBuilder(ascii.Length);
        var lastWasDash = false;
        foreach (var c in ascii)
        {
            if (char.IsLetterOrDigit(c))
            {
                result.Append(c);
                lastWasDash = false;
            }
            else if (!lastWasDash && result.Length > 0)
            {
                result.Append('-');
                lastWasDash = true;
            }
        }

        return result.ToString().Trim('-');
    }
}
