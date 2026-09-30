using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartGest.Infrastructure.Persistence;

namespace SmartGest.Desktop.Services;

public class CategoriaService
{
    private readonly IDbContextFactory<SmartGestDbContext> _factory;
    public CategoriaService(IDbContextFactory<SmartGestDbContext> factory) => _factory = factory;

    public async Task<List<CategoriaItem>> ListarAsync(string? tipo = null)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var query = db.CategoriaContabeis.AsNoTracking().Where(x => x.Ativo);
        if (!string.IsNullOrWhiteSpace(tipo))
            query = query.Where(x => x.Tipo == tipo);

        return await query.OrderBy(x => x.Id)
            .Select(x => new CategoriaItem(x.Id, x.Nome, x.Tipo))
            .ToListAsync();
    }

    public record CategoriaItem(int? Id, string Nome, string Tipo)
    {
        public override string ToString() => Nome;
    }
}