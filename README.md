# Sistema de Gestão Multi-Farmácias

Sistema monolito modular para gestão de múltiplas farmácias (multi-tenant): CRM, Atendimento via WhatsApp, Painel do Proprietário, Dashboard de gestão, Estoque e Financeiro.

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | .NET (ASP.NET Core + EF Core), arquitetura em camadas (Domain/Application/Infrastructure/Api) |
| Web | React |
| Mobile | React Native |
| Automação (CRM + Atendimento WhatsApp) | n8n |
| Banco de dados | PostgreSQL (isolamento por `farmaciaId`) |
| Fila | RabbitMQ |
| Provedor de WhatsApp | Evolution API (self-hosted) |
| Infraestrutura | Docker + Docker Compose |
| CI | GitHub Actions (build do backend a cada push/PR) |
| Gestão de sprints | Linear + GitHub |

## Estrutura do repositório

```
/
├── backend/
│   ├── SistemaFarmacias.slnx
│   ├── src/
│   │   ├── SistemaFarmacias.Api/              # Controllers, Program.cs, autenticação por API Key
│   │   ├── SistemaFarmacias.Application/      # DTOs e interfaces (contratos)
│   │   ├── SistemaFarmacias.Domain/           # Entidades (Farmacia, Contato, Interacao, ReativacaoEnviada...)
│   │   └── SistemaFarmacias.Infrastructure/   # AppDbContext, repositórios, migrations
│   └── tests/
│       └── SistemaFarmacias.Tests/
├── frontend/            # Painel web em React (ainda não iniciado)
├── mobile/               # App React Native (ainda não iniciado)
├── n8n/
│   └── workflows/        # Workflows do n8n exportados (.json), versionados como backup/histórico
├── scripts/
│   └── init-multiple-postgres-databases.sh
├── .github/
│   └── workflows/
│       └── build.yml     # CI: builda backend/SistemaFarmacias.slnx a cada push/PR
├── docker-compose.yml
├── .env.example
└── .gitignore
```

## Pré-requisitos

- Docker + Docker Compose instalados
- .NET SDK 9.0
- Node.js (para frontend/mobile, quando iniciados)
- Conta no Linear com acesso ao workspace do projeto
- [GitHub CLI (`gh`)](https://cli.github.com/) autenticado (`gh auth login`) — usado pela CLI própria pra abrir PRs
- CLI própria (`sprint`) configurada — ver seção abaixo

## Setup do ambiente local

1. Clone o repositório:
   ```bash
   git clone https://github.com/Felipeysz/sistema-farmacias.git
   cd sistema-farmacias
   ```

2. Copie o template de variáveis de ambiente e preencha com valores reais:
   ```bash
   cp .env.example .env
   ```
   Gere chaves aleatórias para os placeholders `gere-uma-chave-aleatoria-aqui` com:
   ```bash
   openssl rand -hex 32
   ```
   **O `.env` nunca deve ser commitado** — já está no `.gitignore`.

3. **(Linux/Mac apenas)** dê permissão de execução ao script de inicialização do Postgres:
   ```bash
   chmod +x scripts/init-multiple-postgres-databases.sh
   ```
   No Windows isso não é necessário — o container já executa o script via bash interno independente da permissão do arquivo no host.

4. Suba os serviços:
   ```bash
   docker compose up -d
   ```

5. Verifique se todos os serviços estão saudáveis:
   ```bash
   docker compose ps
   ```

6. No primeiro acesso ao n8n (veja tabela abaixo), ele vai pedir pra você **criar uma conta de owner** (email + senha) — isso é local, fica só no banco do seu container, não precisa ser um e-mail real nem passa por verificação.

### Serviços disponíveis após o `up`

| Serviço | URL local |
|---|---|
| n8n | http://localhost:5678 |
| Evolution API | http://localhost:8080 |
| RabbitMQ (painel) | http://localhost:15672 |
| Postgres | localhost:5432 |

> Backend e Frontend ainda estão comentados no `docker-compose.yml` — serão ativados assim que tiverem `Dockerfile` próprio.

### Backend (fora do Docker, por enquanto)

Enquanto o backend não tem `Dockerfile` próprio, rode ele direto na máquina, com os serviços de infra já de pé via Docker:

```bash
cd backend
dotnet build
dotnet run --project src/SistemaFarmacias.Api
```

## Fluxo de trabalho (sprints)

O planejamento roda no **Linear**, projeto **"Sprint 1 — Fundação CRM/Atendimento"**. Cada issue já vem com o nome de branch padronizado gerado pelo próprio Linear.

Fluxo do dia a dia, usando a CLI própria:

```bash
sprint list              # lista suas issues do sprint atual
sprint start LASDWAS-XX  # cria/checkout a branch, faz push inicial, atualiza status no Linear
sprint status            # mostra a branch atual e a issue vinculada
sprint finish [msg]      # commita, dá push e garante que o status no Linear reflete o PR
sprint pr                # cria o PR (se não existir) ou sincroniza status: Done / In Progress / In Review
```

A integração nativa Linear ↔ GitHub cuida da transição de status automaticamente conforme a branch recebe push e o PR é aberto/mergeado.

## Regras da branch `main`

- Pull request obrigatório antes de mergear
- Status check **"build"** (GitHub Actions) precisa passar antes do merge
- Aprovação de review **não é exigida** no momento (projeto com desenvolvedor único) — reative "Require approvals" em Settings → Branches assim que houver mais colaboradores
- Histórico linear (squash merge apenas)
- Sem force push, sem deleção da branch

## Documentação de arquitetura

A documentação completa de arquitetura (multi-tenant, resiliência do pipeline de vendas, contratos de API do módulo CRM, estratégia de testes) está centralizada nos documentos do projeto — anexar/linkar aqui conforme forem movidos para dentro do repositório (ex.: pasta `/docs`).
