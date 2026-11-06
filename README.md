# Transactions Ingest

ASP.NET 10 + Angular application that imports, reconciles, and displays card transactions from multiple source systems.

## Estimated vs Actual Hours
- **Estimated**: 12 hours
- **Actual**: ~10 hours (generated with AI assistance; see AI_USAGE.md)

## Build Instructions

### Backend
```bash
cd src/TransactionsIngest.Api
dotnet restore
dotnet build
```

### Frontend
```bash
cd frontend
npm install
```

## Run Instructions

### Backend (SQLite file created automatically)
```bash
cd src/TransactionsIngest.Api
dotnet run
```
API available at `http://localhost:5000` (or the port shown).

### Frontend
```bash
cd frontend
npm start
```
Open `http://localhost:4200`.

Click **Run Import** to pull sample data from the three mock source systems. The table refreshes with the results.

## Design Decisions and Assumptions

- **Composite key**: `(SourceSystem, TransactionId)` as specified. Stored with a surrogate `Id` for easier EF relationships.
- **Status**: `Active` / `Revoked` / `Finalized`. Revocation applies only to transactions whose `TransactionTime` is within the last 24 hours and missing from the current snapshot. Transactions older than 24 h can be finalized (optional requirement implemented).
- **Audit history**: Field-level diffs for updates plus Created / Revoked / Finalized events. Stored in `AuditEntry`.
- **Idempotency**: The import service compares current state; identical snapshots produce no additional changes or audit entries.
- **Source systems**: Mocked via `ISourceSystemClient` implementations with hard-coded recent sample data so the 24-hour revoke rule can be demonstrated. In production these would call real APIs.
- **Database**: SQLite via EF Core Code-First (`EnsureCreated`). No migrations committed; database is recreated on first run.
- **Scheduling**: Not implemented. The same `TransactionImportService.ImportAsync` method can be called from a hosted service or Hangfire job later without changes.
- **Frontend**: Minimal standalone Angular 19 app with a plain HTML table and manual import button. CORS enabled for `localhost:4200`.
- **Error handling & logging**: Basic try/catch in the import endpoint + ILogger throughout.
- **Tests**: xUnit tests covering create and idempotency using EF InMemory.

## API

- `POST /api/import` – runs the import
- `GET /api/transactions` – returns all imported transactions
- `GET /api/transactions/{id}/audit` – returns audit history for a transaction

## Notes

- Sample data uses relative timestamps (`UtcNow.AddHours(...)`) so revoke/finalize behavior is observable.
- To simulate an update or revocation, modify the hard-coded lists in the source clients and re-run the import.
- The solution is designed so the import logic is independent of the trigger mechanism.

## AI Usage

See `AI_USAGE.md`.
