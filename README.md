# VoxBridge

VoxBridge, yazdığınız metni seslendiren ve mikrofondan aldığınız konuşmayı yazıya çeviren bir Windows masaüstü uygulamasıdır. Fırat Üniversitesi Yazılım Mühendisliği 2. sınıf **Nesne Tabanlı Programlama (YMH219)** dersi için geliştirilmiştir.

- **Öğrenci adı ve soyadı** `____________________`
- **Öğrenci numarası** `____________________`

## Teknolojiler

- C#, .NET 10 ve kodla oluşturulan Windows Forms arayüzü
- Hedef çerçeve `net10.0-windows`. `UseWindowsForms` ve `EnableWindowsTargeting` proje dosyasında etkin.
- Windows'un yerel konuşma motorlarına erişim için [System.Speech 10.0.15](https://www.nuget.org/packages/System.Speech/10.0.15)
- Geliştirme ve derleme ortamı macOS. Çalıştırma ortamı Windows.

## Özellikler

- **Text-to-Speech.** Bir metin yazın, yüklü seslerden birini seçin ve hızı −10 ile +10 arasında ayarlayın. **Seslendir** okumayı başlatır. **Durdur** okumayı iptal eder. **Temizle** metni siler ve varsa okumayı durdurur.
- **Speech-to-Text.** Yüklü bir tanıma motoru seçin ve **Dinlemeyi Başlat** düğmesine basın. Tanınan ifadeler çıktı alanının sonuna eklenir. **Dinlemeyi Durdur** oturumu kapatır ve mikrofonu serbest bırakır. **Temizle** yazıyı siler. Dinleme açıksa yeni ifadeler eklenmeye devam eder.
- Her panel etkin durumu gösterir. İki ses işlemi arasında geçiş yapmak için sürmekte olan işlemin tamamlanmasını bekleyin.
- VoxBridge yerel motorları kullanır. Uygulama için API anahtarı gerekmez. Ses dosyası kaydedilmez ve uygulama sesi ağ üzerinden göndermez.

## Windows'ta kurulum ve çalıştırma

### Visual Studio

1. Visual Studio 2026'yı (18.0 veya üstü) **.NET masaüstü geliştirme** iş yükü ve .NET 10 SDK ile yükleyin. [Microsoft .NET kurulum kılavuzu](https://learn.microsoft.com/dotnet/core/install/windows)
2. `VoxBridge.sln` dosyasını açın ve NuGet paketinin geri yüklenmesini bekleyin.
3. Başlangıç projesi olarak **VoxBridge** seçiliyken **F5** tuşuna basın.

### Komut satırı

Windows'a .NET 10 SDK kurduktan sonra proje klasöründe PowerShell veya Komut İstemi açın.

```powershell
dotnet restore VoxBridge.sln
dotnet run --project VoxBridge.csproj --no-restore
```

İlk paket geri yüklemesi NuGet'e ağ erişimi gerektirir. macOS'ta `dotnet restore` ve `dotnet build` ile Windows hedefi derlenebilir. Arayüzü açmak ve ses donanımını denemek için Windows kullanın.

## Windows 11 ses ayarları

### Mikrofon ve hoparlör

1. **Ayarlar → Sistem → Ses → Giriş** altında varsayılan mikrofonu seçin ve Windows'un giriş testinde ses aldığını kontrol edin. **Çıkış** altında hoparlör veya kulaklığı seçin.
2. **Ayarlar → Gizlilik ve güvenlik → Mikrofon** altında **Mikrofon erişimi**, **Uygulamaların mikrofonunuza erişmesine izin ver** ve **Masaüstü uygulamalarının mikrofonunuza erişmesine izin ver** seçeneklerini açın. [Microsoft mikrofon izinleri kılavuzu](https://support.microsoft.com/en-us/windows/privacy/turn-on-app-permissions-for-your-microphone-in-windows)

### Ses ve tanıma dilleri

1. TTS için **Ayarlar → Saat ve dil → Konuşma** bölümünde **Sesleri yönet** altındaki sesleri kontrol edin. Gerekirse **Ses ekle** ile bir dil yükleyin. Bu ayara Ekran Okuyucusu ayarlarındaki **Eski sesler ekle** seçeneğinden de ulaşabilirsiniz. [Microsoft ses yükleme kılavuzu](https://support.microsoft.com/en-us/accessibility/windows/narrator/appendix-a-supported-languages-and-voices)
2. STT için **Ayarlar → Saat ve dil → Dil ve bölge → Dil seçenekleri** yolunda ilgili dilin konuşma tanıma özelliğinin sunulup sunulmadığını kontrol edin. Sunuluyorsa yükleyin. [Microsoft konuşma dili yükleme kılavuzu](https://support.microsoft.com/en-us/windows/hardware/input-devices/manage-the-language-and-keyboard-input-layout-settings-in-windows)
3. Dil veya ses yükledikten sonra VoxBridge'i yeniden açın. **Ses ve dil** ile **Tanıma dili ve motoru** listeleri `System.Speech` tarafından görülebilen bileşenleri gösterir. Türkçe listeleniyorsa seçebilirsiniz. Aksi durumda İngilizce veya listelenen başka bir dili kullanın.

## Bilinen sınırlamalar

- Seslendirme sesi ve konuşma tanıma motoru ayrı bileşenlerdir. Türkçe seslendirme sesi bulunan bir bilgisayarda Türkçe tanıma motoru bulunmayabilir.
- Windows'un sesle yazma ve Ekran Okuyucusu özellikleri farklı motorlar kullanabilir. VoxBridge ile çalışabilecek seçenekleri uygulamanın listelerinden kontrol edin.
- Bazı tanıma motorları serbest dikteyi (`DictationGrammar`) desteklemeyebilir. Böyle bir motor seçildiğinde uygulama hata mesajı gösterir.
- Tanıma kalitesi seçilen dile, mikrofona ve ortam gürültüsüne bağlıdır.

## Ekran görüntüleri

Görüntüler fiziksel Windows bilgisayarındaki test sırasında alınacak.

### 1. Metin seslendirme

Seslendirme sırasında metin, seçili ses, hız ve **Seslendiriliyor…** durumu.

### 2. Konuşmayı yazıya çevirme

Dinleme sırasında seçili tanıma motoru, birden çok tanınan ifade ve **Mikrofon açık** durumu.

## Proje belgeleri

- [Kod açıklamaları](KOD_ACIKLAMALARI.md)
- [Test kontrol listesi](TEST_CHECKLIST.md)
