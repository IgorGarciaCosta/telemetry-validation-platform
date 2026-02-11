using Xunit;

namespace Api.Tests;

public class CalculadoraTeste
{
    [Fact]
    public void DeveSomarDoisNumeros()
    {
        // Arrange (Preparação)
        int a = 5;
        int b = 5;

        // Act (Ação)
        int resultado = a + b;

        // Assert (Verificação)
        Assert.Equal(10, resultado);
    }
}
