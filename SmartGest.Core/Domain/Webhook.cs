namespace SmartGest.Core.Domain;

public sealed class Webhook
{
    public int Id { get; private set; }
    public string Evento { get; private set; }
    public string Url { get; private set; }
    public bool Activo { get; private set; }

    private Webhook() { }

    public Webhook(string evento, string url)
    {
        if (string.IsNullOrWhiteSpace(evento)) throw new ArgumentException("O evento é obrigatório.", nameof(evento));
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("A URL é obrigatória.", nameof(url));
        Evento = evento.Trim();
        Url = url.Trim();
    }
}
