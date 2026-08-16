using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskTree.Core.Abstractions;
using TaskTree.Modules.BugReporter;
using TaskTree.TestSupport;

namespace TaskTree.Modules.BugReporter.Tests;

[TestClass]
public sealed class BugReporterLifecycleTests
{
    [TestMethod]
    public async Task HookGlobalCrashHandler_RepeatedRegistration_SubmitsOnce()
    {
        var queue = new BugReportQueue(new InMemorySecureStore());
        var compliance = new Mock<IComplianceCore>();
        compliance.Setup(c => c.RedactPhi(It.IsAny<string>())).Returns<string>(value => value);
        var hook = new CrashCaptureHook();
        var reporter = new BugReporter(
            queue,
            new RedactionPipeline(compliance.Object),
            hook,
            new FakeClock(new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero)),
            new Mock<IAppLogger>().Object);

        reporter.HookGlobalCrashHandler();
        reporter.HookGlobalCrashHandler();
        hook.RaiseForTests(new InvalidOperationException("synthetic crash"));

        for (var attempt = 0; attempt < 20; attempt++)
        {
            if ((await queue.GetAllAsync()).Count == 1) break;
            await Task.Delay(10);
        }

        Assert.AreEqual(1, (await queue.GetAllAsync()).Count);
    }
}
