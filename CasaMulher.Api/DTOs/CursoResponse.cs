namespace CasaMulher.Api.DTOs;

public class CursoResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int CargaHoraria { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
}
