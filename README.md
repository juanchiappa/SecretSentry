@"
# SecretSentry

A lightweight, self-hostable CLI tool written in C# that scans Git repositories —
including full commit history, not just the current state — for leaked credentials
and secrets. Built as a .NET-native alternative to tools like TruffleHog or Gitleaks.

## Status

🚧 Early development — Phase 1 (current-state scanning) in progress.

## Why

Most secret-scanning tools live in the Go/Python ecosystem. Teams already standardized
on .NET often want a CLI they can extend or integrate natively into their own C# tooling
and CI pipelines, without adding another language to the stack.

## Features (planned)

- Scans the current working tree **and** full Git commit history (via LibGit2Sharp)
- Built-in detection rules for common providers (AWS, GitHub, GCP, Stripe, Slack, JWT,
  SSH/PGP private keys)
- Fully configurable detection rules via external YAML — no hardcoded patterns
- Baseline system to mark known false positives
- JSON output for integration with other tools
- Native GitHub Action to run as a CI step
- 100% local analysis — no secrets are ever sent to an external server

## Usage (planned CLI surface)

``````bash
secretsentry scan .                        # Scan current repo state
secretsentry scan . --history              # Also scan full commit history
secretsentry scan . --rules custom.yaml    # Use custom rules in addition to defaults
secretsentry baseline add <hash>           # Mark a finding as a known false positive
secretsentry scan . --format json          # JSON output
``````

## License

MIT
"@ | Out-File -Encoding utf8 README.md
