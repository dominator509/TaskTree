# Suggested Commands

- Inherited repo rule: prefix shell commands with `rtk`; PowerShell builtins should run through `rtk powershell -NoProfile -Command "..."` or `rtk powershell -NoProfile -File ...`.
- Restore: `rtk dotnet restore TaskTree.sln`.
- Build: `rtk dotnet build TaskTree.sln -c Release`.
- Offline tests: `rtk dotnet test TaskTree.sln -c Release --filter Category!=Live`.
- Spec marker scan: `rtk powershell -NoProfile -File tools/find-spec-derivations.ps1 -Root .`.
- MSIX path when explicitly validating packaging: `rtk powershell -NoProfile -File packaging/build-msix.ps1`.
- Fast file search: `rtk rg --files`; text search: `rtk rg -n <pattern> <paths>`.
- Git commands may fail because root was not a git repo during onboarding; report exact `git status` result instead of assuming.