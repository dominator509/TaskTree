# PHASE3-GAP-MANIFEST.md

> Consolidated Phase 3 gap manifest for Codex/Claude handoff. Covers gaps #209-#281.

## AutoUpdater Key / Signature / Canonicalization

- #209: Verify/patch UpdateManifest schema.
- #210: Nested manifest DTOs are derived.
- #211: Exact manifest JSON casing tests needed.
- #212/#226: Version comparison uses System.Version; SemVer unsupported.
- #213: NSec.Cryptography package version resolution must be verified.
- #214: Real Ed25519 public key required.
- #215: Canonical JSON excluding signature needs owner/Phase 5C vector approval.
- #216: Hash casing upper/lower accepted; confirm compatibility.
- #221: Deterministic Ed25519 positive test vector required.

## AutoUpdater State / Staging / Offline Import / Rollback

- #217/#228/#230: Re-check size/hash and validate staging path/tamper behavior.
- #218/#231/#279: Check/download/apply live flow deferred to Phase 5E.
- #222-#225: UpdaterState enum/state machine/public surface/transition graph require Architecture and Phase 5C validation.
- #227: Rollout bucket derivation undefined.
- #232-#237: Offline import ZIP format/result/entries/constructor/state-path derived.
- #238-#240: Sentinel path, surface, and content format derived.
- #241-#243: Live rollback restore, rollback directory, and selection rule require Phase 5E/Architecture review.

## BugReporter Schema / Redaction / Queue / Capture

- #244-#246: BugReport schema, BugReportType enum, and exact JSON schema tests.
- #247: Validate no unredacted free text is persisted.
- #248-#254: Null normalization, queue key/surface, dedup, dependencies, normalization, and fingerprint formula derived.
- #255: FlushQueueAsync delivery was deferred until 3E and now partially implemented with live stubs retained.
- #256: RedactionEnabled=false still redacts; Phase 4A confirmation required.
- #257/#259: Real crash injection validation deferred to Phase 5E.
- #258: Default crash severity High requires Phase 4A confirmation.
- #260: BugReporter DI registration deferred.

## BugReporter Delivery / Live Adapters / Rate Limiting

- #261/#262: Delivery result and adapter interface are derived.
- #263/#264: Live SMTP and DPAPI SMTP config deferred to Phase 5E.
- #265/#266: Live GitHub Issues and label validation deferred to Phase 5E.
- #267-#269: FileDrop path/JSON/unredacted-write validation required.
- #270/#271: Rate limiter in-memory and outbound/filedrop semantics require review.
- #272: BugSeverity enum values must be reconciled in Phase 5B.
- #273/#276: Partial retry semantics and queue flush behavior need live validation.
- #274: Email/GitHub stubs must be replaced in Phase 5E.
- #275: DeliveryRouter constructor changes require DI/factory updates.

## Phase 3F / Gate Gaps

- #277: Phase 3F does not patch production code unless Phase 5B/5C identifies required fixes.
- #278: Phase 3 owner approval gate required before Phase 4.
- #280: Full Phase 1/2 regression suite must run during Phase 5C.
- #281: Phase 3 completion summary should be reviewed during Phase 5A stitching.
