namespace CasaMulher.Api.DTOs;

public class UpdateCursoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int CargaHoraria { get; set; }
    public bool Ativo { get; set; } = true;
}
