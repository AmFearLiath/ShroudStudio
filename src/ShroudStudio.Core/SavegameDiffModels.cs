namespace ShroudStudio.Core;

public sealed record SavegameByteDiff(
    string LeftFile,
    string RightFile,
    long LeftLength,
    long RightLength,
    string LeftSha256,
    string RightSha256,
    long ComparedBytes,
    long DifferentBytes,
    long? FirstDifferenceOffset);
