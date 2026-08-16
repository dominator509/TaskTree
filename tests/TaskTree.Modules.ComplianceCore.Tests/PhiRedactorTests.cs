// ─────────────────────────────────────────────────────────────────────────────
//  File: tests/TaskTree.Modules.ComplianceCore.Tests/PhiRedactorTests.cs
//  Purpose: 6 unit tests for PhiRedactor per Architecture §9.2.3 and Roadmap P1C-AC3 + P1C-AC4.
//  Architecture.md References: §9.2.3, §10.2
//  Roadmap.md References: Phase 1C — Msg 4 of 5 (tests)
//  D1 anti-drift: header cites Architecture.md sections.
//  D6 anti-drift: all test data is synthetic, non-PHI-shaped (no real names,
//    no real SSNs/MRNs/phones/emails — every pattern is obviously test data).
//  D10 anti-drift: XML doc on every test class.
// ─────────────────────────────────────────────────────────────────────────────
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.Modules.ComplianceCore;

namespace TaskTree.Modules.ComplianceCore.Tests;

/// <summary>
/// Verifies <see cref="PhiRedactor"/> pattern set (§9.2.3) and null/empty
/// contract (P1C-AC4).
/// </summary>
/// <remarks>
/// SPEC-DERIVED-PHASE1C: this test class verifies derivations §6
/// (regex policy), §7 (support-email allowlist), and §8 (null/empty
/// contract) from PHASE1C-DERIVATIONS.md.
/// </remarks>
[TestClass]
public sealed class PhiRedactorTests
{
    /// <summary>P1C-AC4: Redact never throws on null/empty; null → "", "" → "".</summary>
    [TestMethod]
    public void Redact_NullAndEmpty_ReturnsEmpty()
    {
        var r = new PhiRedactor();

        Assert.AreEqual(string.Empty, r.Redact(null));
        Assert.AreEqual(string.Empty, r.Redact(""));
    }

    /// <summary>P1C-AC3: SSN-shaped digits are replaced with the SSN replacement token.</summary>
    [TestMethod]
    public void Redact_Ssn_IsReplaced()
    {
        var r = new PhiRedactor();
        string input = "Synthetic record: SSN 123-45-6789 noted.";

        string result = r.Redact(input);

        StringAssert.Contains(result, PhiRedactor.SsnReplacement);
        Assert.IsFalse(result.Contains("123-45-6789"));
    }

    /// <summary>P1C-AC3: phone-shaped digits are replaced with the phone replacement token.</summary>
    [TestMethod]
    public void Redact_Phone_IsReplaced()
    {
        var r = new PhiRedactor();
        string input = "Call synthetic line 555-123-4567 today.";

        string result = r.Redact(input);

        StringAssert.Contains(result, PhiRedactor.PhoneReplacement);
        Assert.IsFalse(result.Contains("555-123-4567"));
    }

    /// <summary>P1C-AC3: MRN-like 6–10 digit runs are replaced with the MRN replacement token.</summary>
    [TestMethod]
    public void Redact_Mrn_IsReplaced()
    {
        var r = new PhiRedactor();
        string input = "Synthetic MRN reference 1234567 in note.";

        string result = r.Redact(input);

        StringAssert.Contains(result, PhiRedactor.MrnReplacement);
        Assert.IsFalse(result.Contains("1234567"));
    }

    /// <summary>P1C-AC3: ISO 8601 dates are replaced with the date replacement token.</summary>
    [TestMethod]
    public void Redact_IsoDate_IsReplaced()
    {
        var r = new PhiRedactor();
        string input = "Synthetic visit on 2025-06-15 recorded.";

        string result = r.Redact(input);

        StringAssert.Contains(result, PhiRedactor.DateReplacement);
        Assert.IsFalse(result.Contains("2025-06-15"));
    }

    /// <summary>
    /// P1C-AC3 + Derivation 7: emails are redacted by default; allowlisted
    /// addresses are preserved case-insensitively.
    /// </summary>
    [TestMethod]
    public void Redact_Email_RespectsAllowlist()
    {
        // No allowlist: both emails redacted.
        var noAllowlist = new PhiRedactor();
        string input1 = "Synthetic: support@example.com and other@example.com";
        string result1 = noAllowlist.Redact(input1);
        StringAssert.Contains(result1, PhiRedactor.EmailReplacement);
        Assert.IsFalse(result1.Contains("support@example.com"));
        Assert.IsFalse(result1.Contains("other@example.com"));

        // With allowlist: support@ passes through; other@ redacted.
        var withAllowlist = new PhiRedactor(new[] { "support@example.com" });
        string input2 = "Synthetic: SUPPORT@Example.com and other@example.com";
        string result2 = withAllowlist.Redact(input2);
        StringAssert.Contains(result2, "SUPPORT@Example.com");   // case-insensitive match preserves original casing
        Assert.IsFalse(result2.Contains("other@example.com"));
        StringAssert.Contains(result2, PhiRedactor.EmailReplacement);
    }

    /// <summary>Q11 boundary: callers cannot mutate the redaction allowlist through the read-only view.</summary>
    [TestMethod]
    public void AllowedEmails_DoesNotExposeMutableBackingSet()
    {
        var redactor = new PhiRedactor(new[] { "support@example.com" });

        if (redactor.AllowedEmails is System.Collections.Generic.ICollection<string> exposed && !exposed.IsReadOnly)
            exposed.Add("other@example.com");

        var result = redactor.Redact("Synthetic: other@example.com");

        StringAssert.Contains(result, PhiRedactor.EmailReplacement);
        Assert.IsFalse(result.Contains("other@example.com"));
    }
}
