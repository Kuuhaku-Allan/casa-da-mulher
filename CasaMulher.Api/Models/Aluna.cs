namespace CasaMulher.Api.Models;

public class Aluna
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public DateOnly? DataNascimento { get; set; }
    public string? Endereco { get; set; }
    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;

    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    public ICollection<FichaAtendimento> FichasAtendimento { get; set; } = new List<FichaAtendimento>();
}
