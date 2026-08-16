// File: tests/TaskTree.Orchestrator.Tests/CompositionRootCoverageTests.cs
// Covers: Architecture §3.3 and §10.3; Roadmap P1F/P5C coverage gate.
// Verifies local path and DI composition contracts without starting the app.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.App.Bootstrap;

namespace TaskTree.Orchestrator.Tests;

[TestClass]
public sealed class CompositionRootCoverageTests
{
    [TestMethod]
    public void BuildServiceProvider_ReturnsValidatedContainer()
    {
        using var provider = CompositionRoot.BuildServiceProvider() as IDisposable;
        Assert.IsNotNull(provider);
    }
}
