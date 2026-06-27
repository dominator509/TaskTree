# Packaging Core

- Packaging files live under `packaging/`: `TaskTree.Installer.wapproj`, `Package.appxmanifest`, `build-msix.ps1`.
- MSIX packaging/signing/install is a Phase 4/5 environment surface; certificate, signtool, Visual Studio/MSIX workload, and Windows install context may block local validation.
- Packaging project references app/core/orchestrator/UI and declared feature modules; do not add package references or capabilities outside Architecture tech stack without owner approval.
- Update manifests and package artifacts should be treated as generated/release output; keep source manifests/scripts visible but ignore built `.msix`, `.appx`, bundles, publish/AppPackages output in Serena.