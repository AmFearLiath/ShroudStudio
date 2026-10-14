using System.Text;
using System.Text.Json;
using ZstdSharp;

namespace ShroudStudio.Infrastructure;

public sealed class SavegameInfoReader
{
    private static readonly HashSet<string> NonWorldNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "custom",
        "disabled",
        "easy",
        "extreme",
        "gameSettingsPreset",
        "keepProgress",
        "many",
        "normal",
        "noTombstone",
        "progress",
        "progressLevel",
        "seed",
        "settings",
        "value",
        "version",
        "veryEasy"
    };

    public (string? WorldName, string? InfoFileName, string? Error) Read(
        string savegameDirectory,
        string savegameId)
    {
        var infoIndex = Path.Combine(savegameDirectory, $"{savegameId}_info-index");
        var infoPath = ResolveInfoPath(savegameDirectory, savegameId, infoIndex);
        if (infoPath is null)
        {
            return (null, null, null);
        }

        try
        {
            var container = new Ksc1ContainerAnalyzer().Analyze(infoPath);
            var reader = new Ksc1PayloadReader();
            var candidates = new List<string>();
            var rawInfo = File.ReadAllBytes(infoPath);
            foreach (var offset in FindZstdOffsets(rawInfo))
            {
                try
                {
                    using var decompressor = new Decompressor();
                    candidates.AddRange(ExtractStrings(decompressor.Unwrap(rawInfo.AsSpan(offset).ToArray()).ToArray()));
                }
                catch (Exception ex) when (ex is InvalidDataException or ZstdException)
                {
                    // Not every magic sequence in an info file is a complete frame.
                }
            }

            foreach (var blob in container.Blobs)
            {
                var payload = reader.ReadPayload(infoPath, blob);
                candidates.AddRange(ExtractStrings(payload));
            }

            var name = candidates
                .Where(IsPlausibleWorldName)
                .GroupBy(candidate => candidate, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(group => group.Count())
                .ThenByDescending(group => Score(group.Key))
                .Select(group => group.Key)
                .FirstOrDefault();
            return (name, Path.GetFileName(infoPath), null);
        }

        catch (IOException ex)
        {
            return (null, Path.GetFileName(infoPath), ex.Message);
        }
        catch (InvalidDataException ex)
        {
            return (null, Path.GetFileName(infoPath), ex.Message);
        }
    }

    private static IEnumerable<int> FindZstdOffsets(byte[] data)
    {
        for (var index = 0; index <= data.Length - 4; index++)
        {
            if (data[index] == 0x28 && data[index + 1] == 0xB5 &&
                data[index + 2] == 0x2F && data[index + 3] == 0xFD)
            {
                yield return index;
            }
        }
    }

    private static string? ResolveInfoPath(string directory, string id, string indexPath)
    {
        var candidates = new List<string>();
        if (File.Exists(indexPath))
        {
            try
            {
                using var document = JsonDocument.Parse(File.ReadAllBytes(indexPath));
                if (document.RootElement.TryGetProperty("latest", out var latestElement) &&
                    latestElement.TryGetInt32(out var latest))
                {
                    candidates.Add(latest == 0
                        ? Path.Combine(directory, $"{id}_info")
                        : Path.Combine(directory, $"{id}_info-{latest}"));
                }
            }
            catch (JsonException)
            {
                // The fallback candidates below remain safe and deterministic.
            }
        }

        candidates.Add(Path.Combine(directory, $"{id}_info"));
        candidates.AddRange(Directory.Exists(directory)
            ? Directory.EnumerateFiles(directory, $"{id}_info-*")
                .Where(path => !path.EndsWith("-index", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(path => path, StringComparer.OrdinalIgnoreCase)
            : []);
        return candidates.FirstOrDefault(File.Exists);
    }

    private static IEnumerable<string> ExtractStrings(byte[] payload)
    {
        var ascii = new StringBuilder();
        foreach (var value in payload)
        {
            if (value is >= 0x20 and <= 0x7E)
            {
                ascii.Append((char)value);
            }
            else
            {
                if (ascii.Length >= 4)
                {
                    yield return ascii.ToString();
                }

                ascii.Clear();
            }
        }

        if (ascii.Length >= 4)
        {
            yield return ascii.ToString();
        }
    }

    private static bool IsPlausibleWorldName(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length is >= 4 and <= 80 &&
               trimmed.Any(char.IsLetter) &&
               !NonWorldNames.Contains(trimmed) &&
               !LooksLikeEnumOrCamelCase(trimmed) &&
               !trimmed.Contains("KSC1", StringComparison.OrdinalIgnoreCase) &&
               !trimmed.Contains("zstd", StringComparison.OrdinalIgnoreCase) &&
               !trimmed.Contains("http", StringComparison.OrdinalIgnoreCase) &&
               !trimmed.Contains('\\') &&
               !trimmed.Contains('/');
    }

    private static bool LooksLikeEnumOrCamelCase(string value)
    {
        if (value.Contains(' ') || !value.All(char.IsLetterOrDigit))
        {
            return false;
        }

        var transitions = 0;
        for (var index = 1; index < value.Length; index++)
        {
            if (char.IsLower(value[index - 1]) && char.IsUpper(value[index]))
            {
                transitions++;
            }
        }

        return transitions > 0;
    }

    private static int Score(string value)
    {
        var score = value.Count(char.IsLetter) + value.Count(char.IsWhiteSpace) * 2;
        if (value.Contains(' '))
        {
            score += 12;
        }

        return score;
    }
}
