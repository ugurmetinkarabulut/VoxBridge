# VoxBridge kod açıklamaları

## 1. Text-to-Speech nedir?

Text-to-Speech (TTS), yazıyı konuşma sesine dönüştürür. VoxBridge'de **Seslendir** düğmesi `MainForm.StartSpeaking` metodunu çağırır. Bu metot yazıyı, seçilen sesi ve hızı `TextToSpeechService.Speak` metoduna verir.

## 2. Speech-to-Text nedir?

Speech-to-Text (STT), konuşmayı yazıya dönüştürür. **Dinlemeyi Başlat** düğmesiyle açılan oturum, mikrofon sesini seçili motora verir. Motorun tanıdığı ifadeler sağdaki metin kutusunda satır satır birikir.

## 3. System.Speech nedir?

Windows'un konuşma motorlarına C# kodundan erişmemizi sağlayan kütüphanedir. `System.Speech.Synthesis` altında seslendirme, `System.Speech.Recognition` altında tanıma sınıfları bulunur. Dil ve ses listeleri Windows'ta kurulu motorlara bağlıdır.

## 4. NuGet paketi nedir?

Bir .NET kütüphanesini projeye eklemek için kullanılan dağıtım paketidir. `VoxBridge.csproj` içindeki aşağıdaki satır sürümü belirler. `dotnet restore` bu bağımlılığı çözer ve derlemeye hazırlar.

```xml
<PackageReference Include="System.Speech" Version="10.0.15" />
```

## 5. SpeechSynthesizer ne işe yarar?

Bu nesne Windows üzerinden metni seslendirir. `TextToSpeechService.GetVoices`, `GetInstalledVoices` ile aldığı listeden etkin sesleri seçer. `Speak` metodumuz önce `SelectVoice` ile sesi seçer, ardından `Rate` değerini ayarlayıp okumayı başlatır.

## 6. SpeechRecognitionEngine ne işe yarar?

Bu nesne sesi alır ve tanıdığı konuşmayı olaylarla bildirir. `SpeechToTextService.GetRecognizers`, `InstalledRecognizers` ile Windows'taki motorları listeler. `Start` seçilen motorla yeni bir oturum açar, `SetInputToDefaultAudioDevice` ile varsayılan mikrofonu kullanır ve `RecognizeAsync(RecognizeMode.Multiple)` ile art arda gelen ifadeleri dinler.

## 7. DictationGrammar nedir?

Tanıma motorunun serbest konuşmayı metne dönüştürmesi için yüklenen dikte dil bilgisidir. `Start` içinde `engine.LoadGrammar(new DictationGrammar())` çağrısıyla yüklenir. Seçilen motor dikteyi yükleyemezse arayüz başka bir dil denenebileceğini bildirir.

## 8. Speak ve SpeakAsync arasındaki fark nedir?

`SpeechSynthesizer.Speak` çağıran kodu metin bitene kadar bekletir. `SpeakAsync` isteği başlatıp hemen döner. VoxBridge bu sayede okuma sırasında **Durdur** düğmesine yanıt verir. Servisin adı `Speak` olan metodu, motorun `SpeakAsync` metodunu çağırır.

## 9. SpeechRecognized olayı nasıl çalışır?

Motor bir ifadeyi tanıyınca `SpeechRecognized` olayını yayar. Servis bu olayı pencereye iletir. `MainForm.OnSpeechRecognized`, `e.Result.Text` ile ifadeyi alır ve `AppendText` ile mevcut yazının sonuna ekler. Oturum durduruluyorsa kuyrukta kalan sonuç işlenmez.

## 10. Event handler nedir?

Bir olaya yanıt veren metottur. Örneğin `recognition.SpeechRecognized += OnSpeechRecognized` satırı, tanıma olayı geldiğinde çalışacak metodu kaydeder. `-=` aboneliği kaldırır. Benzer biçimde `Click` bir düğmeye basıldığında, `SpeakCompleted` ise asenkron seslendirme tamamlandığında çalışacak kodu bağlar.

## 11. ComboBox ve TrackBar nasıl kullanılır?

`voices` ve `recognizers` adlı `ComboBox` bileşenleri yüklü seçenekleri gösterir. `SelectedItem` seçilen ses veya tanıma motorunu verir. `rate` adlı `TrackBar` üzerinden −10 ile +10 arasında hız seçilir. `ValueChanged` olayı hız etiketini günceller. 0 normal hızdır.

## 12. Enum nedir ve burada nasıl kullanılır?

`enum`, bir değişkenin alabileceği adlandırılmış değerleri tanımlar. `AudioMode` içinde seslendirme için `Speaking`, dinleme için `Listening` değeri vardır. `AudioSession.ActiveMode` boşsa yeni işlem başlayabilir. Durdurma istenince `IsStopping` işaretlenir ve tamamlanma olayına kadar iki başlatma düğmesi de pasif kalır.

