namespace CasaMulher.Api.DTOs;

public class RegistroPresencaItemRequest
{
    public int MatriculaId { get; set; }
    public int AlunaId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Observacao { get; set; }
}
