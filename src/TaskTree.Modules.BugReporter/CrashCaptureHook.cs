// SPEC-DERIVED-PHASE3D  HALT #19/#20
// Architecture.md Section 9.2.2 crash capture policy.
// Gap #257/#259: real crash injection and full crash-capture validation deferred to Phase 5E.

using System;

namespace TaskTree.Modules.BugReporter
{
    /// <summary>Registers global crash capture hooks in an idempotent manner.</summary>
    public sealed class CrashCaptureHook
    {
        private bool _hooked;
        public event EventHandler<Exception>? CrashCaptured;
        public void HookGlobalCrashHandler()
        {
            if (_hooked) return;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            _hooked = true;
        }
        internal void RaiseForTests(Exception ex) => CrashCaptured?.Invoke(this, ex);
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex) CrashCaptured?.Invoke(this, ex);
        }
    }
}
