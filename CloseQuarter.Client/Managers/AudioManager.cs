using Silk.NET.OpenAL;
using System.Collections.Generic;

namespace CloseQuarter.Client.Managers;

public static class AudioManager
{
    private static AL _al = null!;
    private static ALContext _alc = null!;
    private static unsafe Device* _device;
    private static unsafe Context* _context;

    private static uint _bgmBuffer;
    private static uint _bgmSource;


    private static readonly Dictionary<string, uint> _sfxBuffers = new();

    public static bool IsInitialized { get; private set; }

    public static unsafe void Initialize()
    {
        try
        {
            _al = AL.GetApi();
            _alc = ALContext.GetApi();

            _device = _alc.OpenDevice("");
            if (_device == null)
            {
                Console.WriteLine("[Audio Error] Could not open OpenAL device.");
                return;
            }

            _context = _alc.CreateContext(_device, null);
            _alc.MakeContextCurrent(_context);

            IsInitialized = true;
            Console.WriteLine("[Audio] OpenAL initialized successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Audio Error] Could not initialize audio module: {ex.Message}");
        }
    }


    public static void PlayBGM(string wavPath, float volume = 0.5f)
    {
        if (!IsInitialized) return;

        StopBGM();

        try
        {
            var wav = WavLoader.LoadWav(wavPath);

            _bgmBuffer = _al.GenBuffer();
            _bgmSource = _al.GenSource();

            unsafe
            {
                fixed (byte* ptr = wav.Data)
                {
                    _al.BufferData(_bgmBuffer, wav.Format, ptr, wav.Data.Length, wav.SampleRate);
                }
            }

            _al.SetSourceProperty(_bgmSource, SourceInteger.Buffer, (int)_bgmBuffer);
            _al.SetSourceProperty(_bgmSource, SourceBoolean.Looping, true);
            SetBGMVolume(volume);

            _al.SourcePlay(_bgmSource);
            Console.WriteLine($"[Audio] Playing BGM: {wavPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Audio Error] Error playing BGM: {ex.Message}");
        }
    }

    public static void SetBGMVolume(float volume)
    {
        if (!IsInitialized || _bgmSource == 0) return;
        _al.SetSourceProperty(_bgmSource, SourceFloat.Gain, Math.Clamp(volume, 0f, 1f));
    }

    public static void StopBGM()
    {
        if (_bgmSource != 0)
        {
            _al.SourceStop(_bgmSource);
            _al.DeleteSource(_bgmSource);
            _bgmSource = 0;
        }

        if (_bgmBuffer != 0)
        {
            _al.DeleteBuffer(_bgmBuffer);
            _bgmBuffer = 0;
        }
    }



    public static void PlaySFX(string wavPath, float volume = 1.0f)
    {
        if (!IsInitialized) return;

        try
        {
            if (!_sfxBuffers.TryGetValue(wavPath, out uint buffer))
            {
                var wav = WavLoader.LoadWav(wavPath);
                buffer = _al.GenBuffer();

                unsafe
                {
                    fixed (byte* ptr = wav.Data)
                    {
                        _al.BufferData(buffer, wav.Format, ptr, wav.Data.Length, wav.SampleRate);
                    }
                }

                _sfxBuffers[wavPath] = buffer;
            }

            uint source = _al.GenSource();
            _al.SetSourceProperty(source, SourceInteger.Buffer, (int)buffer);
            _al.SetSourceProperty(source, SourceFloat.Gain, Math.Clamp(volume, 0f, 1f));
            _al.SourcePlay(source);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Audio Error] Error playing SFX: {ex.Message}");
        }
    }


    public static unsafe void Shutdown()
    {
        if (!IsInitialized) return;

        StopBGM();

        foreach (var buffer in _sfxBuffers.Values)
        {
            _al.DeleteBuffer(buffer);
        }
        _sfxBuffers.Clear();

        _alc.MakeContextCurrent(null);
        _alc.DestroyContext(_context);
        _alc.CloseDevice(_device);

        IsInitialized = false;
        Console.WriteLine("[Audio] OpenAL shut down successfully.");
    }
}














public static class WavLoader
{
    public record WavData(byte[] Data, BufferFormat Format, int SampleRate);

    public static WavData LoadWav(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"[Audio] WAV file not found: {filePath}");
        }

        using var stream = File.OpenRead(filePath);
        using var reader = new BinaryReader(stream);

        // Header WAV standard (RIFF)
        string riff = new string(reader.ReadChars(4));
        if (riff != "RIFF") throw new InvalidDataException("File is not a valid RIFF format.");

        reader.ReadInt32(); // ChunkSize
        string wave = new string(reader.ReadChars(4));
        if (wave != "WAVE") throw new InvalidDataException("File is not a valid WAVE format.");

        short channels = 0;
        int sampleRate = 0;
        short bitsPerSample = 0;
        byte[]? audioData = null;

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            string chunkId = new string(reader.ReadChars(4));
            int chunkSize = reader.ReadInt32();

            if (chunkId == "fmt ")
            {
                short audioFormat = reader.ReadInt16(); // 1 = PCM
                channels = reader.ReadInt16();
                sampleRate = reader.ReadInt32();
                reader.ReadInt32(); // ByteRate
                reader.ReadInt16(); // BlockAlign
                bitsPerSample = reader.ReadInt16();

                if (chunkSize > 16)
                    reader.ReadBytes(chunkSize - 16);
            }
            else if (chunkId == "data")
            {
                audioData = reader.ReadBytes(chunkSize);
                break;
            }
            else
            {
                reader.ReadBytes(chunkSize);
            }
        }

        if (audioData == null)
            throw new InvalidDataException("Audio data not found in WAV file.");

        BufferFormat format = (channels, bitsPerSample) switch
        {
            (1, 8) => BufferFormat.Mono8,
            (1, 16) => BufferFormat.Mono16,
            (2, 8) => BufferFormat.Stereo8,
            (2, 16) => BufferFormat.Stereo16,
            _ => throw new NotSupportedException($"Unsupported WAV format: {channels} channels, {bitsPerSample} bits per sample.")
        };

        return new WavData(audioData, format, sampleRate);
    }
}