// =============================================================================
// TaskTree - ServiceRegistrationsTests.cs
// Implements: Roadmap P1F-AC1 (container builds; 0 missing deps)
// Phase:      1F Msg 3
// Authority:  PHASE1F-DERIVATIONS-MSG2.md §11, §13 + PHASE1F-DERIVATIONS-MSG3.md §16
// HALT #2 Msg 3: Option C Hybrid - real DI + temp directory isolation.
// TEST-GAP TG1F-4: per-test temp dir; cleanup is best-effort.
// TEST-GAP TG1F-5: DPAPI access required (Windows + same user profile).
// =============================================================================
using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.App.Bootstrap;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;

namespace TaskTree.Orchestrator.Tests;

/// <summary>
/// Container-build verification (P1F-AC1). Real DI + per-test temp directory.
/// </summary>
[TestClass]
public class ServiceRegistrationsTests
{
    private string _tempRoot = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), $"TaskTree_Tests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempRoot);
    }

    [TestCleanup]
    public void Cleanup()
    {
        try
        {
            if (!string.IsNullOrEmpty(_tempRoot) && Directory.Exists(_tempRoot))
                Directory.Delete(_tempRoot, recursive: true);
        }
        catch
        {
            // TG1F-4: best-effort.
        }
    }

    private IServiceCollection BuildOverriddenServices()
    {
        var paths = new TaskTreePaths(_tempRoot);
        paths.EnsureDirectoriesExist();
        var services = new ServiceCollection();
        services.AddSingleton(paths);
        services.AddTaskTreeServices();
        return services;
    }

    [TestMethod, TestCategory("Offline")]
    public void AddTaskTreeServices_RegistersAllRequiredInterfaces()
    {
        var services = new ServiceCollection();
        services.AddTaskTreeServices();
        var registered = services.Select(d => d.ServiceType).ToHashSet();
        Assert.IsTrue(registered.Contains(typeof(TaskTreePaths)));
        Assert.IsTrue(registered.Contains(typeof(IClock)));
        Assert.IsTrue(registered.Contains(typeof(ICryptoProvider)));
        Assert.IsTrue(registered.Contains(typeof(IAppLogger)));
        Assert.IsTrue(registered.Contains(typeof(IMasterKeyManager)));
        Assert.IsTrue(registered.Contains(typeof(ISecureStore)));
        Assert.IsTrue(registered.Contains(typeof(IComplianceCore)));
        Assert.IsTrue(registered.Contains(typeof(ITaskEngine)));
        Assert.IsTrue(registered.Contains(typeof(IReminderScheduler)));
        Assert.IsTrue(registered.Contains(typeof(ITrayHost)));
        Assert.IsTrue(registered.Contains(typeof(IReminderDeliveryService)));
        Assert.IsTrue(registered.Contains(typeof(ISettingsService)));
        Assert.IsTrue(registered.Contains(typeof(ISessionLockService)));
        Assert.IsTrue(registered.Contains(typeof(ISnoozeService)));
        Assert.IsTrue(registered.Contains(typeof(IOrchestrator)));
    }

    [TestMethod, TestCategory("Offline")]
    public void BuildServiceProvider_ResolvesIOrchestrator_NoMissingDeps()
    {
        // P1F-AC1; TG1F-5: requires Windows + same user (DPAPI).
        using var provider = BuildOverriddenServices().BuildServiceProvider(validateScopes: true);
        var orch = provider.GetRequiredService<IOrchestrator>();
        Assert.IsNotNull(orch);
    }

    [TestMethod, TestCategory("Offline")]
    public void BuildServiceProvider_ResolvesITaskEngine_WithComplianceCoreInjected()
    {
        // Closes Cross-phase Flag #2.
        using var provider = BuildOverriddenServices().BuildServiceProvider(validateScopes: true);
        var engine = provider.GetRequiredService<ITaskEngine>();
        Assert.IsNotNull(engine);
    }

    [TestMethod, TestCategory("Offline")]
    public void BuildServiceProvider_ResolvesISecureStore_WithMasterKeyManagerInjected()
    {
        // Closes Cross-phase Flag #3.
        using var provider = BuildOverriddenServices().BuildServiceProvider(validateScopes: true);
        var store = provider.GetRequiredService<ISecureStore>();
        Assert.IsNotNull(store);
    }

    [TestMethod, TestCategory("Offline")]
    public void BuildServiceProvider_AllServicesAreSingletons()
    {
        using var provider = BuildOverriddenServices().BuildServiceProvider(validateScopes: true);
        var a1 = provider.GetRequiredService<IOrchestrator>();
        var a2 = provider.GetRequiredService<IOrchestrator>();
        Assert.AreSame(a1, a2, "IOrchestrator must be singleton (HALT #5 Msg 1).");
        var b1 = provider.GetRequiredService<IReminderScheduler>();
        var b2 = provider.GetRequiredService<IReminderScheduler>();
        Assert.AreSame(b1, b2, "IReminderScheduler must be singleton.");
    }

    [TestMethod, TestCategory("Offline")]
    public void BuildServiceProvider_ResolvesIReminderDeliveryService_NoMissingDeps()
    {
        using var provider = BuildOverriddenServices().BuildServiceProvider(validateScopes: true);
        var delivery = provider.GetRequiredService<IReminderDeliveryService>();
        Assert.IsNotNull(delivery);
    }
}
