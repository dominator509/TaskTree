// SPEC-DERIVED-PHASE2D  HALT #14 (PriorityBrushProvider color tests)

using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TaskTree.Core.Enums;
using TaskTree.UI.Themes;

namespace TaskTree.UI.Tests
{
    [TestClass]
    public class PriorityBrushProviderTests
    {
        [TestMethod] public void GetBrush_Critical_ReturnsCriticalColor()=>AssertColor(Priority.Critical, 0xD1, 0x34, 0x38);
        [TestMethod] public void GetBrush_High_ReturnsHighColor()=>AssertColor(Priority.High, 0xFF, 0x8C, 0x00);
        [TestMethod] public void GetBrush_Normal_ReturnsNormalColor()=>AssertColor(Priority.Normal, 0xFF, 0xD7, 0x00);
        [TestMethod] public void GetBrush_Low_ReturnsLowColor()=>AssertColor(Priority.Low, 0x4F, 0x9D, 0xE8);
        [TestMethod] public void GetBrush_Trivial_ReturnsTrivialColor()=>AssertColor(Priority.Trivial, 0x8A, 0x88, 0x86);
        private static void AssertColor(Priority priority, byte r, byte g, byte b)
        {
            var brush = (SolidColorBrush)PriorityBrushProvider.GetBrush(priority);
            Assert.AreEqual(Color.FromRgb(r,g,b), brush.Color);
        }
    }
}
