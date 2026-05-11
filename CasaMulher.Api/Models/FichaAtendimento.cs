namespace CasaMulher.Api.Models;

public class FichaAtendimento
{
    public int Id { get; set; }
    public int AlunaId { get; set; }
    public DateTime DataAtendimento { get; set; } = DateTime.UtcNow;
    public string? TipoAtendimento { get; set; }
    public string? Observacoes { get; set; }
    public FichaAtendimentoStatus Status { get; set; } = FichaAtendimentoStatus.Aberta;
    public bool Validada { get; set; }
    public string? ValidadaPor { get; set; }
    public DateTime? ValidadaEm { get; set; }

    public Aluna? Aluna { get; set; }
}
