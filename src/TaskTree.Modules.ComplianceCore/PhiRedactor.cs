// ─────────────────────────────────────────────────────────────────────────────
//  File: src/TaskTree.Modules.ComplianceCore/PhiRedactor.cs
//  Purpose: PHI redaction pipeline per Architecture §9.2.3 / §10.2; consumed by ComplianceCore.RedactPhi.
//  Architecture.md References: §9.2.3, §10.2, §4.6
//  Roadmap.md References: Phase 1C — ComplianceCore baseline (Msg 2 of 5)
//  D1 anti-drift: header cites Architecture.md sections.
//  D6 anti-drift: no real-looking PHI patterns — test/synthetic sentinels only.
//  D7 anti-drift: support-email allowlist is constructor-injected — never hardcoded (Q11).
//  D10 anti-drift: XML doc on every public member.
//  KNOWLEDGE GAP Q10: the synthetic name sentinel set in this file is test-only
//    and provides ZERO production coverage. Owner must supply real source list
//    via §Governance amendment before Phase 5F sign-off. See HANDOFF.md §Q10.
// ─────────────────────────────────────────────────────────────────────────────
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace TaskTree.Modules.ComplianceCore;

/// <summary>
/// PHI redaction pipeline per Architecture §9.2.3. Applies six redaction
/// patterns in order of specificity to minimize double-redaction. Email
/// redaction consults a constructor-injected allowlist before substituting.
/// </summary>
/// <remarks>
/// SPEC-DERIVED-PHASE1C: pattern set + allowlist surface + null contract not
/// specified verbatim in Architecture.md; derived from §9.2.3 / §10.2 and
/// approved by human owner on 2026-05-26. See
/// docs/spec-derivations/PHASE1C-DERIVATIONS.md §6, §7, §8.
/// <para>
/// Q10 (HANDOFF.md): the synthetic name sentinel set baked into
/// <see cref="NameSentinelRegex"/> is test-only and provides ZERO production
/// coverage. Owner must supply a real source list before Phase 5F sign-off
/// via §Governance amendment.
/// </para>
/// <para>
/// Q11 (HANDOFF.md): the support-email allowlist is constructor-injected
/// (NOT hardcoded per D7). DI default at Phase 1F is an empty collection.
/// </para>
/// </remarks>
public sealed class PhiRedactor
{
    /// <summary>Replacement token for matched US Social Security numbers.</summary>
    public const string SsnReplacement = "[REDACTED:SSN]";

    /// <summary>Replacement token for matched phone numbers.</summary>
    public const string PhoneReplacement = "[REDACTED:PHONE]";

    /// <summary>Replacement token for matched MRN-like 6–10 digit strings.</summary>
    public const string MrnReplacement = "[REDACTED:MRN]";

    /// <summary>Replacement token for matched ISO 8601 dates / datetimes.</summary>
    public const string DateReplacement = "[REDACTED:DATE]";

    /// <summary>Replacement token for matched email addresses (when not allowlisted).</summary>
    public const string EmailReplacement = "[REDACTED:EMAIL]";

    /// <summary>Replacement token for matched names from the (synthetic) name list.</summary>
    public const string NameReplacement = "[REDACTED:NAME]";

    // SSN: 3-2-4 digit blocks separated by hyphens.
    private static readonly Regex SsnRegex = new(
        @"\b\d{3}-\d{2}-\d{4}\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // Phone: optional +1, optional parens, 3-3-4 with hyphen / space / dot separators.
    private static readonly Regex PhoneRegex = new(
        @"\b(?:\+?1[-\s.]?)?\(?\d{3}\)?[-\s.]?\d{3}[-\s.]?\d{4}\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // MRN-like: pure 6-10 digit run with word boundaries. Applied AFTER SSN+Phone so
    // formatted patterns (with separators) are consumed first.
    private static readonly Regex MrnRegex = new(
        @"\b\d{6,10}\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // ISO date / datetime (subset): YYYY-MM-DD with optional Thh:mm:ss[.fff]Z.
    private static readonly Regex IsoDateRegex = new(
        @"\b\d{4}-\d{2}-\d{2}(?:T\d{2}:\d{2}:\d{2}(?:\.\d+)?Z?)?\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // Email: local-part @ domain.tld
    private static readonly Regex EmailRegex = new(
        @"\b[\w.+-]+@[\w-]+\.[\w.-]+\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // Q10 stub: synthetic test-only name sentinels. ZERO production coverage.
    // Owner must replace via §Governance amendment before Phase 5F sign-off.
    // Each entry is deliberately implausible as a real name.
    internal static readonly ImmutableArray<string> SyntheticNameSentinels =
        ImmutableArray.Create(
            "Pat Test", "Sam Sample", "Lee Lorem", "Ipsum Ng",
            "Mock Patient", "Synth User", "Dummy Doe", "Fake Roe",
            "Placeholder Person", "Tester Tester");

    private static readonly Regex NameSentinelRegex = new(
        @"\b(?:" + string.Join("|", SyntheticNameSentinels.Select(Regex.Escape)) + @")\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private readonly HashSet<string> _allowedEmails;

    /// <summary>
    /// Initializes a new <see cref="PhiRedactor"/>. The support-email allowlist
    /// is constructor-injected per Derivation 7 (D7 compliance). DI default in
    /// Phase 1F is an empty collection (Q11 — owner must populate or explicitly
    /// accept the empty default before Phase 5F sign-off).
    /// </summary>
    /// <param name="allowedEmails">
    /// Case-insensitive set of email addresses to leave unredacted.
    /// </param>
    public PhiRedactor(IReadOnlyCollection<string>? allowedEmails = null)
    {
        _allowedEmails = new HashSet<string>(
            allowedEmails ?? Array.Empty<string>(),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns the case-insensitive allowlist used by <see cref="Redact"/>.
    /// </summary>
    public IReadOnlyCollection<string> AllowedEmails => _allowedEmails;

    /// <summary>
    /// Returns a copy of <paramref name="text"/> with all PHI patterns redacted
    /// per the §9.2.3 balanced-strictness policy.
    /// </summary>
    /// <param name="text">
    /// Input text. <c>null</c> returns <see cref="string.Empty"/> per Derivation 8;
    /// <c>""</c> returns <c>""</c>.
    /// </param>
    /// <returns>The redacted text; never <c>null</c>.</returns>
    public string Redact(string? text)
    {
        if (text is null) return string.Empty;
        if (text.Length == 0) return string.Empty;

        // Apply in order of specificity. SSN first (3-2-4 with hyphens) so its
        // digits are not consumed by the looser MRN pattern. Phone next (uses
        // separators) for the same reason. MRN catches remaining bare digit runs.
        // Date next (YYYY-MM-DD) — its hyphen-3 form is bounded and disjoint from
        // SSN's 3-2-4. Email next (allowlist-aware). Names last.
        string result = text;
        result = SsnRegex.Replace(result, SsnReplacement);
        result = PhoneRegex.Replace(result, PhoneReplacement);
        result = MrnRegex.Replace(result, MrnReplacement);
        result = IsoDateRegex.Replace(result, DateReplacement);
        result = EmailRegex.Replace(result, ReplaceEmailIfNotAllowed);
        result = NameSentinelRegex.Replace(result, NameReplacement);
        return result;
    }

    private string ReplaceEmailIfNotAllowed(Match match)
    {
        // Allowlist is case-insensitive per HashSet construction; pass through
        // unchanged if matched, otherwise redact.
        return _allowedEmails.Contains(match.Value) ? match.Value : EmailReplacement;
    }
}
