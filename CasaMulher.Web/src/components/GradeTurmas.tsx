import { useCallback, useEffect, useMemo, useState } from "react";
import {
  CalendarCheck,
  CalendarDays,
  CalendarPlus,
  ClipboardCheck,
  Loader2,
  MapPin,
  RefreshCw,
  UserRound,
} from "lucide-react";
import { apiGet, apiPost } from "../api/http";
import type { AgendaTurmaResponse, Turma } from "../types";

type GradeTurmasProps = {
  turmas: Turma[];
  turmaSelecionadaId: number | null;
  onSelecionarTurma: (turmaId: number) => void;
  onAbrirChamada: (aulaId: number) => void;
};

type GerarAulasResponse = {
  mensagem: string;
  aulasCriadas: number;
};

function formatDate(value?: string) {
  if (!value) {
    return "-";
  }

  return new Intl.DateTimeFormat("pt-BR", {
    timeZone: "UTC",
  }).format(new Date(value));
}

function formatTime(value?: string) {
  if (!value) {
    return "-";
  }

  return value.substring(0, 5);
}

export function GradeTurmas({
  turmas,
  turmaSelecionadaId,
  onSelecionarTurma,
  onAbrirChamada,
}: GradeTurmasProps) {
  const [agenda, setAgenda] = useState<AgendaTurmaResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [gerando, setGerando] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const [mensagem, setMensagem] = useState<string | null>(null);

  const turmaIdAtual = useMemo(() => {
    return turmaSelecionadaId ?? turmas[0]?.id ?? null;
  }, [turmaSelecionadaId, turmas]);

  const carregarAgenda = useCallback(async (turmaId: number) => {
    try {
      setLoading(true);
      setErro(null);

      const agendaData = await apiGet<AgendaTurmaResponse>(
        `/Agenda/turmas/${turmaId}`,
      );

      setAgenda(agendaData);
    } catch (error) {
      setAgenda(null);
      setErro(
        error instanceof Error
          ? error.message
          : "Nao foi possivel carregar a grade da turma.",
      );
    } finally {
      setLoading(false);
    }
  }, []);

  async function gerarGrade() {
    if (!turmaIdAtual) {
      return;
    }

    try {
      setGerando(true);
      setErro(null);
      setMensagem(null);

      const resposta = await apiPost<GerarAulasResponse>(
        `/Agenda/turmas/${turmaIdAtual}/gerar-aulas`,
      );

      setMensagem(
        `${resposta.mensagem} Aulas criadas: ${resposta.aulasCriadas}.`,
      );

      await carregarAgenda(turmaIdAtual);
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : "Nao foi possivel gerar a grade da turma.",
      );
    } finally {
      setGerando(false);
    }
  }

  useEffect(() => {
    if (!turmaSelecionadaId && turmas.length > 0) {
      onSelecionarTurma(turmas[0].id);
    }
  }, [turmaSelecionadaId, turmas, onSelecionarTurma]);

  useEffect(() => {
    if (turmaIdAtual) {
      void carregarAgenda(turmaIdAtual);
    }
  }, [carregarAgenda, turmaIdAtual]);

  if (turmas.length === 0) {
    return (
      <section className="panel">
        <div className="empty-state">
          <CalendarDays size={40} />
          <h2>Nenhuma turma cadastrada</h2>
          <p>Cadastre uma turma para gerar a grade de aulas automaticamente.</p>
        </div>
      </section>
    );
  }

  return (
    <section className="grade-page">
      <div className="section-title">
        <div>
          <span className="eyebrow">Gestao de grade</span>
          <h2>Turmas e aulas</h2>
          <p>Selecione uma turma, gere a grade e acompanhe as aulas.</p>
        </div>

        <button
          className="primary-button"
          type="button"
          onClick={gerarGrade}
          disabled={gerando || !turmaIdAtual}
        >
          {gerando ? (
            <>
              <Loader2 className="spin" size={18} />
              Gerando
            </>
          ) : (
            <>
              <CalendarPlus size={18} />
              Gerar grade
            </>
          )}
        </button>
      </div>

      {erro && (
        <div className="inline-alert inline-alert-error">
          <strong>Erro</strong>
          <span>{erro}</span>
        </div>
      )}

      {mensagem && (
        <div className="inline-alert inline-alert-success">
          <strong>Sucesso</strong>
          <span>{mensagem}</span>
        </div>
      )}

      <div className="grade-layout">
        <aside className="turmas-list-panel panel">
          <div className="panel-header">
            <div>
              <span className="eyebrow">Turmas</span>
              <h2>Selecionar turma</h2>
            </div>
          </div>

          <div className="turmas-list">
            {turmas.map((turma) => {
              const ativa = turma.id === turmaIdAtual;

              return (
                <button
                  key={turma.id}
                  type="button"
                  className={`turma-card ${ativa ? "active" : ""}`}
                  onClick={() => {
                    setMensagem(null);
                    onSelecionarTurma(turma.id);
                  }}
                >
                  <strong>{turma.nome}</strong>
                  <span>{turma.cursoNome ?? "Curso nao informado"}</span>
                  <small>
                    {formatDate(turma.dataInicio)} ate{" "}
                    {formatDate(turma.dataFim)}
                  </small>
                </button>
              );
            })}
          </div>
        </aside>

        <div className="grade-main">
          <article className="panel">
            <div className="panel-header">
              <div>
                <span className="eyebrow">Detalhes</span>
                <h2>{agenda?.turma.nome ?? "Turma selecionada"}</h2>
              </div>

              <button
                className="secondary-button"
                type="button"
                onClick={() => turmaIdAtual && void carregarAgenda(turmaIdAtual)}
                disabled={loading}
              >
                <RefreshCw size={17} />
                Atualizar
              </button>
            </div>

            {loading && (
              <div className="loading-line">
                <Loader2 className="spin" size={18} />
                <span>Carregando grade...</span>
              </div>
            )}

            {!loading && agenda && (
              <div className="grade-info-grid">
                <div>
                  <MapPin size={18} />
                  <span>Local</span>
                  <strong>{agenda.turma.local ?? "-"}</strong>
                </div>

                <div>
                  <UserRound size={18} />
                  <span>Responsavel</span>
                  <strong>{agenda.turma.responsavel}</strong>
                </div>

                <div>
                  <CalendarDays size={18} />
                  <span>Periodo</span>
                  <strong>
                    {formatDate(agenda.turma.dataInicio)} ate{" "}
                    {formatDate(agenda.turma.dataFim)}
                  </strong>
                </div>

                <div>
                  <CalendarCheck size={18} />
                  <span>Total de aulas</span>
                  <strong>{agenda.quantidadeAulas}</strong>
                </div>
              </div>
            )}
          </article>

          <article className="panel">
            <div className="panel-header">
              <div>
                <span className="eyebrow">Grade completa</span>
                <h2>Aulas da turma</h2>
              </div>

              <span className="badge">{agenda?.quantidadeAulas ?? 0} aulas</span>
            </div>

            {!agenda || agenda.aulas.length === 0 ? (
              <div className="empty-state small">
                <CalendarDays size={34} />
                <h3>Nenhuma aula gerada</h3>
                <p>Clique em Gerar grade para criar as aulas da turma.</p>
              </div>
            ) : (
              <div className="table-wrapper">
                <table>
                  <thead>
                    <tr>
                      <th>Aula</th>
                      <th>Data</th>
                      <th>Horario</th>
                      <th>Status</th>
                      <th>Acao</th>
                    </tr>
                  </thead>
                  <tbody>
                    {agenda.aulas.map((aula) => (
                      <tr key={aula.id}>
                        <td>#{aula.id}</td>
                        <td>{formatDate(aula.data)}</td>
                        <td>
                          {formatTime(aula.horarioInicio)} as{" "}
                          {formatTime(aula.horarioFim)}
                        </td>
                        <td>
                          <span className="soft-badge">
                            {aula.status ?? "Agendada"}
                          </span>
                        </td>
                        <td>
                          <button
                            className="table-action"
                            type="button"
                            onClick={() => onAbrirChamada(aula.id)}
                          >
                            <ClipboardCheck size={15} />
                            Abrir chamada
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </article>
        </div>
      </div>
    </section>
  );
}
