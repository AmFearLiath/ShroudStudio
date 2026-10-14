namespace ShroudStudio.Infrastructure;

public static class Ksc1Crc64
{
    private const ulong ReflectedPolynomial = 0xC96C5795D7870F42;

    public static ulong Compute(ReadOnlySpan<byte> data)
    {
        var crc = ulong.MaxValue;
        foreach (var value in data)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++)
            {
                crc = (crc & 1) != 0
                    ? (crc >> 1) ^ ReflectedPolynomial
                    : crc >> 1;
            }
        }

        return crc ^ ulong.MaxValue;
    }
}
