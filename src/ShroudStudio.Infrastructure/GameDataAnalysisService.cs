using System.Security.Cryptography;
using System.Text.Json;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

/// <summary>
/// Read-only fingerprinting of local game definitions and exported descriptors.
/// The caller may persist the returned report outside the game and save folders.
/// </summary>
public sealed class GameDataAnalysisService
{
    private static readonly string[] RelevantFiles =
    [
        "enshrouded.kfc",
        "enshrouded.kfc_resources",
        "export\\mapping_discovery\\item_index.json",
        "export\\structured_dump\\items_iteminfo.json",
        "export\\weapons_dump\\weapons_unified.json"
    ];

    public GameDataAnalysisReport Analyze(
        string installationPath,
        IReadOnlyList<GameDataFingerprint>? previousSources = null)
    {
        var root = Path.GetFullPath(installationPath);
        if (!Directory.Exists(root))
        {
            return new(root, DateTimeOffset.UtcNow, [], [], 0, EvidenceStatus.Unknown,
                ["Enshrouded-Installation nicht gefunden."]);
        }

        var sources = new List<GameDataFingerprint>();
        var errors = new List<string>();
        foreach (var relativePath in RelevantFiles)
        {
            var path = Path.Combine(root, relativePath);
            if (!File.Exists(path))
                continue;

            try
            {
                var info = new FileInfo(path);
                using var stream = File.OpenRead(path);
                sources.Add(new GameDataFingerprint(
                    relativePath,
                    info.Length,
                    info.LastWriteTimeUtc,
                    Convert.ToHexString(SHA256.HashData(stream))));
            }
            catch (IOException ex)
            {
                errors.Add($"{relativePath}: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                errors.Add($"{relativePath}: {ex.Message}");
            }
        }

        var previous = (previousSources ?? [])
            .ToDictionary(source => source.RelativePath, StringComparer.OrdinalIgnoreCase);
        var changed = sources
            .Where(source => !previous.TryGetValue(source.RelativePath, out var old) ||
                             !string.Equals(source.Sha256, old.Sha256, StringComparison.Ordinal))
            .Select(source => source.RelativePath)
            .ToArray();
        var catalog = new ItemCatalogService().Discover(root);
        errors.AddRange(catalog.Errors);
        return new(
            root,
            DateTimeOffset.UtcNow,
            sources,
            changed,
            catalog.Definitions.Count,
            sources.Count > 0 && catalog.Status is EvidenceStatus.Verified or EvidenceStatus.Detected
                ? EvidenceStatus.Observed
                : EvidenceStatus.Unknown,
            errors);
    }

    public static IReadOnlyList<GameDataFingerprint> ReadBaseline(string path)
    {
        if (!File.Exists(path))
            return [];

        return JsonSerializer.Deserialize<List<GameDataFingerprint>>(File.ReadAllBytes(path)) ?? [];
    }

    public static void WriteBaseline(string path, GameDataAnalysisReport report)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (string.IsNullOrWhiteSpace(directory))
            throw new InvalidOperationException("Ungültiger Speicherort für den Spieldatenabgleich.");

        Directory.CreateDirectory(directory);
        File.WriteAllText(path, JsonSerializer.Serialize(report.Sources, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }
}
