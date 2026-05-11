namespace CasaMulher.Api.Models;

public class Matricula
{
    public int Id { get; set; }
    public int AlunaId { get; set; }
    public int TurmaId { get; set; }
    public DateTime DataMatricula { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Ativa";

    public Aluna? Aluna { get; set; }
    public Turma? Turma { get; set; }
    public ICollection<Presenca> Presencas { get; set; } = new List<Presenca>();
}
