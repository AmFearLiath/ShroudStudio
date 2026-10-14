using System.Buffers.Binary;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

/// <summary>
/// Read-only parser for the documented KNOW progression table in KSC1 containers.
/// Entry semantics remain source-dependent and are not guessed.
/// </summary>
public sealed class KnowledgePayloadService
{
    public KnowledgePayloadReport Analyze(string containerPath)
    {
        try
        {
            var container = new Ksc1ContainerAnalyzer().Analyze(containerPath);
            var blob = container.Blobs.FirstOrDefault(candidate =>
                string.Equals(candidate.Tag, "KNOW", StringComparison.Ordinal) ||
                string.Equals(candidate.Tag, "DC8ED5F0", StringComparison.OrdinalIgnoreCase));
            if (blob is null)
            {
                return new KnowledgePayloadReport(Path.GetFileName(containerPath), string.Empty, 0, [], [],
                    EvidenceStatus.Unknown);
            }

            var payload = new Ksc1PayloadReader().ReadPayload(containerPath, blob);
            if (payload.Length < 12)
                throw new InvalidDataException("Die KNOW-Tabelle ist unvollständig.");

            var version = BinaryPrimitives.ReadUInt32LittleEndian(payload.AsSpan(0, 4));
            var compactCount = BinaryPrimitives.ReadUInt32LittleEndian(payload.AsSpan(4, 4));
            var legacyCount = BinaryPrimitives.ReadUInt32LittleEndian(payload.AsSpan(8, 4));
            var (headerSize, entryCount) = HasExpectedLength(payload.Length, compactCount, 8)
                ? (8, compactCount)
                : HasExpectedLength(payload.Length, legacyCount, 12)
                    ? (12, legacyCount)
                    : throw new InvalidDataException(
                        $"Die KNOW-Tabelle besitzt eine ungültige Eintragslänge (Version {version}, " +
                        $"Header {Convert.ToHexString(payload.AsSpan(0, Math.Min(16, payload.Length)))}, Bytes {payload.Length}).");
            if (entryCount > int.MaxValue / 8)
                throw new InvalidDataException(
                    "Die KNOW-Tabelle überschreitet die Sicherheitsgrenze.");

            var entries = new KnowledgeEntryObservation[entryCount];
            var idsOffset = headerSize;
            var valuesOffset = checked(idsOffset + (int)entryCount * 4);
            for (var index = 0; index < entryCount; index++)
            {
                entries[index] = new KnowledgeEntryObservation(
                    BinaryPrimitives.ReadUInt32LittleEndian(payload.AsSpan(idsOffset + index * 4, 4)),
                    BinaryPrimitives.ReadUInt32LittleEndian(payload.AsSpan(valuesOffset + index * 4, 4)),
                    EvidenceStatus.Observed);
            }

            return new KnowledgePayloadReport(
                Path.GetFileName(containerPath),
                blob.Tag,
                version,
                entries,
                [],
                EvidenceStatus.Observed);
        }
        catch (IOException ex)
        {
            return new KnowledgePayloadReport(Path.GetFileName(containerPath), string.Empty, 0, [], [ex.Message],
                EvidenceStatus.Detected);
        }
        catch (InvalidDataException ex)
        {
            return new KnowledgePayloadReport(Path.GetFileName(containerPath), string.Empty, 0, [], [ex.Message],
                EvidenceStatus.Detected);
        }
    }

    private static bool HasExpectedLength(int length, uint entryCount, int headerSize) =>
        entryCount <= int.MaxValue / 8 && length == headerSize + entryCount * 8;
}
