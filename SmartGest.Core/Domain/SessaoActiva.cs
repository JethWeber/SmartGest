namespace SmartGest.Core.Domain;

public sealed class SessaoActiva
{
    public int Id { get; private set; }
    public int UtilizadorId { get; private set; }
    public string Dispositivo { get; private set; } = string.Empty;
    public string Localizacao { get; private set; } = string.Empty;
    public DateTime UltimaActividade { get; private set; }
    public bool IsAtual { get; private set; }

    private SessaoActiva() { }

    public SessaoActiva(int utilizadorId, string dispositivo, string localizacao = "")
    {
        if (utilizadorId <= 0) throw new ArgumentOutOfRangeException(nameof(utilizadorId));
        UtilizadorId = utilizadorId;
        Dispositivo = dispositivo?.Trim() ?? "";
        Localizacao = localizacao?.Trim() ?? "";
        UltimaActividade = DateTime.UtcNow;
    }
}
