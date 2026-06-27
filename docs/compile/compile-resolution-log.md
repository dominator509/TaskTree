# compile-resolution-log.md - Phase 5B Compile Resolution Log

> Every compile fix must be recorded here by Claude/Codex. This log is the audit trail proving Phase 5B stayed inside compile-closure scope.

## Resolution Entry Template

```text
Resolution ID:
Related compile error ID(s):
File(s) changed:
Change summary:
Exact rationale:
Why this is compile-closure only:
Why runtime behavior is unchanged or minimally changed:
Regression risk:
Tests/build commands re-run:
Command output reference:
Follow-up gap created/updated:
Reviewer/owner note:
```

## Allowed Compile-Closure Fix Types

- Add missing using directive.
- Correct namespace to match project/folder convention.
- Add missing project reference.
- Add approved package reference.
- Reconcile constructor arguments to latest generated constructor.
- Update TestSupport fake to match final interface.
- Resolve duplicate type by selecting latest approved source and logging superseded source.
- Align WPF target framework/project properties.
- Fix XAML/code-behind namespace or partial class mismatch.

## Not Allowed Without New Approval

- Redesigning module behavior.
- Replacing stubs with fake live success.
- Adding external libraries not approved by Architecture/Roadmap.
- Removing audit/redaction/security checks to make tests compile.
- Hardcoding secrets, tokens, cert passwords, private keys, production URLs, or PHI-like data.

## Deletion / Exclusion Rule

Any file deleted or excluded from compile must include:

```text
Path:
Reason:
Proof duplicate/superseded:
Selected replacement path:
Downstream impact:
Owner review needed: yes/no
```

## Phase 5B Gaps

| Gap | Description | Target |
|---|---|---|
| #372 | Every compile fix must be recorded with rationale, changed files, and regression risk | Phase 5B |
| #374 | Any new package reference must be source-approved or documented for owner review | Phase 5B / Owner |
| #365 | Maintain compile-error register and resolution log | Phase 5B |
