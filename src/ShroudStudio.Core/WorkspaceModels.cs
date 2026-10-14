namespace ShroudStudio.Core;

public sealed record SnapshotFile(string RelativePath, long Length, string Sha256);

public sealed record WorkspaceSnapshot(
    string SourceDirectory,
    string SnapshotDirectory,
    DateTimeOffset CreatedAt,
    IReadOnlyList<SnapshotFile> Files);
