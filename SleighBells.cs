using System.Text;

namespace DesktopSleigh;

internal static class SleighBells
{
    public static Stream OpenStream(int volumeDb = 0)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(volumeDb, 0);
        const int sampleRate = 22050;
        const int sampleCount = sampleRate;
        const int dataLength = sampleCount * sizeof(short);
        var stream = new MemoryStream(44 + dataLength);
        using (var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true))
        {
            writer.Write("RIFF"u8);
            writer.Write(36 + dataLength);
            writer.Write("WAVEfmt "u8);
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(sampleRate);
            writer.Write(sampleRate * sizeof(short));
            writer.Write((short)sizeof(short));
            writer.Write((short)16);
            writer.Write("data"u8);
            writer.Write(dataLength);

            double gain = Math.Pow(10, volumeDb / 20.0);
            for (int i = 0; i < sampleCount; i++)
            {
                double time = (double)i / sampleRate;
                double sample = 0;
                for (int strike = 0; strike < 3; strike++)
                {
                    double age = time - strike * 0.18;
                    if (age < 0)
                    {
                        continue;
                    }

                    double frequency = strike == 1 ? 1568 : 1318.51;
                    double attack = Math.Min(1, age / 0.003);
                    sample += attack * Math.Exp(-age * 12)
                        * (Math.Sin(2 * Math.PI * frequency * age)
                        + 0.35 * Math.Sin(2 * Math.PI * frequency * 2.76 * age));
                }
                writer.Write((short)Math.Round(sample * 6500 * gain));
            }
        }
        stream.Position = 0;
        return stream;
    }
}
