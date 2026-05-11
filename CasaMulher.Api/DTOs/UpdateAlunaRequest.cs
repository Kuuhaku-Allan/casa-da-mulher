namespace CasaMulher.Api.DTOs;

public class UpdateAlunaRequest
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
}
