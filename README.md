# Sistema de Gestão Multi-Farmácias

Sistema monolito modular para gestão de múltiplas farmácias (multi-tenant): CRM, Atendimento via WhatsApp, Painel do Proprietário, Dashboard de gestão, Estoque e Financeiro.

## Stack

| Camada | Tecnologia |
|---|---|
| Backend | .NET (ASP.NET Core + EF Core) |
| Web | React |
| Mobile | React Native |
| Automação (CRM + Atendimento WhatsApp) | n8n |
| Banco de dados | PostgreSQL (multi-tenant via `tenant_id`) |
| Fila | RabbitMQ |
| Provedor de WhatsApp | Evolution API (self-hosted) |
| Infraestrutura | Docker + Docker Compose |
| Gestão de sprints | Linear + GitHub |

## Estrutura do repositório

```
/
├── backend/            # API .NET (ainda não iniciado)
├── frontend/           # Painel web em React (ainda não iniciado)
├── mobile/             # App React Native (ainda não iniciado)
├── scripts/
│   └── init-multiple-postgres-databases.sh
├── docker-compose.yml
├── .env.example
└── .gitignore
```

## Pré-requisitos

- Docker + Docker Compose instalados
- .NET SDK (versão a definir quando o backend for iniciado)
- Node.js (para frontend/mobile)
- Conta no Linear com acesso ao workspace do projeto
- CLI própria (`sprint`) configurada — ver seção abaixo

## Setup do ambiente local

1. Clone o repositório:
   ```bash
   git clone <url-do-repositorio>
   cd <nome-do-repositorio>
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

3. Dê permissão de execução ao script de inicialização do Postgres:
   ```bash
   chmod +x scripts/init-multiple-postgres-databases.sh
   ```

4. Suba os serviços:
   ```bash
   docker compose up -d
   ```

5. Verifique se todos os serviços estão saudáveis:
   ```bash
   docker compose ps
   ```

### Serviços disponíveis após o `up`

| Serviço | URL local |
|---|---|
| n8n | http://localhost:5678 |
| Evolution API | http://localhost:8080 |
| RabbitMQ (painel) | http://localhost:15672 |
| Postgres | localhost:5432 |

> Backend e Frontend ainda estão comentados no `docker-compose.yml` — serão ativados assim que tiverem `Dockerfile` próprio.

## Fluxo de trabalho (sprints)

O planejamento roda no **Linear**, projeto **"Sprint 1 — Fundação CRM/Atendimento"**. Cada issue já vem com o nome de branch padronizado gerado pelo próprio Linear.

Fluxo do dia a dia, usando a CLI própria:

```bash
sprint list              # lista suas issues do sprint atual
sprint start LASDWAS-XX  # cria/checkout a branch, abre o editor, atualiza status no Linear
sprint status             # mostra a branch atual e a issue vinculada
sprint pr                 # abre o PR da branch atual já linkando a issue
```

A integração nativa Linear ↔ GitHub cuida da transição de status automaticamente conforme a branch recebe push, o PR é aberto e depois mergeado.

## Regras da branch `main`

- Pull request obrigatório, com pelo menos 1 aprovação
- Status checks (CI) precisam passar antes do merge — inclui os testes de idempotência do pipeline de vendas, que são gate obrigatório
- Histórico linear (squash merge apenas)
- Sem force push, sem deleção da branch

## Documentação de arquitetura

A documentação completa de arquitetura (multi-tenant, resiliência do pipeline de vendas, contratos de API do módulo CRM, estratégia de testes) está centralizada nos documentos do projeto — anexar/linkar aqui conforme forem movidos para dentro do repositório (ex.: pasta `/docs`).
