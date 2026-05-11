namespace CasaMulher.Api.DTOs;

public class MatriculaResponse
{
    public int Id { get; set; }
    public int AlunaId { get; set; }
    public string? Aluna { get; set; }
    public int TurmaId { get; set; }
    public string? Turma { get; set; }
    public string? Curso { get; set; }
    public DateTime DataMatricula { get; set; }
    public bool Ativa { get; set; }
}
