using System.Globalization;
using CasaMulher.Api.Data;
using CasaMulher.Api.DTOs;
using CasaMulher.Api.Models;
using CasaMulher.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasaMulher.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChamadaController(AppDbContext context, ChamadaService chamadaService) : ControllerBase
{
    [HttpPost("aulas/{aulaId:int}/gerar-lista")]
    public async Task<IActionResult> GerarListaDaAula(int aulaId)
    {
        var resultado = await chamadaService.GerarListaDaAulaAsync(aulaId);

        if (!resultado.Sucesso)
        {
            return BadRequest(new
            {
                mensagem = resultado.Mensagem,
                presencasCriadas = resultado.Criadas
            });
        }

        return Ok(new
        {
            mensagem = resultado.Mensagem,
            presencasCriadas = resultado.Criadas
        });
    }

    [HttpGet("aulas/{aulaId:int}")]
    public async Task<IActionResult> GetListaDaAula(int aulaId)
    {
        var aula = await context.Aulas
            .AsNoTracking()
            .Include(a => a.Turma)
                .ThenInclude(t => t!.Curso)
            .FirstOrDefaultAsync(a => a.Id == aulaId);

        if (aula is null)
        {
            return NotFound(new { mensagem = "Aula nao encontrada." });
        }

        var lista = await context.Presencas
            .AsNoTracking()
            .Include(p => p.Matricula)
                .ThenInclude(m => m!.Aluna)
            .Where(p => p.AulaId == aulaId)
            .OrderBy(p => p.Matricula!.Aluna!.Nome)
            .ToListAsync();

        return Ok(new
        {
            aula = new
            {
                aula.Id,
                aula.Data,
                HorarioInicio = FormatTime(aula.HoraInicio),
                HorarioFim = FormatTime(aula.HoraFim),
                TurmaId = aula.TurmaId,
                Turma = aula.Turma?.Nome,
                Curso = aula.Turma?.Curso?.Nome,
                Local = aula.Turma?.Local,
                Responsavel = aula.Turma?.Responsavel
            },
            quantidade = lista.Count,
            lista = lista.Select(p => new
            {
                p.Id,
                p.MatriculaId,
                AlunaId = p.Matricula?.AlunaId,
                Aluna = p.Matricula?.Aluna?.Nome,
                Status = p.Status.ToString(),
                p.Observacao,
                RegistradoEm = p.RegistradaEm
            }).ToList()
        });
    }

    [HttpPost("aulas/{aulaId:int}/registrar")]
    public async Task<IActionResult> RegistrarPresenca(int aulaId, RegistrarPresencaRequest request)
    {
        var resultado = request.MatriculaId > 0
            ? await chamadaService.RegistrarPresencaPorMatriculaAsync(
                aulaId,
                request.MatriculaId,
                request.Status,
                request.Observacao)
            : await chamadaService.RegistrarPresencaAsync(
                aulaId,
                request.AlunaId,
                request.Status,
                request.Observacao);

        if (!resultado.Sucesso)
        {
            return BadRequest(new { mensagem = resultado.Mensagem });
        }

        return Ok(new { mensagem = resultado.Mensagem });
    }

    [HttpPost("aulas/{aulaId:int}/registrar-lote")]
    public async Task<IActionResult> RegistrarPresencasEmLote(
        int aulaId,
        RegistrarPresencaEmLoteRequest request)
    {
        if (request.Presencas.Count == 0)
        {
            return BadRequest(new { mensagem = "Informe pelo menos uma presenca para registrar." });
        }

        var erros = new List<object>();
        var sucessos = 0;

        foreach (var item in request.Presencas)
        {
            var resultado = item.MatriculaId > 0
                ? await chamadaService.RegistrarPresencaPorMatriculaAsync(
                    aulaId,
                    item.MatriculaId,
                    item.Status,
                    item.Observacao)
                : await chamadaService.RegistrarPresencaAsync(
                    aulaId,
                    item.AlunaId,
                    item.Status,
                    item.Observacao);

            if (resultado.Sucesso)
            {
                sucessos++;
            }
            else
            {
                erros.Add(new
                {
                    item.MatriculaId,
                    item.AlunaId,
                    mensagem = resultado.Mensagem
                });
            }
        }

        return Ok(new
        {
            mensagem = "Processamento da chamada concluido.",
            registrosComSucesso = sucessos,
            registrosComErro = erros.Count,
            erros
        });
    }

    [HttpGet("alunas/{alunaId:int}/historico")]
    public async Task<IActionResult> GetHistoricoDaAluna(int alunaId)
    {
        var aluna = await context.Alunas
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == alunaId);

        if (aluna is null)
        {
            return NotFound(new { mensagem = "Aluna nao encontrada." });
        }

        var historico = await context.Presencas
            .AsNoTracking()
            .Include(p => p.Matricula)
            .Include(p => p.Aula)
                .ThenInclude(a => a!.Turma)
                    .ThenInclude(t => t!.Curso)
            .Where(p => p.Matricula!.AlunaId == alunaId)
            .OrderBy(p => p.Aula!.Data)
            .ThenBy(p => p.Aula!.HoraInicio)
            .ToListAsync();

        var total = historico.Count;
        var presentes = historico.Count(h => h.Status == PresencaStatus.Presente);
        var faltas = historico.Count(h => h.Status == PresencaStatus.Faltou);
        var justificadas = historico.Count(h => h.Status == PresencaStatus.FaltaJustificada);
        var pendentes = historico.Count(h => h.Status == PresencaStatus.Pendente);

        return Ok(new
        {
            aluna = new
            {
                aluna.Id,
                NomeCompleto = aluna.Nome,
                aluna.Telefone,
                aluna.Email
            },
            resumo = new
            {
                total,
                presentes,
                faltas,
                faltasJustificadas = justificadas,
                pendentes
            },
            historico = historico.Select(p => new
            {
                PresencaId = p.Id,
                AulaId = p.AulaId,
                Data = p.Aula?.Data,
                HorarioInicio = p.Aula is null ? null : FormatTime(p.Aula.HoraInicio),
                HorarioFim = p.Aula is null ? null : FormatTime(p.Aula.HoraFim),
                Turma = p.Aula?.Turma?.Nome,
                Curso = p.Aula?.Turma?.Curso?.Nome,
                Local = p.Aula?.Turma?.Local,
                Responsavel = p.Aula?.Turma?.Responsavel,
                Status = p.Status.ToString(),
                p.Observacao,
                RegistradoEm = p.RegistradaEm
            }).ToList()
        });
    }

    private static string FormatTime(TimeOnly time)
    {
        return time.ToString("HH:mm", CultureInfo.InvariantCulture);
    }
}
