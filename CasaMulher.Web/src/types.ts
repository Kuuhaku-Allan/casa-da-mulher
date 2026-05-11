export type Curso = {
  id: number;
  nome: string;
  descricao?: string | null;
  cargaHoraria?: number;
  ativo: boolean;
  criadoEm?: string;
};

export type Aluna = {
  id: number;
  nomeCompleto: string;
  telefone?: string | null;
  email?: string | null;
  dataCadastro?: string;
};

export type Turma = {
  id: number;
  cursoId: number;
  cursoNome?: string | null;
  nome: string;
  local?: string | null;
  responsavel: string;
  dataInicio: string;
  dataFim: string;
  horarioInicio: string;
  horarioFim: string;
  diasDaSemana: string;
  vagas: number;
  ativa: boolean;
};

export type AulaResumo = {
  id: number;
  data: string;
  horarioInicio: string;
  horarioFim: string;
  status?: string;
};

export type AgendaTurmaResponse = {
  turma: {
    id: number;
    nome: string;
    curso?: string | null;
    local?: string | null;
    responsavel: string;
    dataInicio: string;
    dataFim: string;
    horarioInicio: string;
    horarioFim: string;
    diasDaSemana: string;
  };
  quantidadeAulas: number;
  aulas: AulaResumo[];
};

export type RelatorioTurmaResponse = {
  turma: {
    id: number;
    nome: string;
    curso?: string | null;
    local?: string | null;
    responsavel: string;
    dataInicio?: string;
    dataFim?: string;
    horarioInicio?: string;
    horarioFim?: string;
    diasDaSemana?: string;
  };
  resumo: {
    totalAulas: number;
    totalAlunas: number;
    totalPossivelDeRegistros: number;
    presentes: number;
    faltas: number;
    faltasJustificadas: number;
    pendentes: number;
    percentualPresenca: number;
  };
  alunas: Array<{
    matriculaId: number;
    alunaId: number;
    aluna?: string | null;
    totalAulas: number;
    presentes: number;
    faltas: number;
    faltasJustificadas: number;
    pendentes: number;
    percentualPresenca: number;
    situacao: string;
  }>;
};

export type RelatorioAulasTurmaResponse = {
  turmaId: number;
  quantidadeAulas: number;
  aulas: Array<{
    id: number;
    data: string;
    horarioInicio: string;
    horarioFim: string;
    totalRegistros: number;
    presentes: number;
    faltas: number;
    faltasJustificadas: number;
    pendentes: number;
  }>;
};
