namespace ShroudStudio.Core;

public sealed record SavegameByteDiff(
    string LeftFile,
    string RightFile,
    long LeftLength,
    long RightLength,
    string LeftSha256,
    string RightSha256,
    long ComparedBytes,
    long DifferentBytes,
    long? FirstDifferenceOffset);

public enum SavegameChangeKind
{
    Added,
    Removed,
    Changed
}

public sealed record SavegameFieldChange(
    string Component,
    string FieldPath,
    string? BeforeValue,
    string? AfterValue,
    SavegameChangeKind Kind,
    EvidenceStatus Status);

public sealed record SavegameComponentChange(
    ulong EntityId,
    string Component,
    SavegameChangeKind Kind,
    int? BeforeSize,
    int? AfterSize,
    IReadOnlyList<SavegameFieldChange> Fields,
    EvidenceStatus Status);

public sealed record SavegameSemanticDiff(
    int BeforeEntityCount,
    int AfterEntityCount,
    IReadOnlyList<SavegameComponentChange> Changes,
    IReadOnlyList<string> Errors,
    EvidenceStatus Status)
{
    public int ChangedEntityCount => Changes.Select(change => change.EntityId).Distinct().Count();
    public int ChangedFieldCount => Changes.Sum(change => change.Fields.Count);
}
