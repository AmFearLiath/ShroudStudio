using System.Security.Cryptography;
using ShroudStudio.Core;

namespace ShroudStudio.Infrastructure;

public sealed class SavegameDiffService
{
    private const int BufferSize = 64 * 1024;

    public SavegameByteDiff Compare(string leftPath, string rightPath)
    {
        EnsureFile(leftPath);
        EnsureFile(rightPath);

        var leftInfo = new FileInfo(leftPath);
        var rightInfo = new FileInfo(rightPath);
        var comparedBytes = 0L;
        var differentBytes = 0L;
        long? firstDifference = null;
        var leftBuffer = new byte[BufferSize];
        var rightBuffer = new byte[BufferSize];

        using var left = File.OpenRead(leftPath);
        using var right = File.OpenRead(rightPath);
        while (true)
        {
            var leftRead = left.Read(leftBuffer, 0, leftBuffer.Length);
            var rightRead = right.Read(rightBuffer, 0, rightBuffer.Length);
            var blockLength = Math.Max(leftRead, rightRead);
            if (blockLength == 0)
            {
                break;
            }

            for (var index = 0; index < blockLength; index++)
            {
                var leftHasByte = index < leftRead;
                var rightHasByte = index < rightRead;
                if (leftHasByte && rightHasByte)
                {
                    comparedBytes++;
                }

                if (!leftHasByte || !rightHasByte || leftBuffer[index] != rightBuffer[index])
                {
                    differentBytes++;
                    firstDifference ??= comparedBytes + (leftHasByte && rightHasByte ? -1 : 0);
                }
            }
        }

        return new SavegameByteDiff(
            leftPath,
            rightPath,
            leftInfo.Length,
            rightInfo.Length,
            ComputeSha256(leftPath),
            ComputeSha256(rightPath),
            comparedBytes,
            differentBytes,
            firstDifference);
    }

    private static void EnsureFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Savegame file was not found.", path);
        }
    }

    private static string ComputeSha256(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}
