import { useEffect, useMemo, useState, type ReactNode } from "react";
import {
  BarChart3,
  CalendarDays,
  ClipboardCheck,
  GraduationCap,
  HeartHandshake,
  LayoutDashboard,
  Loader2,
  Users,
} from "lucide-react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { apiGet } from "./api/http";
import type {
  AgendaTurmaResponse,
  Aluna,
  Curso,
  RelatorioTurmaResponse,
  Turma,
} from "./types";
import { ChamadaAula } from "./components/ChamadaAula";
import { GradeTurmas } from "./components/GradeTurmas";
import { RelatoriosFrequencia } from "./components/RelatoriosFrequencia";
import "./styles.css";

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

function normalizeStatusClass(value: string) {
  return value
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .toLowerCase();
}

function StatCard({
  title,
  value,
  detail,
  icon,
}: {
  title: string;
  value: string | number;
  detail: string;
  icon: ReactNode;
}) {
  return (
    <article className="stat-card">
      <div className="stat-icon">{icon}</div>
      <div>
        <span>{title}</span>
        <strong>{value}</strong>
        <small>{detail}</small>
      </div>
    </article>
  );
}

function App() {
  const [cursos, setCursos] = useState<Curso[]>([]);
  const [alunas, setAlunas] = useState<Aluna[]>([]);
  const [turmas, setTurmas] = useState<Turma[]>([]);
  const [turmaSelecionadaId, setTurmaSelecionadaId] = useState<number | null>(
    null,
  );
  const [agenda, setAgenda] = useState<AgendaTurmaResponse | null>(null);
  const [relatorio, setRelatorio] = useState<RelatorioTurmaResponse | null>(
    null,
  );
  const [loading, setLoading] = useState(true);
  const [erro, setErro] = useState<string | null>(null);
  const [paginaAtual, setPaginaAtual] = useState<
    "dashboard" | "grade" | "chamada" | "relatorios"
  >("dashboard");
  const [aulaSelecionadaId, setAulaSelecionadaId] = useState<number | null>(
    null,
  );

  async function carregarDadosIniciais() {
    try {
      setLoading(true);
      setErro(null);

      const [cursosData, alunasData, turmasData] = await Promise.all([
        apiGet<Curso[]>("/Curso"),
        apiGet<Aluna[]>("/Aluna"),
        apiGet<Turma[]>("/Turma"),
      ]);

      setCursos(cursosData);
      setAlunas(alunasData);
      setTurmas(turmasData);

      if (turmasData.length > 0) {
        setTurmaSelecionadaId(turmasData[0].id);
      }
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : "Nao foi possivel carregar os dados.",
      );
    } finally {
      setLoading(false);
    }
  }

  async function carregarDadosDaTurma(turmaId: number) {
    try {
      setErro(null);

      const [agendaData, relatorioData] = await Promise.all([
        apiGet<AgendaTurmaResponse>(`/Agenda/turmas/${turmaId}`),
        apiGet<RelatorioTurmaResponse>(
          `/Relatorio/frequencia/turmas/${turmaId}`,
        ),
      ]);

      setAgenda(agendaData);
      setRelatorio(relatorioData);
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : "Nao foi possivel carregar os dados da turma.",
      );
      setAgenda(null);
      setRelatorio(null);
    }
  }

  useEffect(() => {
    void carregarDadosIniciais();
  }, []);

  useEffect(() => {
    if (turmaSelecionadaId) {
      void carregarDadosDaTurma(turmaSelecionadaId);
    }
  }, [turmaSelecionadaId]);

  function abrirChamada(aulaId: number) {
    setAulaSelecionadaId(aulaId);
    setPaginaAtual("chamada");
  }

  const proximasAulas = useMemo(() => {
    return agenda?.aulas.slice(0, 5) ?? [];
  }, [agenda]);

  const graficoFrequencia = useMemo(() => {
    if (!relatorio) {
      return [];
    }

    return [
      { name: "Presencas", value: relatorio.resumo.presentes, fill: "#2f7d72" },
      { name: "Faltas", value: relatorio.resumo.faltas, fill: "#b84a62" },
      {
        name: "Justificadas",
        value: relatorio.resumo.faltasJustificadas,
        fill: "#b77834",
      },
      { name: "Pendentes", value: relatorio.resumo.pendentes, fill: "#7c668c" },
    ];
  }, [relatorio]);

  const percentual = relatorio?.resumo.percentualPresenca ?? 0;
  const turmaAtual = relatorio?.turma ?? agenda?.turma;
  const pageTitle =
    paginaAtual === "dashboard"
      ? "Dashboard"
      : paginaAtual === "grade"
        ? "Grade"
        : paginaAtual === "chamada"
          ? "Chamada"
          : "Relatórios";
  const pageDescription =
    paginaAtual === "dashboard"
      ? "Turmas, agenda e frequência"
      : paginaAtual === "grade"
        ? "Turmas, detalhes e grade de aulas"
        : paginaAtual === "chamada"
          ? "Lista de chamada e registro de presença"
          : "Gráficos e indicadores de frequência";

  return (
    <main className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-icon">
            <HeartHandshake size={26} />
          </div>
          <div>
            <strong>Casa da Mulher</strong>
            <span>Gestao de Turmas</span>
          </div>
        </div>

        <nav className="nav-menu" aria-label="Navegacao principal">
          <button
            className={paginaAtual === "dashboard" ? "active" : ""}
            type="button"
            onClick={() => setPaginaAtual("dashboard")}
          >
            <LayoutDashboard size={18} />
            Dashboard
          </button>
          <button type="button">
            <GraduationCap size={18} />
            Cursos
          </button>
          <button
            className={paginaAtual === "grade" ? "active" : ""}
            type="button"
            onClick={() => setPaginaAtual("grade")}
          >
            <CalendarDays size={18} />
            Grade
          </button>
          <button
            className={paginaAtual === "chamada" ? "active" : ""}
            type="button"
            onClick={() => {
              if (aulaSelecionadaId) {
                setPaginaAtual("chamada");
                return;
              }

              setPaginaAtual("grade");
            }}
          >
            <ClipboardCheck size={18} />
            Chamada
          </button>
          <button
            className={paginaAtual === "relatorios" ? "active" : ""}
            type="button"
            onClick={() => setPaginaAtual("relatorios")}
          >
            <BarChart3 size={18} />
            Relatórios
          </button>
          <button type="button">
            <Users size={18} />
            Alunas
          </button>
        </nav>

        <div className="sidebar-footer">
          <span>MVP prototipo</span>
          <strong>Back-end conectado</strong>
        </div>
      </aside>

      <section className="content">
        <header className="topbar">
          <div>
            <span className="eyebrow">Painel administrativo</span>
            <h1>{pageTitle}</h1>
            <p>{pageDescription}</p>
          </div>

          <div className="select-group">
            <label htmlFor="turma">Turma selecionada</label>
            <select
              id="turma"
              value={turmaSelecionadaId ?? ""}
              disabled={turmas.length === 0}
              onChange={(event) =>
                setTurmaSelecionadaId(Number(event.target.value))
              }
            >
              {turmas.length === 0 && <option value="">Sem turmas</option>}
              {turmas.map((turma) => (
                <option key={turma.id} value={turma.id}>
                  {turma.nome}
                </option>
              ))}
            </select>
          </div>
        </header>

        {loading && (
          <section className="state-card">
            <Loader2 className="spin" />
            <p>Carregando dados...</p>
          </section>
        )}

        {erro && !loading && (
          <section className="error-card">
            <strong>Falha ao carregar o painel</strong>
            <p>{erro}</p>
            <span>API esperada em http://localhost:5005.</span>
          </section>
        )}

        {!loading && !erro && paginaAtual === "dashboard" && (
          <>
            <section className="stats-grid">
              <StatCard
                title="Cursos"
                value={cursos.length}
                detail="Atividades cadastradas"
                icon={<GraduationCap size={22} />}
              />
              <StatCard
                title="Turmas"
                value={turmas.length}
                detail="Turmas no sistema"
                icon={<CalendarDays size={22} />}
              />
              <StatCard
                title="Alunas"
                value={alunas.length}
                detail="Participantes cadastradas"
                icon={<Users size={22} />}
              />
              <StatCard
                title="Frequencia"
                value={`${percentual.toFixed(2)}%`}
                detail="Presenca geral da turma"
                icon={<ClipboardCheck size={22} />}
              />
            </section>

            <section className="quick-actions panel">
              <div>
                <span className="eyebrow">Ações rápidas</span>
                <h2>Continuar o acompanhamento</h2>
                <p>
                  Acesse rapidamente as principais funções do protótipo.
                </p>
              </div>

              <div className="quick-actions-buttons">
                <button
                  className="primary-button"
                  type="button"
                  onClick={() => setPaginaAtual("grade")}
                >
                  Ver grade
                </button>

                <button
                  className="secondary-button"
                  type="button"
                  onClick={() => setPaginaAtual("relatorios")}
                >
                  Abrir relatórios
                </button>

                <button
                  className="secondary-button"
                  type="button"
                  onClick={() => {
                    if (aulaSelecionadaId) {
                      setPaginaAtual("chamada");
                    } else {
                      setPaginaAtual("grade");
                    }
                  }}
                >
                  Ir para chamada
                </button>
              </div>
            </section>

            <section className="dashboard-grid">
              <article className="panel panel-wide">
                <div className="panel-header">
                  <div>
                    <span className="eyebrow">Resumo da turma</span>
                    <h2>{turmaAtual?.nome ?? "Turma"}</h2>
                  </div>
                  <span className="badge">{turmaAtual?.curso ?? "Curso"}</span>
                </div>

                <div className="summary-layout">
                  <div
                    className="progress-ring"
                    style={{
                      background: `conic-gradient(#2f7d72 ${percentual * 3.6}deg, #e7dee8 0deg)`,
                    }}
                  >
                    <div>
                      <strong>{percentual.toFixed(0)}%</strong>
                      <span>presenca</span>
                    </div>
                  </div>

                  <div className="summary-body">
                    <div className="mini-stats">
                      <div>
                        <strong>{relatorio?.resumo.totalAulas ?? 0}</strong>
                        <span>Aulas</span>
                      </div>
                      <div>
                        <strong>{relatorio?.resumo.presentes ?? 0}</strong>
                        <span>Presencas</span>
                      </div>
                      <div>
                        <strong>{relatorio?.resumo.faltas ?? 0}</strong>
                        <span>Faltas</span>
                      </div>
                      <div>
                        <strong>{relatorio?.resumo.pendentes ?? 0}</strong>
                        <span>Pendentes</span>
                      </div>
                    </div>

                    <div className="chart-box" aria-label="Distribuicao de frequencia">
                      <ResponsiveContainer width="100%" height={150}>
                        <BarChart data={graficoFrequencia}>
                          <CartesianGrid vertical={false} stroke="#eee7ef" />
                          <XAxis dataKey="name" tickLine={false} axisLine={false} />
                          <YAxis allowDecimals={false} tickLine={false} axisLine={false} />
                          <Tooltip cursor={{ fill: "#f6f0f6" }} />
                          <Bar dataKey="value" radius={[4, 4, 0, 0]} />
                        </BarChart>
                      </ResponsiveContainer>
                    </div>
                  </div>
                </div>
              </article>

              <article className="panel">
                <div className="panel-header">
                  <div>
                    <span className="eyebrow">Grade</span>
                    <h2>Proximas aulas</h2>
                  </div>
                  <span className="soft-badge">{agenda?.quantidadeAulas ?? 0} aulas</span>
                </div>

                <div className="list">
                  {proximasAulas.length === 0 && (
                    <p className="empty">Nenhuma aula gerada.</p>
                  )}

                  {proximasAulas.map((aula) => (
                    <div className="list-item" key={aula.id}>
                      <div>
                        <strong>{formatDate(aula.data)}</strong>
                        <span>
                          {formatTime(aula.horarioInicio)} as{" "}
                          {formatTime(aula.horarioFim)}
                        </span>
                      </div>
                      <span>Aula #{aula.id}</span>
                    </div>
                  ))}
                </div>
              </article>
            </section>

            <section className="dashboard-grid">
              <article className="panel">
                <div className="panel-header">
                  <div>
                    <span className="eyebrow">Alunas</span>
                    <h2>Situacao de frequencia</h2>
                  </div>
                </div>

                <div className="table-wrapper">
                  <table>
                    <thead>
                      <tr>
                        <th>Aluna</th>
                        <th>Presenca</th>
                        <th>Situacao</th>
                      </tr>
                    </thead>
                    <tbody>
                      {relatorio?.alunas.map((item) => (
                        <tr key={item.matriculaId}>
                          <td>{item.aluna ?? "-"}</td>
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
              </article>

              <article className="panel">
                <div className="panel-header">
                  <div>
                    <span className="eyebrow">Turma</span>
                    <h2>Informacoes</h2>
                  </div>
                </div>

                <dl className="info-list">
                  <div>
                    <dt>Local</dt>
                    <dd>{agenda?.turma.local ?? "-"}</dd>
                  </div>
                  <div>
                    <dt>Responsavel</dt>
                    <dd>{agenda?.turma.responsavel ?? "-"}</dd>
                  </div>
                  <div>
                    <dt>Periodo</dt>
                    <dd>
                      {formatDate(agenda?.turma.dataInicio)} ate{" "}
                      {formatDate(agenda?.turma.dataFim)}
                    </dd>
                  </div>
                  <div>
                    <dt>Dias</dt>
                    <dd>{agenda?.turma.diasDaSemana ?? "-"}</dd>
                  </div>
                </dl>
              </article>
            </section>
          </>
        )}

        {!loading && !erro && paginaAtual === "grade" && (
          <GradeTurmas
            turmas={turmas}
            turmaSelecionadaId={turmaSelecionadaId}
            onSelecionarTurma={setTurmaSelecionadaId}
            onAbrirChamada={abrirChamada}
          />
        )}

        {!loading && !erro && paginaAtual === "chamada" && aulaSelecionadaId && (
          <ChamadaAula
            aulaId={aulaSelecionadaId}
            onVoltarGrade={() => setPaginaAtual("grade")}
            onChamadaAtualizada={() => {
              if (turmaSelecionadaId) {
                void carregarDadosDaTurma(turmaSelecionadaId);
              }
            }}
          />
        )}

        {!loading && !erro && paginaAtual === "relatorios" && (
          <RelatoriosFrequencia
            turmas={turmas}
            turmaSelecionadaId={turmaSelecionadaId}
            onSelecionarTurma={setTurmaSelecionadaId}
          />
        )}
      </section>
    </main>
  );
}

export default App;
