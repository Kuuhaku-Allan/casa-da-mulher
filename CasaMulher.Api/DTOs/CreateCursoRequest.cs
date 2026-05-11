namespace CasaMulher.Api.DTOs;

public class CreateCursoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int CargaHoraria { get; set; }
}
