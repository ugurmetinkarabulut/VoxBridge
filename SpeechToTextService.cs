using System.Speech.Recognition;

namespace VoxBridge;

internal sealed class SpeechToTextService : IDisposable
{
    private SpeechRecognitionEngine? engine;

    public bool HasOpenEngine => engine is not null;
    public bool IsReleasePending { get; private set; }

    public event EventHandler<SpeechRecognizedEventArgs>? SpeechRecognized;
    public event EventHandler<RecognizeCompletedEventArgs>? RecognitionCompleted;

    public IReadOnlyList<RecognizerInfo> GetRecognizers()
    {
        return SpeechRecognitionEngine.InstalledRecognizers();
    }

    public void Start(RecognizerInfo recognizer)
    {
        if (engine is not null)
            throw new InvalidOperationException("Mevcut dinleme oturumunun bitmesini bekleyin.");

        // Her oturum kendi motorunu açar. Oturum bittiğinde mikrofon serbest bırakılır.
        string errorMessage = "Seçilen konuşma tanıma motoru başlatılamadı.";
        try
        {
            engine = new SpeechRecognitionEngine(recognizer);

            errorMessage = "Seçilen motor serbest dikteyi (DictationGrammar) yükleyemedi. Başka bir dil deneyin.";
            engine.LoadGrammar(new DictationGrammar());

            errorMessage = "Varsayılan mikrofon açılamadı. Ses giriş aygıtını ve mikrofon izinlerini kontrol edin.";
            engine.SetInputToDefaultAudioDevice();

            errorMessage = "Dinleme başlatılamadı. Mikrofonu ve seçilen konuşma tanıma motorunu kontrol edin.";
            engine.SpeechRecognized += OnSpeechRecognized;
            engine.RecognizeCompleted += OnRecognitionCompleted;
            engine.RecognizeAsync(RecognizeMode.Multiple);
        }
        catch (Exception exception)
        {
            try
            {
                ReleaseEngine();
            }
            catch (Exception cleanupError)
            {
                throw new InvalidOperationException(errorMessage, new AggregateException(exception, cleanupError));
            }
            throw new InvalidOperationException(errorMessage, exception);
        }
    }

    public void Stop()
    {
        engine?.RecognizeAsyncCancel();
    }

    public bool IsCurrentEngine(object? sender)
    {
        return engine is not null && !IsReleasePending && ReferenceEquals(engine, sender);
    }

    public void ReleaseEngine(bool cancel = false)
    {
        if (engine is null)
            return;

        bool firstAttempt = !IsReleasePending;
        IsReleasePending = true;
        try
        {
            if (firstAttempt)
            {
                engine.SpeechRecognized -= OnSpeechRecognized;
                engine.RecognizeCompleted -= OnRecognitionCompleted;
            }

            if (cancel)
                engine.RecognizeAsyncCancel();
        }
        finally
        {
            // Dispose başarısızsa motoru sakla. Başka bir işlem açmadan temizliği tekrar dene.
            engine.Dispose();
            engine = null;
            IsReleasePending = false;
        }
    }

    public void Dispose()
    {
        // Kapanırken yeni arayüz olayları göndermeden dinlemeyi iptal et.
        ReleaseEngine(cancel: true);
    }

    private void OnSpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
    {
        SpeechRecognized?.Invoke(sender, e);
    }

    private void OnRecognitionCompleted(object? sender, RecognizeCompletedEventArgs e)
    {
        RecognitionCompleted?.Invoke(sender, e);
    }
}
