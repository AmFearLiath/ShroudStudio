using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

public sealed class SavegameSlotService
{
    private static readonly (int Slot, string Id)[] KnownSlots =
    [
        (1, "3ad85aea"),
        (2, "3bd85c7d"),
        (3, "38d857c4"),
        (4, "39d85957"),
        (5, "36d8549e"),
        (6, "37d85631"),
        (7, "34d85178"),
        (8, "35d8530b"),
        (9, "32d84e52"),
        (10, "33d84fe5")
    ];

    public SavegameSlotReport Analyze(string savegameDirectory)
    {
        var report = new SavegameAnalysisService().Analyze(savegameDirectory);
        var infoReader = new SavegameInfoReader();
        var errors = new List<string>(report.Errors);
        var groups = report.Groups.ToDictionary(group => group.GroupId, StringComparer.OrdinalIgnoreCase);
        var slots = KnownSlots.Select(definition =>
        {
            if (!groups.TryGetValue(definition.Id, out var group))
            {
                return new SavegameSlot(definition.Slot, definition.Id, false, null, null, null, 0);
            }

            var index = group.Indexes.FirstOrDefault(candidate =>
                candidate.FileName.Equals($"{definition.Id}-index", StringComparison.OrdinalIgnoreCase));
            var latest = ParseAutosave(index?.Latest);
            var latestFile = latest is null
                ? null
                : ResolveVersionFile(group, definition.Id, latest.Value);
            var info = infoReader.Read(savegameDirectory, definition.Id);
            if (info.Error is not null)
            {
                errors.Add($"Info für Slot {definition.Slot} konnte nicht gelesen werden: {info.Error}");
            }
            return new SavegameSlot(
                definition.Slot,
                definition.Id,
                true,
                latest,
                latestFile,
                index?.UnixTime,
                group.Versions.Count,
                info.WorldName,
                info.InfoFileName,
                GetAutosaves(group, definition.Id, latest));
        }).ToArray();

        return new SavegameSlotReport(slots, errors);
    }

    private static int? ParseAutosave(string? value)
    {
        return int.TryParse(value, out var parsed) && parsed is >= 0 and <= 9
            ? parsed
            : null;
    }

    private static string? ResolveVersionFile(SavegameVersionGroup group, string id, int autosave)
    {
        var candidates = autosave == 0
            ? new[] { id }
            : new[] { $"{id}-{autosave}", $"{id}_{autosave}" };
        return candidates.FirstOrDefault(candidate =>
            group.Versions.Contains(candidate, StringComparer.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<SavegameAutosave> GetAutosaves(
        SavegameVersionGroup group,
        string id,
        int? latest)
    {
        var result = new List<SavegameAutosave>();
        foreach (var file in group.Versions)
        {
            var number = file.Equals(id, StringComparison.OrdinalIgnoreCase)
                ? 0
                : ParseSuffix(file, id);
            if (number is null)
            {
                continue;
            }

            result.Add(new SavegameAutosave(number.Value, file, number == latest));
        }

        return result.OrderBy(autosave => autosave.Number).ToArray();
    }

    private static int? ParseSuffix(string fileName, string id)
    {
        var prefix = fileName.StartsWith($"{id}-", StringComparison.OrdinalIgnoreCase)
            ? $"{id}-"
            : fileName.StartsWith($"{id}_", StringComparison.OrdinalIgnoreCase)
                ? $"{id}_"
                : null;
        return prefix is not null &&
               int.TryParse(fileName[prefix.Length..], out var number) &&
               number is >= 0 and <= 9
            ? number
            : null;
    }
}
