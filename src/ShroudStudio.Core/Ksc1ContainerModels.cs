namespace ShroudStudio.Core;

public sealed record Ksc1Blob(
    int Index,
    uint OwnerId,
    string Tag,
    uint CompressedSize,
    long PayloadOffset,
    bool LooksLikeZstd);

public sealed record Ksc1ContainerReport(
    string FileName,
    long FileLength,
    uint BlobCount,
    ulong StoredTableCrc,
    ulong CalculatedTableCrc,
    ulong StoredDataCrc,
    ulong CalculatedDataCrc,
    bool TableCrcValid,
    bool DataCrcValid,
    IReadOnlyList<Ksc1Blob> Blobs,
    IReadOnlyList<string> Errors,
    EvidenceStatus Status);
