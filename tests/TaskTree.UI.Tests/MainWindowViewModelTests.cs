// SPEC-DERIVED-PHASE2B
// SPEC-DERIVED-PHASE2C
// SPEC-DERIVED-PHASE2E  Msg 2 Settings property test

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
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

        [TestMethod]
        public async Task DeleteTaskCommand_DeletesAndRefreshesTree()
        {
            var node = new TaskNode { Id = Guid.NewGuid(), Title = "synthetic-delete" };
            var engine = new Mock<ITaskEngine>(MockBehavior.Strict);
            engine.Setup(e => e.DeleteAsync(node.Id)).Returns(Task.CompletedTask);
            engine.Setup(e => e.GetTreeAsync()).ReturnsAsync(Array.Empty<TaskNode>());
            var logger = new Mock<IAppLogger>(MockBehavior.Loose);
            var settings = new Mock<ISettingsService>(MockBehavior.Strict);
            settings.Setup(s => s.GetAsync()).ReturnsAsync(TaskTreeSettings.Default);
            var vm = new MainWindowViewModel(engine.Object, new FakeClock(), logger.Object, settings.Object);

            await vm.DeleteTaskCommand.ExecuteAsync(node);

            engine.Verify(e => e.DeleteAsync(node.Id), Times.Once);
            engine.Verify(e => e.GetTreeAsync(), Times.Once);
            Assert.AreEqual("Task deleted", vm.StatusMessage);
            Assert.AreEqual(0, vm.Tasks.Count);
        }
    }
}
