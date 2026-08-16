# Core

- Windows-only TaskTree; authority is doc-first: `Architecture.md`/`docs/Architecture.md`, `Roadmap.md`/`docs/Roadmap.md`, additive `docs/HANDOFF*.md`.
- `REPO_BRIEF.md` is the compact Codex/Obsidian entrypoint; link to phase evidence in `docs/*-gap-report.md` instead of duplicating source.
- Anti-drift/HALT/owner-approval rules are load-bearing; preserve synthetic PHI data, encrypted storage, redaction, audit-chain behavior, and additive handoffs.
- Current checkout is a valid Git worktree on `main` with the TaskTree remote; verify status/remote before publish claims.
- Invalid startup audit chains continue startup after recording `ChainVerifyFailedAtStartup`; the validated prefix is exported atomically under the local incident root when available. Read `mem:task_completion` for the validation and acceptance gates.
- Implementation map: `mem:tech_stack`, `mem:conventions`, `mem:app/core`, `mem:corelib/core`, `mem:modules/core`, `mem:ui/core`, `mem:packaging/core`.