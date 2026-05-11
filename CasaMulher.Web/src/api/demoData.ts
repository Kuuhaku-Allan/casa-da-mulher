import type {
  AgendaTurmaResponse,
  Aluna,
  Curso,
  RelatorioAulasTurmaResponse,
  RelatorioTurmaResponse,
  Turma,
} from "../types";

export const demoCursos: Curso[] = [
  {
    id: 1,
    nome: "Informática Básica",
    descricao: "Curso introdutório de informática para mulheres da comunidade.",
    ativo: true,
  },
];

export const demoAlunas: Aluna[] = [
  {
    id: 1,
    nomeCompleto: "Maria Oliveira",
    telefone: "(11) 99999-9999",
    email: "maria@email.com",
    dataCadastro: "2026-05-11T00:00:00",
  },
  {
    id: 2,
    nomeCompleto: "Ana Souza",
    telefone: "(11) 98888-8888",
    email: "ana@email.com",
    dataCadastro: "2026-05-11T00:00:00",
  },
  {
    id: 3,
    nomeCompleto: "Joana Santos",
    telefone: "(11) 97777-7777",
    email: "joana@email.com",
    dataCadastro: "2026-05-11T00:00:00",
  },
];

export const demoTurmas: Turma[] = [
  {
    id: 1,
    cursoId: 1,
    cursoNome: "Informática Básica",
    nome: "Turma A - Tarde",
    local: "Sala 2",
    responsavel: "Professora Ana",
    dataInicio: "2026-06-01T00:00:00",
    dataFim: "2026-07-31T00:00:00",
    horarioInicio: "14:00:00",
    horarioFim: "16:00:00",
    diasDaSemana: "Segunda,Quarta",
    vagas: 20,
    ativa: true,
  },
];

const demoAulas = Array.from({ length: 18 }, (_, index) => ({
  id: index + 1,
  data: new Date(Date.UTC(2026, 5, 1 + index * 3)).toISOString(),
  horarioInicio: "14:00:00",
  horarioFim: "16:00:00",
  status: "Agendada",
}));

export const demoAgendaTurma: AgendaTurmaResponse = {
  turma: {
    id: 1,
    nome: "Turma A - Tarde",
    curso: "Informática Básica",
    local: "Sala 2",
    responsavel: "Professora Ana",
    dataInicio: "2026-06-01T00:00:00",
    dataFim: "2026-07-31T00:00:00",
    horarioInicio: "14:00:00",
    horarioFim: "16:00:00",
    diasDaSemana: "Segunda,Quarta",
  },
  quantidadeAulas: 18,
  aulas: demoAulas,
};

export const demoRelatorioTurma: RelatorioTurmaResponse = {
  turma: {
    id: 1,
    nome: "Turma A - Tarde",
    curso: "Informática Básica",
    local: "Sala 2",
    responsavel: "Professora Ana",
  },
  resumo: {
    totalAulas: 18,
    totalAlunas: 3,
    totalPossivelDeRegistros: 54,
    presentes: 38,
    faltas: 4,
    faltasJustificadas: 2,
    pendentes: 10,
    percentualPresenca: 77.78,
  },
  alunas: [
    {
      matriculaId: 1,
      alunaId: 1,
      aluna: "Maria Oliveira",
      totalAulas: 18,
      presentes: 15,
      faltas: 2,
      faltasJustificadas: 1,
      pendentes: 0,
      percentualPresenca: 83.33,
      situacao: "Regular",
    },
    {
      matriculaId: 2,
      alunaId: 2,
      aluna: "Ana Souza",
      totalAulas: 18,
      presentes: 14,
      faltas: 2,
      faltasJustificadas: 1,
      pendentes: 1,
      percentualPresenca: 77.78,
      situacao: "Regular",
    },
    {
      matriculaId: 3,
      alunaId: 3,
      aluna: "Joana Santos",
      totalAulas: 18,
      presentes: 9,
      faltas: 0,
      faltasJustificadas: 0,
      pendentes: 9,
      percentualPresenca: 50.0,
      situacao: "Atenção",
    },
  ],
};

export const demoRelatorioAulas: RelatorioAulasTurmaResponse = {
  turmaId: 1,
  quantidadeAulas: 18,
  aulas: demoAulas.map((aula, index) => ({
    id: aula.id,
    data: aula.data,
    horarioInicio: aula.horarioInicio,
    horarioFim: aula.horarioFim,
    totalRegistros: index < 9 ? 3 : 0,
    presentes: index < 9 ? 3 : 0,
    faltas: 0,
    faltasJustificadas: 0,
    pendentes: index < 9 ? 0 : 3,
  })),
};

export const demoChamada = {
  aula: {
    id: 1,
    data: "2026-06-01T00:00:00",
    horarioInicio: "14:00:00",
    horarioFim: "16:00:00",
    turmaId: 1,
    turma: "Turma A - Tarde",
    curso: "Informática Básica",
    local: "Sala 2",
    responsavel: "Professora Ana",
  },
  quantidade: 3,
  lista: [
    {
      id: 1,
      presencaId: 1,
      matriculaId: 1,
      alunaId: 1,
      aluna: "Maria Oliveira",
      status: "Presente",
      observacao: "Compareceu no horário.",
      registradoEm: "2026-06-01T14:00:00",
    },
    {
      id: 2,
      presencaId: 2,
      matriculaId: 2,
      alunaId: 2,
      aluna: "Ana Souza",
      status: "Presente",
      observacao: null,
      registradoEm: "2026-06-01T14:05:00",
    },
    {
      id: 3,
      presencaId: 3,
      matriculaId: 3,
      alunaId: 3,
      aluna: "Joana Santos",
      status: "Faltou",
      observacao: null,
      registradoEm: "2026-06-01T14:10:00",
    },
  ],
};
