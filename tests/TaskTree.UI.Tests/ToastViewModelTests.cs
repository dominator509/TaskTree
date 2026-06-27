// SPEC-DERIVED-PHASE2B  HALT #19
// SPEC-DERIVED-PHASE2D  HALT #15 provider brush semantics (Gap #145)

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.Core.Enums;
using TaskTree.Core.Models;
using TaskTree.UI.Themes;
using TaskTree.UI.ViewModels;

namespace TaskTree.UI.Tests
{
    [TestClass]
    public class ToastViewModelTests
    {
        private static ReminderEvent MakeEvent(Priority p, ReminderReason r) => new()
        { TaskId = Guid.NewGuid(), FiredAtUtc = new DateTimeOffset(2026,6,1,12,0,0,TimeSpan.Zero), Priority = p, Reason = r };
        [TestMethod] public void Constructor_DefaultsToEmpty(){var vm=new ToastViewModel();Assert.AreEqual(string.Empty,vm.Title);Assert.AreEqual(string.Empty,vm.Message);Assert.AreEqual(string.Empty,vm.ReasonLabel);Assert.AreEqual(PriorityBrushProvider.TrivialBrush,vm.PriorityColor);}
        [TestMethod] public void UpdateContent_Critical_UsesProviderBrush(){var vm=new ToastViewModel();vm.UpdateContent(MakeEvent(Priority.Critical,ReminderReason.Initial));Assert.AreEqual(PriorityBrushProvider.GetBrush(Priority.Critical),vm.PriorityColor);}
        [TestMethod] public void UpdateContent_Trivial_OverdueAtDeadline_SoftenedTitle(){var vm=new ToastViewModel();vm.UpdateContent(MakeEvent(Priority.Trivial,ReminderReason.Overdue));Assert.AreEqual("Trivial task - review when ready",vm.Title);Assert.AreEqual(string.Empty,vm.ReasonLabel);}
        [TestMethod] public void UpdateContent_NormalReason_TitleEndsWithTaskDue(){var vm=new ToastViewModel();vm.UpdateContent(MakeEvent(Priority.Normal,ReminderReason.Initial));StringAssert.EndsWith(vm.Title,"task due");}
        [TestMethod] public void UpdateContent_OverdueReason_TitleEndsWithTaskOverdue(){var vm=new ToastViewModel();vm.UpdateContent(MakeEvent(Priority.High,ReminderReason.Overdue));StringAssert.EndsWith(vm.Title,"task overdue");}
        [TestMethod] public void UpdateContent_ReasonLabel_FormatsCorrectly(){var vm=new ToastViewModel();vm.UpdateContent(MakeEvent(Priority.Normal,ReminderReason.Initial));Assert.AreEqual("(Initial)",vm.ReasonLabel);}
    }
}
