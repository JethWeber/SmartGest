using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartGest.API.Data;
using SmartGest.API.DTOs.Requests;
using SmartGest.API.DTOs.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartGest.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;

    public AuthController(AppDbContext db, IConfiguration cfg)
    {
        _db  = db;
        _cfg = cfg;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Utilizadores
            .FirstOrDefaultAsync(u => u.Telefone == req.Telefone && u.Activo);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Número ou senha incorretos." });

        var token = GerarToken(user);
        return Ok(new LoginResponse(token, user.Nome, user.Perfil, user.Iniciais, user.CorAvatar));
    }

    [HttpPost("alterar-senha")]
    [Authorize]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaRequest req)
    {
        var id   = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Utilizadores.FindAsync(id);
        if (user is null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(req.SenhaAtual, user.PasswordHash))
            return BadRequest(new { message = "Senha actual incorrecta." });

        if (req.SenhaNova.Length < 8)
            return BadRequest(new { message = "A nova senha deve ter pelo menos 8 caracteres." });

        if (req.SenhaNova != req.SenhaConfirmacao)
            return BadRequest(new { message = "As senhas não coincidem." });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.SenhaNova);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Senha alterada com sucesso." });
    }

    private string GerarToken(Models.Utilizador user)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Nome),
            new Claim(ClaimTypes.Role, user.Perfil),
        };

        var token = new JwtSecurityToken(
            issuer:             _cfg["Jwt:Issuer"],
            audience:           _cfg["Jwt:Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
