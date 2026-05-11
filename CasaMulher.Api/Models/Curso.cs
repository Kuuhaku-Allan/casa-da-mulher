namespace CasaMulher.Api.Models;

public class Curso
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int CargaHoraria { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public ICollection<Turma> Turmas { get; set; } = new List<Turma>();
}