## 13. Invoke ve BeginInvoke neden kullanılır?

Windows Forms kontrolleri, pencereyi oluşturan iş parçacığından güncellenmelidir. Motor olayları başka bir iş parçacığından gelebilir. `MainForm.RunOnUiThread`, `BeginInvoke` ile güncellemeyi pencerenin kuyruğuna ekler ve olayın geldiği iş parçacığını bekletmez. `Invoke` ise güncellemenin yapılmasını bekler.

## 14. IDisposable ve Dispose nedir?

`IDisposable`, nesneyle işimiz bittiğinde `Dispose` ile kaynaklarını bırakabileceğimizi gösterir. `MainForm.Dispose` kapanışta olay aboneliklerini kaldırır ve servisleri kapatır. `SpeechToTextService.ReleaseEngine` biten oturumun motorunu ve mikrofon kaynağını bırakır. `TextToSpeechService.Dispose` ise okumayı iptal edip seslendirme motorunu bırakır.

## 15. try-catch neden kullanılır?

Programın kontrolü dışındaki işlemler hata verebilir. Örneğin Windows varsayılan mikrofonu açamazsa `SpeechToTextService.Start` motoru kapatır ve açıklayıcı bir hata iletir. `MainForm` hatayı yakalayıp durum etiketinde gösterir. Bir `finally` bloğu, hata oluştuğunda da kaynak bırakma adımının denenmesini sağlar.

## 16. Sınıfların görevlerini neden ayırdık?

| Sınıf | Görevi |
| --- | --- |
| `Program` | Uygulamayı hazırlar ve ana pencereyi açar. |
| `MainForm` | Arayüzü kurar, düğmeleri ve durum etiketlerini günceller. |
| `TextToSpeechService` | Sesleri listeler ve seslendirme motorunu yönetir. |
| `SpeechToTextService` | Tanıma motorlarını listeler, mikrofon oturumunu yönetir ve motor olaylarını iletir. |
| `AudioSession` | Hangi ses işleminin etkin olduğunu ve durdurmanın beklenip beklenmediğini tutar. |

Bu ayrım sayesinde ses servisleri Windows Forms kontrollerine erişmez. Örneğin durdurma sırasında ikinci bir ses işleminin başlatılmadığını, formu açmadan `AudioSession` üzerinde kontrol edebiliriz.

## Sözlü sınav için beş kod değiştirme sorusu

1. **Varsayılan konuşma hızını +2 yapmak için neyi değiştirirsiniz?**

   `MainForm` içindeki `rate` tanımına `Value = 2` ekleyin ve ilk `rateLabel` metnini `"Konuşma hızı · +2"` yapın. Kullanıcı hızı değiştirince mevcut `ValueChanged` kodu etiketi günceller.

2. **Tanınan ifadeleri aynı paragrafta nasıl gösterirsiniz?**

   `MainForm.OnSpeechRecognized` içinde `recognizedText.AppendText(Environment.NewLine)` yerine `recognizedText.AppendText(" ")` yazın. İlk ifade için boşluk eklenmemesini sağlayan `TextLength > 0` koşulunu koruyun.

3. **Seslendirme hızını −5 ile +5 arasına nasıl sınırlarsınız?**

   `TextToSpeechService` içinde `MinRate = -5` ve `MaxRate = 5` yapın. `ValidateSpeech` hata metnini de bu değerlerle güncelleyin. `MainForm` kaydırıcısı aynı sabitleri kullandığı için yeni aralığı otomatik alır.

4. **Dinleme temizlenemezse Durdur düğmesi nasıl yeniden kullanılabilir?**

   `MainForm.UpdateControls` içinde düğmeyi `listening && (!audio.IsStopping || recognition.IsReleasePending)` koşuluyla etkinleştirin. `StopListening` içinde `IsReleasePending` doğruysa iptali tekrar göndermek yerine `FinishListening` ile kaynak temizliğini yeniden deneyin. Bu akış projede uygulanmıştır.

5. **Her ses işleminde yalnızca doğru tamamlanma bildiriminin durumu sıfırladığını nasıl kontrol edersiniz?**

   Bir `AudioSession` oluşturun. `TryStart(AudioMode.Speaking)` ardından yanlış türde bir bildirim gibi `Complete(AudioMode.Listening)` çağırın. `ActiveMode` hâlâ `Speaking` olmalı. Sonra `Complete(AudioMode.Speaking)` çağırdığınızda `ActiveMode` boş olmalı. Kontrolü testte görünür kılmak için her çağrıdan sonra beklenen değeri bir `if` ile denetleyin ve farklıysa hata fırlatın.
