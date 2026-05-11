using CasaMulher.Api.Data;
using CasaMulher.Api.DTOs;
using CasaMulher.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasaMulher.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CursoController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CursoResponse>>> GetAll()
    {
        var cursos = await context.Cursos
            .AsNoTracking()
            .OrderBy(curso => curso.Nome)
            .ToListAsync();

        return Ok(cursos.Select(ToResponse).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CursoResponse>> GetById(int id)
    {
        var curso = await context.Cursos
            .AsNoTracking()
            .FirstOrDefaultAsync(curso => curso.Id == id);

        if (curso is null)
        {
            return NotFound(new { mensagem = "Curso nao encontrado." });
        }

        return Ok(ToResponse(curso));
    }

    [HttpPost]
    public async Task<ActionResult<CursoResponse>> Create(CreateCursoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return BadRequest(new { mensagem = "O nome do curso e obrigatorio." });
        }

        if (request.CargaHoraria < 0)
        {
            return BadRequest(new { mensagem = "A carga horaria nao pode ser negativa." });
        }

        var curso = new Curso
        {
            Nome = request.Nome.Trim(),
            Descricao = request.Descricao?.Trim(),
            CargaHoraria = request.CargaHoraria,
            Ativo = true
        };

        context.Cursos.Add(curso);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = curso.Id }, ToResponse(curso));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CursoResponse>> Update(int id, UpdateCursoRequest request)
    {
        var curso = await context.Cursos.FindAsync(id);

        if (curso is null)
        {
            return NotFound(new { mensagem = "Curso nao encontrado." });
        }

        if (string.IsNullOrWhiteSpace(request.Nome))
        {
            return BadRequest(new { mensagem = "O nome do curso e obrigatorio." });
        }

        if (request.CargaHoraria < 0)
        {
            return BadRequest(new { mensagem = "A carga horaria nao pode ser negativa." });
        }

        curso.Nome = request.Nome.Trim();
        curso.Descricao = request.Descricao?.Trim();
        curso.CargaHoraria = request.CargaHoraria;
        curso.Ativo = request.Ativo;

        await context.SaveChangesAsync();

        return Ok(ToResponse(curso));
    }

    [HttpPatch("{id:int}/desativar")]
    public async Task<IActionResult> Desativar(int id)
    {
        var curso = await context.Cursos.FindAsync(id);

        if (curso is null)
        {
            return NotFound(new { mensagem = "Curso nao encontrado." });
        }

        curso.Ativo = false;
        await context.SaveChangesAsync();

        return Ok(new { mensagem = "Curso desativado com sucesso." });
    }

    private static CursoResponse ToResponse(Curso curso)
    {
        return new CursoResponse
        {
            Id = curso.Id,
            Nome = curso.Nome,
            Descricao = curso.Descricao,
            CargaHoraria = curso.CargaHoraria,
            Ativo = curso.Ativo,
            CriadoEm = curso.CriadoEm
        };
    }
}
