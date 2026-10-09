using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Snapricot.Core.Platform;
using static Snapricot.Platform.Windows.Interop.NativeMethods;

namespace Snapricot.Platform.Windows;

/// <summary>
/// Plays a short shutter sound with <c>PlaySound(SND_MEMORY | SND_ASYNC)</c>. The WAV is synthesized once into
/// unmanaged memory (it must outlive async playback), so there's no audio asset to ship.
/// </summary>
internal sealed unsafe class Win32SoundService : ISoundService, IDisposable
{
    private readonly Lazy<nint> _wave = new(CreateShutterWave);

    public void PlayCaptureSound() => PlaySound((byte*)_wave.Value, 0, SND_MEMORY | SND_ASYNC | SND_NODEFAULT);

    public void Dispose()
    {
        if (_wave.IsValueCreated)
        {
            PlaySound(null, 0, 0); // stop before freeing the buffer it may be reading
            Marshal.FreeHGlobal(_wave.Value);
        }
    }

    /// <summary>Two soft, decaying noise clicks (~140 ms): "ka-chk".</summary>
    private static nint CreateShutterWave()
    {
        const int sampleRate = 44100;
        var samples = new short[sampleRate * 140 / 1000];
        var random = new Random(7); // deterministic: the same sound every time
        double smoothed = 0;

        for (var i = 0; i < samples.Length; i++)
        {
            var t = i / (double)sampleRate;
            var envelope = Click(t, start: 0.000, length: 0.030, gain: 1.0) + Click(t, start: 0.065, length: 0.045, gain: 0.7);
            smoothed += 0.35 * (((random.NextDouble() * 2) - 1) - smoothed); // one-pole low-pass: softer, less hissy
            samples[i] = (short)Math.Clamp(smoothed * envelope * 0.55 * short.MaxValue, short.MinValue, short.MaxValue);
        }

        var dataBytes = samples.Length * sizeof(short);
        var wave = new byte[44 + dataBytes];
        var span = wave.AsSpan();
        "RIFF"u8.CopyTo(span);
        BinaryPrimitives.WriteInt32LittleEndian(span[4..], 36 + dataBytes);
        "WAVEfmt "u8.CopyTo(span[8..]);
        BinaryPrimitives.WriteInt32LittleEndian(span[16..], 16);           // fmt chunk size
        BinaryPrimitives.WriteInt16LittleEndian(span[20..], 1);            // PCM
        BinaryPrimitives.WriteInt16LittleEndian(span[22..], 1);            // mono
        BinaryPrimitives.WriteInt32LittleEndian(span[24..], sampleRate);
        BinaryPrimitives.WriteInt32LittleEndian(span[28..], sampleRate * 2); // byte rate
        BinaryPrimitives.WriteInt16LittleEndian(span[32..], 2);            // block align
        BinaryPrimitives.WriteInt16LittleEndian(span[34..], 16);           // bits per sample
        "data"u8.CopyTo(span[36..]);
        BinaryPrimitives.WriteInt32LittleEndian(span[40..], dataBytes);
        MemoryMarshal.AsBytes(samples.AsSpan()).CopyTo(span[44..]);

        var buffer = Marshal.AllocHGlobal(wave.Length);
        wave.CopyTo(new Span<byte>((void*)buffer, wave.Length));
        return buffer;

        static double Click(double t, double start, double length, double gain) =>
            t < start || t > start + length ? 0 : gain * Math.Exp(-(t - start) / (length / 5));
    }
}
