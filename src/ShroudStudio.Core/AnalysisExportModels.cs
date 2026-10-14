namespace ShroudStudio.Core;

public sealed record SavegameAnalysisExport(
    int SchemaVersion,
    DateTimeOffset ExportedAt,
    string SavegameDirectory,
    string InstallationDirectory,
    CharacterSavegameOption Character,
    CharacterPayloadReport CharacterPayload,
    SavedEntityArchiveReport EntityArchive,
    CharacterInventoryReport Inventory,
    ItemExtractionReport Items,
    ItemCatalogExportSummary Catalog,
    string WritePolicy);

public sealed record ItemCatalogExportSummary(
    int DefinitionCount,
    int ResolvedDefinitionCount,
    IReadOnlyList<ItemDefinition> ResolvedDefinitions,
    IReadOnlyList<string> InspectedFiles,
    IReadOnlyList<string> Errors,
    EvidenceStatus Status);
