// SPEC-DERIVED-PHASE2E  Msg 2 Settings UI tests

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.UI.ViewModels;

namespace TaskTree.UI.Tests
{
    [TestClass]
    public class SettingsViewModelTests
    {
        private static (SettingsViewModel vm, Mock<ISettingsService> svc) Build(TaskTreeSettings? settings=null)
        { var svc=new Mock<ISettingsService>(MockBehavior.Strict); svc.Setup(s=>s.GetAsync()).ReturnsAsync(settings??TaskTreeSettings.Default); svc.Setup(s=>s.SaveAsync(It.IsAny<TaskTreeSettings>())).Returns(Task.CompletedTask); svc.Setup(s=>s.ResetAsync()).Returns(Task.CompletedTask); var logger=new Mock<IAppLogger>(MockBehavior.Loose); return (new SettingsViewModel(svc.Object,logger.Object),svc); }
        [TestMethod] public void Constructor_NullArgs_Throw(){var svc=new Mock<ISettingsService>().Object;var logger=new Mock<IAppLogger>().Object;Assert.ThrowsException<System.ArgumentNullException>(()=>new SettingsViewModel(null!,logger));Assert.ThrowsException<System.ArgumentNullException>(()=>new SettingsViewModel(svc,null!));}
        [TestMethod] public async Task InitializeAsync_LoadsSettings(){var settings=TaskTreeSettings.Default with { ThemePreference=ThemePreference.Dark, ShowCompletedTasks=true };var (vm,_)=Build(settings);await vm.InitializeAsync();Assert.AreEqual(ThemePreference.Dark,vm.ThemePreference);Assert.IsTrue(vm.ShowCompletedTasks);}
        [TestMethod] public async Task SaveCommand_CallsSaveAsync(){var (vm,svc)=Build();vm.ThemePreference=ThemePreference.Light;await vm.SaveCommand.ExecuteAsync(null);svc.Verify(s=>s.SaveAsync(It.Is<TaskTreeSettings>(x=>x.ThemePreference==ThemePreference.Light)),Times.Once);Assert.AreEqual("Settings saved",vm.StatusMessage);}
        [TestMethod] public async Task ResetCommand_CallsResetAsync_AndAppliesDefault(){var (vm,svc)=Build();vm.ThemePreference=ThemePreference.Dark;await vm.ResetCommand.ExecuteAsync(null);svc.Verify(s=>s.ResetAsync(),Times.Once);Assert.AreEqual(ThemePreference.System,vm.ThemePreference);Assert.AreEqual("Settings reset",vm.StatusMessage);}
        [TestMethod] public async Task LoadCommand_WhenServiceThrows_SetsStatus(){var svc=new Mock<ISettingsService>(MockBehavior.Strict);svc.Setup(s=>s.GetAsync()).ThrowsAsync(new System.InvalidOperationException("x"));var logger=new Mock<IAppLogger>(MockBehavior.Loose);var vm=new SettingsViewModel(svc.Object,logger.Object);await vm.LoadCommand.ExecuteAsync(null);Assert.AreEqual("Settings load failed - see log",vm.StatusMessage);}
        [TestMethod] public async Task SaveCommand_WhenServiceThrows_SetsStatus(){var (vm,svc)=Build();svc.Setup(s=>s.SaveAsync(It.IsAny<TaskTreeSettings>())).ThrowsAsync(new System.InvalidOperationException("x"));await vm.SaveCommand.ExecuteAsync(null);Assert.AreEqual("Settings save failed - see log",vm.StatusMessage);}
    }
}
