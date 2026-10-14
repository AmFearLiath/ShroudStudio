using System.Text.Json.Serialization;

namespace ShroudStudio.Core;

public sealed record CharacterFieldObservation(
    int ParentNode,
    string Name,
    int ChildNode,
    string Value,
    EvidenceStatus Status);

public sealed record CharacterBlobObservation(
    string ContainerFile,
    int BlobIndex,
    uint OwnerId,
    string Tag,
    int UncompressedSize,
    string? Name,
    uint? Level,
    int NodeCount,
    int ObjectCount,
    IReadOnlyList<CharacterFieldObservation> Fields,
    EvidenceStatus Status,
    string? Error = null);

public sealed record CharacterPayloadReport(
    string ContainerFile,
    IReadOnlyList<CharacterBlobObservation> Characters,
    IReadOnlyList<string> Errors,
    EvidenceStatus Status);

public sealed record CharacterSavegameOption(
    int Number,
    string FileName,
    bool IsLatest,
    string? CharacterName,
    uint? Level,
    bool HasInventoryData = false,
    bool HasActiveQuest = false,
    bool HasEquipmentData = false,
    bool HasFavoritesData = false,
    int BlobIndex = -1);

public sealed record SavedEntityComponentObservation(
    string Name,
    ushort ComponentId,
    int Size,
    bool IsClientComponent,
    EvidenceStatus Status,
    IReadOnlyList<ComponentSchemaFieldObservation>? Fields = null,
    [property: JsonIgnore]
    byte[]? Data = null);

public sealed record ComponentSchemaFieldObservation(
    string Name,
    int Offset,
    int Size,
    uint Kind,
    EvidenceStatus Status,
    int? ElementCount = null,
    IReadOnlyList<ComponentSchemaFieldObservation>? ElementFields = null);

public sealed record SavedEntityObservation(
    ulong EntityId,
    ushort TemplateIndex,
    IReadOnlyList<SavedEntityComponentObservation> Components,
    EvidenceStatus Status);

public sealed record SavedEntityArchiveReport(
    int ByteLength,
    IReadOnlyList<SavedEntityObservation> Entities,
    IReadOnlyList<string> ComponentDefinitions,
    IReadOnlyList<string> Errors,
    EvidenceStatus Status);

public sealed record CharacterInventorySlotObservation(
    string ContainerName,
    int SlotIndex,
    uint ItemReference,
    EvidenceStatus Status,
    uint? Quantity = null,
    uint? ItemStateReference = null,
    string? ItemName = null,
    string? Category = null,
    string? Details = null,
    ItemInstanceObservation? Instance = null,
    ItemDefinition? Definition = null);

public sealed record ItemInstanceObservation(
    ulong EntityId,
    uint ItemReference,
    ushort? BaseDamage,
    byte? RarityCode,
    byte? Level,
    bool HasDurability,
    bool HasPerks,
    bool HasWeaponGem,
    bool HasColorPalette,
    EvidenceStatus Status,
    IReadOnlyList<ItemPropertyObservation>? Properties = null,
    IReadOnlyList<string>? Components = null);

public sealed record ItemPropertyObservation(
    string Component,
    string Name,
    string Value,
    EvidenceStatus Status,
    string Evidence);

public sealed record CharacterInventoryContainerObservation(
    string Name,
    ulong SetupEntityId,
    int StorageAreaCount,
    int SlotCount,
    byte? AvailableSlotCount,
    EvidenceStatus Status);

public sealed record CharacterInventoryReport(
    IReadOnlyList<CharacterInventorySlotObservation> Slots,
    IReadOnlyList<CharacterInventoryContainerObservation> Containers,
    IReadOnlyList<string> Errors,
    EvidenceStatus Status);
