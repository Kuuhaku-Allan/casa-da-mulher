namespace CasaMulher.Api.Models;

public class Aula
{
    public int Id { get; set; }
    public int TurmaId { get; set; }
    public DateOnly Data { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFim { get; set; }
    public string? Conteudo { get; set; }

    public Turma? Turma { get; set; }
    public ICollection<Presenca> Presencas { get; set; } = new List<Presenca>();
}
