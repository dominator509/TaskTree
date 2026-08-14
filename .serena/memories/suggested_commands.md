# Suggested Commands

- Inherited rule: prefix external shell commands with `rtk`; use `rtk powershell -NoProfile -File ...` for PowerShell scripts.
- SDK on this host: `C:\\Users\\domin\\.dotnet\\dotnet.exe` (8.0.422); use its full path when PATH lacks `dotnet`.
- Restore: `rtk C:\\Users\\domin\\.dotnet\\dotnet.exe restore TaskTree.sln`.
- Build: `rtk C:\\Users\\domin\\.dotnet\\dotnet.exe build TaskTree.sln -c Release`.
- Offline tests: `rtk C:\\Users\\domin\\.dotnet\\dotnet.exe test TaskTree.sln -c Release --filter TestCategory!=Live`.
- Spec scan: `rtk powershell -NoProfile -File tools/find-spec-derivations.ps1 -Root .`.
- Packaging path: `rtk powershell -NoProfile -File packaging/build-msix.ps1`; missing WAP/MakeAppx/cert prerequisites are environment blockers.
- Use `rtk rg --files`, `rtk rg -n`, `git diff --check`, `git diff --name-only`, and `git status --short` for focused checks.