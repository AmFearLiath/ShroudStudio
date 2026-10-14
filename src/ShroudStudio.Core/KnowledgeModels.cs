namespace ShroudStudio.Core;

public sealed record KnowledgeEntryObservation(
    uint Id,
    uint Value,
    EvidenceStatus Status);

public sealed record KnowledgePayloadReport(
    string ContainerFile,
    string BlobTag,
    uint Version,
    IReadOnlyList<KnowledgeEntryObservation> Entries,
    IReadOnlyList<string> Errors,
    EvidenceStatus Status);
