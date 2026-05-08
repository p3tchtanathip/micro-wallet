# MicroWallet

A digital wallet API built with .NET 9 following Clean Architecture principles.

## Architecture

- **DDD** - Domain-Driven Design with Entities and Value Objects
- **CQRS** - Read/Write separation via MediatR
- **Repository Pattern** - Data access abstraction with EF Core
- **Concurrency Control** - Database locking to prevent double spending

## Tech Stack

- **Frontend**: Next.js 16 (React 19) with Tailwind CSS 4
- **Framework**: ASP.NET Core 9 (Web API)
- **Database**: PostgreSQL with Entity Framework Core
- **Validation**: FluentValidation & Data Annotations
- **Auth**: JWT + Refresh Tokens, Google OAuth2, RBAC
- **Security**: Argon2 password hashing, HTTPS/HSTS
- **Infrastructure**: Docker, Kubernetes (k3s), Terraform (GCP)
- **Monitoring**: Prometheus + Grafana

## Quick Start

### Backend

```bash
docker-compose up -d
```

API: https://localhost:5001/swagger

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Web: https://localhost:3000

## Features

- Deposit, Withdraw, Transfer between wallets
- Atomic transactions (all-or-nothing)
- Immutable transaction ledger
- Idempotent API (prevents duplicates)
- Kubernetes-ready for auto-scaling

## Monitoring

- Prometheus: http://localhost:9090
- Grafana: http://localhost:3001

## Security

- Input validation via FluentValidation
- Data encryption at rest
- VPC and firewall rules via Terraform