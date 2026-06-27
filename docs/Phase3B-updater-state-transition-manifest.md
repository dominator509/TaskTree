# Phase 3B Updater State Transition Manifest

> Source: Architecture.md Section 9.1.1. Status: active handoff manifest.

## Allowed Transitions

- Idle -> Checking
- Checking -> Idle
- Checking -> Downloading
- Downloading -> Verifying
- Verifying -> Staging
- Verifying -> Failed
- Staging -> Applying
- Applying -> Applied
- Applying -> Failed
- Failed -> Idle
- Applied -> Idle

## Codex/Claude Notes

- Gap #225: Validate transition graph against full updater flow in Phase 5C.
- Gap #231: Live check/download orchestration remains deferred.
- Gap #219: Audit vocabulary still deferred to Phase 3C/4A.
