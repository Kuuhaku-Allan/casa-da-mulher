namespace CasaMulher.Api.DTOs;

public class RegistrarPresencaEmLoteRequest
{
    public List<RegistroPresencaItemRequest> Presencas { get; set; } = new();
}
