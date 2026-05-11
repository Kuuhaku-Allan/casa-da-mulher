import { useCallback, useEffect, useMemo, useState } from "react";
import {
  BarChart3,
  ClipboardList,
  Loader2,
  PieChart as PieChartIcon,
  RefreshCw,
  Users,
} from "lucide-react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Legend,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { apiGet } from "../api/http";
import type {
  RelatorioAulasTurmaResponse,
  RelatorioTurmaResponse,
  Turma,
} from "../types";

type RelatoriosFrequenciaProps = {
  turmas: Turma[];
  turmaSelecionadaId: number | null;
  onSelecionarTurma: (turmaId: number) => void;
};

const COLORS = {
  presentes: "#287447",
  faltas: "#a72e46",
  justificadas: "#8a5d00",
  pendentes: "#5e3370",
  primary: "#50305e",
};

function formatDate(value?: string) {
  if (!value) {
    return "-";
  }

  return new Intl.DateTimeFormat("pt-BR", {
    timeZone: "UTC",
    day: "2-digit",
    month: "2-digit",
  }).format(new Date(value));
}

function normalizeStatusClass(value: string) {
  return value
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLowerCase();
}

export function RelatoriosFrequencia({
  turmas,
  turmaSelecionadaId,
  onSelecionarTurma,
}: RelatoriosFrequenciaProps) {
  const [relatorio, setRelatorio] = useState<RelatorioTurmaResponse | null>(
    null,
  );
  const [relatorioAulas, setRelatorioAulas] =
    useState<RelatorioAulasTurmaResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

  const turmaIdAtual = turmaSelecionadaId ?? turmas[0]?.id ?? null;

  const carregarRelatorios = useCallback(async (turmaId: number) => {
    try {
      setLoading(true);
      setErro(null);

      const [relatorioData, aulasData] = await Promise.all([
        apiGet<RelatorioTurmaResponse>(
          `/Relatorio/frequencia/turmas/${turmaId}`,
        ),
        apiGet<RelatorioAulasTurmaResponse>(
          `/Relatorio/frequencia/turmas/${turmaId}/aulas`,
        ),
      ]);

      setRelatorio(relatorioData);
      setRelatorioAulas(aulasData);
    } catch (error) {
      setRelatorio(null);
      setRelatorioAulas(null);
      setErro(
        error instanceof Error
          ? error.message
          : "Nao foi possivel carregar os relatorios.",
      );
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (!turmaSelecionadaId && turmas.length > 0) {
      onSelecionarTurma(turmas[0].id);
    }
  }, [turmaSelecionadaId, turmas, onSelecionarTurma]);

  useEffect(() => {
    if (turmaIdAtual) {
      void carregarRelatorios(turmaIdAtual);
    }
  }, [carregarRelatorios, turmaIdAtual]);

  const dadosPizza = useMemo(() => {
    if (!relatorio) {
      return [];
    }

    return [
      {
        name: "Presentes",
        value: relatorio.resumo.presentes,
        color: COLORS.presentes,
      },
      {
        name: "Faltas",
        value: relatorio.resumo.faltas,
        color: COLORS.faltas,
      },
      {
        name: "Justificadas",
        value: relatorio.resumo.faltasJustificadas,
        color: COLORS.justificadas,
      },
      {
        name: "Pendentes",
        value: relatorio.resumo.pendentes,
        color: COLORS.pendentes,
      },
    ];
  }, [relatorio]);

  const dadosAlunas = useMemo(() => {
    return (
      relatorio?.alunas.map((item) => ({
        nome: item.aluna ?? "Sem nome",
        presenca: item.percentualPresenca,
        presentes: item.presentes,
        faltas: item.faltas,
        pendentes: item.pendentes,
        situacao: item.situacao,
      })) ?? []
    );
  }, [relatorio]);

  const dadosAulas = useMemo(() => {
    return (
      relatorioAulas?.aulas.map((aula) => ({
        aula: `#${aula.id}`,
        data: formatDate(aula.data),
        presentes: aula.presentes,
        faltas: aula.faltas,
        justificadas: aula.faltasJustificadas,
        pendentes: aula.pendentes,
      })) ?? []
    );
  }, [relatorioAulas]);

  if (turmas.length === 0) {
    return (
      <section className="panel">
        <div className="empty-state">
          <BarChart3 size={40} />
          <h2>Nenhuma turma cadastrada</h2>
          <p>Cadastre turmas, gere aulas e registre chamadas para visualizar os relatorios.</p>
        </div>
      </section>
    );
  }

  return (
    <section className="relatorios-page">
      <div className="section-title">
        <div>
          <span className="eyebrow">Relatorios</span>
          <h2>Frequencia e acompanhamento</h2>
          <p>Veja a frequencia geral, a situacao das alunas e o resumo por aula.</p>
        </div>

        <div className="actions-row">
          <div className="compact-select">
            <label htmlFor="turma-relatorio">Turma</label>
            <select
              id="turma-relatorio"
              value={turmaIdAtual ?? ""}
              onChange={(event) => onSelecionarTurma(Number(event.target.value))}
            >
              {turmas.map((turma) => (
                <option key={turma.id} value={turma.id}>
                  {turma.nome}
                </option>
              ))}
            </select>
          </div>

          <button
            className="secondary-button"
            type="button"
            disabled={loading || !turmaIdAtual}
            onClick={() => turmaIdAtual && void carregarRelatorios(turmaIdAtual)}
          >
            {loading ? (
              <Loader2 className="spin" size={18} />
            ) : (
              <RefreshCw size={18} />
            )}
            Atualizar
          </button>
        </div>
      </div>

      {erro && (
        <div className="inline-alert inline-alert-error">
          <strong>Erro</strong>
          <span>{erro}</span>
        </div>
      )}

      {loading && (
        <section className="state-card">
          <Loader2 className="spin" />
          <p>Carregando relatorios...</p>
        </section>
      )}

      {!loading && relatorio && (
        <>
          <section className="stats-grid">
            <article className="stat-card">
              <div className="stat-icon">
                <ClipboardList size={22} />
              </div>
              <div>
                <span>Total de aulas</span>
                <strong>{relatorio.resumo.totalAulas}</strong>
                <small>Aulas geradas na turma</small>
              </div>
            </article>

            <article className="stat-card">
              <div className="stat-icon">
                <Users size={22} />
              </div>
              <div>
                <span>Total de alunas</span>
                <strong>{relatorio.resumo.totalAlunas}</strong>
                <small>Participantes matriculadas</small>
              </div>
            </article>

            <article className="stat-card">
              <div className="stat-icon">
                <PieChartIcon size={22} />
              </div>
              <div>
                <span>Frequencia</span>
                <strong>{relatorio.resumo.percentualPresenca.toFixed(2)}%</strong>
                <small>Presenca geral da turma</small>
              </div>
            </article>

            <article className="stat-card">
              <div className="stat-icon">
                <BarChart3 size={22} />
              </div>
              <div>
                <span>Pendentes</span>
                <strong>{relatorio.resumo.pendentes}</strong>
                <small>Registros ainda sem chamada</small>
              </div>
            </article>
          </section>

          <section className="dashboard-grid">
            <article className="panel">
              <div className="panel-header">
                <div>
                  <span className="eyebrow">Distribuicao</span>
                  <h2>Presencas, faltas e pendencias</h2>
                </div>
              </div>

              <div className="report-chart-box">
                <ResponsiveContainer width="100%" height={280}>
                  <PieChart>
                    <Pie
                      data={dadosPizza}
                      dataKey="value"
                      nameKey="name"
                      innerRadius={64}
                      outerRadius={96}
                      paddingAngle={2}
                    >
                      {dadosPizza.map((entry) => (
                        <Cell key={entry.name} fill={entry.color} />
                      ))}
                    </Pie>
                    <Tooltip />
                    <Legend />
                  </PieChart>
                </ResponsiveContainer>
              </div>
            </article>

            <article className="panel">
              <div className="panel-header">
                <div>
                  <span className="eyebrow">Resumo</span>
                  <h2>{relatorio.turma.nome}</h2>
                </div>
                <span className="badge">{relatorio.turma.curso ?? "Curso"}</span>
              </div>

              <dl className="info-list">
                <div>
                  <dt>Presentes</dt>
                  <dd>{relatorio.resumo.presentes}</dd>
                </div>
                <div>
                  <dt>Faltas</dt>
                  <dd>{relatorio.resumo.faltas}</dd>
                </div>
                <div>
                  <dt>Justificadas</dt>
                  <dd>{relatorio.resumo.faltasJustificadas}</dd>
                </div>
                <div>
                  <dt>Registros</dt>
                  <dd>{relatorio.resumo.totalPossivelDeRegistros}</dd>
                </div>
              </dl>
            </article>
          </section>

          <section className="panel relatorio-panel">
            <div className="panel-header">
              <div>
                <span className="eyebrow">Por aluna</span>
                <h2>Percentual de frequencia individual</h2>
              </div>
            </div>

            <div className="report-chart-box large">
              <ResponsiveContainer width="100%" height={340}>
                <BarChart data={dadosAlunas}>
                  <CartesianGrid vertical={false} stroke="#eee7ef" />
                  <XAxis dataKey="nome" tickLine={false} axisLine={false} />
                  <YAxis domain={[0, 100]} tickLine={false} axisLine={false} />
                  <Tooltip />
                  <Bar
                    dataKey="presenca"
                    name="Presenca (%)"
                    fill={COLORS.primary}
                    radius={[4, 4, 0, 0]}
                  />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </section>

          <section className="panel relatorio-panel">
            <div className="panel-header">
              <div>
                <span className="eyebrow">Por aula</span>
                <h2>Resumo das chamadas</h2>
              </div>
              <span className="badge">
                {relatorioAulas?.quantidadeAulas ?? 0} aulas
              </span>
            </div>

            <div className="report-chart-box large">
              <ResponsiveContainer width="100%" height={360}>
                <BarChart data={dadosAulas}>
                  <CartesianGrid vertical={false} stroke="#eee7ef" />
                  <XAxis dataKey="aula" tickLine={false} axisLine={false} />
                  <YAxis allowDecimals={false} tickLine={false} axisLine={false} />
                  <Tooltip />
                  <Legend />
                  <Bar
                    dataKey="presentes"
                    name="Presentes"
                    stackId="a"
                    fill={COLORS.presentes}
                  />
                  <Bar
                    dataKey="faltas"
                    name="Faltas"
                    stackId="a"
                    fill={COLORS.faltas}
                  />
                  <Bar
                    dataKey="justificadas"
                    name="Justificadas"
                    stackId="a"
                    fill={COLORS.justificadas}
                  />
                  <Bar
                    dataKey="pendentes"
                    name="Pendentes"
                    stackId="a"
                    fill={COLORS.pendentes}
                  />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </section>

          <section className="panel relatorio-panel">
            <div className="panel-header">
              <div>
                <span className="eyebrow">Tabela</span>
                <h2>Situacao das alunas</h2>
              </div>
            </div>

            <div className="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Aluna</th>
                    <th>Presentes</th>
                    <th>Faltas</th>
                    <th>Justificadas</th>
                    <th>Pendentes</th>
                    <th>Frequencia</th>
                    <th>Situacao</th>
                  </tr>
                </thead>
                <tbody>
                  {relatorio.alunas.map((item) => (
                    <tr key={item.matriculaId}>
                      <td>{item.aluna ?? "-"}</td>
                      <td>{item.presentes}</td>
                      <td>{item.faltas}</td>
                      <td>{item.faltasJustificadas}</td>
                      <td>{item.pendentes}</td>
                      <td>{item.percentualPresenca.toFixed(2)}%</td>
                      <td>
                        <span
                          className={`status status-${normalizeStatusClass(
                            item.situacao,
                          )}`}
                        >
                          {item.situacao}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>
        </>
      )}
    </section>
  );
}
