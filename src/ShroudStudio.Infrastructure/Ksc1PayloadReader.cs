using System.Security.Cryptography;
using ZstdSharp;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

public sealed class Ksc1PayloadReader
{
    public Ksc1PayloadObservation Read(string containerPath, Ksc1Blob blob)
    {
        var uncompressed = ReadPayload(containerPath, blob);
        return new Ksc1PayloadObservation(
            Path.GetFileName(containerPath),
            blob.Index,
            blob.Tag,
            blob.CompressedSize,
            uncompressed.Length,
            Convert.ToHexString(SHA256.HashData(uncompressed)),
            EvidenceStatus.Verified);
    }

    public byte[] ReadPayload(string containerPath, Ksc1Blob blob)
    {
        var container = new Ksc1ContainerAnalyzer().Analyze(containerPath);
        if (!container.TableCrcValid || !container.DataCrcValid)
        {
            throw new InvalidDataException("The KSC1 container failed CRC validation.");
        }

        var payload = new byte[checked((int)blob.CompressedSize)];
        using (var stream = File.OpenRead(containerPath))
        {
            stream.Position = blob.PayloadOffset;
            ReadExactly(stream, payload);
        }

        if (!blob.LooksLikeZstd)
        {
            throw new InvalidDataException($"Blob {blob.Index} ({blob.Tag}) is not a recognized zstd frame.");
        }

        using var decompressor = new Decompressor();
        return decompressor.Unwrap(payload).ToArray();
    }

    private static void ReadExactly(Stream stream, byte[] buffer)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = stream.Read(buffer, offset, buffer.Length - offset);
            if (read == 0)
            {
                throw new EndOfStreamException("The KSC1 payload ended before its declared size.");
            }

            offset += read;
        }
    }
}
