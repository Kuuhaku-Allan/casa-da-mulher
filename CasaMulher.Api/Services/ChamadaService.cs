using CasaMulher.Api.Data;
using CasaMulher.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CasaMulher.Api.Services;

public class ChamadaService(AppDbContext context)
{
    private const string StatusMatriculaAtiva = "Ativa";

    public async Task<(bool Sucesso, string Mensagem, int Criadas)> GerarListaDaAulaAsync(int aulaId)
    {
        var aula = await context.Aulas
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == aulaId);

        if (aula is null)
        {
            return (false, "Aula nao encontrada.", 0);
        }

        var matriculasAtivas = await context.Matriculas
            .AsNoTracking()
            .Where(m => m.TurmaId == aula.TurmaId && m.Status == StatusMatriculaAtiva)
            .Select(m => m.Id)
            .ToListAsync();

        if (matriculasAtivas.Count == 0)
        {
            return (false, "Nao existem alunas ativas matriculadas nesta turma.", 0);
        }

        var presencasExistentes = await context.Presencas
            .AsNoTracking()
            .Where(p => p.AulaId == aulaId)
            .Select(p => p.MatriculaId)
            .ToListAsync();

        var presencasExistentesSet = presencasExistentes.ToHashSet();

        var novasPresencas = matriculasAtivas
            .Where(matriculaId => !presencasExistentesSet.Contains(matriculaId))
            .Select(matriculaId => new Presenca
            {
                AulaId = aulaId,
                MatriculaId = matriculaId,
                Status = PresencaStatus.Pendente,
                Observacao = null,
                RegistradaEm = null
            })
            .ToList();

        if (novasPresencas.Count == 0)
        {
            return (true, "Lista de chamada ja estava gerada.", 0);
        }

        context.Presencas.AddRange(novasPresencas);
        await context.SaveChangesAsync();

        return (true, "Lista de chamada gerada com sucesso.", novasPresencas.Count);
    }

    public async Task<(bool Sucesso, string Mensagem)> RegistrarPresencaAsync(
        int aulaId,
        int alunaId,
        string status,
        string? observacao)
    {
        if (!TryParseStatus(status, out var statusNormalizado) ||
            statusNormalizado == PresencaStatus.Pendente)
        {
            return (false, "Status invalido. Use: Presente, Faltou ou FaltaJustificada.");
        }

        var aula = await context.Aulas
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == aulaId);

        if (aula is null)
        {
            return (false, "Aula nao encontrada.");
        }

        var matricula = await context.Matriculas
            .AsNoTracking()
            .FirstOrDefaultAsync(m =>
                m.AlunaId == alunaId &&
                m.TurmaId == aula.TurmaId &&
                m.Status == StatusMatriculaAtiva);

        if (matricula is null)
        {
            return (false, "A aluna nao possui matricula ativa na turma desta aula.");
        }

        var presenca = await context.Presencas
            .FirstOrDefaultAsync(p => p.AulaId == aulaId && p.MatriculaId == matricula.Id);

        if (presenca is null)
        {
            presenca = new Presenca
            {
                AulaId = aulaId,
                MatriculaId = matricula.Id
            };

            context.Presencas.Add(presenca);
        }

        presenca.Status = statusNormalizado;
        presenca.Observacao = observacao?.Trim();
        presenca.RegistradaEm = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return (true, "Presenca registrada com sucesso.");
    }

    public async Task<(bool Sucesso, string Mensagem)> RegistrarPresencaPorMatriculaAsync(
        int aulaId,
        int matriculaId,
        string status,
        string? observacao)
    {
        if (!TryParseStatus(status, out var statusNormalizado) ||
            statusNormalizado == PresencaStatus.Pendente)
        {
            return (false, "Status invalido. Use: Presente, Faltou ou FaltaJustificada.");
        }

        var aula = await context.Aulas
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == aulaId);

        if (aula is null)
        {
            return (false, "Aula nao encontrada.");
        }

        var matriculaAtiva = await context.Matriculas.AnyAsync(m =>
            m.Id == matriculaId &&
            m.TurmaId == aula.TurmaId &&
            m.Status == StatusMatriculaAtiva);

        if (!matriculaAtiva)
        {
            return (false, "A matricula informada nao esta ativa na turma desta aula.");
        }

        var presenca = await context.Presencas
            .FirstOrDefaultAsync(p => p.AulaId == aulaId && p.MatriculaId == matriculaId);

        if (presenca is null)
        {
            presenca = new Presenca
            {
                AulaId = aulaId,
                MatriculaId = matriculaId
            };

            context.Presencas.Add(presenca);
        }

        presenca.Status = statusNormalizado;
        presenca.Observacao = observacao?.Trim();
        presenca.RegistradaEm = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return (true, "Presenca registrada com sucesso.");
    }

    private static bool TryParseStatus(string status, out PresencaStatus statusNormalizado)
    {
        statusNormalizado = PresencaStatus.Pendente;

        if (string.IsNullOrWhiteSpace(status))
        {
            return false;
        }

        var valor = status.Trim().ToLowerInvariant().Replace(" ", string.Empty);

        switch (valor)
        {
            case "pendente":
                statusNormalizado = PresencaStatus.Pendente;
                return true;
            case "presente":
                statusNormalizado = PresencaStatus.Presente;
                return true;
            case "faltou":
            case "falta":
                statusNormalizado = PresencaStatus.Faltou;
                return true;
            case "faltajustificada":
            case "justificada":
                statusNormalizado = PresencaStatus.FaltaJustificada;
                return true;
            default:
                return Enum.TryParse(status.Trim(), ignoreCase: true, out statusNormalizado);
        }
    }
}
