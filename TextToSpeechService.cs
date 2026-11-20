using System.Speech.Synthesis;

namespace VoxBridge;

internal sealed class TextToSpeechService : IDisposable
{
    public const int MinRate = -10;
    public const int MaxRate = 10;

    private readonly SpeechSynthesizer synthesizer = new();

    public TextToSpeechService()
    {
        try
        {
            synthesizer.SpeakCompleted += OnSpeakCompleted;
        }
        catch
        {
            synthesizer.Dispose();
            throw;
        }
    }

    public event EventHandler<SpeakCompletedEventArgs>? SpeakCompleted;

    public List<VoiceInfo> GetVoices()
    {
        return synthesizer.GetInstalledVoices()
            .Where(voice => voice.Enabled)
            .Select(voice => voice.VoiceInfo)
            .ToList();
    }

    public Prompt Speak(string text, VoiceInfo voice, int rate)
    {
        ValidateSpeech(text, rate);
        synthesizer.SelectVoice(voice.Name);
        synthesizer.Rate = rate;

        // SpeakAsync, metni okurken pencerenin açık ve kullanılabilir kalmasını sağlar.
        return synthesizer.SpeakAsync(text);
    }

    internal static void ValidateSpeech(string text, int rate)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Seslendirmek için bir metin yazın.");

        if (rate < MinRate || rate > MaxRate)
            throw new ArgumentOutOfRangeException(nameof(rate), "Konuşma hızı -10 ile 10 arasında olmalı.");
    }

    public void Stop()
    {
        synthesizer.SpeakAsyncCancelAll();
    }

    public void Dispose()
    {
        try
        {
            synthesizer.SpeakCompleted -= OnSpeakCompleted;
            Stop();
        }
        finally
        {
            synthesizer.Dispose();
        }
    }

    private void OnSpeakCompleted(object? sender, SpeakCompletedEventArgs e)
    {
        SpeakCompleted?.Invoke(this, e);
    }
}
