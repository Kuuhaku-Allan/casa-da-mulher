import {
  demoAgendaTurma,
  demoAlunas,
  demoChamada,
  demoCursos,
  demoRelatorioAulas,
  demoRelatorioTurma,
  demoTurmas,
} from "./demoData";

const API_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5005/api";
const DEMO_MODE = import.meta.env.VITE_DEMO_MODE === "true";

function getDemoResponse<T>(path: string): T {
  if (path === "/Curso") return demoCursos as T;
  if (path === "/Aluna") return demoAlunas as T;
  if (path === "/Turma") return demoTurmas as T;

  if (path.startsWith("/Agenda/turmas/")) return demoAgendaTurma as T;

  if (
    path.startsWith("/Relatorio/frequencia/turmas/") &&
    path.endsWith("/aulas")
  ) {
    return demoRelatorioAulas as T;
  }

  if (path.startsWith("/Relatorio/frequencia/turmas/")) {
    return demoRelatorioTurma as T;
  }

  if (path.startsWith("/Relatorio/frequencia/alunas/")) {
    return {
      aluna: demoAlunas[0],
      resumoGeral: {
        totalAulas: 18,
        presentes: 15,
        faltas: 2,
        faltasJustificadas: 1,
        pendentes: 0,
        percentualPresenca: 83.33,
        situacao: "Regular",
      },
      turmas: [],
    } as T;
  }

  if (path.startsWith("/Chamada/aulas/")) return demoChamada as T;

  throw new Error(`Rota demo não configurada: ${path}`);
}

export async function apiGet<T>(path: string): Promise<T> {
  if (DEMO_MODE) {
    return getDemoResponse<T>(path);
  }

  const response = await fetch(`${API_URL}${path}`);

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || "Erro ao consultar a API.");
  }

  return response.json();
}

export async function apiPost<TResponse, TBody = unknown>(
  path: string,
  body?: TBody,
): Promise<TResponse> {
  if (DEMO_MODE) {
    if (path.includes("/gerar-aulas")) {
      return {
        mensagem: "Modo demonstração: a grade já está gerada.",
        aulasCriadas: 0,
      } as TResponse;
    }

    if (path.includes("/gerar-lista")) {
      return {
        mensagem: "Modo demonstração: a chamada já está gerada.",
        presencasCriadas: 0,
      } as TResponse;
    }

    if (path.includes("/registrar")) {
      return {
        mensagem: "Modo demonstração: presença simulada com sucesso.",
      } as TResponse;
    }

    return {
      mensagem: "Ação simulada no modo demonstração.",
    } as TResponse;
  }

  const response = await fetch(`${API_URL}${path}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: body ? JSON.stringify(body) : undefined,
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || "Erro ao enviar dados para a API.");
  }

  return response.json();
}
