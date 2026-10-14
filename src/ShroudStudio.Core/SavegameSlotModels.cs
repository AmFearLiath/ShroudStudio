namespace ShroudStudio.Core;

public sealed record SavegameAutosave(
    int Number,
    string FileName,
    bool IsLatest);

public sealed record SavegameSlot(
    int SlotNumber,
    string SavegameId,
    bool Available,
    int? LatestAutosave,
    string? LatestFileName,
    long? LatestUnixTime,
    int VersionCount,
    string? WorldName = null,
    string? InfoFileName = null,
    IReadOnlyList<SavegameAutosave>? Autosaves = null);

public sealed record SavegameSlotReport(
    IReadOnlyList<SavegameSlot> Slots,
    IReadOnlyList<string> Errors);
