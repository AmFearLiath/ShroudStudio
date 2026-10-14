using System.Buffers.Binary;
using System.Text;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

public sealed class Ksc1ContainerAnalyzer
{
    private const int HeaderSize = 0x18;
    private const int TableEntrySize = 12;
    private const uint MaxReasonableBlobCount = 100_000;
    private static readonly byte[] ZstdMagic = [0x28, 0xB5, 0x2F, 0xFD];

    public Ksc1ContainerReport Analyze(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Savegame file was not found.", path);
        }

        var info = new FileInfo(path);
        var bytes = File.ReadAllBytes(path);
        var errors = new List<string>();
        if (bytes.Length < HeaderSize || !EnshroudedInventoryScanner.HasKscHeader(bytes))
        {
            throw new InvalidDataException("The file does not contain a KSC1 container.");
        }

        var blobCount = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(4, 4));
        var storedTableCrc = BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(8, 8));
        var storedDataCrc = BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(16, 8));
        if (blobCount > MaxReasonableBlobCount ||
            blobCount > (bytes.Length - HeaderSize) / TableEntrySize)
        {
            throw new InvalidDataException($"Invalid KSC1 blob count: {blobCount}.");
        }

        var tableLength = checked((int)blobCount * TableEntrySize);
        var table = bytes.AsSpan(HeaderSize, tableLength);
        var payloadOffset = HeaderSize + tableLength;
        var blobs = new List<Ksc1Blob>((int)blobCount);
        for (var index = 0; index < blobCount; index++)
        {
            var entry = table.Slice(index * TableEntrySize, TableEntrySize);
            var ownerId = BinaryPrimitives.ReadUInt32LittleEndian(entry[..4]);
            var tag = ReadTag(entry.Slice(4, 4));
            var compressedSize = BinaryPrimitives.ReadUInt32LittleEndian(entry.Slice(8, 4));
            if (compressedSize > bytes.Length - payloadOffset)
            {
                errors.Add($"Blob {index} exceeds the file boundary.");
                break;
            }

            blobs.Add(new Ksc1Blob(
                index,
                ownerId,
                tag,
                compressedSize,
                payloadOffset,
                bytes.AsSpan(payloadOffset, (int)compressedSize).StartsWith(ZstdMagic)));
            payloadOffset += checked((int)compressedSize);
        }

        if (payloadOffset != bytes.Length)
        {
            errors.Add($"Payload table ends at {payloadOffset}, file length is {bytes.Length}.");
        }

        var calculatedTableCrc = Ksc1Crc64.Compute(table);
        var calculatedDataCrc = Ksc1Crc64.Compute(bytes.AsSpan(HeaderSize + tableLength));
        var tableCrcValid = calculatedTableCrc == storedTableCrc;
        var dataCrcValid = calculatedDataCrc == storedDataCrc;
        if (!tableCrcValid)
        {
            errors.Add("Table CRC-64 does not match the stored value.");
        }

        if (!dataCrcValid)
        {
            errors.Add("Payload CRC-64 does not match the stored value.");
        }

        return new Ksc1ContainerReport(
            info.Name,
            info.Length,
            blobCount,
            storedTableCrc,
            calculatedTableCrc,
            storedDataCrc,
            calculatedDataCrc,
            tableCrcValid,
            dataCrcValid,
            blobs,
            errors,
            tableCrcValid && dataCrcValid && errors.Count == 0
                ? EvidenceStatus.Verified
                : EvidenceStatus.Detected);
    }

    private static string ReadTag(ReadOnlySpan<byte> bytes)
    {
        var printable = true;
        foreach (var value in bytes)
        {
            if (value is < 0x20 or > 0x7E)
            {
                printable = false;
                break;
            }
        }

        return printable
            ? Encoding.ASCII.GetString(bytes)
            : Convert.ToHexString(bytes);
    }
}
