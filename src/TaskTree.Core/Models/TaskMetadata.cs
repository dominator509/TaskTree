// SPEC-DERIVED-PHASE2C  HALT #2/#3/#4/#5/#6
// Roadmap P2C-AC1: metadata carries patient text + labs + delivery hints without PHI leakage.
// Gap #128/#129: model is PHI-minimal but not a formal PHI scanner.

using System;

namespace TaskTree.Core.Models
{
    /// <summary>
    /// PHI-minimal task metadata for operational hints. No patient name, MRN, DOB,
    /// phone, email, address, arbitrary dictionary, or free-form notes blob fields.
    /// </summary>
    public sealed record TaskMetadata(
        string PatientText,
        string LabHint,
        string DeliveryHint,
        bool RequiresLabReview,
        bool RequiresDeliveryCoordination,
        DateTimeOffset? LabDueAtUtc)
    {
        public static TaskMetadata Empty => new(
            PatientText: string.Empty,
            LabHint: string.Empty,
            DeliveryHint: string.Empty,
            RequiresLabReview: false,
            RequiresDeliveryCoordination: false,
            LabDueAtUtc: null);
    }
}
