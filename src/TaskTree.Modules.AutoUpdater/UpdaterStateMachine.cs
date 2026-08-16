// SPEC-DERIVED-PHASE3B  HALT #2/#3/#4/#5/#6/#7/#8
// Architecture.md Section 9.1.1 updater state-machine flow.
// Gap #223/#224/#225: public surface/event args/transition graph require Phase 5C validation and Architecture v1.0.3 documentation.

using System;
using TaskTree.Core.Abstractions;
using TaskTree.Core.Enums;

namespace TaskTree.Modules.AutoUpdater
{
    /// <summary>Deterministic updater state machine implementing Architecture.md Section 9.1.1 states.</summary>
    public sealed class UpdaterStateMachine
    {
        private readonly IClock _clock;
        private readonly object _gate = new();
        private UpdaterState _current = UpdaterState.Idle;
        public event EventHandler<UpdaterStateChangedEventArgs>? StateChanged;
        public UpdaterState Current { get { lock (_gate) return _current; } }
        public UpdaterStateMachine(IClock clock) => _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        public void TransitionTo(UpdaterState next)
        {
            UpdaterState previous;
            lock (_gate)
            {
                if (next == _current) return;
                if (!IsValidTransition(_current, next)) throw new InvalidOperationException($"Invalid updater transition: {_current} -> {next}.");
                previous = _current;
                _current = next;
            }
            StateChanged?.Invoke(this, new UpdaterStateChangedEventArgs(previous, next, _clock.UtcNow));
        }
        public void Reset()
        {
            UpdaterState previous;
            lock (_gate)
            {
                if (_current == UpdaterState.Idle) return;
                previous = _current;
                _current = UpdaterState.Idle;
            }
            StateChanged?.Invoke(this, new UpdaterStateChangedEventArgs(previous, UpdaterState.Idle, _clock.UtcNow));
        }
        private static bool IsValidTransition(UpdaterState current, UpdaterState next) => (current, next) switch
        {
            (UpdaterState.Idle, UpdaterState.Checking) => true,
            (UpdaterState.Checking, UpdaterState.Idle) => true,
            (UpdaterState.Checking, UpdaterState.Downloading) => true,
            (UpdaterState.Checking, UpdaterState.Failed) => true,
            (UpdaterState.Downloading, UpdaterState.Verifying) => true,
            (UpdaterState.Downloading, UpdaterState.Failed) => true,
            (UpdaterState.Verifying, UpdaterState.Staging) => true,
            (UpdaterState.Verifying, UpdaterState.Failed) => true,
            (UpdaterState.Staging, UpdaterState.Applying) => true,
            (UpdaterState.Staging, UpdaterState.Failed) => true,
            (UpdaterState.Applying, UpdaterState.Applied) => true,
            (UpdaterState.Applying, UpdaterState.Failed) => true,
            (UpdaterState.Failed, UpdaterState.Idle) => true,
            (UpdaterState.Applied, UpdaterState.Idle) => true,
            _ => false,
        };
    }
    /// <summary>Updater state transition event data.</summary>
    public sealed class UpdaterStateChangedEventArgs : EventArgs
    {
        public UpdaterStateChangedEventArgs(UpdaterState previous, UpdaterState current, DateTimeOffset changedAtUtc)
        { Previous = previous; Current = current; ChangedAtUtc = changedAtUtc; }
        public UpdaterState Previous { get; }
        public UpdaterState Current { get; }
        public DateTimeOffset ChangedAtUtc { get; }
    }
}
