# 🏦 BankSystemPro

An ASP.NET Core Web API demonstrating a layered banking system: role-based access (Admin / Employee / Customer), account operations with transactional transfers, and a loan request/approval workflow — backed by SQL Server via EF Core.

Built as a portfolio project to show clean architecture, real business rules, and secure API design — not just CRUD.

---

## 🧱 Architecture

The solution follows a **Clean Architecture** style split into four projects, so business logic never depends on EF Core, ASP.NET Core, or any other framework detail:

```
BankSystemPro/
├── src/
│   ├── BankSystemPro.Domain          # Entities & enums. Zero dependencies.
│   ├── BankSystemPro.Application     # DTOs, interfaces, and business logic (services).
│   ├── BankSystemPro.Infrastructure  # EF Core DbContext, repositories, JWT, password hashing.
│   └── BankSystemPro.Api             # Controllers, middleware, DI wiring, Swagger.
├── database/
│   └── schema.sql                    # Manual schema + stored procedure + seed data.
├── BankSystemPro.sln
└── README.md
```

**Dependency direction:** `Api → Infrastructure → Application → Domain`. The `Application` layer defines interfaces (`IUnitOfWork`, `IAccountRepository`, etc.); `Infrastructure` implements them. This means the business rules in `AccountService` and `LoanService` could be unit-tested with an in-memory fake, with no database involved.

---

## ✅ What this demonstrates (the "why" behind each choice)

| Feature | Why it's here |
|---|---|
| **Role-based JWT auth** (Admin / Employee / Customer) | Real authorization logic, not just "logged in or not." |
| **Atomic transfers** (`AccountService.TransferAsync`) | Wraps a two-account balance change in a DB transaction — if either leg fails, both roll back. |
| **Optimistic concurrency** (`Account.RowVersion`) | Protects balances from being silently overwritten by two simultaneous requests. |
| **Loan state machine** (Pending → Approved/Rejected) | A loan can only be reviewed once — models a real workflow, not flat CRUD. |
| **Repository + Unit of Work pattern** | Decouples business logic from EF Core; makes the service layer testable. |
| **Global exception middleware** | Domain exceptions (`NotFoundException`, `BadRequestException`) map to clean HTTP status codes instead of leaking stack traces. |
| **Password hashing via ASP.NET Core Identity** | Uses a vetted PBKDF2 implementation instead of a hand-rolled hash. |
| **Stored procedure** (`GetBranchAccountSummary`) | Shows raw T-SQL alongside EF Core — a common interview follow-up ("what if you needed to bypass the ORM?"). |

---

## 🛠️ Tech Stack

- **.NET 8** / ASP.NET Core Web API
- **Entity Framework Core 8** (SQL Server provider)
- **JWT Bearer authentication**
- **Swagger / Swashbuckle** for API docs
- **SQL Server** (LocalDB, Express, or full instance)

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (Express, LocalDB, or a full instance)

### 1. Clone and restore

```bash
git clone <your-repo-url>
cd BankSystemPro
dotnet restore
```

### 2. Set up the database

**Option A — run the SQL script directly:**

```bash
sqlcmd -S localhost -i database/schema.sql
```

**Option B — use EF Core migrations instead** (recommended if you want to keep evolving the schema through code):

```bash
cd src/BankSystemPro.Api
dotnet tool install --global dotnet-ef   # first time only
dotnet ef migrations add InitialCreate --project ../BankSystemPro.Infrastructure --startup-project .
dotnet ef database update --project ../BankSystemPro.Infrastructure --startup-project .
```

> If you use Option B, you don't need `database/schema.sql` — EF Core will create the schema from the entity configuration in `BankSystemDbContext`.

### 3. Configure secrets

Update `src/BankSystemPro.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BankSystemProDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "REPLACE_WITH_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS"
  }
}
```

> ⚠️ Never commit a real JWT secret or production connection string. Use environment variables or `dotnet user-secrets` for anything beyond local development.

### 4. Run the API

```bash
cd src/BankSystemPro.Api
dotnet run
```

Swagger UI will be available at `https://localhost:<port>/swagger` — use it to register a user, log in, and copy the returned token into the "Authorize" button to call protected endpoints.

---

## 📡 API Overview

| Endpoint | Method | Access | Purpose |
|---|---|---|---|
| `/api/auth/register` | POST | Public | Create a user (Admin/Employee/Customer) |
| `/api/auth/login` | POST | Public | Get a JWT |
| `/api/accounts` | POST | Admin, Employee | Open an account for a customer |
| `/api/accounts/customer/{id}` | GET | Authenticated | List a customer's accounts |
| `/api/accounts/deposit` | POST | Admin, Employee | Deposit into an account |
| `/api/accounts/withdraw` | POST | Admin, Employee | Withdraw from an account |
| `/api/accounts/transfer` | POST | Authenticated | Transfer between two accounts (atomic) |
| `/api/loans` | POST | Customer | Request a loan |
| `/api/loans/pending` | GET | Admin, Employee | List pending loans |
| `/api/loans/{id}/review` | POST | Admin, Employee | Approve or reject a loan |

---

## 🧭 Roadmap / Possible Extensions

- [ ] Unit tests for `AccountService` and `LoanService` (the layering makes this straightforward — mock `IUnitOfWork`)
- [ ] FluentValidation for request DTOs instead of manual checks in services
- [ ] Refresh token rotation
- [ ] Pagination on list endpoints
- [ ] Dockerfile + docker-compose (API + SQL Server) for one-command local setup
- [ ] Audit log for admin actions
