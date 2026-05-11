namespace CasaMulher.Api.Models;

public class Presenca
{
    public int Id { get; set; }
    public int AulaId { get; set; }
    public int MatriculaId { get; set; }
    public PresencaStatus Status { get; set; } = PresencaStatus.Pendente;
    public string? Observacao { get; set; }
    public DateTime? RegistradaEm { get; set; }

    public Aula? Aula { get; set; }
    public Matricula? Matricula { get; set; }
}
