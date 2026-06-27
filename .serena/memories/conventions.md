# Conventions

- Namespace pattern: `TaskTree.<Layer>.<Module>`; file/type names should match Architecture/Roadmap declarations.
- Public types/methods/properties require XML docs per Roadmap D10; code files cite relevant Architecture sections per D1.
- Async methods use `Async`; injected `IClock` is preferred over direct wall-clock calls in domain/testable logic.
- Constructor injection and module interfaces are the normal seam; modules should remain independently testable through `TaskTree.Core` abstractions.
- Do not replace HALT/NotImplemented environment gaps with guesses; live Windows/provider work must be explicitly verified or honestly left as an environment gap.
- Test data must be synthetic and not resemble real PHI; keep Q10/Q11 blockers visible until owner resolves them.
- Handoff docs are additive/versioned; do not delete old delta docs or flatten history without owner approval.
- No behavior changes for repo-tooling tasks; keep `.serena`, `AGENTS.md`, and `REPO_BRIEF.md` edits scoped.