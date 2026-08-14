using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;
using TaskTree.Orchestrator;

namespace TaskTree.Orchestrator.Tests;

[TestClass]
public sealed class ToastTier3AdapterTests
{
    [TestMethod]
    public void TryDeliver_UsesTrayBalloon()
    {
        var tray = new Mock<ITrayHost>(MockBehavior.Strict);
        tray.Setup(x => x.ShowBalloon("TaskTree reminder", "A task reminder is due."));
        using var adapter = new ToastTier3Adapter(tray.Object, new Mock<IAppLogger>(MockBehavior.Loose).Object);

        Assert.IsTrue(adapter.TryDeliver(new ReminderEvent()));
        tray.Verify(x => x.ShowBalloon("TaskTree reminder", "A task reminder is due."), Times.Once);
    }

    [TestMethod]
    public void TryDeliver_WhenTrayFails_ReturnsFalse()
    {
        var tray = new Mock<ITrayHost>(MockBehavior.Strict);
        tray.Setup(x => x.ShowBalloon(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new InvalidOperationException("headless"));
        using var adapter = new ToastTier3Adapter(tray.Object, new Mock<IAppLogger>(MockBehavior.Loose).Object);

        Assert.IsFalse(adapter.TryDeliver(new ReminderEvent()));
    }

    [TestMethod]
    public void TryDeliver_NullEvent_Throws()
    {
        using var adapter = new ToastTier3Adapter(new Mock<ITrayHost>(MockBehavior.Loose).Object, new Mock<IAppLogger>(MockBehavior.Loose).Object);

        Assert.ThrowsException<ArgumentNullException>(() => adapter.TryDeliver(null!));
    }
}
