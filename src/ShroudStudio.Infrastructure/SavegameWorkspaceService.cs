using System.Security.Cryptography;
using System.Text.Json;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

public sealed class SavegameWorkspaceService
{
    private const string ManifestName = "snapshot-manifest.json";

    public WorkspaceSnapshot CreateSnapshot(string sourceDirectory, string workspaceRoot)
    {
        var source = Path.GetFullPath(sourceDirectory);
        var root = Path.GetFullPath(workspaceRoot);
        if (!Directory.Exists(source))
        {
            throw new DirectoryNotFoundException($"Savegame directory was not found: {source}");
        }

        if (IsSameOrChildPath(root, source))
        {
            throw new ArgumentException("Workspace root must not be inside the source savegame directory.", nameof(workspaceRoot));
        }

        var snapshotDirectory = Path.Combine(root, DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        Directory.CreateDirectory(snapshotDirectory);
        var files = new List<SnapshotFile>();

        foreach (var sourceFile in Directory.EnumerateFiles(source, "*", SearchOption.TopDirectoryOnly))
        {
            var relativePath = Path.GetRelativePath(source, sourceFile);
            var destinationFile = Path.Combine(snapshotDirectory, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
            File.Copy(sourceFile, destinationFile, overwrite: false);
            files.Add(new SnapshotFile(relativePath, new FileInfo(sourceFile).Length, ComputeSha256(sourceFile)));
        }

        var snapshot = new WorkspaceSnapshot(source, snapshotDirectory, DateTimeOffset.UtcNow, files);
        var manifestPath = Path.Combine(snapshotDirectory, ManifestName);
        File.WriteAllText(manifestPath, JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true }));
        return snapshot;
    }

    private static bool IsSameOrChildPath(string candidate, string parent)
    {
        var normalizedCandidate = Path.TrimEndingDirectorySeparator(candidate) + Path.DirectorySeparatorChar;
        var normalizedParent = Path.TrimEndingDirectorySeparator(parent) + Path.DirectorySeparatorChar;
        return normalizedCandidate.StartsWith(normalizedParent, StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}
