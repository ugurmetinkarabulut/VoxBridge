using System.Diagnostics;
using System.Speech.Recognition;
using System.Speech.Synthesis;

namespace VoxBridge;

internal sealed class MainForm : Form
{
    private static readonly Color Background = Color.FromArgb(13, 19, 35);
    private static readonly Color PanelColor = Color.FromArgb(23, 32, 53);
    private static readonly Color FieldColor = Color.FromArgb(11, 18, 34);
    private static readonly Color TextColor = Color.FromArgb(241, 245, 255);
    private static readonly Color Muted = Color.FromArgb(167, 183, 209);
    private static readonly Color Purple = Color.FromArgb(202, 180, 255);
    private static readonly Color Blue = Color.FromArgb(141, 209, 255);
    private static readonly Color Green = Color.FromArgb(103, 226, 174);
    private static readonly Color Red = Color.FromArgb(255, 170, 176);

    private readonly AudioSession audio = new();
    private readonly SpeechToTextService recognition = new();
    private readonly System.Windows.Forms.Timer recognitionStopTimer = new() { Interval = 3000 };
    private TextToSpeechService? speech;
    private Prompt? currentPrompt;
    private bool closeAfterListening;
    private volatile bool closing;

    private readonly RichTextBox speechText = CreateTextBox("Seslendirilecek metin", readOnly: false);
    private readonly RichTextBox recognizedText = CreateTextBox("Tanınan metin", readOnly: true);
    private readonly ComboBox voices = CreateComboBox("Seslendirme sesi");
    private readonly ComboBox recognizers = CreateComboBox("Tanıma dili ve motoru");
    private readonly TrackBar rate = new()
    {
        AccessibleName = "Konuşma hızı",
        Minimum = TextToSpeechService.MinRate,
        Maximum = TextToSpeechService.MaxRate,
        TickFrequency = 5,
        BackColor = PanelColor,
        AutoSize = false,
        Height = 30,
        Dock = DockStyle.Fill,
        Margin = Padding.Empty
    };

    private readonly Label rateLabel = CreateLabel("Konuşma hızı · 0 (normal)", 10, Muted);
    private readonly Button speakButton = CreateButton("Seslendir", Color.FromArgb(107, 64, 194));
    private readonly Button listenButton = CreateButton("Dinlemeyi Başlat", Color.FromArgb(19, 94, 155));
    private readonly Button stopSpeechButton = CreateButton("Durdur", Color.FromArgb(146, 45, 61));
    private readonly Button stopListeningButton = CreateButton("Dinlemeyi Durdur", Color.FromArgb(146, 45, 61));
    private readonly Label speechStatus = CreateStatusLabel("Seslendirme durumu");
    private readonly Label recognitionStatus = CreateStatusLabel("Mikrofon durumu");

    private (string Text, Color Color) speechMessage = ("Sesler yükleniyor…", Muted);
    private (string Text, Color Color) recognitionMessage = ("Tanıma motorları yükleniyor…", Muted);

