using System.Globalization;
using CasaMulher.Api.Data;
using CasaMulher.Api.DTOs;
using CasaMulher.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasaMulher.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TurmaController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TurmaResponse>>> GetAll()
    {
        var turmas = await context.Turmas
            .AsNoTracking()
            .Include(turma => turma.Curso)
            .OrderBy(turma => turma.Nome)
            .ToListAsync();

        return Ok(turmas.Select(ToResponse).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TurmaResponse>> GetById(int id)
    {
        var turma = await context.Turmas
            .AsNoTracking()
            .Include(turma => turma.Curso)
            .FirstOrDefaultAsync(turma => turma.Id == id);

        if (turma is null)
        {
            return NotFound(new { mensagem = "Turma nao encontrada." });
        }

        return Ok(ToResponse(turma));
    }

    [HttpPost]
    public async Task<ActionResult<TurmaResponse>> Create(CreateTurmaRequest request)
    {
        var validationMessage = await ValidateTurmaRequest(
            request.CursoId,
            request.Nome,
            request.Local,
            request.Responsavel,
            request.DataInicio,
            request.DataFim,
            request.HorarioInicio,
            request.HorarioFim,
            request.DiasDaSemana,
            request.Vagas);

        if (validationMessage is not null)
        {
            return BadRequest(new { mensagem = validationMessage });
        }

        var horaInicio = ParseTime(request.HorarioInicio);
        var horaFim = ParseTime(request.HorarioFim);

        var turma = new Turma
        {
            CursoId = request.CursoId,
            Nome = request.Nome.Trim(),
            Local = request.Local.Trim(),
            Responsavel = request.Responsavel.Trim(),
            DataInicio = request.DataInicio,
            DataFim = request.DataFim,
            HoraInicio = horaInicio,
            HoraFim = horaFim,
            DiasSemana = request.DiasDaSemana.Trim(),
            Vagas = request.Vagas,
            Ativa = true
        };

        context.Turmas.Add(turma);
        await context.SaveChangesAsync();
        await context.Entry(turma).Reference(t => t.Curso).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = turma.Id }, ToResponse(turma));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TurmaResponse>> Update(int id, UpdateTurmaRequest request)
    {
        var turma = await context.Turmas.FindAsync(id);

        if (turma is null)
        {
            return NotFound(new { mensagem = "Turma nao encontrada." });
        }

        var validationMessage = await ValidateTurmaRequest(
            request.CursoId,
            request.Nome,
            request.Local,
            request.Responsavel,
            request.DataInicio,
            request.DataFim,
            request.HorarioInicio,
            request.HorarioFim,
            request.DiasDaSemana,
            request.Vagas);

        if (validationMessage is not null)
        {
            return BadRequest(new { mensagem = validationMessage });
        }

        turma.CursoId = request.CursoId;
        turma.Nome = request.Nome.Trim();
        turma.Local = request.Local.Trim();
        turma.Responsavel = request.Responsavel.Trim();
        turma.DataInicio = request.DataInicio;
        turma.DataFim = request.DataFim;
        turma.HoraInicio = ParseTime(request.HorarioInicio);
        turma.HoraFim = ParseTime(request.HorarioFim);
        turma.DiasSemana = request.DiasDaSemana.Trim();
        turma.Vagas = request.Vagas;
        turma.Ativa = request.Ativa;

        await context.SaveChangesAsync();

        await context.Entry(turma).Reference(t => t.Curso).LoadAsync();

        return Ok(ToResponse(turma));
    }

    private async Task<string?> ValidateTurmaRequest(
        int cursoId,
        string nome,
        string local,
        string responsavel,
        DateOnly dataInicio,
        DateOnly dataFim,
        string horarioInicio,
        string horarioFim,
        string diasDaSemana,
        int vagas)
    {
        var cursoExiste = await context.Cursos.AnyAsync(curso => curso.Id == cursoId && curso.Ativo);

        if (!cursoExiste)
        {
            return "Curso nao encontrado ou inativo.";
        }

        if (string.IsNullOrWhiteSpace(nome))
        {
            return "O nome da turma e obrigatorio.";
        }

        if (string.IsNullOrWhiteSpace(local))
        {
            return "O local da turma e obrigatorio.";
        }

        if (string.IsNullOrWhiteSpace(responsavel))
        {
            return "O responsavel pela turma e obrigatorio.";
        }

        if (dataInicio == default || dataFim == default)
        {
            return "Informe data inicial e data final.";
        }

        if (dataFim < dataInicio)
        {
            return "A data final nao pode ser anterior a data inicial.";
        }

        if (!TryParseTime(horarioInicio, out var inicio))
        {
            return "Horario inicial invalido. Use o formato HH:mm. Exemplo: 14:00.";
        }

        if (!TryParseTime(horarioFim, out var fim))
        {
            return "Horario final invalido. Use o formato HH:mm. Exemplo: 16:00.";
        }

        if (fim <= inicio)
        {
            return "O horario final deve ser maior que o horario inicial.";
        }

        if (string.IsNullOrWhiteSpace(diasDaSemana))
        {
            return "Informe os dias da semana. Exemplo: Segunda,Quarta.";
        }

        if (vagas < 0)
        {
            return "O numero de vagas nao pode ser negativo.";
        }

        return null;
    }

    private static bool TryParseTime(string value, out TimeOnly time)
    {
        return TimeOnly.TryParseExact(
            value,
            "HH:mm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out time);
    }

    private static TimeOnly ParseTime(string value)
    {
        return TimeOnly.ParseExact(value, "HH:mm", CultureInfo.InvariantCulture);
    }

    private static TurmaResponse ToResponse(Turma turma)
    {
        return new TurmaResponse
        {
            Id = turma.Id,
            CursoId = turma.CursoId,
            CursoNome = turma.Curso?.Nome,
            Nome = turma.Nome,
            Local = turma.Local,
            Responsavel = turma.Responsavel,
            DataInicio = turma.DataInicio,
            DataFim = turma.DataFim,
            HorarioInicio = turma.HoraInicio.ToString("HH:mm", CultureInfo.InvariantCulture),
            HorarioFim = turma.HoraFim.ToString("HH:mm", CultureInfo.InvariantCulture),
            DiasDaSemana = turma.DiasSemana,
            Vagas = turma.Vagas,
            Ativa = turma.Ativa
        };
    }
}
