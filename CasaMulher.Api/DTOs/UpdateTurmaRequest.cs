namespace CasaMulher.Api.DTOs;

public class UpdateTurmaRequest
{
    public int CursoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Local { get; set; } = string.Empty;
    public string Responsavel { get; set; } = string.Empty;
    public DateOnly DataInicio { get; set; }
    public DateOnly DataFim { get; set; }
    public string HorarioInicio { get; set; } = string.Empty;
    public string HorarioFim { get; set; } = string.Empty;
    public string DiasDaSemana { get; set; } = string.Empty;
    public int Vagas { get; set; }
    public bool Ativa { get; set; } = true;
}
