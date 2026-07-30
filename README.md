# SecretSentry

Generated with archgen using the **N-Tier** architecture pattern.

## Layers

- `SecretSentry.Entities` — plain domain entities, no dependencies.
- `SecretSentry.DataAccess` — persistence layer (Json).
- `SecretSentry.BusinessLogic` — application/business rules, depends on DataAccess.
- `SecretSentry.UI` — Console entry point, depends on BusinessLogic.

## Getting started

```bash
dotnet restore
dotnet build
dotnet run --project src/SecretSentry.UI
```