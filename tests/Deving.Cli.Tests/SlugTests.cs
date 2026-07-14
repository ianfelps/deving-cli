using Deving.Cli.Infrastructure;

namespace Deving.Cli.Tests;

public class SlugTests
{
    [Theory]
    [InlineData("Minha Feature Nova", "minha-feature-nova")]
    [InlineData("Corrige Bug #42 no Login!", "corrige-bug-42-no-login")]
    [InlineData("  espaços   demais  ", "espacos-demais")]
    [InlineData("Ação de Configuração", "acao-de-configuracao")]
    [InlineData("já-kebab", "ja-kebab")]
    [InlineData("", "")]
    public void Kebab_Normalizes(string input, string expected)
    {
        Assert.Equal(expected, Slug.Kebab(input));
    }
}
