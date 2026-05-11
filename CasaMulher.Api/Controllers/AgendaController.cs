using System.Globalization;
using CasaMulher.Api.Data;
using CasaMulher.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasaMulher.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgendaController(AppDbContext context, GeradorGradeService geradorGradeService) : ControllerBase
{
    private const string StatusMatriculaAtiva = "Ativa";

    [HttpPost("turmas/{turmaId:int}/gerar-aulas")]
    public async Task<IActionResult> GerarAulasDaTurma(int turmaId)
    {
        var resultado = await geradorGradeService.GerarAulasDaTurmaAsync(turmaId);

        if (!resultado.Sucesso)
        {
            return BadRequest(new
            {
                mensagem = resultado.Mensagem,
                aulasCriadas = resultado.AulasCriadas
            });
        }

        return Ok(new
        {
            mensagem = resultado.Mensagem,
            aulasCriadas = resultado.AulasCriadas
        });
    }

    [HttpGet("turmas/{turmaId:int}")]
    public async Task<IActionResult> GetGradeDaTurma(int turmaId)
    {
        var turma = await context.Turmas
            .AsNoTracking()
            .Include(t => t.Curso)
            .FirstOrDefaultAsync(t => t.Id == turmaId);

        if (turma is null)
        {
            return NotFound(new { mensagem = "Turma nao encontrada." });
        }

        var aulas = await context.Aulas
            .AsNoTracking()
            .Where(aula => aula.TurmaId == turmaId)
            .OrderBy(aula => aula.Data)
            .ThenBy(aula => aula.HoraInicio)
            .ToListAsync();

        return Ok(new
        {
            turma = new
            {
                turma.Id,
                turma.Nome,
                Curso = turma.Curso?.Nome,
                turma.Local,
                turma.Responsavel,
                turma.DataInicio,
                turma.DataFim,
                HorarioInicio = FormatTime(turma.HoraInicio),
                HorarioFim = FormatTime(turma.HoraFim),
                DiasDaSemana = turma.DiasSemana
            },
            quantidadeAulas = aulas.Count,
            aulas = aulas.Select(aula => new
            {
                aula.Id,
                aula.Data,
                HorarioInicio = FormatTime(aula.HoraInicio),
                HorarioFim = FormatTime(aula.HoraFim),
                Status = "Agendada"
            }).ToList()
        });
    }

    [HttpGet("alunas/{alunaId:int}")]
    public async Task<IActionResult> GetGradeDaAluna(int alunaId)
    {
        var aluna = await context.Alunas
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == alunaId);

        if (aluna is null)
        {
            return NotFound(new { mensagem = "Aluna nao encontrada." });
        }

        var turmaIds = await context.Matriculas
            .AsNoTracking()
            .Where(matricula =>
                matricula.AlunaId == alunaId &&
                matricula.Status == StatusMatriculaAtiva)
            .Select(matricula => matricula.TurmaId)
            .ToListAsync();

        var aulas = await context.Aulas
            .AsNoTracking()
            .Include(aula => aula.Turma)
                .ThenInclude(turma => turma!.Curso)
            .Where(aula => turmaIds.Contains(aula.TurmaId))
            .OrderBy(aula => aula.Data)
            .ThenBy(aula => aula.HoraInicio)
            .ToListAsync();

        return Ok(new
        {
            aluna = new
            {
                aluna.Id,
                NomeCompleto = aluna.Nome,
                aluna.Telefone,
                aluna.Email
            },
            quantidadeAulas = aulas.Count,
            aulas = aulas.Select(aula => new
            {
                aula.Id,
                aula.Data,
                HorarioInicio = FormatTime(aula.HoraInicio),
                HorarioFim = FormatTime(aula.HoraFim),
                Status = "Agendada",
                TurmaId = aula.TurmaId,
                Turma = aula.Turma?.Nome,
                Curso = aula.Turma?.Curso?.Nome,
                Local = aula.Turma?.Local,
                Responsavel = aula.Turma?.Responsavel
            }).ToList()
        });
    }

    private static string FormatTime(TimeOnly time)
    {
        return time.ToString("HH:mm", CultureInfo.InvariantCulture);
    }
}
