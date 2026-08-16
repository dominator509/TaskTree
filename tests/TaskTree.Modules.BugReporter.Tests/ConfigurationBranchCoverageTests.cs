// File: tests/TaskTree.Modules.BugReporter.Tests/ConfigurationBranchCoverageTests.cs
// Covers: Architecture §9.2.6; Roadmap P5C coverage gate.
// Configuration is synthetic and restored after every test.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.Modules.BugReporter;

namespace TaskTree.Modules.BugReporter.Tests;

[TestClass]
public sealed class ConfigurationBranchCoverageTests
{
    private static BugReport Report(bool redacted = true) => new(
        Guid.NewGuid(),
        new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero),
        BugReportType.UserSubmitted,
        BugSeverity.Critical,
        "Synthetic report",
        new BugReportDescription("Expected", "Actual"),
        new BugReportEnvironment("Synthetic OS", "1.0.0", "test", UpdateChannel.Stable),
        Guid.NewGuid(),
        new string('A', 64),
        Array.Empty<BugReportAttachment>(),
        redacted);

    [TestMethod]
    public async Task Email_UnredactedReport_FailsBeforeConfiguration()
    {
        var result = await new EmailDeliveryAdapter().DeliverAsync(Report(redacted: false));
        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.Message, "redacted");
    }

    [TestMethod]
    public async Task Email_InvalidPort_FailsClosed()
    {
        using var env = new EnvironmentScope(
            ("TASKTREE_SMTP_HOST", "127.0.0.1"),
            ("TASKTREE_SMTP_FROM", "from@example.invalid"),
            ("TASKTREE_SMTP_TO", "to@example.invalid"),
            ("TASKTREE_SMTP_PORT", "0"),
            ("TASKTREE_SMTP_USERNAME", null),
            ("TASKTREE_SMTP_PASSWORD", null));
        var result = await new EmailDeliveryAdapter().DeliverAsync(Report());
        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.Message, "port");
    }

    [TestMethod]
    public async Task Email_IncompleteCredentials_FailsClosed()
    {
        using var env = new EnvironmentScope(
            ("TASKTREE_SMTP_HOST", "127.0.0.1"),
            ("TASKTREE_SMTP_FROM", "from@example.invalid"),
            ("TASKTREE_SMTP_TO", "to@example.invalid"),
            ("TASKTREE_SMTP_PORT", "587"),
            ("TASKTREE_SMTP_USERNAME", "synthetic-user"),
            ("TASKTREE_SMTP_PASSWORD", null));
        var result = await new EmailDeliveryAdapter().DeliverAsync(Report());
        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.Message, "credentials");
    }

    [TestMethod]
    public async Task Email_DisabledTls_FailsClosed()
    {
        using var env = new EnvironmentScope(
            ("TASKTREE_SMTP_HOST", "127.0.0.1"),
            ("TASKTREE_SMTP_FROM", "from@example.invalid"),
            ("TASKTREE_SMTP_TO", "to@example.invalid"),
            ("TASKTREE_SMTP_PORT", "1"),
            ("TASKTREE_SMTP_TLS", "false"),
            ("TASKTREE_SMTP_USERNAME", null),
            ("TASKTREE_SMTP_PASSWORD", null));
        var result = await new EmailDeliveryAdapter().DeliverAsync(Report());
        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.Message, "TLS");
    }

    [TestMethod]
    public async Task GitHub_UnredactedReport_FailsBeforeConfiguration()
    {
        var result = await new GitHubIssueAdapter().DeliverAsync(Report(redacted: false));
        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.Message, "redacted");
    }

    [TestMethod]
    public async Task GitHub_InvalidRepositoryShape_FailsClosed()
    {
        using var env = new EnvironmentScope(
            ("TASKTREE_GITHUB_REPOSITORY", "invalid-repository"),
            ("TASKTREE_GITHUB_TOKEN", "synthetic-token"));
        var result = await new GitHubIssueAdapter().DeliverAsync(Report());
        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.Message, "owner/name");
    }

    [TestMethod]
    public async Task GitHub_UnsafeRepositorySegment_FailsClosed()
    {
        using var env = new EnvironmentScope(
            ("TASKTREE_GITHUB_REPOSITORY", "owner/repo?redirect"),
            ("TASKTREE_GITHUB_TOKEN", "synthetic-token"));
        var result = await new GitHubIssueAdapter().DeliverAsync(Report());
        Assert.IsFalse(result.Success);
        StringAssert.Contains(result.Message, "owner/name");
    }

    [TestMethod]
    public void GitHub_Labels_CoverSeverityMapping()
    {
        var adapter = new GitHubIssueAdapter();
        Assert.AreEqual("critical", adapter.GetLabel(BugSeverity.Critical));
        Assert.AreEqual("high", adapter.GetLabel(BugSeverity.High));
        Assert.AreEqual("bug", adapter.GetLabel(BugSeverity.Normal));
        Assert.AreEqual("enhancement", adapter.GetLabel(BugSeverity.Low));
        Assert.AreEqual("bug", adapter.GetLabel(BugSeverity.Trivial));
    }

    private sealed class EnvironmentScope : IDisposable
    {
        private readonly Dictionary<string, string?> _previous = new(StringComparer.Ordinal);

        public EnvironmentScope(params (string Name, string? Value)[] values)
        {
            foreach (var (name, value) in values)
            {
                _previous[name] = Environment.GetEnvironmentVariable(name);
                Environment.SetEnvironmentVariable(name, value);
            }
        }

        public void Dispose()
        {
            foreach (var entry in _previous)
                Environment.SetEnvironmentVariable(entry.Key, entry.Value);
        }
    }
}
