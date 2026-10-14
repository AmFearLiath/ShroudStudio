namespace ShroudStudio.Core;

public sealed record Ksc1PayloadObservation(
    string ContainerFile,
    int BlobIndex,
    string Tag,
    uint CompressedSize,
    int UncompressedSize,
    string UncompressedSha256,
    EvidenceStatus Status);
