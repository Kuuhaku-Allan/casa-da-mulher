namespace CasaMulher.Api.DTOs;

public class CreateAlunaRequest
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
}
