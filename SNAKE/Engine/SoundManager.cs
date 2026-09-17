using System.IO;
using System.Media;

namespace SNAKE.Engine;

public class SoundManager
{
    private static SoundManager? _instance;
    public static SoundManager Instance => _instance ??= new SoundManager();

    public bool IsMuted { get; set; } = false;

    private readonly byte[] _eatAppleWav;
    private readonly byte[] _eatBonusWav;
    private readonly byte[] _turnWav;
    private readonly byte[] _gameOverWav;
    private readonly byte[] _victoryWav;
    private readonly byte[] _clickWav;

    private SoundManager()
    {
        _eatAppleWav = GenerateChirpWav(520, 960, 0.08, 0.28);
        _eatBonusWav = GenerateDoubleChimeWav(784, 1175, 0.14, 0.3);
        _turnWav = GenerateToneWav(160, 0.03, 0.12, isSquare: false);
        _gameOverWav = GenerateDescendingThudWav(340, 90, 0.35, 0.35);
        _victoryWav = GenerateVictoryFanfareWav();
        _clickWav = GenerateToneWav(240, 0.03, 0.15, isSquare: false);
    }

    public void PlayEatApple() => PlayBytes(_eatAppleWav);
    public void PlayEatBonus() => PlayBytes(_eatBonusWav);
    public void PlayTurn() => PlayBytes(_turnWav);
    public void PlayGameOver() => PlayBytes(_gameOverWav);
    public void PlayVictory() => PlayBytes(_victoryWav);
    public void PlayClick() => PlayBytes(_clickWav);

    private void PlayBytes(byte[] wavData)
    {
        if (IsMuted) return;

        try
        {
            Task.Run(() =>
            {
                try
                {
                    using var ms = new MemoryStream(wavData);
                    using var player = new SoundPlayer(ms);
                    player.PlaySync();
                }
                catch
                {
                    // Audio fallback if sound device is unavailable
                }
            });
        }
        catch
        {
            // Ignore audio device exceptions
        }
    }

    private static byte[] GenerateChirpWav(double startFreq, double endFreq, double durationSec, double volume)
    {
        int sampleRate = 22050;
        int sampleCount = (int)(sampleRate * durationSec);
        short[] samples = new short[sampleCount];

        double phase = 0;
        for (int i = 0; i < sampleCount; i++)
        {
            double t = (double)i / sampleCount;
            double freq = startFreq + (endFreq - startFreq) * t;
            phase += 2.0 * Math.PI * freq / sampleRate;

            // Envelope: sharp attack, gentle decay
            double env = Math.Sin(Math.PI * t);
            double sample = Math.Sin(phase) * env * volume;
            samples[i] = (short)(sample * short.MaxValue);
        }

        return CreateWav(samples, sampleRate);
    }

    private static byte[] GenerateDoubleChimeWav(double freq1, double freq2, double durationSec, double volume)
    {
        int sampleRate = 22050;
        int totalSamples = (int)(sampleRate * durationSec);
        int halfSamples = totalSamples / 2;
        short[] samples = new short[totalSamples];

        double phase = 0;
        for (int i = 0; i < totalSamples; i++)
        {
            double freq = (i < halfSamples) ? freq1 : freq2;
            phase += 2.0 * Math.PI * freq / sampleRate;

            int segI = i < halfSamples ? i : i - halfSamples;
            double segT = (double)segI / halfSamples;
            double env = Math.Sin(Math.PI * segT) * Math.Pow(1.0 - segT, 0.4);

            double sample = Math.Sin(phase) * env * volume;
            samples[i] = (short)(sample * short.MaxValue);
        }

        return CreateWav(samples, sampleRate);
    }

    private static byte[] GenerateToneWav(double freq, double durationSec, double volume, bool isSquare)
    {
        int sampleRate = 22050;
        int sampleCount = (int)(sampleRate * durationSec);
        short[] samples = new short[sampleCount];

        double phase = 0;
        for (int i = 0; i < sampleCount; i++)
        {
            phase += 2.0 * Math.PI * freq / sampleRate;
            double t = (double)i / sampleCount;
            double env = 1.0 - t;

            double raw = Math.Sin(phase);
            if (isSquare) raw = raw > 0 ? 0.7 : -0.7;

            double sample = raw * env * volume;
            samples[i] = (short)(sample * short.MaxValue);
        }

        return CreateWav(samples, sampleRate);
    }

    private static byte[] GenerateDescendingThudWav(double startFreq, double endFreq, double durationSec, double volume)
    {
        int sampleRate = 22050;
        int sampleCount = (int)(sampleRate * durationSec);
        short[] samples = new short[sampleCount];

        double phase = 0;
        for (int i = 0; i < sampleCount; i++)
        {
            double t = (double)i / sampleCount;
            double freq = startFreq - (startFreq - endFreq) * Math.Pow(t, 0.5);
            phase += 2.0 * Math.PI * freq / sampleRate;

            // Envelope with wobble
            double wobble = 1.0 + 0.25 * Math.Sin(2.0 * Math.PI * 18.0 * t);
            double env = Math.Pow(1.0 - t, 1.2) * wobble;

            double sample = Math.Sin(phase) * env * volume;
            samples[i] = (short)(Math.Clamp(sample, -1.0, 1.0) * short.MaxValue);
        }

        return CreateWav(samples, sampleRate);
    }

    private static byte[] GenerateVictoryFanfareWav()
    {
        int sampleRate = 22050;
        double[] notes = { 523.25, 659.25, 783.99, 1046.50 }; // C5, E5, G5, C6
        double noteDuration = 0.12;
        int noteSamples = (int)(sampleRate * noteDuration);
        int totalSamples = noteSamples * notes.Length;
        short[] samples = new short[totalSamples];

        for (int n = 0; n < notes.Length; n++)
        {
            double freq = notes[n];
            double phase = 0;
            for (int i = 0; i < noteSamples; i++)
            {
                phase += 2.0 * Math.PI * freq / sampleRate;
                double t = (double)i / noteSamples;
                double env = Math.Sin(Math.PI * t) * (1.0 - 0.2 * t);
                double sample = Math.Sin(phase) * env * 0.32;

                samples[n * noteSamples + i] = (short)(sample * short.MaxValue);
            }
        }

        return CreateWav(samples, sampleRate);
    }

    private static byte[] CreateWav(short[] samples, int sampleRate)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        int subchunk2Size = samples.Length * 2; // 16-bit mono = 2 bytes/sample
        int chunkSize = 36 + subchunk2Size;

        // RIFF header
        writer.Write("RIFF"u8);
        writer.Write(chunkSize);
        writer.Write("WAVE"u8);

        // fmt subchunk
        writer.Write("fmt "u8);
        writer.Write(16); // Subchunk1Size for PCM
        writer.Write((short)1); // AudioFormat: PCM
        writer.Write((short)1); // NumChannels: 1 (mono)
        writer.Write(sampleRate);
        writer.Write(sampleRate * 2); // ByteRate = SampleRate * NumChannels * BitsPerSample/8
        writer.Write((short)2); // BlockAlign = NumChannels * BitsPerSample/8
        writer.Write((short)16); // BitsPerSample

        // data subchunk
        writer.Write("data"u8);
        writer.Write(subchunk2Size);

        for (int i = 0; i < samples.Length; i++)
        {
            writer.Write(samples[i]);
        }

        return ms.ToArray();
    }
}
