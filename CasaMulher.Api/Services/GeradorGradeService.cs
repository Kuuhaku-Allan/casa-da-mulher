using System.Globalization;
using System.Text;
using CasaMulher.Api.Data;
using CasaMulher.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CasaMulher.Api.Services;

public class GeradorGradeService(AppDbContext context)
{
    public async Task<(bool Sucesso, string Mensagem, int AulasCriadas)> GerarAulasDaTurmaAsync(int turmaId)
    {
        var turma = await context.Turmas
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == turmaId);

        if (turma is null)
        {
            return (false, "Turma nao encontrada.", 0);
        }

        var diasDaSemana = ConverterDiasDaSemana(turma.DiasSemana);

        if (diasDaSemana.Count == 0)
        {
            return (false, "Nenhum dia da semana valido foi informado para a turma.", 0);
        }

        if (turma.DataFim < turma.DataInicio)
        {
            return (false, "A data final da turma nao pode ser anterior a data inicial.", 0);
        }

        var datasExistentes = (await context.Aulas
                .Where(aula => aula.TurmaId == turmaId)
                .Select(aula => aula.Data)
                .ToListAsync())
            .ToHashSet();

        var novasAulas = new List<Aula>();

        for (var data = turma.DataInicio; data <= turma.DataFim; data = data.AddDays(1))
        {
            if (!diasDaSemana.Contains(data.DayOfWeek))
            {
                continue;
            }

            if (datasExistentes.Contains(data))
            {
                continue;
            }

            novasAulas.Add(new Aula
            {
                TurmaId = turma.Id,
                Data = data,
                HoraInicio = turma.HoraInicio,
                HoraFim = turma.HoraFim,
                Conteudo = "Aula agendada"
            });
        }

        if (novasAulas.Count == 0)
        {
            return (true, "Nenhuma nova aula foi criada. A grade provavelmente ja tinha sido gerada.", 0);
        }

        context.Aulas.AddRange(novasAulas);
        await context.SaveChangesAsync();

        return (true, "Grade gerada com sucesso.", novasAulas.Count);
    }

    private static HashSet<DayOfWeek> ConverterDiasDaSemana(string diasDaSemana)
    {
        var resultado = new HashSet<DayOfWeek>();

        if (string.IsNullOrWhiteSpace(diasDaSemana))
        {
            return resultado;
        }

        var partes = diasDaSemana
            .Split(',', ';', '|')
            .Select(parte => NormalizarTexto(parte.Trim()))
            .Where(parte => !string.IsNullOrWhiteSpace(parte));

        foreach (var parte in partes)
        {
            switch (parte)
            {
                case "domingo":
                case "dom":
                    resultado.Add(DayOfWeek.Sunday);
                    break;
                case "segunda":
                case "segunda feira":
                case "seg":
                    resultado.Add(DayOfWeek.Monday);
                    break;
                case "terca":
                case "terca feira":
                case "ter":
                    resultado.Add(DayOfWeek.Tuesday);
                    break;
                case "quarta":
                case "quarta feira":
                case "qua":
                    resultado.Add(DayOfWeek.Wednesday);
                    break;
                case "quinta":
                case "quinta feira":
                case "qui":
                    resultado.Add(DayOfWeek.Thursday);
                    break;
                case "sexta":
                case "sexta feira":
                case "sex":
                    resultado.Add(DayOfWeek.Friday);
                    break;
                case "sabado":
                case "sab":
                    resultado.Add(DayOfWeek.Saturday);
                    break;
            }
        }

        return resultado;
    }

    private static string NormalizarTexto(string texto)
    {
        var textoNormalizado = texto
            .ToLowerInvariant()
            .Replace("-", " ")
            .Normalize(NormalizationForm.FormD);

        var caracteres = textoNormalizado
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray();

        return new string(caracteres).Normalize(NormalizationForm.FormC);
    }
}
