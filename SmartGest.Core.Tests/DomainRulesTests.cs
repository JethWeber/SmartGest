using SmartGest.Core.Domain;

namespace SmartGest.Core.Tests;

public sealed class DomainRulesTests
{
    [Fact]
    public void Lancamento_Rejects_NonPositiveValue()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Lancamento(DateTime.UtcNow, "Teste", "Entrada", 0m, 1));
    }

    [Fact]
    public void Lancamento_Rejects_InvalidType()
    {
        Assert.Throws<ArgumentException>(() =>
            new Lancamento(DateTime.UtcNow, "Teste", "Transferência", 100m, 1));
    }

    [Fact]
    public void Lancamento_CanBeCancelled_OnlyOnce()
    {
        var lancamento = new Lancamento(DateTime.UtcNow, "Teste", "Saída", 100m, 1);

        lancamento.Anular("Administrador", "Teste de anulação");

        Assert.Throws<InvalidOperationException>(() =>
            lancamento.Anular("Administrador", "Segunda tentativa"));
    }

    [Fact]
    public void MovimentoBancario_Rejects_InvalidType()
    {
        Assert.Throws<ArgumentException>(() =>
            new MovimentoBancario(1, DateTime.UtcNow, "Teste", "", "Transferência", 100m));
    }

    [Fact]
    public void Utilizador_GeneratesExpectedInitials()
    {
        var user = new Utilizador(
            "João Weber",
            "joao@example.local",
            "923000000",
            "hash",
            "Administrador");

        Assert.Equal("JW", user.Iniciais);
        Assert.True(user.Activo);
    }

    [Fact]
    public void CategoriaContabil_RequiresValidType()
    {
        Assert.Throws<ArgumentException>(() =>
            new CategoriaContabil("Teste", "Outro", "1", "2"));
    }
}
