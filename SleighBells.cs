using System.Buffers.Binary;

namespace DesktopSleigh;

internal static class SleighBells
{
    public static Stream OpenStream(int volumeDb = 0)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(volumeDb, 0);
        using Stream recording = typeof(SleighBells).Assembly
            .GetManifestResourceStream("DesktopSleigh.Assets.bells-jingling-2.wav")
            ?? throw new InvalidOperationException("The embedded sleigh bell recording is missing.");
        using var buffer = new MemoryStream();
        recording.CopyTo(buffer);
        byte[] wave = buffer.ToArray();

        // The bundled recording is stereo 16-bit PCM with a canonical 44-byte WAV header.
        const int headerLength = 44;
        double gain = Math.Pow(10, volumeDb / 20.0);
        for (int offset = headerLength; offset < wave.Length; offset += sizeof(short))
        {
            Span<byte> sampleBytes = wave.AsSpan(offset, sizeof(short));
            short sample = BinaryPrimitives.ReadInt16LittleEndian(sampleBytes);
            BinaryPrimitives.WriteInt16LittleEndian(sampleBytes, (short)Math.Round(sample * gain));
        }

        return new MemoryStream(wave, writable: false);
    }
}
