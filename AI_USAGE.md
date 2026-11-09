# AI Usage Disclosure

**AI tools were used** to generate the majority of this solution.

## Tools
- Grok (xAI)

## Prompts / Instructions Given
The full coding exercise PDF was provided with the request to implement the basics of the Transactions Ingest application from scratch (ASP.NET 10, EF Core + SQLite, Angular frontend, reconciliation logic, audit history, idempotency, etc.).

Key instructions included:
- Follow all functional, API, frontend, and non-functional requirements
- Mock source systems with interfaces and hard-coded sample data
- Implement the 24-hour revocation rule and optional finalization
- Ensure idempotent imports
- Include tests, logging, and a complete README
- Produce a ready-to-commit folder structure

## Conventions / Rules Applied
- Clean architecture: services, interfaces for sources, DbContext
- Field-level audit entries
- Composite unique index on (SourceSystem, TransactionId)
- Status enum (Active / Revoked / Finalized)
- Standalone Angular components
- Relative timestamps in mocks so the 24-hour window is demonstrable

## Modifications Made to AI-Generated Output
- Manual review and adjustment of the revocation window logic to match the acceptance criteria exactly
- Added optional finalization of transactions older than 24 hours
- Simplified Angular to a single standalone component for the exercise scope
- Ensured CORS, Swagger, and EnsureCreated database setup
- Added basic xUnit tests for create and idempotency

All generated code was reviewed for correctness against the exercise requirements before inclusion.
