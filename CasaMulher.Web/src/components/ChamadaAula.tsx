import { useCallback, useEffect, useMemo, useState } from "react";
import {
  ArrowLeft,
  CalendarDays,
  CheckCircle2,
  ClipboardCheck,
  Loader2,
  RefreshCw,
  UserCheck,
  UserX,
} from "lucide-react";
import { apiGet, apiPost } from "../api/http";

type ChamadaAulaProps = {
  aulaId: number;
  onVoltarGrade: () => void;
  onChamadaAtualizada?: () => void;
};

type AulaChamada = {
  id: number;
  data: string;
  horarioInicio: string;
  horarioFim: string;
  turmaId: number;
  turma: string;
  curso: string;
  local?: string | null;
  responsavel: string;
};

type ItemChamada = {
  id?: number;
  presencaId?: number;
  matriculaId: number;
  alunaId?: number;
  aluna: string;
  status: string;
  observacao?: string | null;
  registradoEm?: string | null;
};

type ListaChamadaResponse = {
  aula: AulaChamada;
  quantidade: number;
  lista: ItemChamada[];
};

type GerarListaResponse = {
  mensagem: string;
  presencasCriadas: number;
};

type RegistrarPresencaResponse = {
  mensagem: string;
};

function formatDate(value?: string) {
  if (!value) {
    return "-";
  }

  return new Intl.DateTimeFormat("pt-BR", {
    timeZone: "UTC",
    weekday: "long",
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(new Date(value));
}

function formatTime(value?: string) {
  if (!value) {
    return "-";
  }

  return value.substring(0, 5);
}

function getStatusClass(status: string) {
  const normalizado = status
    .toLowerCase()
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "");

  if (normalizado === "presente") {
    return "presente";
  }

  if (normalizado === "faltou") {
    return "faltou";
  }

  if (normalizado === "faltajustificada") {
    return "justificada";
  }

  return "pendente";
}

export function ChamadaAula({
  aulaId,
  onVoltarGrade,
  onChamadaAtualizada,
}: ChamadaAulaProps) {
  const [dados, setDados] = useState<ListaChamadaResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [gerando, setGerando] = useState(false);
  const [salvandoMatriculaId, setSalvandoMatriculaId] = useState<number | null>(
    null,
  );
  const [erro, setErro] = useState<string | null>(null);
  const [mensagem, setMensagem] = useState<string | null>(null);
  const [observacoes, setObservacoes] = useState<Record<number, string>>({});

  const carregarChamada = useCallback(async () => {
    try {
      setLoading(true);
      setErro(null);

      const response = await apiGet<ListaChamadaResponse>(
        `/Chamada/aulas/${aulaId}`,
      );

      setDados(response);

      const observacoesIniciais: Record<number, string> = {};

      response.lista.forEach((item) => {
        observacoesIniciais[item.matriculaId] = item.observacao ?? "";
      });

      setObservacoes(observacoesIniciais);
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : "Nao foi possivel carregar a lista de chamada.",
      );
    } finally {
      setLoading(false);
    }
  }, [aulaId]);

  async function gerarLista() {
    try {
      setGerando(true);
      setErro(null);
      setMensagem(null);

      const response = await apiPost<GerarListaResponse>(
        `/Chamada/aulas/${aulaId}/gerar-lista`,
      );

      setMensagem(
        `${response.mensagem} Presencas criadas: ${response.presencasCriadas}.`,
      );

      await carregarChamada();
      onChamadaAtualizada?.();
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : "Nao foi possivel gerar a lista de chamada.",
      );
    } finally {
      setGerando(false);
    }
  }

  async function registrarPresenca(matriculaId: number, status: string) {
    try {
      setSalvandoMatriculaId(matriculaId);
      setErro(null);
      setMensagem(null);

      const response = await apiPost<
        RegistrarPresencaResponse,
        {
          matriculaId: number;
          status: string;
          observacao?: string;
        }
      >(`/Chamada/aulas/${aulaId}/registrar`, {
        matriculaId,
        status,
        observacao: observacoes[matriculaId] ?? "",
      });

      setMensagem(response.mensagem);

      await carregarChamada();
      onChamadaAtualizada?.();
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : "Nao foi possivel registrar a presenca.",
      );
    } finally {
      setSalvandoMatriculaId(null);
    }
  }

  useEffect(() => {
    void carregarChamada();
  }, [carregarChamada]);

  const resumo = useMemo(() => {
    const lista = dados?.lista ?? [];

    return {
      total: lista.length,
      presentes: lista.filter((item) => item.status === "Presente").length,
      faltas: lista.filter((item) => item.status === "Faltou").length,
      justificadas: lista.filter((item) => item.status === "FaltaJustificada")
        .length,
      pendentes: lista.filter((item) => item.status === "Pendente").length,
    };
  }, [dados]);

  return (
    <section className="chamada-page">
      <div className="section-title">
        <div>
          <span className="eyebrow">Lista de chamada</span>
          <h2>Registrar frequencia da aula</h2>
          <p>Gere a chamada e registre a situacao de cada participante.</p>
        </div>

        <div className="actions-row">
          <button
            className="secondary-button"
            type="button"
            onClick={onVoltarGrade}
          >
            <ArrowLeft size={18} />
            Voltar para grade
          </button>

          <button
            className="secondary-button"
            type="button"
            onClick={() => void carregarChamada()}
            disabled={loading}
          >
            <RefreshCw size={18} />
            Atualizar
          </button>

          <button
            className="primary-button"
            type="button"
            onClick={gerarLista}
            disabled={gerando}
          >
            {gerando ? (
              <>
                <Loader2 className="spin" size={18} />
                Gerando
              </>
            ) : (
              <>
                <ClipboardCheck size={18} />
                Gerar chamada
              </>
            )}
          </button>
        </div>
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

      {loading && (
        <section className="state-card">
          <Loader2 className="spin" />
          <p>Carregando lista de chamada...</p>
        </section>
      )}

      {!loading && dados && (
        <>
          <article className="panel chamada-hero">
            <div className="chamada-hero-main">
              <div className="hero-icon">
                <CalendarDays size={28} />
              </div>

              <div>
                <span className="eyebrow">Aula #{dados.aula.id}</span>
                <h2>{dados.aula.curso}</h2>
                <p>
                  {dados.aula.turma} | {dados.aula.local ?? "-"} |{" "}
                  {dados.aula.responsavel}
                </p>
                <strong>
                  {formatDate(dados.aula.data)} |{" "}
                  {formatTime(dados.aula.horarioInicio)} as{" "}
                  {formatTime(dados.aula.horarioFim)}
                </strong>
              </div>
            </div>
          </article>

          <section className="chamada-stats-grid">
            <article className="chamada-stat">
              <strong>{resumo.total}</strong>
              <span>Total</span>
            </article>
            <article className="chamada-stat">
              <strong>{resumo.presentes}</strong>
              <span>Presentes</span>
            </article>
            <article className="chamada-stat">
              <strong>{resumo.faltas}</strong>
              <span>Faltas</span>
            </article>
            <article className="chamada-stat">
              <strong>{resumo.justificadas}</strong>
              <span>Justificadas</span>
            </article>
            <article className="chamada-stat">
              <strong>{resumo.pendentes}</strong>
              <span>Pendentes</span>
            </article>
          </section>

          <article className="panel">
            <div className="panel-header">
              <div>
                <span className="eyebrow">Participantes</span>
                <h2>Lista da aula</h2>
              </div>

              <span className="badge">{dados.quantidade} registros</span>
            </div>

            {dados.lista.length === 0 ? (
              <div className="empty-state small">
                <ClipboardCheck size={34} />
                <h3>Chamada ainda não gerada</h3>
                <p>
                  Clique em <strong>Gerar chamada</strong> para criar
                  automaticamente a lista de presença com as alunas
                  matriculadas nesta turma.
                </p>
              </div>
            ) : (
              <div className="chamada-list">
                {dados.lista.map((item) => {
                  const statusClass = getStatusClass(item.status);
                  const salvando = salvandoMatriculaId === item.matriculaId;

                  return (
                    <article className="chamada-item" key={item.matriculaId}>
                      <div className="chamada-item-main">
                        <div>
                          <strong>{item.aluna}</strong>
                          <span>Matricula #{item.matriculaId}</span>
                        </div>

                        <span className={`status-pill status-pill-${statusClass}`}>
                          {item.status}
                        </span>
                      </div>

                      <textarea
                        placeholder="Observacao opcional..."
                        value={observacoes[item.matriculaId] ?? ""}
                        onChange={(event) =>
                          setObservacoes((atual) => ({
                            ...atual,
                            [item.matriculaId]: event.target.value,
                          }))
                        }
                      />

                      <div className="chamada-actions">
                        <button
                          type="button"
                          className="presence-button present"
                          disabled={salvando}
                          onClick={() =>
                            void registrarPresenca(item.matriculaId, "Presente")
                          }
                        >
                          {salvando ? (
                            <Loader2 className="spin" size={16} />
                          ) : (
                            <UserCheck size={16} />
                          )}
                          Presente
                        </button>

                        <button
                          type="button"
                          className="presence-button absent"
                          disabled={salvando}
                          onClick={() =>
                            void registrarPresenca(item.matriculaId, "Faltou")
                          }
                        >
                          {salvando ? (
                            <Loader2 className="spin" size={16} />
                          ) : (
                            <UserX size={16} />
                          )}
                          Faltou
                        </button>

                        <button
                          type="button"
                          className="presence-button justified"
                          disabled={salvando}
                          onClick={() =>
                            void registrarPresenca(
                              item.matriculaId,
                              "FaltaJustificada",
                            )
                          }
                        >
                          {salvando ? (
                            <Loader2 className="spin" size={16} />
                          ) : (
                            <CheckCircle2 size={16} />
                          )}
                          Justificada
                        </button>
                      </div>
                    </article>
                  );
                })}
              </div>
            )}
          </article>
        </>
      )}
    </section>
  );
}
