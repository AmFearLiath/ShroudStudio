namespace ShroudStudio.Core;

public sealed record ItemCandidate(
    string IdOrReference,
    int? Quantity,
    string SourcePath,
    string Evidence,
    EvidenceStatus EvidenceStatus,
    string? Slot = null,
    string? ComponentName = null,
    IReadOnlyList<string>? UnassignedFields = null,
    string? ResolvedName = null,
    string? ResolvedCategory = null,
    string? ResolvedDetails = null,
    EvidenceStatus ResolutionStatus = EvidenceStatus.Unknown,
    string? ResolutionEvidence = null)
{
    public string DisplayName => ResolvedName ?? "Name nicht aufgelöst";

    public string ResolutionLabel => ResolutionStatus switch
    {
        EvidenceStatus.Verified => "Name bestätigt",
        EvidenceStatus.Observed => "Name erkannt",
        EvidenceStatus.Detected => "Zuordnung offen",
        EvidenceStatus.Incompatible => "Nicht kompatibel",
        _ => "Name nicht aufgelöst"
    };
}

public sealed record ItemEntity(
    string? ItemId,
    string? Reference,
    int? Quantity,
    string? Slot,
    string? ComponentName,
    string SourcePath,
    IReadOnlyList<string> UnassignedFields,
    EvidenceStatus EvidenceStatus);

public sealed record ItemExtractionReport(
    string ContainerFile,
    int? CharacterBlobIndex,
    IReadOnlyList<ItemCandidate> Candidates,
    IReadOnlyList<string> UnassignedStructures,
    EvidenceStatus Status,
    IReadOnlyList<ItemEntity>? Entities = null,
    IReadOnlyList<ItemContainerObservation>? Containers = null);

public sealed record ItemContainerObservation(
    string Name,
    string DisplayName,
    string SourcePath,
    int NodeId,
    string Structure,
    EvidenceStatus Status,
    int? ItemCount = null);

public sealed record InventorySlotDisplay(
    int SlotNumber,
    string SlotLabel,
    string ItemName,
    string? Details,
    bool IsOccupied,
    string? Evidence = null,
    ItemInstanceObservation? Instance = null,
    uint? Quantity = null,
    string? Category = null,
    ItemDefinition? Definition = null);

public sealed record EquipmentSlotDisplay(
    string SlotKey,
    string SlotLabel,
    InventorySlotDisplay? Item,
    bool IsPrestige,
    string Evidence)
{
    public string DisplayName => Item?.ItemName ?? "Leer";
}

public sealed record ItemDefinition(
    string Reference,
    long? NumericId,
    string? DisplayName,
    string SourcePath,
    EvidenceStatus EvidenceStatus,
    string Evidence,
    string? Category = null,
    string? Rarity = null,
    int? MinLevel = null,
    int? MaxLevel = null,
    string? EquipmentSlot = null,
    string? Description = null,
    string? DamageSummary = null,
    IReadOnlyList<string>? Buffs = null);

public sealed record ItemResolution(
    string Input,
    ItemDefinition? Definition,
    EvidenceStatus Status,
    string Evidence);

public sealed record ItemCatalogReport(
    string InstallationPath,
    IReadOnlyList<ItemDefinition> Definitions,
    IReadOnlyList<string> InspectedFiles,
    IReadOnlyList<string> Errors,
    EvidenceStatus Status);

public sealed record GameDataFingerprint(
    string RelativePath,
    long Length,
    DateTimeOffset LastWriteTimeUtc,
    string Sha256);

public sealed record GameDataAnalysisReport(
    string InstallationPath,
    DateTimeOffset AnalyzedAt,
    IReadOnlyList<GameDataFingerprint> Sources,
    IReadOnlyList<string> ChangedSources,
    int ItemDefinitionCount,
    EvidenceStatus Status,
    IReadOnlyList<string> Errors);
