using System.Text;
using Deving.Cli.Commands.Encode;

namespace Deving.Cli.Tests;

public class CodecsTests
{
    [Theory]
    [InlineData("ola mundo", "b2xhIG11bmRv")]
    [InlineData("", "")]
    public void Base64_RoundTrips(string plain, string encoded)
    {
        Assert.Equal(encoded, Codecs.Base64Encode(plain));
        Assert.Equal(plain, Codecs.Base64Decode(encoded));
    }

    [Fact]
    public void Base64Url_Decodes_Without_Padding()
    {
        // {"a":1} em base64url é eyJhIjoxfQ (sem '=')
        var bytes = Codecs.Base64UrlDecode("eyJhIjoxfQ");
        Assert.Equal("{\"a\":1}", Encoding.UTF8.GetString(bytes));
    }

    [Theory]
    [InlineData("sha256", "abc", "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad")]
    [InlineData("md5", "abc", "900150983cd24fb0d6963f7d28e17f72")]
    public void Hash_Produces_Known_Digests(string algo, string input, string expected)
    {
        Assert.Equal(expected, Codecs.Hash(algo, input));
    }

    [Fact]
    public void Hash_Rejects_Unknown_Algorithm()
    {
        Assert.Throws<ArgumentException>(() => Codecs.Hash("crc32", "x"));
    }

    [Fact]
    public void FormatJson_Minify_Removes_Whitespace()
    {
        var result = Codecs.FormatJson("{ \"a\" : 1 }", minify: true);
        Assert.Equal("{\"a\":1}", result);
    }

    [Fact]
    public void FormatJson_Pretty_Adds_Indentation()
    {
        var result = Codecs.FormatJson("{\"a\":1}", minify: false);
        Assert.Contains("\n", result);
    }

    [Fact]
    public void GuidV7_Has_Version_And_Variant_Bits()
    {
        var guid = Codecs.NewGuidV7();
        var bytes = guid.ToByteArray(bigEndian: true);
        Assert.Equal(0x70, bytes[6] & 0xF0);          // versão 7
        Assert.Equal(0x80, bytes[8] & 0xC0);          // variante RFC 4122
    }

    [Fact]
    public void GuidV7_Is_Time_Ordered()
    {
        var older = Codecs.NewGuidV7(DateTimeOffset.UnixEpoch.AddSeconds(1));
        var newer = Codecs.NewGuidV7(DateTimeOffset.UnixEpoch.AddSeconds(2));
        Assert.True(string.CompareOrdinal(older.ToString(), newer.ToString()) < 0);
    }

    [Theory]
    [InlineData(0, "1970-01-01T00:00:00")]
    [InlineData(1609459200, "2021-01-01T00:00:00")]
    public void EpochToDate_Handles_Seconds(long epoch, string expectedPrefix)
    {
        var dt = Codecs.EpochToDate(epoch);
        Assert.StartsWith(expectedPrefix, dt.UtcDateTime.ToString("s"));
    }

    [Fact]
    public void EpochToDate_Detects_Milliseconds()
    {
        var dt = Codecs.EpochToDate(1609459200000);
        Assert.Equal(2021, dt.Year);
    }
}
