// SPEC-DERIVED-PHASE1F
// SPEC-DERIVED-PHASE1G-MSG2
// SPEC-DERIVED-PHASE2A
// SPEC-DERIVED-PHASE2E
// SPEC-DERIVED-PHASE2F
// SPEC-DERIVED-PHASE2G  HALT #21 SnoozeService registration

using System;
using Microsoft.Extensions.DependencyInjection;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Logging;
using TaskTree.Core.Security;
using TaskTree.Modules.ComplianceCore;
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
            services.AddSingleton<IClock, Clock>(); services.AddSingleton<ICryptoProvider, AesGcmCryptoProvider>(); services.AddSingleton<IAppLogger>(sp=>new FileAppLogger(CompositionRoot.GetLogDirectory()));
            services.AddSingleton<IMasterKeyManager>(sp=>new MasterKeyManager(CompositionRoot.GetKeyDirectory(),sp.GetRequiredService<IAppLogger>(),"master.bin"));
            services.AddSingleton<ISecureStore>(sp=>new SecureStore(CompositionRoot.GetStorageDirectory(),sp.GetRequiredService<IMasterKeyManager>(),sp.GetRequiredService<ICryptoProvider>(),sp.GetRequiredService<IAppLogger>()));
            services.AddSingleton<IComplianceCore>(sp=>{var logger=sp.GetRequiredService<IAppLogger>();var writer=new AuditChainWriter(sp.GetRequiredService<ISecureStore>(),sp.GetRequiredService<ICryptoProvider>());return new ComplianceCore(writer,new PhiRedactor(Array.Empty<string>()),sp.GetRequiredService<IClock>(),logger,sp.GetRequiredService<ISecureStore>());});
            services.AddSingleton<ISnoozeService>(sp=>new SnoozeService(sp.GetRequiredService<ISecureStore>(),sp.GetRequiredService<IComplianceCore>(),sp.GetRequiredService<IClock>(),sp.GetRequiredService<IAppLogger>()));
            services.AddSingleton<ISettingsService>(sp=>new SettingsService(sp.GetRequiredService<ISecureStore>(),sp.GetRequiredService<IComplianceCore>(),sp.GetRequiredService<IClock>(),sp.GetRequiredService<IAppLogger>()));
            services.AddSingleton<ISessionLockService>(sp=>new SessionLockService(sp.GetRequiredService<IClock>(),sp.GetRequiredService<IComplianceCore>(),sp.GetRequiredService<IAppLogger>()));
            services.AddSingleton<ITaskEngine>(sp=>new TaskEngine(sp.GetRequiredService<ISecureStore>(),sp.GetRequiredService<IComplianceCore>(),sp.GetRequiredService<IClock>(),sp.GetRequiredService<IAppLogger>()));
            services.AddSingleton<IReminderScheduler>(sp=>new ReminderScheduler(sp.GetRequiredService<IClock>(),sp.GetRequiredService<ITaskEngine>(),sp.GetRequiredService<IComplianceCore>(),sp.GetRequiredService<IAppLogger>()));
            services.AddSingleton<ITrayHost>(sp=>new TrayHost(sp.GetRequiredService<IAppLogger>(),sp.GetRequiredService<IComplianceCore>()));
            services.AddSingleton<ToastTier1Adapter>(sp=>new ToastTier1Adapter(sp.GetRequiredService<IAppLogger>()));
            services.AddSingleton<ToastTier2Adapter>(sp=>new ToastTier2Adapter(sp.GetRequiredService<IAppLogger>(),sp.GetRequiredService<ISessionLockService>()));
            services.AddSingleton<ToastTier3Adapter>(sp=>new ToastTier3Adapter(sp.GetRequiredService<ITrayHost>(),sp.GetRequiredService<IAppLogger>()));
            services.AddSingleton<IReminderDeliveryService>(sp=>new ReminderDeliveryService(sp.GetRequiredService<IReminderScheduler>(),sp.GetRequiredService<ToastTier1Adapter>(),sp.GetRequiredService<ToastTier2Adapter>(),sp.GetRequiredService<ToastTier3Adapter>(),sp.GetRequiredService<IClock>(),sp.GetRequiredService<IAppLogger>(),sp.GetRequiredService<IComplianceCore>(),sp.GetRequiredService<ISnoozeService>()));
            services.AddSingleton<IOrchestrator>(sp=>new Orchestrator(sp.GetRequiredService<ITaskEngine>(),sp.GetRequiredService<IReminderScheduler>(),sp.GetRequiredService<IComplianceCore>(),sp.GetRequiredService<ITrayHost>(),sp.GetRequiredService<IReminderDeliveryService>(),sp.GetRequiredService<ISettingsService>(),sp.GetRequiredService<ISessionLockService>(),sp.GetRequiredService<IAppLogger>(),sp.GetRequiredService<IClock>()));
            return services;
        }
    }
}
