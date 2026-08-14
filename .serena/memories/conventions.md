# Conventions

- Namespace/file/type names follow Architecture/Roadmap; public API has XML docs and code cites governing sections.
- Constructor injection/module interfaces are the test seam; prefer injected `IClock`.
- Environment-dependent paths must be real and fail closed: no fake success, hardcoded secrets, production URLs, or real-looking PHI.
- Runtime credentials/config are deployment-only: `TASKTREE_SMTP_*`, `TASKTREE_GITHUB_*`, and opt-in updater settings; never commit values.
- Handoff docs are additive/versioned. Keep Q10 PHI source list, Q11 support-email decision, coverage, live E2E, and signing/install gates visible.
- Keep `.serena`, `.obsidian`, build output, caches, and local state out of publish scope unless explicitly requested.