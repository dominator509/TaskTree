// File: tests/TaskTree.UI.Tests/MainWindowViewModelCoverageTests.cs
// Covers: Architecture §4.2 and §7; Roadmap P2B/P5C coverage gate.
// Synthetic task data only; no PHI-shaped values.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;
using TaskTree.TestSupport;
using TaskTree.UI.ViewModels;

namespace TaskTree.UI.Tests;

[TestClass]
public sealed class MainWindowViewModelCoverageTests
{
    private static (MainWindowViewModel ViewModel, Mock<ITaskEngine> Engine, Mock<ISettingsService> Settings) Build()
    {
        var engine = new Mock<ITaskEngine>(MockBehavior.Strict);
        engine.Setup(e => e.GetTreeAsync()).ReturnsAsync(Array.Empty<TaskNode>());
        engine.Setup(e => e.AddAsync(It.IsAny<TaskNode>(), It.IsAny<Guid?>()))
            .ReturnsAsync((TaskNode node, Guid? _) => node);
        var settings = new Mock<ISettingsService>(MockBehavior.Strict);
        settings.Setup(s => s.GetAsync()).ReturnsAsync(TaskTreeSettings.Default);
        var logger = new Mock<IAppLogger>(MockBehavior.Loose);
        return (new MainWindowViewModel(engine.Object, new FakeClock(), logger.Object, settings.Object), engine, settings);
    }

    [TestMethod]
    public void Constructor_NullDependencies_Throw()
    {
        var engine = new Mock<ITaskEngine>().Object;
        var clock = new FakeClock();
        var logger = new Mock<IAppLogger>().Object;
        var settings = new Mock<ISettingsService>().Object;
        Assert.ThrowsException<ArgumentNullException>(() => new MainWindowViewModel(null!, clock, logger, settings));
        Assert.ThrowsException<ArgumentNullException>(() => new MainWindowViewModel(engine, null!, logger, settings));
        Assert.ThrowsException<ArgumentNullException>(() => new MainWindowViewModel(engine, clock, null!, settings));
        Assert.ThrowsException<ArgumentNullException>(() => new MainWindowViewModel(engine, clock, logger, null!));
    }

    [TestMethod]
    public async Task InitializeAsync_LoadsTreeAndSettings()
    {
        var (vm, engine, settings) = Build();
        var first = new TaskNode { Id = Guid.NewGuid(), Title = "First" };
        var second = new TaskNode { Id = Guid.NewGuid(), Title = "Second" };
        engine.Setup(e => e.GetTreeAsync()).ReturnsAsync(new List<TaskNode> { first, second });
        await vm.InitializeAsync();
        Assert.AreEqual(2, vm.Tasks.Count);
        Assert.AreEqual("Loaded 2 tasks", vm.StatusMessage);
        settings.Verify(s => s.GetAsync(), Times.Once);
    }

    [TestMethod]
    public async Task Refresh_WhenEngineThrows_SetsFailureStatus()
    {
        var (vm, engine, _) = Build();
        engine.Setup(e => e.GetTreeAsync()).ThrowsAsync(new InvalidOperationException("synthetic"));
        await vm.RefreshCommand.ExecuteAsync(null);
        Assert.AreEqual("Refresh failed - see log", vm.StatusMessage);
        Assert.IsFalse(vm.IsBusy);
    }

    [TestMethod]
    public async Task QuickAdd_ValidationAndSuccessPaths()
    {
        var (vm, engine, _) = Build();
        await vm.QuickAddCommand.ExecuteAsync(null);
        Assert.AreEqual("Title required", vm.StatusMessage);

        vm.NewTaskTitle = new string('x', 201);
        await vm.QuickAddCommand.ExecuteAsync(null);
        StringAssert.Contains(vm.StatusMessage, "too long");

        vm.NewTaskTitle = "  synthetic task  ";
        await vm.QuickAddCommand.ExecuteAsync(null);
        Assert.AreEqual("Task added", vm.StatusMessage);
        Assert.AreEqual(string.Empty, vm.NewTaskTitle);
        Assert.AreEqual(1, vm.Tasks.Count);
        engine.Verify(e => e.AddAsync(It.Is<TaskNode>(n => n.Title == "synthetic task"), It.IsAny<Guid?>()), Times.Once);
    }

    [TestMethod]
    public async Task QuickAdd_WhenEngineThrows_SetsFailureStatus()
    {
        var (vm, engine, _) = Build();
        engine.Setup(e => e.AddAsync(It.IsAny<TaskNode>(), It.IsAny<Guid?>()))
            .ThrowsAsync(new InvalidOperationException("synthetic"));
        vm.NewTaskTitle = "synthetic task";
        await vm.QuickAddCommand.ExecuteAsync(null);
        Assert.AreEqual("Add failed - see log", vm.StatusMessage);
        Assert.IsFalse(vm.IsBusy);
    }
}
