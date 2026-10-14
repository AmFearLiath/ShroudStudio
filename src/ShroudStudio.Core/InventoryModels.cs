namespace ShroudStudio.Core;

public enum EvidenceStatus
{
    Unknown,
    Detected,
    Observed,
    Verified,
    Incompatible
}

public sealed record ProcessObservation(
    string ProcessName,
    int ProcessId,
    string? ExecutablePath,
    DateTime? StartTime,
    bool Responding,
    EvidenceStatus Status);

public sealed record SavegameObservation(
    string FileName,
    long Length,
    string Header,
    string? Latest,
    long? UnixTime,
    bool Deleted,
    string? Sha256,
    EvidenceStatus Status);

public sealed record InventoryReport(
    DateTimeOffset CollectedAt,
    string? InstallationPath,
    string? EmlConfigurationPath,
    IReadOnlyList<ProcessObservation> Processes,
    IReadOnlyList<SavegameObservation> Savegames,
    IReadOnlyList<string> Errors);
