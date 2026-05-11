namespace CasaMulher.Api.Models;

public class Turma
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateOnly DataInicio { get; set; }
    public DateOnly DataFim { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFim { get; set; }
    public string DiasSemana { get; set; } = string.Empty;
    public string? Local { get; set; }
    public string Responsavel { get; set; } = string.Empty;
    public int Vagas { get; set; }
    public bool Ativa { get; set; } = true;

    public Curso? Curso { get; set; }
    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    public ICollection<Aula> Aulas { get; set; } = new List<Aula>();
}
