// SPEC-DERIVED-PHASE2B
// SPEC-DERIVED-PHASE2C
// SPEC-DERIVED-PHASE2E  Msg 2 Settings property test

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;
using TaskTree.TestSupport;
using TaskTree.UI.ViewModels;

namespace TaskTree.UI.Tests
{
    [TestClass]
    public class MainWindowViewModelTests
    {
        [TestMethod]
        public void Constructor_CreatesSettingsViewModel()
        {
            var engine=new Mock<ITaskEngine>(MockBehavior.Loose);
            var logger=new Mock<IAppLogger>(MockBehavior.Loose);
            var settings=new Mock<ISettingsService>(MockBehavior.Strict);
            settings.Setup(s=>s.GetAsync()).ReturnsAsync(TaskTreeSettings.Default);
            var vm=new MainWindowViewModel(engine.Object,new FakeClock(),logger.Object,settings.Object);
            Assert.IsNotNull(vm.Settings);
        }
    }
}
