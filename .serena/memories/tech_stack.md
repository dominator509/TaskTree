# Tech Stack

- C# 12 / .NET 8, all projects target `net8.0-windows`; WPF projects set `UseWPF=true`.
- Solution: `TaskTree.sln`; app host `src/TaskTree.App`; core contracts/models/enums/security/logging in `src/TaskTree.Core`.
- UI: WPF + ModernWpfUI `0.9.6`, CommunityToolkit.Mvvm `8.2.2`; tray integration uses H.NotifyIcon.Wpf `2.0.108`.
- DI/logging: Microsoft.Extensions.DependencyInjection/Logging 8.x.
- Tests: MSTest + Moq; `tests/TaskTree.TestSupport` holds shared fakes such as clocks/stores.
- AutoUpdater uses NSec.Cryptography for Ed25519 manifest/signature work.
- Packaging: MSIX/WAP project under `packaging/`, with PowerShell packaging script.
- Storage/security: local `%LOCALAPPDATA%\TaskTree\...`, AES-GCM, DPAPI-wrapped master key, hash-chained audit JSON.
- External/live surfaces are environment-sensitive: Windows tray/hotkeys/toasts/CredUI, MSIX install/signing, updater HTTPS, SMTP, GitHub Issues.