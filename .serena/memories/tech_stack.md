# Tech Stack

- C# 12/.NET 8, `net8.0-windows`; WPF, ModernWpfUI, CommunityToolkit.Mvvm, H.NotifyIcon.Wpf.
- Solution `TaskTree.sln`; app `src/TaskTree.App`; contracts/models/security/logging `src/TaskTree.Core`; modules under `src/TaskTree.Modules.*`; tests mirror modules.
- DI/logging: Microsoft.Extensions.DependencyInjection/Logging 8.x; tests MSTest + Moq + `tests/TaskTree.TestSupport`.
- Security/storage: AES-GCM, DPAPI-wrapped local master key, hash-chained audit JSON under `%LOCALAPPDATA%\\TaskTree`.
- AutoUpdater: NSec Ed25519 verification, HTTPS manifest/download opt-in, SHA-256 staging, Add-AppxPackage installer, rollback service.
- Environment capabilities: tray/hotkey/session idle/lock, Tier 3 balloon, runtime SMTP/GitHub adapters; Tier 1 toast/package identity, MSIX signing/install, CredUI, providers, and owner compliance inputs remain live gates.