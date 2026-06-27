// SPEC-DERIVED-PHASE1A  TaskNode baseline
// SPEC-DERIVED-PHASE2C  HALT #1/#8 (TaskMetadata? Metadata additive model patch)
// Gap #127/#131: Phase 5B/5C must verify serialization compatibility and downstream tests.

using System;
using System.Collections.Generic;
using TaskTree.Core.Enums;

namespace TaskTree.Core.Models
{
    /// <summary>Core task model. Phase 2C adds nullable Metadata as additive JSON-safe field.</summary>
    public sealed class TaskNode
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public Priority Priority { get; set; } = Priority.Normal;
        public TaskStatus Status { get; set; } = TaskStatus.Open;
        public DateTimeOffset? Deadline { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ModifiedAt { get; set; }
        public List<TaskNode> Children { get; set; } = new();

        /// <summary>Optional PHI-minimal operational metadata. Older serialized tasks deserialize with null.</summary>
        public TaskMetadata? Metadata { get; set; }
    }
}
