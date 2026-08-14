// SPEC-DERIVED-MSG2  IClock production implementation

using System;
using TaskTree.Core.Abstractions;

namespace TaskTree.Core;

/// <summary>Production UTC time source for dependency-injected modules.</summary>
public sealed class Clock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
