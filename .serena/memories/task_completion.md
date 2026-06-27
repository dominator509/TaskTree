# Task Completion

- Re-read relevant Architecture/Roadmap/HANDOFF sections before coding; verify current files when handoff state and source tree diverge.
- For code changes, normal local completion path is restore/build/offline tests: `rtk dotnet restore TaskTree.sln`, `rtk dotnet build TaskTree.sln -c Release`, `rtk dotnet test TaskTree.sln -c Release --filter Category!=Live`.
- For spec-derived work, also run `rtk powershell -NoProfile -File tools/find-spec-derivations.ps1 -Root .` if marker counts or derivations are touched.
- For packaging changes, use documented `packaging/build-msix.ps1`; do not treat missing cert/signing/MSIX install prerequisites as code failures.
- For doc/tooling-only changes, validate syntax/format locally and avoid full app tests unless needed.
- Always report skipped commands, local environment blockers, and local-only versus live/provider validation boundaries.