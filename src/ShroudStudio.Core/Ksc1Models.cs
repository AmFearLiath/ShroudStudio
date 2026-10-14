namespace ShroudStudio.Core;

public sealed record Ksc1Marker(long Offset, string Name, uint? CandidateLength);

public sealed record Ksc1StructureReport(
    string FileName,
    long FileLength,
    string Magic,
    uint FormatValue,
    string OpaqueHeaderField,
    uint? HeaderValue,
    IReadOnlyList<Ksc1Marker> Markers,
    EvidenceStatus Status);
