using System;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using SmartGest.Core.Domain;
using SmartGest.Infrastructure.Persistence;

namespace SmartGest.Desktop.Services;

public class AuthService
{
    private readonly SmartGestDbContext _db;
    private readonly TokenStore _store;

    public AuthService(SmartGestDbContext db, TokenStore store)
    {
        _db = db;
        _store = store;
    }

    public async Task LoginAsync(string telefone, string password)
    {
        var user = await _db.Utilizadores
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Telefone == telefone && u.Activo);

        if (user is null || !BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Número ou senha incorretos.");

        _store.Token = "LOCAL";
        _store.Telefone = user.Telefone;
        _store.Nome = user.Nome;
        _store.Perfil = user.Perfil;
        _store.Iniciais = user.Iniciais;
        _store.CorAvatar = user.CorAvatar;
    }

    public async Task AlterarSenhaAsync(string senhaAtual, string senhaNova, string senhaConf)
    {
        if (senhaNova.Length < 8)
            throw new ArgumentException("A nova senha deve ter pelo menos 8 caracteres.");
        if (senhaNova != senhaConf)
            throw new ArgumentException("As senhas não coincidem.");

        var user = await _db.Utilizadores.FirstOrDefaultAsync(u => u.Telefone == _store.Telefone);
        if (user is null || !BCrypt.Verify(senhaAtual, user.PasswordHash))
            throw new UnauthorizedAccessException("Senha actual incorrecta.");

        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Utilizadores SET PasswordHash = {BCrypt.HashPassword(senhaNova, workFactor: 12)} WHERE Id = {user.Id}");
    }

    public void Logout() => _store.Limpar();
}