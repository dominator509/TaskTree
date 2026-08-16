// SPEC-DERIVED-PHASE1F
// SPEC-DERIVED-PHASE1G-MSG2
// SPEC-DERIVED-PHASE2F
// SPEC-DERIVED-PHASE2G  HALT #21 SnoozeService registration

using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TaskTree.Core.Abstractions;
using TaskTree.Core;
using TaskTree.Core.Logging;
using TaskTree.Core.Models;
using TaskTree.Core.Security;
using TaskTree.Modules.ComplianceCore;
using TaskTree.Modules.AutoUpdater;
using TaskTree.Modules.BugReporter;
using TaskTree.Modules.ReminderScheduler;
using TaskTree.Modules.SecureStore;
using TaskTree.Modules.SessionLock;
using TaskTree.Modules.Settings;
using TaskTree.Modules.Snooze;
using TaskTree.Modules.TaskEngine;
using TaskTree.Modules.TrayHost;
using TaskTree.Orchestrator;

namespace TaskTree.App.Bootstrap
{
    public static class ServiceRegistrations
    {
        public static IServiceCollection AddTaskTreeServices(this IServiceCollection services)
        {
            services.TryAddSingleton<TaskTreePaths>();
            services.AddSingleton<IClock, Clock>(); services.AddSingleton<ICryptoProvider, AesGcmCryptoProvider>(); services.AddSingleton<IAppLogger>(sp=>{var paths=sp.GetRequiredService<TaskTreePaths>();paths.EnsureDirectoriesExist();return new FileAppLogger(paths.LogDir);});
            services.AddSingleton<IMasterKeyManager>(sp=>{var paths=sp.GetRequiredService<TaskTreePaths>();paths.EnsureDirectoriesExist();return new MasterKeyManager(paths.KeyDir,sp.GetRequiredService<IAppLogger>(),"master.bin");});
            services.AddSingleton<ISecureStore>(sp=>{var paths=sp.GetRequiredService<TaskTreePaths>();paths.EnsureDirectoriesExist();return new SecureStore(paths.StorageDir,sp.GetRequiredService<IMasterKeyManager>(),sp.GetRequiredService<ICryptoProvider>(),sp.GetRequiredService<IAppLogger>());});
            services.AddSingleton<IComplianceCore>(sp=>{var logger=sp.GetRequiredService<IAppLogger>();var writer=new AuditChainWriter(sp.GetRequiredService<ISecureStore>(),sp.GetRequiredService<IClock>(),logger);return new ComplianceCore(sp.GetRequiredService<ISecureStore>(),sp.GetRequiredService<IClock>(),logger,new PhiRedactor(Array.Empty<string>()),writer);});
            services.AddSingleton<SentinelService>(sp => new SentinelService(sp.GetRequiredService<TaskTreePaths>().SentinelPath));
            services.AddSingleton<RollbackService>(sp => new RollbackService(Path.Combine(sp.GetRequiredService<TaskTreePaths>().UpdatesDir, "rollback")));
            services.AddSingleton<IAutoUpdater>(sp=>
            {
                var paths=sp.GetRequiredService<TaskTreePaths>();
                var signer=new ManifestSigner();
                var hash=new HashVerifier();
                var staging=new StagingService(paths.UpdatesDir,hash);
                var logger=sp.GetRequiredService<IAppLogger>();
                return new AutoUpdater(signer,hash,new UpdaterStateMachine(sp.GetRequiredService<IClock>()),staging,new VersionEligibilityEvaluator(),new OfflineImportService(signer,hash,staging,logger),paths.UpdatesDir,sp.GetRequiredService<SentinelService>());
            });
            services.AddSingleton<BugReportQueue>(sp=>new BugReportQueue(sp.GetRequiredService<ISecureStore>()));
            services.AddSingleton<RedactionPipeline>(sp=>new RedactionPipeline(sp.GetRequiredService<IComplianceCore>()));
            services.AddSingleton<CrashCaptureHook>();
            services.AddSingleton<EmailDeliveryAdapter>();
            services.AddSingleton<GitHubIssueAdapter>();
            services.AddSingleton<FileDropAdapter>(sp=>new FileDropAdapter(System.IO.Path.Combine(sp.GetRequiredService<TaskTreePaths>().BugReportsDir,"out")));
            services.AddSingleton<BugReportRateLimiter>();
            services.AddSingleton<DeliveryRouter>(sp=>new DeliveryRouter(sp.GetRequiredService<EmailDeliveryAdapter>(),sp.GetRequiredService<GitHubIssueAdapter>(),sp.GetRequiredService<FileDropAdapter>(),sp.GetRequiredService<BugReportRateLimiter>(),sp.GetRequiredService<IClock>()));
            services.AddSingleton<IBugReporter>(sp=>new BugReporter(sp.GetRequiredService<BugReportQueue>(),sp.GetRequiredService<RedactionPipeline>(),sp.GetRequiredService<CrashCaptureHook>(),sp.GetRequiredService<IClock>(),sp.GetRequiredService<IAppLogger>(),sp.GetRequiredService<DeliveryRouter>()));
            services.AddSingleton<ISnoozeService>(sp=>new SnoozeService(sp.GetRequiredService<ISecureStore>(),sp.GetRequiredService<IComplianceCore>(),sp.GetRequiredService<IClock>(),sp.GetRequiredService<IAppLogger>()));
            services.AddSingleton<ISettingsService>(sp=>new SettingsService(sp.GetRequiredService<ISecureStore>(),sp.GetRequiredService<IComplianceCore>(),sp.GetRequiredService<IClock>(),sp.GetRequiredService<IAppLogger>()));
            services.AddSingleton<ISessionLockService>(sp=>new SessionLockService(sp.GetRequiredService<IClock>(),sp.GetRequiredService<IComplianceCore>(),sp.GetRequiredService<IAppLogger>()));
            services.AddSingleton<ITaskEngine>(sp=>new TaskEngine(sp.GetRequiredService<ISecureStore>(),sp.GetRequiredService<IClock>(),sp.GetRequiredService<IAppLogger>(),sp.GetRequiredService<IComplianceCore>()));
            services.AddSingleton<IReminderScheduler>(sp=>new ReminderScheduler(sp.GetRequiredService<IClock>(),sp.GetRequiredService<ITaskEngine>(),sp.GetRequiredService<IComplianceCore>(),sp.GetRequiredService<IAppLogger>()));
            services.AddSingleton<HotkeyManager>(sp=>new HotkeyManager(sp.GetRequiredService<IAppLogger>(),sp.GetRequiredService<IComplianceCore>(),sp.GetRequiredService<ISecureStore>(),sp.GetRequiredService<IClock>()));
            services.AddSingleton<ITrayHost>(sp=>new TrayHost(sp.GetRequiredService<IAppLogger>(),sp.GetRequiredService<IComplianceCore>(),sp.GetRequiredService<HotkeyManager>()));
            services.AddSingleton<ToastTier1Adapter>(sp=>new ToastTier1Adapter(sp.GetRequiredService<IAppLogger>()));
            services.AddSingleton<ToastTier2Adapter>(sp=>new ToastTier2Adapter(sp.GetRequiredService<IAppLogger>(),sp.GetRequiredService<ISessionLockService>()));
            services.AddSingleton<ToastTier3Adapter>(sp=>new ToastTier3Adapter(sp.GetRequiredService<ITrayHost>(),sp.GetRequiredService<IAppLogger>()));
            services.AddSingleton<IReminderDeliveryService>(sp=>new ReminderDeliveryService(sp.GetRequiredService<IReminderScheduler>(),sp.GetRequiredService<ToastTier1Adapter>(),sp.GetRequiredService<ToastTier2Adapter>(),sp.GetRequiredService<ToastTier3Adapter>(),sp.GetRequiredService<IClock>(),sp.GetRequiredService<IAppLogger>(),sp.GetRequiredService<IComplianceCore>(),sp.GetRequiredService<ISnoozeService>()));
            services.AddSingleton<IOrchestrator>(sp=>new TaskTree.Orchestrator.Orchestrator(sp.GetRequiredService<ITaskEngine>(),sp.GetRequiredService<IReminderScheduler>(),sp.GetRequiredService<IComplianceCore>(),sp.GetRequiredService<ITrayHost>(),sp.GetRequiredService<IReminderDeliveryService>(),sp.GetRequiredService<ISettingsService>(),sp.GetRequiredService<ISessionLockService>(),sp.GetRequiredService<IAppLogger>(),sp.GetRequiredService<IClock>()));
            return services;
        }
    }
}
