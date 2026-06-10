using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartGest.Desktop.Services;

/// <summary>
/// Obtém as categorias financeiras válidas da API.
/// GET /api/lancamentos/categorias?tipo=Entrada|Saída
/// </summary>
public class CategoriaService
{
    private readonly ApiClient _api;
    public CategoriaService(ApiClient api) => _api = api;

    public async Task<List<CategoriaItem>> ListarAsync(string? tipo = null)
    {
        var url = "api/lancamentos/categorias";
        if (!string.IsNullOrEmpty(tipo))
            url += $"?tipo={System.Uri.EscapeDataString(tipo)}";

        var resp = await _api.GetAsync<List<CategoriaItem>>(url);
        return resp ?? new List<CategoriaItem>();
    }

    public record CategoriaItem(int? Id, string Nome, string Tipo)
    {
        public override string ToString() => Nome;
    }
}