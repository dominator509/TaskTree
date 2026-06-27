// ─────────────────────────────────────────────────────────────────────────────
//  File: tests/TaskTree.Core.Tests/Logging/FileAppLoggerTests.cs
//  Purpose: Verifies FileAppLogger writes one valid JSON line per call per Roadmap P0-AC5.
//  Architecture.md References: §3.3, §10.5, P0-AC5
//  Roadmap.md References: Phase 0 — Project Scaffold (Msg 6 — Test Skeletons + 5 Primitive Tests)
//  D1: header cites Architecture.md sections.
//  D6: all test data is synthetic, non-PHI-shaped.
//  D10: XML doc on every test class.
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.Core.Logging;

namespace TaskTree.Core.Tests.Logging;

/// <summary>
/// Verifies the JSON-lines output of <see cref="FileAppLogger"/>.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-MSG6: test naming convention (MethodName_Scenario_ExpectedOutcome)
/// and test category vocabulary (Offline default / Live / Integration) not
/// specified verbatim in Architecture.md; derived from documented usage and
/// approved by human owner on 2026-05-26. See
/// docs/spec-derivations/PHASE0-MSG6-DERIVATIONS.md.
/// </remarks>
[TestClass]
public sealed class FileAppLoggerTests
{
    /// <summary>LogInformation appends one JSON line with timestamp, level, and message fields (P0-AC5).</summary>
    [TestMethod]
    public void LogInformation_WritesValidJsonLine()
    {
        var tmpDir = Path.Combine(Path.GetTempPath(), "TaskTreeTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmpDir);
        try
        {
            var logger = new FileAppLogger(tmpDir);
            logger.LogInformation("synthetic-test-message-msg6");

            var lines = File.ReadAllLines(logger.LogFilePath);
            Assert.AreEqual(1, lines.Length);

            using var doc = JsonDocument.Parse(lines[0]);
            var root = doc.RootElement;
            Assert.AreEqual("information", root.GetProperty("Level").GetString());
            Assert.AreEqual("synthetic-test-message-msg6", root.GetProperty("Message").GetString());
            Assert.IsTrue(root.TryGetProperty("Timestamp", out _));
        }
        finally
        {
            if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, recursive: true);
        }
    }
}
