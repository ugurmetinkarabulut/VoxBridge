# VoxBridge test kontrol listesi

## macOS'ta gerçekleştirilen kontroller

| Kontrol | Sonuç |
| --- | --- |
| `dotnet restore VoxBridge.sln` | Normal ağ erişimiyle tamamlandı. Kısıtlı ağdaki ilk denemede NuGet adresi çözümlenemedi (`NU1301`, `NU1900`). |
| Debug ve Release | .NET SDK 10.0.401 ile Windows hedefi derlendi. Her iki yapılandırmada 0 uyarı ve 0 hata. |
| Paket güvenlik denetimi | NuGet denetimi açık kaldı. Normal ağ üzerinden yapılan `dotnet package list --vulnerable --include-transitive` sorgusu bilinen açık listelemedi. |
| Platformdan bağımsız mantık | Geçici bir .NET 10 konsol projesiyle `AudioSession` ve `TextToSpeechService.ValidateSpeech` üzerinde 38 kontrol geçti. |

Mantık kontrolleri boş metni ve yalnızca boşluk içeren metni reddetmeyi, geçersiz hızları reddetmeyi ve −10, 0, +10 değerlerini kabul etmeyi kapsadı. Her iki işlem için tekrarlı veya eş zamanlı başlatma, durdurma süresince başlatmayı engelleme, ilgisiz tamamlanma olayını yok sayma, normal bitiş ve iptal sonrası yeniden başlatma kontrol edildi.

## Fiziksel Windows bilgisayarında yapılacak testler

- **Windows sürümü** `____________________`
- **Test tarihi** `____________________`
- **Hoparlör ve mikrofon** `____________________`
- **Denenen sesler ve tanıma dilleri** `____________________`

Kutuları gerçek testin ardından işaretleyin. Motora veya aygıta bağlı bir senaryo eldeki bilgisayarda uygulanamıyorsa sonucu gerekçesiyle kaydedin.

### Başlatma ve arayüz

- [ ] Visual Studio'da `VoxBridge.sln` açın. Paketleri geri yükleyip **F5** ile çalıştırın. İki panel, metin kutuları, seçim listeleri, hız ayarı ve durum etiketleri görünür.
- [ ] Pencereyi yeniden boyutlandırın ve mümkünse %100 ile %125 ekran ölçeğinde açın. Düğme yazıları ve çok satırlı durum mesajları okunabilir. **Tab** tuşuyla metin alanlarına ve etkin kontrollere erişilebilir.
- [ ] Açılışta Windows'un mikrofon kullanım göstergesini kontrol edin. Mikrofon, **Dinlemeyi Başlat** tıklanana kadar kapalıdır.

### Text-to-Speech

- [ ] Boş metinle ve sadece boşluk veya satır sonu içeren bir metinle **Seslendir** düğmesine basın. Ses çıkmaz ve açıklayıcı bir durum mesajı gösterilir.
- [ ] Seçili sesin dilinde kısa bir metin okutun. Ses varsayılan çıkıştan gelir. Durum önce **Seslendiriliyor…**, ardından **Seslendirme tamamlandı** olur. Yeniden seslendirme mümkündür.
- [ ] Aynı metni −10, 0 ve +10 hızlarında okutun. Hız etiketi değişir ve okumalar arasındaki hız farkı duyulur.
- [ ] Varsa iki farklı sesle uygun dillerde metin okutun. Listede ses adı ve dili görünür. Türkçe yüklüyse Türkçe sesi deneyin. Türkçe yoksa listelenen bir sesle devam edin.
- [ ] Uzun bir metni okutun. Pencere yanıt vermeye devam eder. **Durdur** sesi keser. İptal tamamlanınca **Seslendirme durduruldu** durumu görünür ve metin yeniden okutulabilir.
- [ ] Aktif seslendirmede **Temizle** düğmesine basın. Yazı silinir ve ses durur. İptal bitince yeni metin okutulabilir.
- [ ] Kullanılabilir ses bulunmayan bir Windows ortamında açın. Seslendirme devre dışıdır ve durum açıklaması görünür. Uygun bir tanıma motoru varsa STT çalışır.

### Speech-to-Text

- [ ] Dikte destekli bir tanıma motoru seçin. **Dinlemeyi Başlat** düğmesine basın ve seçili dilde, aralarında kısa duraklama bulunan iki ifade söyleyin. Mikrofon durumu etkin görünür ve yeni ifadeler önceki yazıyı koruyarak eklenir.
- [ ] Dinleme sürerken metni kopyalayın ve pencereyi hareket ettirin. Kontroller yanıt verir. **Temizle** yazıyı siler ve yeni tanınan ifade boş kutuya eklenir.
- [ ] **Dinlemeyi Durdur** düğmesine basıp konuşmaya devam edin. Yeni metin eklenmez. **Mikrofon kapalı** mesajı geldikten sonra Windows mikrofon kullanımının bittiğini kontrol edin. Yeni bir oturum açın ve metin ekleyebildiğinizi doğrulayın.
- [ ] Dinleme başlar başlamaz durdurmayı deneyin. Kapanış tamamlanana kadar yeniden başlatma bekler ve ardından yeni bir oturum açılabilir.
- [ ] Varsayılan mikrofonu çıkarın veya devre dışı bırakın ve dinlemeyi başlatın. Uygulama açık kalır. Windows'un bildirdiği bir hata durum etiketinde gösterilir. Çalışan bir mikrofon seçip tekrar deneyin.
- [ ] Masaüstü uygulamalarının mikrofon iznini kapatıp tekrar deneyin. Uygulama açık kalır. İzin engelliyken konuşma metne dönüştürülmez. İzni geri açınca tekrar deneyin.
- [ ] Kullanılabilir tanıma motoru olmayan bir Windows ortamında açın. Dinleme devre dışıdır ve durum açıklaması görünür. Kullanılabilir bir ses varsa TTS çalışır.
- [ ] Serbest dikteyi desteklemeyen bir motor varsa seçip dinlemeyi başlatın. Dikte hatası açıklanır. Başka bir uygun motorla tekrar başlatılabilir.
- [ ] Türkçe tanıma motoru listeleniyorsa Türkçe dikteyi deneyin. Listelenmiyorsa mevcut bir motorun diliyle test edin.

### İşlemler arası geçiş ve kapanış

- [ ] Seslendirme sırasında dinleme düğmesi pasiftir. Seslendirmeyi durdurun. Tamamlanma ardından dinlemeyi başlatın. Dinleme sırasında seslendirme düğmesi pasiftir ve mikrofon bırakılınca yeniden etkinleşir.
- [ ] Uzun bir metin seslendirilirken pencereyi kapatın. Ses kesilir ve işlem kapanır. Uygulamayı yeniden açıp seslendirmeyi tekrar deneyin.
- [ ] Dinleme sürerken pencereyi kapatın. Windows mikrofon göstergesi söner ve işlem kapanır. Uygulamayı yeniden açıp mikrofonun kullanılabildiğini kontrol edin.
- [ ] README'deki iki bölüm için Windows'tan bir seslendirme ve bir dinleme ekran görüntüsü alın.
