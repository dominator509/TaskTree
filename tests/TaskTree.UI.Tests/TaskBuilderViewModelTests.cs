// SPEC-DERIVED-PHASE2C  HALT #17 (14 tests for P2C-AC1/AC2/AC3)

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;
using TaskTree.TestSupport;
using TaskTree.UI.ViewModels;

namespace TaskTree.UI.Tests
{
    [TestClass]
    public class TaskBuilderViewModelTests
    {
        private static readonly DateTimeOffset TestEpoch = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

        private static (TaskBuilderViewModel vm, FakeClock clock, Mock<ITaskEngine> engine, Mock<IAppLogger> logger) Build()
        {
            var clock = new FakeClock(TestEpoch);
            var engine = new Mock<ITaskEngine>(MockBehavior.Strict);
            engine.Setup(e => e.AddAsync(It.IsAny<TaskNode>(), It.IsAny<Guid?>()))
                  .ReturnsAsync((TaskNode node, Guid? _) => node);
            var logger = new Mock<IAppLogger>(MockBehavior.Loose);
            return (new TaskBuilderViewModel(engine.Object, clock, logger.Object), clock, engine, logger);
        }

        [TestMethod] public void Constructor_NullArgs_Throw(){var c=new FakeClock(TestEpoch);var e=new Mock<ITaskEngine>().Object;var l=new Mock<IAppLogger>().Object;Assert.ThrowsException<ArgumentNullException>(()=>new TaskBuilderViewModel(null!,c,l));Assert.ThrowsException<ArgumentNullException>(()=>new TaskBuilderViewModel(e,null!,l));Assert.ThrowsException<ArgumentNullException>(()=>new TaskBuilderViewModel(e,c,null!));}
        [TestMethod] public void Defaults_AreExpected(){var (vm,_,_,_)=Build();Assert.AreEqual(string.Empty,vm.Title);Assert.AreEqual(Core.Enums.Priority.Normal,vm.Priority);Assert.IsNull(vm.Deadline);Assert.AreEqual(string.Empty,vm.PatientText);Assert.IsFalse(vm.RequiresLabReview);}
        [TestMethod] public async Task CreateTaskAsync_EmptyTitle_SetsStatus_NoEngineCall(){var (vm,_,engine,_)=Build();await vm.CreateTaskCommand.ExecuteAsync(null);Assert.AreEqual("Title required",vm.StatusMessage);engine.Verify(e=>e.AddAsync(It.IsAny<TaskNode>(), It.IsAny<Guid?>()),Times.Never);}
        [TestMethod] public async Task CreateTaskAsync_WhitespaceTitle_SetsStatus_NoEngineCall(){var (vm,_,engine,_)=Build();vm.Title="   ";await vm.CreateTaskCommand.ExecuteAsync(null);Assert.AreEqual("Title required",vm.StatusMessage);engine.Verify(e=>e.AddAsync(It.IsAny<TaskNode>(), It.IsAny<Guid?>()),Times.Never);}
        [TestMethod] public async Task CreateTaskAsync_TitleTooLong_SetsStatus_NoEngineCall(){var (vm,_,engine,_)=Build();vm.Title=new string('a',201);await vm.CreateTaskCommand.ExecuteAsync(null);StringAssert.Contains(vm.StatusMessage,"too long");engine.Verify(e=>e.AddAsync(It.IsAny<TaskNode>(), It.IsAny<Guid?>()),Times.Never);}
        [TestMethod] public async Task CreateTaskAsync_PatientTextTooLong_SetsStatus_NoEngineCall(){var (vm,_,engine,_)=Build();vm.Title="ok";vm.PatientText=new string('p',161);await vm.CreateTaskCommand.ExecuteAsync(null);StringAssert.Contains(vm.StatusMessage,"Patient text too long");engine.Verify(e=>e.AddAsync(It.IsAny<TaskNode>(), It.IsAny<Guid?>()),Times.Never);}
        [TestMethod] public async Task CreateTaskAsync_LabHintTooLong_SetsStatus_NoEngineCall(){var (vm,_,engine,_)=Build();vm.Title="ok";vm.LabHint=new string('l',121);await vm.CreateTaskCommand.ExecuteAsync(null);StringAssert.Contains(vm.StatusMessage,"Lab hint too long");engine.Verify(e=>e.AddAsync(It.IsAny<TaskNode>(), It.IsAny<Guid?>()),Times.Never);}
        [TestMethod] public async Task CreateTaskAsync_DeliveryHintTooLong_SetsStatus_NoEngineCall(){var (vm,_,engine,_)=Build();vm.Title="ok";vm.DeliveryHint=new string('d',121);await vm.CreateTaskCommand.ExecuteAsync(null);StringAssert.Contains(vm.StatusMessage,"Delivery hint too long");engine.Verify(e=>e.AddAsync(It.IsAny<TaskNode>(), It.IsAny<Guid?>()),Times.Never);}
        [TestMethod] public async Task CreateTaskAsync_EmailLikeText_Rejected(){var (vm,_,engine,_)=Build();vm.Title="ok";vm.PatientText="x@y.com";await vm.CreateTaskCommand.ExecuteAsync(null);StringAssert.Contains(vm.StatusMessage,"PHI-like");engine.Verify(e=>e.AddAsync(It.IsAny<TaskNode>(), It.IsAny<Guid?>()),Times.Never);}
        [TestMethod] public async Task CreateTaskAsync_DigitRun_Rejected(){var (vm,_,engine,_)=Build();vm.Title="ok";vm.LabHint="1234567";await vm.CreateTaskCommand.ExecuteAsync(null);StringAssert.Contains(vm.StatusMessage,"PHI-like");engine.Verify(e=>e.AddAsync(It.IsAny<TaskNode>(), It.IsAny<Guid?>()),Times.Never);}
        [TestMethod] public async Task CreateTaskAsync_ValidInput_CallsEngineAddAsync_WithMetadata(){var (vm,_,engine,_)=Build();vm.Title="task";vm.PatientText="safe";vm.RequiresLabReview=true;await vm.CreateTaskCommand.ExecuteAsync(null);engine.Verify(e=>e.AddAsync(It.Is<TaskNode>(n=>n.Metadata!=null && n.Metadata.PatientText=="safe" && n.Metadata.RequiresLabReview), It.IsAny<Guid?>()),Times.Once);Assert.AreEqual("Task created",vm.StatusMessage);}
        [TestMethod] public async Task CreateTaskAsync_ValidInput_RaisesTaskCreatedEvent(){var (vm,_,_,_)=Build();TaskNode? node=null;vm.TaskCreated+=(s,e)=>node=e.Node;vm.Title="task";await vm.CreateTaskCommand.ExecuteAsync(null);Assert.IsNotNull(node);}
        [TestMethod] public void ResetForm_ClearsAllFields(){var (vm,_,_,_)=Build();vm.Title="x";vm.PatientText="y";vm.RequiresDeliveryCoordination=true;vm.ResetFormCommand.Execute(null);Assert.AreEqual(string.Empty,vm.Title);Assert.AreEqual(string.Empty,vm.PatientText);Assert.IsFalse(vm.RequiresDeliveryCoordination);}
        [TestMethod] public async Task CreateTaskAsync_EngineThrows_SetsStatus_DoesNotPropagate(){var c=new FakeClock(TestEpoch);var e=new Mock<ITaskEngine>(MockBehavior.Strict);e.Setup(x=>x.AddAsync(It.IsAny<TaskNode>(), It.IsAny<Guid?>())).ThrowsAsync(new InvalidOperationException("synthetic"));var l=new Mock<IAppLogger>(MockBehavior.Loose);var vm=new TaskBuilderViewModel(e.Object,c,l.Object);vm.Title="task";await vm.CreateTaskCommand.ExecuteAsync(null);Assert.AreEqual("Create failed - see log",vm.StatusMessage);}
    }
}
