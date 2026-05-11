using CasaMulher.Api.Data;
using CasaMulher.Api.DTOs;
using CasaMulher.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasaMulher.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlunaController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AlunaResponse>>> GetAll()
    {
        var alunas = await context.Alunas
            .AsNoTracking()
            .OrderBy(aluna => aluna.Nome)
            .ToListAsync();

        return Ok(alunas.Select(ToResponse).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AlunaResponse>> GetById(int id)
    {
        var aluna = await context.Alunas
            .AsNoTracking()
            .FirstOrDefaultAsync(aluna => aluna.Id == id);

        if (aluna is null)
        {
            return NotFound(new { mensagem = "Aluna nao encontrada." });
        }

        return Ok(ToResponse(aluna));
    }

    [HttpPost]
    public async Task<ActionResult<AlunaResponse>> Create(CreateAlunaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NomeCompleto))
        {
            return BadRequest(new { mensagem = "O nome completo da aluna e obrigatorio." });
        }

        var aluna = new Aluna
        {
            Nome = request.NomeCompleto.Trim(),
            Telefone = request.Telefone?.Trim(),
            Email = request.Email?.Trim(),
            CriadaEm = DateTime.UtcNow
        };

        context.Alunas.Add(aluna);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = aluna.Id }, ToResponse(aluna));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AlunaResponse>> Update(int id, UpdateAlunaRequest request)
    {
        var aluna = await context.Alunas.FindAsync(id);

        if (aluna is null)
        {
            return NotFound(new { mensagem = "Aluna nao encontrada." });
        }

        if (string.IsNullOrWhiteSpace(request.NomeCompleto))
        {
            return BadRequest(new { mensagem = "O nome completo da aluna e obrigatorio." });
        }

        aluna.Nome = request.NomeCompleto.Trim();
        aluna.Telefone = request.Telefone?.Trim();
        aluna.Email = request.Email?.Trim();

        await context.SaveChangesAsync();

        return Ok(ToResponse(aluna));
    }

    private static AlunaResponse ToResponse(Aluna aluna)
    {
        return new AlunaResponse
        {
            Id = aluna.Id,
            NomeCompleto = aluna.Nome,
            Telefone = aluna.Telefone,
            Email = aluna.Email,
            DataCadastro = aluna.CriadaEm
        };
    }
}
