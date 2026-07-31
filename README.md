# SecretSentry

A lightweight, self-hostable CLI tool written in C# that scans Git repositories —
including full commit history, not just the current state — for leaked credentials
and secrets. Built as a .NET-native alternative to tools like TruffleHog or Gitleaks.

## Status

Early development — Phase 1 (current-state scanning) in progress.

## Why

Most secret-scanning tools live in the Go/Python ecosystem. Teams already standardized
on .NET often want a CLI they can extend or integrate natively into their own C# tooling
and CI pipelines, without adding another language to the stack.

## Features (planned)

- Scans the current working tree and full Git commit history (via LibGit2Sharp)
- Built-in detection rules for common providers (AWS, GitHub, GCP, Stripe, Slack, JWT, SSH/PGP private keys)
- Fully configurable detection rules via external YAML — no hardcoded patterns
- Baseline system to mark known false positives
- JSON output for integration with other tools
- Native GitHub Action to run as a CI step
- 100% local analysis — no secrets are ever sent to an external server

## Architecture

SecretSentry follows a classic N-Tier layering:

- `src/SecretSentry.Entities/` — Domain models (SecretRule, Finding, etc.)
- `src/SecretSentry.DataAccess/` — Git history access (LibGit2Sharp), rule loading (YAML)
- `src/SecretSentry.BusinessLogic/` — Pattern matching, scanning logic
- `src/SecretSentry.UI/` — CLI entry point, reporting output
- `action/` — GitHub Action wrapper
- `examples/` — Example custom rule files

## Usage (planned CLI surface)

    secretsentry scan .                        # Scan current repo state
    secretsentry scan . --history              # Also scan full commit history
    secretsentry scan . --rules custom.yaml    # Use custom rules in addition to defaults
    secretsentry baseline add <hash>           # Mark a finding as a known false positive
    secretsentry scan . --format json          # JSON output

## Custom rules

Detection patterns live in YAML, never hardcoded. Example:

    rules:
      - id: aws-access-key
        description: "AWS Access Key ID"
        pattern: 'AKIA[0-9A-Z]{16}'
        severity: critical

Drop your own rules file and pass it with `--rules custom.yaml` — no code changes required.

## CI integration

SecretSentry ships a GitHub Action that fails the build if a secret is found on push or PR.
See `action/action.yml` (coming in Phase 4).

## Roadmap

- [ ] Phase 1 — Current-state scanning (CLI base, default rules, console report)
- [ ] Phase 2 — Full commit history scanning via LibGit2Sharp
- [ ] Phase 3 — Configurable rules (YAML) + baseline system
- [ ] Phase 4 — GitHub Action + NuGet global tool publication

## Contributing

Issues and PRs are welcome. If you want to add a detection pattern for a new provider,
make sure it's specific enough to avoid false positives — document why the pattern
matches that provider's real key/token format in your PR description.

## License

MIT — see [LICENSE](LICENSE).