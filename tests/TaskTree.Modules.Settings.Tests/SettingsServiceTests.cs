// SPEC-DERIVED-PHASE2E  HALT #19 (12 tests for SettingsService)

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.Modules.Settings;
using TaskTree.TestSupport;

namespace TaskTree.Modules.Settings.Tests
{
    [TestClass]
    public class SettingsServiceTests
    {
        private static readonly DateTimeOffset TestEpoch = new(2026,6,1,12,0,0,TimeSpan.Zero);
        private static (SettingsService svc, InMemorySecureStore store, Mock<IComplianceCore> compliance) Build()
        {
            var store = new InMemorySecureStore();
            var compliance = new Mock<IComplianceCore>(MockBehavior.Strict);
            compliance.Setup(c => c.AuditAsync(It.IsAny<AuditEntry>())).Returns(Task.CompletedTask);
            var logger = new Mock<IAppLogger>(MockBehavior.Loose);
            var svc = new SettingsService(store, compliance.Object, new FakeClock(TestEpoch), logger.Object);
            return (svc, store, compliance);
        }

        [TestMethod] public void Constructor_NullArgs_Throw(){var s=new InMemorySecureStore();var c=new Mock<IComplianceCore>().Object;var clk=new FakeClock(TestEpoch);var l=new Mock<IAppLogger>().Object;Assert.ThrowsException<ArgumentNullException>(()=>new SettingsService(null!,c,clk,l));Assert.ThrowsException<ArgumentNullException>(()=>new SettingsService(s,null!,clk,l));Assert.ThrowsException<ArgumentNullException>(()=>new SettingsService(s,c,null!,l));Assert.ThrowsException<ArgumentNullException>(()=>new SettingsService(s,c,clk,null!));}
        [TestMethod] public async Task GetAsync_NoExistingSettings_ReturnsDefault(){var (svc,_,_)=Build();Assert.AreEqual(TaskTreeSettings.Default, await svc.GetAsync());}
        [TestMethod] public async Task SaveAsync_ValidSettings_Persists(){var (svc,_,_)=Build();await svc.SaveAsync(TaskTreeSettings.Default with { ShowCompletedTasks=true });Assert.IsTrue((await svc.GetAsync()).ShowCompletedTasks);}
        [TestMethod] public async Task GetAsync_AfterSave_ReturnsSaved(){var (svc,_,_)=Build();var settings=TaskTreeSettings.Default with { ThemePreference=ThemePreference.Dark, ReminderSnoozeMinutes=15 };await svc.SaveAsync(settings);Assert.AreEqual(settings, await svc.GetAsync());}
        [TestMethod] public async Task SaveAsync_InvalidSnoozeLow_Throws(){var (svc,_,_)=Build();await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(()=>svc.SaveAsync(TaskTreeSettings.Default with { ReminderSnoozeMinutes=0 }));}
        [TestMethod] public async Task SaveAsync_InvalidSnoozeHigh_Throws(){var (svc,_,_)=Build();await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(()=>svc.SaveAsync(TaskTreeSettings.Default with { ReminderSnoozeMinutes=241 }));}
        [TestMethod] public async Task SaveAsync_InvalidTheme_Throws(){var (svc,_,_)=Build();await Assert.ThrowsExceptionAsync<ArgumentException>(()=>svc.SaveAsync(TaskTreeSettings.Default with { ThemePreference=(ThemePreference)999 }));}
        [TestMethod] public async Task SaveAsync_RaisesSettingsChanged(){var (svc,_,_)=Build();var raised=false;svc.SettingsChanged+=(s,e)=>raised=true;await svc.SaveAsync(TaskTreeSettings.Default);Assert.IsTrue(raised);}
        [TestMethod] public async Task SaveAsync_AuditsSettingsSaved(){var (svc,_,c)=Build();await svc.SaveAsync(TaskTreeSettings.Default);c.Verify(x=>x.AuditAsync(It.Is<AuditEntry>(e=>e.Module=="SettingsService"&&e.Action=="SettingsSaved"&&e.Result=="success")),Times.Once);}
        [TestMethod] public async Task ResetAsync_SavesDefault(){var (svc,_,_)=Build();await svc.SaveAsync(TaskTreeSettings.Default with { ShowCompletedTasks=true });await svc.ResetAsync();Assert.AreEqual(TaskTreeSettings.Default, await svc.GetAsync());}
        [TestMethod] public async Task ResetAsync_RaisesSettingsChanged(){var (svc,_,_)=Build();var raised=false;svc.SettingsChanged+=(s,e)=>raised=true;await svc.ResetAsync();Assert.IsTrue(raised);}
        [TestMethod] public async Task ResetAsync_AuditsSettingsReset(){var (svc,_,c)=Build();await svc.ResetAsync();c.Verify(x=>x.AuditAsync(It.Is<AuditEntry>(e=>e.Module=="SettingsService"&&e.Action=="SettingsReset"&&e.Result=="success")),Times.Once);}
    }
}
