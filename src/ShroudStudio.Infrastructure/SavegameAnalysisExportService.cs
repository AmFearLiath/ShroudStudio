using System.Text.Json;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

public sealed class SavegameAnalysisExportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public SavegameAnalysisExport ExportCharacter(
        string savegameDirectory,
        string installationDirectory,
        CharacterSavegameOption character)
    {
        var characterPayload = new CharacterPayloadService().Analyze(savegameDirectory);
        var items = new ItemExtractionService().Extract(savegameDirectory, character);
        var catalog = new ItemCatalogService().Discover(installationDirectory);
        var entities = new CharacterPayloadService().AnalyzeSavedEntities(savegameDirectory, character);
        var inventory = new CharacterInventoryService().Analyze(savegameDirectory, character, catalog);
        var catalogService = new ItemCatalogService();
        var resolvedDefinitions = items.Candidates
            .Select(candidate => catalogService.Resolve(candidate, catalog).Definition)
            .Where(definition => definition is not null)
            .Cast<ItemDefinition>()
            .GroupBy(definition => definition.Reference, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();

        return new SavegameAnalysisExport(
            2,
            DateTimeOffset.UtcNow,
            Path.GetFullPath(savegameDirectory),
            Path.GetFullPath(installationDirectory),
            character,
            characterPayload,
            entities,
            inventory,
            items,
            new ItemCatalogExportSummary(
                catalog.Definitions.Count,
                resolvedDefinitions.Length,
                resolvedDefinitions,
                catalog.InspectedFiles,
                catalog.Errors,
                catalog.Status),
            "Read-only Analyseexport. Originale Savegames bleiben unverändert; kein Rückschreiben unterstützt.");
    }

    public void WriteCharacterExport(
        string savegameDirectory,
        string installationDirectory,
        CharacterSavegameOption character,
        string outputPath)
    {
        var fullSavegameDirectory = Path.GetFullPath(savegameDirectory);
        var fullOutputPath = Path.GetFullPath(outputPath);
        if (IsInside(fullOutputPath, fullSavegameDirectory))
        {
            throw new InvalidOperationException(
                "Der Analyseexport darf nicht innerhalb des Savegame-Verzeichnisses geschrieben werden.");
        }

        var export = ExportCharacter(savegameDirectory, installationDirectory, character);
        var directory = Path.GetDirectoryName(fullOutputPath);
        if (string.IsNullOrWhiteSpace(directory))
            throw new InvalidOperationException("Der Exportpfad enthält kein gültiges Zielverzeichnis.");

        Directory.CreateDirectory(directory);
        File.WriteAllText(fullOutputPath, JsonSerializer.Serialize(export, JsonOptions));
    }

    private static bool IsInside(string path, string directory)
    {
        var normalizedDirectory = directory.TrimEnd(Path.DirectorySeparatorChar) +
                                  Path.DirectorySeparatorChar;
        return path.StartsWith(normalizedDirectory, StringComparison.OrdinalIgnoreCase);
    }
}
