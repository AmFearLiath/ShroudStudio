using System.Buffers.Binary;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

public sealed class Ksc1StructureAnalyzer
{
    private const int SampleLength = 64 * 1024;

    public Ksc1StructureReport Analyze(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Savegame file was not found.", path);
        }

        var info = new FileInfo(path);
        var bytes = new byte[(int)Math.Min(SampleLength, info.Length)];
        using var stream = File.OpenRead(path);
        var read = stream.Read(bytes, 0, bytes.Length);
        if (read < 8 || !EnshroudedInventoryScanner.HasKscHeader(bytes))
        {
            throw new InvalidDataException("The file does not contain a KSC1 header.");
        }

        var formatValue = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(4, 4));
        var opaqueHeaderField = read >= 24
            ? Convert.ToHexString(bytes, 8, 16)
            : string.Empty;
        uint? headerValue = read >= 28
            ? BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(24, 4))
            : null;
        var markers = FindMarkers(bytes.AsSpan(28, read - 28));

        return new Ksc1StructureReport(
            info.Name,
            info.Length,
            "KSC1",
            formatValue,
            opaqueHeaderField,
            headerValue,
            markers,
            EvidenceStatus.Detected);
    }

    private static IReadOnlyList<Ksc1Marker> FindMarkers(ReadOnlySpan<byte> bytes)
    {
        var markers = new List<Ksc1Marker>();
        for (var index = 0; index <= bytes.Length - 4; index++)
        {
            if (!IsMarkerByte(bytes[index]) ||
                !IsMarkerByte(bytes[index + 1]) ||
                !IsMarkerByte(bytes[index + 2]) ||
                !IsMarkerByte(bytes[index + 3]))
            {
                continue;
            }

            uint? candidateLength = bytes.Length - index >= 8
                ? BinaryPrimitives.ReadUInt32LittleEndian(bytes.Slice(index + 4, 4))
                : null;
            markers.Add(new Ksc1Marker(index + 28, new string(new[]
            {
                (char)bytes[index],
                (char)bytes[index + 1],
                (char)bytes[index + 2],
                (char)bytes[index + 3]
            }), candidateLength));
        }

        return markers;
    }

    private static bool IsMarkerByte(byte value) =>
        value is >= (byte)'A' and <= (byte)'Z' ||
        value is >= (byte)'0' and <= (byte)'9' ||
        value == (byte)'_';
}
