# Core

- Windows-only TaskTree repo; source authority is doc-first, not code-first: `Architecture.md`/`docs/Architecture.md`, `Roadmap.md`/`docs/Roadmap.md`, and additive `docs/HANDOFF*.md` govern names, order, HALT rules, and gaps.
- Root `REPO_BRIEF.md` is the compact Codex/Obsidian entrypoint; do not duplicate large docs into memories.
- Current tree contains many modules beyond the early Phase 1C state described at the top of `docs/HANDOFF.md`; reconcile source/tests against the full handoff chain before asserting phase status.
- Architecture/Roadmap anti-drift is load-bearing: no invented libs, no renamed architecture types, no skipped phases, no real-looking PHI test data, no hardcoded secrets/production URLs.
- Treat app/task/report text as possible PHI; redaction, encrypted storage, and audit chain behavior are compliance-sensitive.
- No `.git` detected at `C:\dev\TaskTree` during onboarding; publish/sync claims require verifying actual repo state first.
- Read for implementation map: `mem:tech_stack`, `mem:conventions`, `mem:app/core`, `mem:corelib/core`, `mem:modules/core`, `mem:ui/core`, `mem:packaging/core`.