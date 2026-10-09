# Scheduler

## Getting Started

### Database

This demo uses a local SQL Server (LocalDB) instance. The connection string is:

```json
"ConnectionStrings": {
  "Scheduler": "Server=(localdb)\\MSSQLLocalDB;Database=Scheduler;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### Migrations

> **Note:** When running in **Development** mode, the application automatically applies any pending migrations on startup.

**Add a migration**

```bash
dotnet-ef migrations add <MigrationName> --project Scheduler.Infrastructure\Scheduler.Infrastructure.csproj --startup-project Scheduler.Api\Scheduler.Api.csproj
```

**Apply migrations manually**

```bash
dotnet-ef database update --project Scheduler.Infrastructure\Scheduler.Infrastructure.csproj --startup-project Scheduler.Api\Scheduler.Api.csproj
```

---

## Development Notes

A commit-by-commit record of my thought process while building this project.

### 1. Initial setup

Initial setup and design often take me the longest. My thought process usually starts with one approach, and then I pivot as needed. I generally follow a template when setting up new projects and like to pull in features or lessons learnt from previous projects. I also try to implement things I wasn't able to in earlier projects due to budget or deadline constraints.

For this project I used a SQL database since I already had one set up, and got at least the **Event list** API, services and domain set up.

### 2. Create event

Added the **create event** functionality so I could get data into the database, test the list endpoint, and confirm that the **Domain Events** were working.

### 3. Global exception handling

Added `ExceptionToProblemDetailsHandler`, a global exception handler that converts unhandled exceptions into `ProblemDetails` responses. I had seen this pattern in another project, and Claude also suggested it while I was prepping for this test.

### 4. Unit Testing
Added the xUnit Test Project, I unfortunately ran out of time and wasn't able to fully go through the testing process. 