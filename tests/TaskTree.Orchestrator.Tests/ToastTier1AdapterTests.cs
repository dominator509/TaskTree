using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Models;
using TaskTree.Orchestrator;

namespace TaskTree.Orchestrator.Tests;

[TestClass]
public sealed class ToastTier1AdapterTests
{
    [TestMethod]
    public void TryDeliver_WithoutPackageIdentity_ReturnsFalse()
    {
        using var adapter = new ToastTier1Adapter(new Mock<IAppLogger>(MockBehavior.Loose).Object);

        Assert.IsFalse(adapter.TryDeliver(new ReminderEvent()));
    }

    [TestMethod]
    public void TryDeliver_NullEvent_Throws()
    {
        using var adapter = new ToastTier1Adapter(new Mock<IAppLogger>(MockBehavior.Loose).Object);

        Assert.ThrowsException<ArgumentNullException>(() => adapter.TryDeliver(null!));
    }

    [TestMethod]
    public void TryDeliver_AfterDispose_ThrowsObjectDisposedException()
    {
        var adapter = new ToastTier1Adapter(new Mock<IAppLogger>(MockBehavior.Loose).Object);
        adapter.Dispose();

        Assert.ThrowsException<ObjectDisposedException>(() => adapter.TryDeliver(new ReminderEvent()));
    }
}