    public MainForm()
    {
        SuspendLayout();
        Text = "VoxBridge";
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1140, 716);
        MinimumSize = new Size(990, 630);
        BackColor = Background;
        ForeColor = TextColor;
        Font = new Font("Segoe UI", 10);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(26, 20, 26, 24),
            ColumnCount = 2,
            RowCount = 3
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 53));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 41));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = CreateLabel("VoxBridge", 28, TextColor, FontStyle.Bold);
        var subtitle = CreateLabel("Yazdığınız metni seslendirin veya mikrofonla konuşarak metin oluşturun.", 11, Muted);
        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(subtitle, 0, 1);
        layout.SetColumnSpan(title, 2);
        layout.SetColumnSpan(subtitle, 2);

        var speechOptions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        speechOptions.Margin = Padding.Empty;
        speechOptions.RowStyles.Add(new RowStyle(SizeType.Absolute, 23));
        speechOptions.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        speechOptions.Controls.Add(rateLabel, 0, 0);
        speechOptions.Controls.Add(rate, 0, 1);

        var microphoneHint = CreateLabel("Windows’un varsayılan mikrofonu kullanılır.", 10, Muted);
        var clearSpeech = CreateButton("Temizle", Color.FromArgb(46, 58, 85));
        var clearRecognition = CreateButton("Temizle", Color.FromArgb(46, 58, 85));
        clearSpeech.AccessibleName = "Seslendirilecek metni temizle";
        clearRecognition.AccessibleName = "Tanınan metni temizle";

        var speechPanel = CreateSection("Text-to-Speech", Purple, "Seslendirilecek metin",
            speechText, "Ses ve dil", voices, speechOptions,
            CreateButtonRow(speakButton, stopSpeechButton, clearSpeech), speechStatus);
        speechPanel.Margin = new Padding(0, 0, 8, 0);
        var recognitionPanel = CreateSection("Speech-to-Text", Blue, "Tanınan metin",
            recognizedText, "Tanıma dili ve motoru", recognizers, microphoneHint,
            CreateButtonRow(listenButton, stopListeningButton, clearRecognition), recognitionStatus);
        recognitionPanel.Margin = new Padding(8, 0, 0, 0);

        layout.Controls.Add(speechPanel, 0, 2);
        layout.Controls.Add(recognitionPanel, 1, 2);
        Controls.Add(layout);

        voices.Format += (_, e) =>
        {
            if (e.ListItem is VoiceInfo voice)
                e.Value = $"{voice.Name} · {voice.Culture.NativeName}";
        };
        recognizers.Format += (_, e) =>
        {
            if (e.ListItem is RecognizerInfo recognizer)
                e.Value = $"{recognizer.Culture.NativeName} · {recognizer.Name}";
        };
        rate.ValueChanged += (_, _) =>
        {
            string value = rate.Value == 0 ? "0 (normal)" : rate.Value.ToString("+0;-0");
            rateLabel.Text = $"Konuşma hızı · {value}";
        };

        speakButton.Click += (_, _) => StartSpeaking();
        stopSpeechButton.Click += (_, _) => StopSpeaking();
        clearSpeech.Click += (_, _) =>
        {
            StopSpeaking();
            speechText.Clear();
        };
        listenButton.Click += (_, _) => StartListening();
        stopListeningButton.Click += (_, _) => StopListening();
        clearRecognition.Click += (_, _) => recognizedText.Clear();

        recognition.SpeechRecognized += OnSpeechRecognized;
        recognition.RecognitionCompleted += OnRecognitionCompleted;
        recognitionStopTimer.Tick += (_, _) =>
        {
            // Tamamlanma olayı gelmezse motoru kapat ve sonucu arayüze bildir.
            FinishListening("Durdurma yanıtı alınamadı. Motor kapatıldı ve mikrofon bırakıldı.", Red, cancel: true);
        };
        Shown += (_, _) => LoadSpeechOptions();

        UpdateControls();
        ResumeLayout();
    }

    private void LoadSpeechOptions()
    {
        // İki liste bağımsız yüklenir. Bir motordaki hata diğer özelliği etkilemez.
        try
        {
            speech = new TextToSpeechService();
            speech.SpeakCompleted += OnSpeakCompleted;
            foreach (var voice in speech.GetVoices().OrderByDescending(v => v.Culture.TwoLetterISOLanguageName == "tr"))
                voices.Items.Add(voice);

            if (voices.Items.Count > 0)
                voices.SelectedIndex = 0;

            speechMessage = voices.Items.Count > 0
                ? ("Seslendirmeye hazır.", Muted)
                : ("Kullanılabilir ses bulunamadı. Windows konuşma ayarlarından ses ekleyin.", Red);
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            voices.Items.Clear();
            speechMessage = ("Seslendirme motoru yüklenemedi. Windows konuşma ayarlarını kontrol edin.", Red);
        }

        try
        {
            foreach (var recognizer in recognition.GetRecognizers().OrderByDescending(r => r.Culture.TwoLetterISOLanguageName == "tr"))
                recognizers.Items.Add(recognizer);

            if (recognizers.Items.Count > 0)
                recognizers.SelectedIndex = 0;

            recognitionMessage = recognizers.Items.Count > 0
                ? ("Mikrofon kapalı. Dinlemeye hazır.", Muted)
                : ("Konuşma tanıma motoru bulunamadı. Windows dil seçeneklerini kontrol edin.", Red);
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            recognizers.Items.Clear();
            recognitionMessage = ("Tanıma motorları yüklenemedi. Windows dil seçeneklerini kontrol edin.", Red);
        }

        UpdateControls();
    }

    private void StartSpeaking()
    {
        if (speech is null || voices.SelectedItem is not VoiceInfo voice || !audio.TryStart(AudioMode.Speaking))
            return;

        try
        {
            currentPrompt = speech.Speak(speechText.Text, voice, rate.Value);
            speechMessage = ("Seslendiriliyor…", Green);
        }
        catch (ArgumentException exception) when (string.IsNullOrWhiteSpace(speechText.Text))
        {
            audio.Complete(AudioMode.Speaking);
            speechMessage = (exception.Message, Red);
            speechText.Focus();
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            audio.Complete(AudioMode.Speaking);
            speechMessage = ("Seslendirme başlatılamadı. Seçilen sesi ve Windows ses çıkışını kontrol edin.", Red);
        }

        UpdateControls();
    }

    private void StopSpeaking()
    {
        if (speech is null || audio.ActiveMode != AudioMode.Speaking || audio.IsStopping)
            return;

        try
        {
            speech.Stop();
            audio.TryStop(AudioMode.Speaking);
            speechMessage = ("Seslendirme durduruluyor…", Muted);
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            speechMessage = ("Seslendirme durdurulamadı. Tekrar deneyin.", Red);
        }

        UpdateControls();
    }

    private void OnSpeakCompleted(object? sender, SpeakCompletedEventArgs e)
    {
        RunOnUiThread(() =>
        {
            if (currentPrompt is null || !ReferenceEquals(currentPrompt, e.Prompt))
                return;

            currentPrompt = null;
            audio.Complete(AudioMode.Speaking);
            if (e.Cancelled || e.Error is OperationCanceledException)
            {
                speechMessage = ("Seslendirme durduruldu.", Muted);
            }
            else if (e.Error is not null)
            {
                Debug.WriteLine(e.Error);
                speechMessage = ("Seslendirme hatası. Seçilen sesi ve Windows ses çıkışını kontrol edin.", Red);
            }
            else
            {
                speechMessage = ("Seslendirme tamamlandı.", Muted);
            }

            UpdateControls();
        });
    }

    private void StartListening()
    {
        if (recognizers.SelectedItem is not RecognizerInfo recognizer || !audio.TryStart(AudioMode.Listening))
            return;

        try
        {
            recognition.Start(recognizer);
            recognitionMessage = ($"Mikrofon açık · {recognizer.Culture.NativeName}", Green);
        }
        catch (Exception exception)
        {
            HandleRecognitionError(exception, exception.Message);
        }

        UpdateControls();
    }

    private void StopListening()
    {
        if (recognition.IsReleasePending)
        {
            FinishListening("Dinleme durduruldu. Mikrofon kapalı.", Muted);
            return;
        }

        if (!audio.TryStop(AudioMode.Listening))
            return;

        recognitionMessage = ("Dinleme durduruluyor…", Muted);
        try
        {
            recognition.Stop();
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            recognitionMessage = ("İptal isteği başarısız. Motorun kapanması bekleniyor.", Red);
        }

        recognitionStopTimer.Start();
        UpdateControls();
    }

    private void OnSpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
    {
        string text = e.Result.Text;
        RunOnUiThread(() =>
        {
            if (!recognition.IsCurrentEngine(sender) || audio.IsStopping)
                return;

            // Yeni cümleyi önceki tanınan metni silmeden sonuna ekle.
            if (recognizedText.TextLength > 0)
                recognizedText.AppendText(Environment.NewLine);

            recognizedText.AppendText(text);
        });
    }

    private void OnRecognitionCompleted(object? sender, RecognizeCompletedEventArgs e)
    {
        RunOnUiThread(() =>
        {
            if (!recognition.IsCurrentEngine(sender))
                return;

            if (e.Error is not null)
            {
                Debug.WriteLine(e.Error);
                FinishListening("Dinleme sona erdi. Windows ses girişini ve mikrofon izinlerini kontrol edin.", Red);
            }
            else
            {
                FinishListening(e.Cancelled ? "Dinleme durduruldu. Mikrofon kapalı." : "Dinleme bitti. Mikrofon kapalı.", Muted);
            }
        });
    }

    private void FinishListening(string message, Color color, bool cancel = false)
    {
        recognitionStopTimer.Stop();
        try
        {
            recognition.ReleaseEngine(cancel);
            audio.Complete(AudioMode.Listening);
            recognitionMessage = (message, color);
        }
        catch (Exception exception)
        {
            HandleRecognitionError(exception, "Durdurma sırasında hata oluştu. Mikrofon kapalı.");
        }

        if (closeAfterListening && !recognition.HasOpenEngine)
        {
            RunOnUiThread(() =>
            {
                closeAfterListening = false;
                Close();
                if (!closing)
                    UpdateControls();
            });
        }
        else
        {
            closeAfterListening = false;
        }

        UpdateControls();
    }

    private void HandleRecognitionError(Exception exception, string message)
    {
        Debug.WriteLine(exception);
        if (recognition.HasOpenEngine)
        {
            audio.TryStop(AudioMode.Listening);
            recognitionMessage = ("Mikrofon kapatılamadı. Dinlemeyi Durdur ile tekrar deneyin veya pencereyi kapatın.", Red);
        }
        else
        {
            audio.Complete(AudioMode.Listening);
            recognitionMessage = (message, Red);
        }
    }

    private void RunOnUiThread(Action action)
    {
        if (closing || !IsHandleCreated)
            return;

        try
        {
            // Ses olayları farklı iş parçacıklarından gelebilir.
            BeginInvoke(() =>
            {
                if (!closing)
                    action();
            });
        }
        catch (InvalidOperationException) when (closing || !IsHandleCreated)
        {
            // Olay kuyruğa alınırken pencere kapanmış olabilir.
        }
    }

    private void UpdateControls()
    {
        bool idle = audio.ActiveMode is null && !closeAfterListening;
        bool speaking = audio.ActiveMode == AudioMode.Speaking;
        bool listening = audio.ActiveMode == AudioMode.Listening;

        speakButton.Enabled = idle && voices.Items.Count > 0;
        listenButton.Enabled = idle && recognizers.Items.Count > 0;
        stopSpeechButton.Enabled = speaking && !audio.IsStopping;
        stopListeningButton.Enabled = listening && (!audio.IsStopping || recognition.IsReleasePending);
        voices.Enabled = speakButton.Enabled;
        rate.Enabled = speakButton.Enabled;
        recognizers.Enabled = listenButton.Enabled;

        (speechStatus.Text, speechStatus.ForeColor) = listening && voices.Items.Count > 0
            ? ("Seslendirme için dinlemenin bitmesini bekleyin.", Muted)
            : speechMessage;
        (recognitionStatus.Text, recognitionStatus.ForeColor) = speaking && recognizers.Items.Count > 0
            ? ("Dinlemek için seslendirmenin bitmesini bekleyin.", Muted)
            : recognitionMessage;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (e.Cancel || e.CloseReason != CloseReason.UserClosing ||
            audio.ActiveMode != AudioMode.Listening || recognition.IsReleasePending)
            return;

        // Motorun arka plandaki başlangıcı bitmeden Dispose çağırmamak için önce dinlemeyi durdur.
        e.Cancel = true;
        closeAfterListening = true;
        StopListening();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !closing)
        {
            closing = true;
            recognitionStopTimer.Dispose();

            recognition.SpeechRecognized -= OnSpeechRecognized;
            recognition.RecognitionCompleted -= OnRecognitionCompleted;
            DisposeAudioService(recognition);

            if (speech is not null)
            {
                speech.SpeakCompleted -= OnSpeakCompleted;
                DisposeAudioService(speech);
            }
        }

        base.Dispose(disposing);
    }

    private static void DisposeAudioService(IDisposable service)
    {
        try
        {
            service.Dispose();
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }

    private static TableLayoutPanel CreateSection(string title, Color accent, string textLabel,
        RichTextBox text, string choiceLabel, ComboBox choices, Control options, Control actions, Label status)
    {
        var panel = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = 9,
            Dock = DockStyle.Fill,
            Padding = new Padding(18, 15, 18, 16),
            BackColor = PanelColor
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 3));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

        panel.Controls.Add(new Panel { BackColor = accent, Dock = DockStyle.Fill, Margin = Padding.Empty }, 0, 0);
        panel.Controls.Add(CreateLabel(title, 19, accent, FontStyle.Bold), 0, 1);
        panel.Controls.Add(CreateLabel(textLabel, 10, Muted), 0, 2);
        panel.Controls.Add(text, 0, 3);
        panel.Controls.Add(CreateLabel(choiceLabel, 10, Muted), 0, 4);
        panel.Controls.Add(choices, 0, 5);
        panel.Controls.Add(options, 0, 6);
        panel.Controls.Add(actions, 0, 7);
        panel.Controls.Add(status, 0, 8);
        return panel;
    }

    private static TableLayoutPanel CreateButtonRow(Button start, Button stop, Button clear)
    {
        var row = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Margin = Padding.Empty };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 23));
        clear.Margin = Padding.Empty;
        row.Controls.Add(start, 0, 0);
        row.Controls.Add(stop, 1, 0);
        row.Controls.Add(clear, 2, 0);
        return row;
    }

    private static Button CreateButton(string text, Color color)
    {
        var button = new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            BackColor = color,
            ForeColor = TextColor,
            UseVisualStyleBackColor = false,
            Margin = new Padding(0, 0, 8, 0)
        };
        button.FlatAppearance.BorderSize = 0;
        button.EnabledChanged += (_, _) => button.BackColor = button.Enabled ? color : FieldColor;
        return button;
    }

    private static Label CreateLabel(string text, float size, Color color, FontStyle style = FontStyle.Regular)
    {
        return new Label
        {
            Text = text,
            Font = new Font("Segoe UI", size, style),
            ForeColor = color,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = Padding.Empty
        };
    }

    private static Label CreateStatusLabel(string name)
    {
        var label = CreateLabel(string.Empty, 10, Muted);
        label.AccessibleName = name;
        label.AccessibleRole = AccessibleRole.StatusBar;
        label.BackColor = FieldColor;
        label.Margin = new Padding(0, 10, 0, 0);
        label.Padding = new Padding(10, 0, 10, 0);
        return label;
    }

    private static RichTextBox CreateTextBox(string name, bool readOnly)
    {
        return new RichTextBox
        {
            AccessibleName = name,
            ReadOnly = readOnly,
            DetectUrls = false,
            BorderStyle = BorderStyle.None,
            BackColor = FieldColor,
            ForeColor = TextColor,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 8),
            Font = new Font("Segoe UI", 12)
        };
    }

    private static ComboBox CreateComboBox(string name)
    {
        return new ComboBox
        {
            AccessibleName = name,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FormattingEnabled = true,
            FlatStyle = FlatStyle.Flat,
            BackColor = FieldColor,
            ForeColor = TextColor,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty
        };
    }
}
