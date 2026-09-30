using System;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using SmartGest.Core.Domain;
using SmartGest.Infrastructure.Persistence;

namespace SmartGest.Desktop.Services;

public class AuthService
{
    private readonly IDbContextFactory<SmartGestDbContext> _factory;
    private readonly TokenStore _store;
    private readonly AuditService _audit;

    public AuthService(IDbContextFactory<SmartGestDbContext> factory, TokenStore store, AuditService audit)
    {
        _factory = factory;
        _store = store;
        _audit = audit;
    }

    public async Task LoginAsync(string telefone, string password)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var user = await db.Utilizadores
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Telefone == telefone && u.Activo);

        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Número ou senha incorretos.");

        _store.Token = "LOCAL";
        _store.Telefone = user.Telefone;
        _store.Nome = user.Nome;
        _store.Perfil = user.Perfil;
        _store.Iniciais = user.Iniciais;
        _store.CorAvatar = user.CorAvatar;
        await _audit.RegistarAsync("Login", "Utilizador", user.Id.ToString());
    }

    public async Task AlterarSenhaAsync(string senhaAtual, string senhaNova, string senhaConf)
    {
        await using var db = await _factory.CreateDbContextAsync();
        if (senhaNova.Length < 8)
            throw new ArgumentException("A nova senha deve ter pelo menos 8 caracteres.");
        if (senhaNova != senhaConf)
            throw new ArgumentException("As senhas não coincidem.");

        var user = await db.Utilizadores.FirstOrDefaultAsync(u => u.Telefone == _store.Telefone);
        if (user is null || !BCrypt.Net.BCrypt.Verify(senhaAtual, user.PasswordHash))
            throw new UnauthorizedAccessException("Senha actual incorrecta.");

        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Utilizadores SET PasswordHash = {BCrypt.Net.BCrypt.HashPassword(senhaNova, workFactor: 12)} WHERE Id = {user.Id}");
        await _audit.RegistarAsync("Alteração de senha", "Utilizador", user.Id.ToString());
    }

    public async Task LogoutAsync()
    {
        await _audit.RegistarAsync("Logout", "Utilizador");
        _store.Limpar();
    }

    public void Logout() => _store.Limpar();
}