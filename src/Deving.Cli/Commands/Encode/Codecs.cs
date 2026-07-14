using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Deving.Cli.Commands.Encode;

/// <summary>Lógica pura de codificação/decodificação — sem I/O, fácil de testar.</summary>
public static class Codecs
{
    // ---- Base64 ----
    public static string Base64Encode(string text) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

    public static string Base64Decode(string base64) =>
        Encoding.UTF8.GetString(Convert.FromBase64String(base64));

    // ---- Base64Url (sem padding, usado em JWT) ----
    public static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }

    // ---- Hash ----
    public static string Hash(string algorithm, byte[] data)
    {
        using HashAlgorithm hasher = algorithm.ToLowerInvariant() switch
        {
            "md5" => MD5.Create(),
            "sha1" => SHA1.Create(),
            "sha256" => SHA256.Create(),
            "sha384" => SHA384.Create(),
            "sha512" => SHA512.Create(),
            _ => throw new ArgumentException($"Algoritmo não suportado: {algorithm}"),
        };
        return Convert.ToHexString(hasher.ComputeHash(data)).ToLowerInvariant();
    }

    public static string Hash(string algorithm, string text) =>
        Hash(algorithm, Encoding.UTF8.GetBytes(text));

    // ---- JSON ----
    public static string FormatJson(string json, bool minify)
    {
        using var doc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions
        {
            WriteIndented = !minify,
        });
    }

    // ---- UUID v7 (ordenável por tempo, RFC 9562) ----
    public static Guid NewGuidV7() => NewGuidV7(DateTimeOffset.UtcNow);

    public static Guid NewGuidV7(DateTimeOffset timestamp)
    {
        Span<byte> bytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(bytes);

        long unixMs = timestamp.ToUnixTimeMilliseconds();
        // 48 bits de timestamp (big-endian) nos primeiros 6 bytes
        bytes[0] = (byte)(unixMs >> 40);
        bytes[1] = (byte)(unixMs >> 32);
        bytes[2] = (byte)(unixMs >> 24);
        bytes[3] = (byte)(unixMs >> 16);
        bytes[4] = (byte)(unixMs >> 8);
        bytes[5] = (byte)unixMs;

        // versão 7 no nibble alto do byte 6
        bytes[6] = (byte)(0x70 | (bytes[6] & 0x0F));
        // variante RFC 4122 no byte 8
        bytes[8] = (byte)(0x80 | (bytes[8] & 0x3F));

        return GuidFromBigEndian(bytes);
    }

    private static Guid GuidFromBigEndian(ReadOnlySpan<byte> b)
    {
        // Guid(byte[]) usa layout little-endian nos 3 primeiros grupos; convertemos.
        Span<byte> le = stackalloc byte[16];
        b.CopyTo(le);
        (le[0], le[3]) = (le[3], le[0]);
        (le[1], le[2]) = (le[2], le[1]);
        (le[4], le[5]) = (le[5], le[4]);
        (le[6], le[7]) = (le[7], le[6]);
        return new Guid(le);
    }

    // ---- Epoch <-> ISO ----
    public static DateTimeOffset EpochToDate(long value)
    {
        // Heurística: valores >= 1e12 são milissegundos.
        return value >= 1_000_000_000_000
            ? DateTimeOffset.FromUnixTimeMilliseconds(value)
            : DateTimeOffset.FromUnixTimeSeconds(value);
    }
}
