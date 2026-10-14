using System.Text.Json;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

public sealed class SavegameAnalysisService
{
    public SavegameAnalysisReport Analyze(string savegameDirectory)
    {
        var errors = new List<string>();
        if (!Directory.Exists(savegameDirectory))
        {
            return new SavegameAnalysisReport(
                DateTimeOffset.UtcNow,
                [],
                [$"Savegame directory was not found: {savegameDirectory}"]);
        }

        var groups = new Dictionary<string, GroupAccumulator>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in Directory.EnumerateFiles(savegameDirectory).OrderBy(path => path))
        {
            try
            {
                AddFile(groups, path);
            }
            catch (IOException ex)
            {
                errors.Add($"Could not analyze {Path.GetFileName(path)}: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                errors.Add($"Access denied for {Path.GetFileName(path)}: {ex.Message}");
            }
            catch (JsonException ex)
            {
                errors.Add($"Invalid JSON index {Path.GetFileName(path)}: {ex.Message}");
            }
        }

        var result = groups.Values
            .OrderBy(group => group.GroupId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.ToModel())
            .ToArray();
        return new SavegameAnalysisReport(DateTimeOffset.UtcNow, result, errors);
    }

    private static void AddFile(Dictionary<string, GroupAccumulator> groups, string path)
    {
        var fileName = Path.GetFileName(path);
        var groupId = GetGroupId(fileName);
        if (!groups.TryGetValue(groupId, out var group))
        {
            group = new GroupAccumulator(groupId);
            groups.Add(groupId, group);
        }

        var length = new FileInfo(path).Length;
        group.FileCount++;
        group.TotalBytes += length;

        if (fileName.EndsWith("-index", StringComparison.OrdinalIgnoreCase))
        {
            group.Indexes.Add(ReadIndex(path));
        }
        else if (fileName.Contains("_info", StringComparison.OrdinalIgnoreCase))
        {
            group.InfoFiles.Add(fileName);
        }
        else
        {
            group.Versions.Add(fileName);
        }
    }

    private static SavegameIndexObservation ReadIndex(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;
        var latest = root.TryGetProperty("latest", out var latestElement)
            ? latestElement.ToString()
            : null;
        long? unixTime = root.TryGetProperty("time", out var timeElement) &&
                         timeElement.TryGetInt64(out var parsedTime)
            ? parsedTime
            : null;
        var deleted = root.TryGetProperty("deleted", out var deletedElement) &&
                      deletedElement.ValueKind == JsonValueKind.True;
        return new SavegameIndexObservation(
            Path.GetFileName(path),
            latest,
            unixTime,
            deleted,
            EvidenceStatus.Verified);
    }

    private static string GetGroupId(string fileName)
    {
        var index = fileName.IndexOfAny(['-', '_']);
        return index > 0 ? fileName[..index] : fileName;
    }

    private sealed class GroupAccumulator(string groupId)
    {
        public string GroupId { get; } = groupId;
        public int FileCount { get; set; }
        public long TotalBytes { get; set; }
        public List<string> Versions { get; } = [];
        public List<string> InfoFiles { get; } = [];
        public List<SavegameIndexObservation> Indexes { get; } = [];

        public SavegameVersionGroup ToModel() =>
            new(
                GroupId,
                FileCount,
                TotalBytes,
                Versions.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                InfoFiles.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                Indexes.OrderBy(index => index.FileName, StringComparer.OrdinalIgnoreCase).ToArray());
    }
}
