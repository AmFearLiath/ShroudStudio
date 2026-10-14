using System.Text.Json;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

public sealed class CharacterPayloadService
{
    public CharacterPayloadReport Analyze(string savegameDirectory)
    {
        var indexPath = Path.Combine(savegameDirectory, "characters-index");
        if (!File.Exists(indexPath))
        {
            throw new FileNotFoundException("characters-index was not found.", indexPath);
        }

        var index = JsonDocument.Parse(File.ReadAllBytes(indexPath)).RootElement;
        if (!index.TryGetProperty("latest", out var latestElement) ||
            latestElement.ValueKind != JsonValueKind.Number ||
            !latestElement.TryGetInt32(out var latest) ||
            latest is < 0 or > 9)
        {
            throw new InvalidDataException("characters-index has no valid latest slot.");
        }

        var fileName = latest == 0 ? "characters" : $"characters-{latest}";
        return AnalyzeFile(savegameDirectory, fileName);
    }

    public IReadOnlyList<CharacterSavegameOption> ListOptions(string savegameDirectory)
    {
        var latest = ReadLatestNumber(savegameDirectory);
        var fileName = latest == 0 ? "characters" : $"characters-{latest}";
        var report = AnalyzeFile(savegameDirectory, fileName);
        var characters = report.Characters
            .Where(character => character.Name is not null || character.Level is not null)
            .GroupBy(character => $"{character.Name}\u001f{character.Level}")
            .Select(group => group.First())
            .OrderByDescending(IsPlayableCharacter)
            .ThenBy(character => character.BlobIndex)
            .ToArray();
        var options = new List<CharacterSavegameOption>(characters.Length);
        for (var index = 0; index < characters.Length; index++)
        {
            var character = characters[index];
            var rootFields = character.Fields
                .Where(field => field.ParentNode == 1)
                .Select(field => field.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
            options.Add(new CharacterSavegameOption(
                index + 1,
                fileName,
                true,
                character.Name,
                character.Level,
                rootFields.Contains("items") ||
                rootFields.Contains("itemLastUsageOrders") ||
                rootFields.Contains("itemFavoritesOrders"),
                rootFields.Contains("activeQuestId"),
                rootFields.Contains("setup") || rootFields.Contains("data"),
                rootFields.Contains("itemFavoritesOrders"),
                character.BlobIndex));
        }

        return options;
    }

    public SavedEntityArchiveReport AnalyzeSavedEntities(
        string savegameDirectory,
        CharacterSavegameOption character)
    {
        var path = Path.Combine(savegameDirectory, character.FileName);
        if (!File.Exists(path))
        {
            return new SavedEntityArchiveReport(0, [], [],
                [$"Die Charakterdatei '{character.FileName}' wurde nicht gefunden."],
                EvidenceStatus.Unknown);
        }

        try
        {
            var container = new Ksc1ContainerAnalyzer().Analyze(path);
            var blob = container.Blobs.FirstOrDefault(item =>
                item.Index == character.BlobIndex && string.Equals(item.Tag, "CHAR", StringComparison.Ordinal));
            if (blob is null)
            {
                return new SavedEntityArchiveReport(0, [], [],
                    ["Der ausgewählte CHAR-Blob wurde nicht gefunden."], EvidenceStatus.Unknown);
            }

            var payload = new Ksc1PayloadReader().ReadPayload(path, blob);
            var savedData = new BdbCharacterAnalyzer().ReadByteArrayField(payload, "data");
            return new SavedEntityArchiveAnalyzer().Analyze(savedData);
        }
        catch (IOException ex)
        {
            return new SavedEntityArchiveReport(0, [], [], [ex.Message], EvidenceStatus.Detected);
        }
        catch (InvalidDataException ex)
        {
            return new SavedEntityArchiveReport(0, [], [], [ex.Message], EvidenceStatus.Detected);
        }
    }

    private static bool IsPlayableCharacter(CharacterBlobObservation character)
    {
        var fields = character.Fields
            .Where(field => field.ParentNode == 1)
            .Select(field => field.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return fields.Contains("activeQuestId") ||
               fields.Contains("itemLastUsageOrders") ||
               fields.Contains("itemFavoritesOrders");
    }

    private CharacterPayloadReport AnalyzeFile(string savegameDirectory, string fileName)
    {
        var path = Path.Combine(savegameDirectory, fileName);
        var container = new Ksc1ContainerAnalyzer().Analyze(path);
        var payloadReader = new Ksc1PayloadReader();
        var analyzer = new BdbCharacterAnalyzer();
        var observations = new List<CharacterBlobObservation>();
        var errors = new List<string>(container.Errors);

        foreach (var blob in container.Blobs.Where(blob =>
                     string.Equals(blob.Tag, "CHAR", StringComparison.Ordinal)))
        {
            try
            {
                var payload = payloadReader.ReadPayload(path, blob);
                observations.Add(analyzer.Analyze(fileName, blob, payload));
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException)
            {
                errors.Add($"Blob {blob.Index} ({blob.Tag}) konnte nicht gelesen werden: {ex.Message}");
            }
        }

        var status = observations.Count > 0 && errors.Count == 0
            ? EvidenceStatus.Observed
            : observations.Count > 0
                ? EvidenceStatus.Detected
                : EvidenceStatus.Unknown;
        return new CharacterPayloadReport(fileName, observations, errors, status);
    }

    private static int ReadLatestNumber(string savegameDirectory)
    {
        using var document = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(savegameDirectory, "characters-index")));
        return document.RootElement.TryGetProperty("latest", out var latestElement) &&
               latestElement.TryGetInt32(out var latest) &&
               latest is >= 0 and <= 9
            ? latest
            : 0;
    }
}
