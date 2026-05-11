# Casa da Mulher — MVP de Gestão de Turmas e Frequência

Protótipo desenvolvido para automatizar a organização de turmas, geração de grade de aulas, lista de chamada digital e relatórios de frequência.

## Módulos implementados

- Cadastro de cursos
- Cadastro de alunas
- Cadastro de turmas
- Matrícula de alunas
- Geração automática da grade
- Lista de chamada digital
- Relatórios de frequência
- Dashboard visual
- Interface web responsiva

## Tecnologias

### Back-end

- C#
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- Swagger/OpenAPI

### Front-end

- React
- TypeScript
- Vite
- Recharts
- Lucide React

## Rodar API

```powershell
cd CasaMulher.Api
dotnet run --urls http://localhost:5005
```

Swagger:

```text
http://localhost:5005/swagger/index.html
```

## Rodar Front-end

```powershell
cd CasaMulher.Web
npm install
npm run dev
```

Front-end:

```text
http://localhost:5173
```

## Versão publicada

A versão publicada no GitHub Pages roda em modo demonstração, com dados fictícios, para facilitar a visualização sem depender da API local.

```text
https://kuuhaku-allan.github.io/casa-da-mulher/
```

## Status do MVP

Fluxo funcional:

```text
Dashboard → Grade → Chamada → Relatórios
```
