# TaskTree User Guide

> Phase 4D documentation scaffold. Status: release-prep documentation only. Final wording and screenshots must be reconciled against the stitched UI in Phase 5D/5F.

## Overview

TaskTree is a local Windows task reminder application designed for privacy-aware task management. It supports task creation, reminder delivery, snooze behavior, session lock/privacy behavior, settings, updates, and local bug reporting.

TaskTree is designed as a local HIPAA-aware tool, but final compliance depends on deployment configuration, validation, and organizational policy.

## Core Workflows

### Creating Tasks

Use the task entry workflow to create a task with a title, optional due/reminder information, and priority/order information. Avoid entering unnecessary sensitive details in task titles or notes.

Safe generic examples:

- Review task
- Call pharmacy
- Follow up on item
- Prepare handoff

Do not use sample patient names, MRNs, DOBs, phone numbers, addresses, emails, or patient-like details in examples, screenshots, demonstrations, or tests.

### Editing, Completing, and Deleting Tasks

Open an existing task to update task details. Use complete when a task is finished and delete only when a task should be removed. Audit behavior and retention behavior must be verified during Phase 5 validation.

### Reminder and Snooze Behavior

TaskTree reminder behavior is designed around local reminder delivery tiers. Snooze allows a reminder to be temporarily deferred. Notification, tray, WPF, and snooze audit behavior must be validated in the stitched app.

### Session Lock and Privacy Mode

Session lock/privacy mode is designed to protect local task visibility when the user is idle or locked. Real Windows idle detection and re-authentication must be validated in Phase 5E.

### Settings

Settings control reminder preferences, privacy behavior, update settings, and compliance-related defaults. Compliance Mode semantics must be confirmed during Phase 4A/5F follow-up.

### Bug Report Submission

Bug reports are designed to be redacted before queueing or delivery. Local file-drop delivery rejects reports that are not marked redacted. Email and GitHub delivery are stubs until configured and validated in Phase 5E.

### Updates

TaskTree includes scaffolding for update manifest parsing, signature verification, package hash verification, staging, offline import, and rollback planning. Live update check/download/apply behavior requires Phase 5E validation.

## Known Limitations

- Live updater download/apply is not validated in generated documentation.
- Live SMTP/GitHub delivery is not active until Phase 5E.
- MSIX packaging/signing/install requires live Windows tooling.
- Tray/UI latency, idle RAM, CPU, and first-launch behavior require installed-app validation.
- Final compliance status requires organizational policy review and Phase 5F sign-off.

## Safety and Privacy Notes

- Treat task content as potentially sensitive.
- Avoid unnecessary sensitive details in task text.
- Do not include patient-like data in screenshots, demos, tests, or sample docs.
- Review `docs/compliance-audit.md` before release.

## Claude/Codex Handoff Gap

Gap #336: User guide must be reviewed against final stitched UI and actual feature behavior in Phase 5D/5F.
