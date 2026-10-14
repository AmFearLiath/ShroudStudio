namespace ShroudStudio.Core;

public sealed record SavegameIndexObservation(
    string FileName,
    string? Latest,
    long? UnixTime,
    bool Deleted,
    EvidenceStatus Status);

public sealed record SavegameVersionGroup(
    string GroupId,
    int FileCount,
    long TotalBytes,
    IReadOnlyList<string> Versions,
    IReadOnlyList<string> InfoFiles,
    IReadOnlyList<SavegameIndexObservation> Indexes);

public sealed record SavegameAnalysisReport(
    DateTimeOffset AnalyzedAt,
    IReadOnlyList<SavegameVersionGroup> Groups,
    IReadOnlyList<string> Errors);
