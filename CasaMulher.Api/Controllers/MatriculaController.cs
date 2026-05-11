using CasaMulher.Api.Data;
using CasaMulher.Api.DTOs;
using CasaMulher.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasaMulher.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatriculaController(AppDbContext context) : ControllerBase
{
    private const string StatusAtiva = "Ativa";
    private const string StatusCancelada = "Cancelada";

    [HttpGet]
    public async Task<ActionResult<List<MatriculaResponse>>> GetAll()
    {
        var matriculas = await context.Matriculas
            .AsNoTracking()
            .Include(matricula => matricula.Aluna)
            .Include(matricula => matricula.Turma)
                .ThenInclude(turma => turma!.Curso)
            .OrderByDescending(matricula => matricula.DataMatricula)
            .ToListAsync();

        return Ok(matriculas.Select(ToResponse).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MatriculaResponse>> GetById(int id)
    {
        var matricula = await context.Matriculas
            .AsNoTracking()
            .Include(m => m.Aluna)
            .Include(m => m.Turma)
                .ThenInclude(t => t!.Curso)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (matricula is null)
        {
            return NotFound(new { mensagem = "Matricula nao encontrada." });
        }

        return Ok(ToResponse(matricula));
    }

    [HttpGet("aluna/{alunaId:int}")]
    public async Task<ActionResult<List<MatriculaResponse>>> GetByAluna(int alunaId)
    {
        var alunaExiste = await context.Alunas.AnyAsync(aluna => aluna.Id == alunaId);

        if (!alunaExiste)
        {
            return NotFound(new { mensagem = "Aluna nao encontrada." });
        }

        var matriculas = await context.Matriculas
            .AsNoTracking()
            .Include(matricula => matricula.Aluna)
            .Include(matricula => matricula.Turma)
                .ThenInclude(turma => turma!.Curso)
            .Where(matricula => matricula.AlunaId == alunaId)
            .OrderByDescending(matricula => matricula.DataMatricula)
            .ToListAsync();

        return Ok(matriculas.Select(ToResponse).ToList());
    }

    [HttpGet("turma/{turmaId:int}")]
    public async Task<ActionResult<List<MatriculaResponse>>> GetByTurma(int turmaId)
    {
        var turmaExiste = await context.Turmas.AnyAsync(turma => turma.Id == turmaId);

        if (!turmaExiste)
        {
            return NotFound(new { mensagem = "Turma nao encontrada." });
        }

        var matriculas = await context.Matriculas
            .AsNoTracking()
            .Include(matricula => matricula.Aluna)
            .Include(matricula => matricula.Turma)
                .ThenInclude(turma => turma!.Curso)
            .Where(matricula => matricula.TurmaId == turmaId)
            .OrderBy(matricula => matricula.Aluna!.Nome)
            .ToListAsync();

        return Ok(matriculas.Select(ToResponse).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<MatriculaResponse>> Create(CreateMatriculaRequest request)
    {
        var alunaExiste = await context.Alunas.AnyAsync(aluna => aluna.Id == request.AlunaId);

        if (!alunaExiste)
        {
            return BadRequest(new { mensagem = "Aluna nao encontrada." });
        }

        var turmaExiste = await context.Turmas.AnyAsync(turma => turma.Id == request.TurmaId);

        if (!turmaExiste)
        {
            return BadRequest(new { mensagem = "Turma nao encontrada." });
        }

        var matriculaExistente = await context.Matriculas
            .Include(matricula => matricula.Aluna)
            .Include(matricula => matricula.Turma)
                .ThenInclude(turma => turma!.Curso)
            .FirstOrDefaultAsync(matricula =>
                matricula.AlunaId == request.AlunaId &&
                matricula.TurmaId == request.TurmaId);

        if (matriculaExistente is not null)
        {
            if (MatriculaEstaAtiva(matriculaExistente))
            {
                return BadRequest(new { mensagem = "A aluna ja esta matriculada nesta turma." });
            }

            matriculaExistente.Status = StatusAtiva;
            matriculaExistente.DataMatricula = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return Ok(ToResponse(matriculaExistente));
        }

        var matricula = new Matricula
        {
            AlunaId = request.AlunaId,
            TurmaId = request.TurmaId,
            DataMatricula = DateTime.UtcNow,
            Status = StatusAtiva
        };

        context.Matriculas.Add(matricula);
        await context.SaveChangesAsync();

        await context.Entry(matricula).Reference(m => m.Aluna).LoadAsync();
        await context.Entry(matricula).Reference(m => m.Turma).LoadAsync();

        if (matricula.Turma is not null)
        {
            await context.Entry(matricula.Turma).Reference(t => t.Curso).LoadAsync();
        }

        return CreatedAtAction(nameof(GetById), new { id = matricula.Id }, ToResponse(matricula));
    }

    [HttpPatch("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        var matricula = await context.Matriculas.FindAsync(id);

        if (matricula is null)
        {
            return NotFound(new { mensagem = "Matricula nao encontrada." });
        }

        if (!MatriculaEstaAtiva(matricula))
        {
            return BadRequest(new { mensagem = "Esta matricula ja esta cancelada." });
        }

        matricula.Status = StatusCancelada;
        await context.SaveChangesAsync();

        return Ok(new { mensagem = "Matricula cancelada com sucesso." });
    }

    private static MatriculaResponse ToResponse(Matricula matricula)
    {
        return new MatriculaResponse
        {
            Id = matricula.Id,
            AlunaId = matricula.AlunaId,
            Aluna = matricula.Aluna?.Nome,
            TurmaId = matricula.TurmaId,
            Turma = matricula.Turma?.Nome,
            Curso = matricula.Turma?.Curso?.Nome,
            DataMatricula = matricula.DataMatricula,
            Ativa = MatriculaEstaAtiva(matricula)
        };
    }

    private static bool MatriculaEstaAtiva(Matricula matricula)
    {
        return string.Equals(matricula.Status, StatusAtiva, StringComparison.OrdinalIgnoreCase);
    }
}
