// ============================================================================
// File: tests/TaskTree.Modules.ReminderScheduler.Tests/CadencePolicyTests.cs
// Covers: Architecture §5.3; Roadmap P1D-AC2
// SPEC-DERIVED-PHASE1D-MSG2  HALT-Msg2 #1/#3/#9
// Test count: 10. Gaps covered: #41/#42/#43/#50/#51/#55.
// ============================================================================

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;

namespace TaskTree.Modules.ReminderScheduler.Tests
{
    [TestClass]
    public class CadencePolicyTests
    {
        private static readonly DateTimeOffset Now = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);

        private static TaskNode Node(Priority p, DateTimeOffset? deadline = null, TaskStatus status = TaskStatus.Open)
            => new()
            {
                Id = Guid.NewGuid(),
                Title = "synthetic",
                Priority = p,
                Deadline = deadline,
                Status = status,
                CreatedAt = Now,
                ModifiedAt = Now,
            };

        [DataTestMethod, TestCategory("Offline")]
        [DataRow(Priority.Critical, 5)]
        [DataRow(Priority.High, 15)]
        [DataRow(Priority.Normal, 30)]
        [DataRow(Priority.Low, 120)]
        [DataRow(Priority.Trivial, 480)]
        public void GetRepeatCadence_AllPriorities_Matches53Table(Priority p, int expectedMinutes)
        {
            Assert.AreEqual(TimeSpan.FromMinutes(expectedMinutes), CadencePolicy.GetRepeatCadence(p));
        }

        [DataTestMethod, TestCategory("Offline")]
        [DataRow(Priority.Critical, 0)]
        [DataRow(Priority.High, 30)]
        [DataRow(Priority.Normal, 60)]
        [DataRow(Priority.Low, 240)]
        [DataRow(Priority.Trivial, 0)]
        public void GetInitialOffsetBeforeDeadline_AllPriorities_Matches53Table(Priority p, int expectedMinutes)
        {
            Assert.AreEqual(TimeSpan.FromMinutes(expectedMinutes), CadencePolicy.GetInitialOffsetBeforeDeadline(p));
        }

        [TestMethod, TestCategory("Offline")]
        public void ShouldFire_NullNode_ReturnsFalse()
        {
            var fired = CadencePolicy.ShouldFire(null!, lastFiredUtc: null, nowUtc: Now, out var reason);
            Assert.IsFalse(fired);
            Assert.AreEqual(ReminderReason.Initial, reason);
        }

        [TestMethod, TestCategory("Offline")]
        public void ShouldFire_NullDeadline_Critical_FiresInitialThenRepeat()
        {
            var node = Node(Priority.Critical);
            Assert.IsTrue(CadencePolicy.ShouldFire(node, null, Now, out var r1));
            Assert.AreEqual(ReminderReason.Initial, r1);
            Assert.IsFalse(CadencePolicy.ShouldFire(node, Now, Now.AddMinutes(4), out _));
            Assert.IsTrue(CadencePolicy.ShouldFire(node, Now, Now.AddMinutes(5), out var r3));
            Assert.AreEqual(ReminderReason.Repeat, r3);
        }

        [DataTestMethod, TestCategory("Offline")]
        [DataRow(Priority.High)]
        [DataRow(Priority.Normal)]
        [DataRow(Priority.Low)]
        [DataRow(Priority.Trivial)]
        public void ShouldFire_NullDeadline_NonCritical_Skips(Priority p)
        {
            var node = Node(p);
            Assert.IsFalse(CadencePolicy.ShouldFire(node, null, Now, out _));
            Assert.IsFalse(CadencePolicy.ShouldFire(node, Now.AddDays(-7), Now, out _));
        }

        [DataTestMethod, TestCategory("Offline")]
        [DataRow(Priority.High, 30)]
        [DataRow(Priority.Normal, 60)]
        [DataRow(Priority.Low, 240)]
        public void ShouldFire_InitialBoundary_ExactOffset_Fires(Priority p, int offsetMinutes)
        {
            var nodeAt = Node(p, deadline: Now.AddMinutes(offsetMinutes));
            Assert.IsTrue(CadencePolicy.ShouldFire(nodeAt, null, Now, out var reason));
            Assert.AreEqual(ReminderReason.Initial, reason);
            var nodeOutside = Node(p, deadline: Now.AddMinutes(offsetMinutes).AddSeconds(1));
            Assert.IsFalse(CadencePolicy.ShouldFire(nodeOutside, null, Now, out _));
        }

        [TestMethod, TestCategory("Offline")]
        public void ShouldFire_Trivial_AtDeadlineExactly_FiresOverdue()
        {
            // Gap #43 / #50 — Trivial at deadline collapses to Overdue (documented semantic)
            var node = Node(Priority.Trivial, deadline: Now);
            Assert.IsTrue(CadencePolicy.ShouldFire(node, null, Now, out var reason));
            Assert.AreEqual(ReminderReason.Overdue, reason);
        }

        [TestMethod, TestCategory("Offline")]
        public void ShouldFire_PrecedenceCollision_OverdueWinsOverRepeat()
        {
            // Item #9 / Gap #42 — both Overdue + Repeat eligible; precedence demands Overdue
            var node = Node(Priority.High, deadline: Now.AddHours(-1));
            Assert.IsTrue(CadencePolicy.ShouldFire(node, Now.AddMinutes(-20), Now, out var reason));
            Assert.AreEqual(ReminderReason.Overdue, reason);
        }

        [TestMethod, TestCategory("Offline")]
        public void ShouldFire_RepeatCadenceNotElapsed_ReturnsFalse()
        {
            var node = Node(Priority.High, deadline: Now.AddHours(-1));
            Assert.IsFalse(CadencePolicy.ShouldFire(node, Now.AddMinutes(-5), Now, out _));
        }

        [TestMethod, TestCategory("Offline")]
        public void ShouldFire_PreDeadlineRepeat_LowPriority_RespectsLadder()
        {
            var node = Node(Priority.Low, deadline: Now.AddHours(2));
            Assert.IsFalse(CadencePolicy.ShouldFire(node, Now.AddHours(-1), Now, out _));
            Assert.IsTrue(CadencePolicy.ShouldFire(node, Now.AddHours(-3), Now, out var reason));
            Assert.AreEqual(ReminderReason.Repeat, reason);
        }
    }
}
