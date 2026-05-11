using System.Globalization;
using CasaMulher.Api.Data;
using CasaMulher.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasaMulher.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RelatorioController(AppDbContext context) : ControllerBase
{
    private const string StatusMatriculaAtiva = "Ativa";

    [HttpGet("frequencia/turmas/{turmaId:int}")]
    public async Task<IActionResult> GetRelatorioFrequenciaDaTurma(int turmaId)
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
            .Where(a => a.TurmaId == turmaId)
            .OrderBy(a => a.Data)
            .ThenBy(a => a.HoraInicio)
            .ToListAsync();

        var matriculasAtivas = await context.Matriculas
            .AsNoTracking()
            .Include(m => m.Aluna)
            .Where(m => m.TurmaId == turmaId && m.Status == StatusMatriculaAtiva)
            .OrderBy(m => m.Aluna!.Nome)
            .ToListAsync();

        var aulaIds = aulas.Select(a => a.Id).ToList();
        var matriculaIds = matriculasAtivas.Select(m => m.Id).ToList();

        var presencas = await context.Presencas
            .AsNoTracking()
            .Where(p => aulaIds.Contains(p.AulaId) && matriculaIds.Contains(p.MatriculaId))
            .ToListAsync();

        var totalAulas = aulas.Count;
        var totalAlunas = matriculasAtivas.Count;
        var totalPossivelDeRegistros = totalAulas * totalAlunas;

        var presentes = presencas.Count(p => p.Status == PresencaStatus.Presente);
        var faltas = presencas.Count(p => p.Status == PresencaStatus.Faltou);
        var faltasJustificadas = presencas.Count(p => p.Status == PresencaStatus.FaltaJustificada);
        var pendentesRegistrados = presencas.Count(p => p.Status == PresencaStatus.Pendente);

        var registrosExistentes = presencas.Count;
        var pendentesNaoGerados = Math.Max(totalPossivelDeRegistros - registrosExistentes, 0);
        var pendentes = pendentesRegistrados + pendentesNaoGerados;

        var percentualPresenca = CalcularPercentual(presentes, totalPossivelDeRegistros);

        var alunas = matriculasAtivas.Select(m =>
        {
            var presencasDaAluna = presencas
                .Where(p => p.MatriculaId == m.Id)
                .ToList();

            var presentesAluna = presencasDaAluna.Count(p => p.Status == PresencaStatus.Presente);
            var faltasAluna = presencasDaAluna.Count(p => p.Status == PresencaStatus.Faltou);
            var justificadasAluna = presencasDaAluna.Count(p => p.Status == PresencaStatus.FaltaJustificada);
            var pendentesRegistradosAluna = presencasDaAluna.Count(p => p.Status == PresencaStatus.Pendente);
            var pendentesNaoGeradosAluna = Math.Max(totalAulas - presencasDaAluna.Count, 0);
            var pendentesAluna = pendentesRegistradosAluna + pendentesNaoGeradosAluna;
            var percentualAluna = CalcularPercentual(presentesAluna, totalAulas);

            return new
            {
                matriculaId = m.Id,
                alunaId = m.AlunaId,
                aluna = m.Aluna?.Nome,
                totalAulas,
                presentes = presentesAluna,
                faltas = faltasAluna,
                faltasJustificadas = justificadasAluna,
                pendentes = pendentesAluna,
                percentualPresenca = percentualAluna,
                situacao = DefinirSituacaoFrequencia(percentualAluna)
            };
        }).ToList();

        return Ok(new
        {
            turma = ToTurmaResumo(turma),
            resumo = new
            {
                totalAulas,
                totalAlunas,
                totalPossivelDeRegistros,
                presentes,
                faltas,
                faltasJustificadas,
                pendentes,
                percentualPresenca
            },
            alunas
        });
    }

    [HttpGet("frequencia/alunas/{alunaId:int}")]
    public async Task<IActionResult> GetRelatorioFrequenciaDaAluna(int alunaId)
    {
        var aluna = await context.Alunas
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == alunaId);

        if (aluna is null)
        {
            return NotFound(new { mensagem = "Aluna nao encontrada." });
        }

        var matriculas = await context.Matriculas
            .AsNoTracking()
            .Include(m => m.Turma)
                .ThenInclude(t => t!.Curso)
            .Where(m => m.AlunaId == alunaId)
            .OrderByDescending(m => m.DataMatricula)
            .ToListAsync();

        var relatoriosPorTurma = new List<object>();

        var totalGeralAulas = 0;
        var totalGeralPresentes = 0;
        var totalGeralFaltas = 0;
        var totalGeralJustificadas = 0;
        var totalGeralPendentes = 0;

        foreach (var matricula in matriculas)
        {
            var aulas = await context.Aulas
                .AsNoTracking()
                .Where(a => a.TurmaId == matricula.TurmaId)
                .OrderBy(a => a.Data)
                .ThenBy(a => a.HoraInicio)
                .ToListAsync();

            var aulaIds = aulas.Select(a => a.Id).ToList();

            var presencas = await context.Presencas
                .AsNoTracking()
                .Where(p => p.MatriculaId == matricula.Id && aulaIds.Contains(p.AulaId))
                .ToListAsync();

            var totalAulas = aulas.Count;
            var presentes = presencas.Count(p => p.Status == PresencaStatus.Presente);
            var faltas = presencas.Count(p => p.Status == PresencaStatus.Faltou);
            var justificadas = presencas.Count(p => p.Status == PresencaStatus.FaltaJustificada);
            var pendentesRegistrados = presencas.Count(p => p.Status == PresencaStatus.Pendente);
            var pendentesNaoGerados = Math.Max(totalAulas - presencas.Count, 0);
            var pendentes = pendentesRegistrados + pendentesNaoGerados;
            var percentual = CalcularPercentual(presentes, totalAulas);

            totalGeralAulas += totalAulas;
            totalGeralPresentes += presentes;
            totalGeralFaltas += faltas;
            totalGeralJustificadas += justificadas;
            totalGeralPendentes += pendentes;

            relatoriosPorTurma.Add(new
            {
                matriculaId = matricula.Id,
                turmaId = matricula.TurmaId,
                turma = matricula.Turma?.Nome,
                curso = matricula.Turma?.Curso?.Nome,
                local = matricula.Turma?.Local,
                responsavel = matricula.Turma?.Responsavel,
                statusMatricula = matricula.Status,
                totalAulas,
                presentes,
                faltas,
                faltasJustificadas = justificadas,
                pendentes,
                percentualPresenca = percentual,
                situacao = DefinirSituacaoFrequencia(percentual)
            });
        }

        var percentualGeral = CalcularPercentual(totalGeralPresentes, totalGeralAulas);

        return Ok(new
        {
            aluna = new
            {
                aluna.Id,
                NomeCompleto = aluna.Nome,
                aluna.Telefone,
                aluna.Email
            },
            resumoGeral = new
            {
                totalAulas = totalGeralAulas,
                presentes = totalGeralPresentes,
                faltas = totalGeralFaltas,
                faltasJustificadas = totalGeralJustificadas,
                pendentes = totalGeralPendentes,
                percentualPresenca = percentualGeral,
                situacao = DefinirSituacaoFrequencia(percentualGeral)
            },
            turmas = relatoriosPorTurma
        });
    }

    [HttpGet("frequencia/turmas/{turmaId:int}/aulas")]
    public async Task<IActionResult> GetRelatorioAulasDaTurma(int turmaId)
    {
        var turmaExiste = await context.Turmas
            .AsNoTracking()
            .AnyAsync(t => t.Id == turmaId);

        if (!turmaExiste)
        {
            return NotFound(new { mensagem = "Turma nao encontrada." });
        }

        var aulas = await context.Aulas
            .AsNoTracking()
            .Where(a => a.TurmaId == turmaId)
            .OrderBy(a => a.Data)
            .ThenBy(a => a.HoraInicio)
            .ToListAsync();

        var aulaIds = aulas.Select(a => a.Id).ToList();

        var presencas = await context.Presencas
            .AsNoTracking()
            .Where(p => aulaIds.Contains(p.AulaId))
            .ToListAsync();

        var relatorioAulas = aulas.Select(aula =>
        {
            var presencasDaAula = presencas
                .Where(p => p.AulaId == aula.Id)
                .ToList();

            var presentes = presencasDaAula.Count(p => p.Status == PresencaStatus.Presente);
            var faltas = presencasDaAula.Count(p => p.Status == PresencaStatus.Faltou);
            var justificadas = presencasDaAula.Count(p => p.Status == PresencaStatus.FaltaJustificada);
            var pendentes = presencasDaAula.Count(p => p.Status == PresencaStatus.Pendente);

            return new
            {
                aula.Id,
                aula.Data,
                HorarioInicio = FormatTime(aula.HoraInicio),
                HorarioFim = FormatTime(aula.HoraFim),
                totalRegistros = presencasDaAula.Count,
                presentes,
                faltas,
                faltasJustificadas = justificadas,
                pendentes
            };
        }).ToList();

        return Ok(new
        {
            turmaId,
            quantidadeAulas = relatorioAulas.Count,
            aulas = relatorioAulas
        });
    }

    private static decimal CalcularPercentual(int parte, int total)
    {
        return total == 0
            ? 0
            : Math.Round((decimal)parte / total * 100, 2);
    }

    private static string DefinirSituacaoFrequencia(decimal percentual)
    {
        if (percentual >= 75)
        {
            return "Regular";
        }

        if (percentual >= 50)
        {
            return "Atencao";
        }

        return "Critica";
    }

    private static string FormatTime(TimeOnly time)
    {
        return time.ToString("HH:mm", CultureInfo.InvariantCulture);
    }

    private static object ToTurmaResumo(Turma turma)
    {
        return new
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
        };
    }
}
