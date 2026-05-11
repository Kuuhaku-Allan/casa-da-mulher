namespace CasaMulher.Api.DTOs;

public class AlunaResponse
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public DateTime DataCadastro { get; set; }
}
