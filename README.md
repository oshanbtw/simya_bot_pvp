# Osi2 Simya

**Metin2 simya arındırma işlemlerini ekran üzerinden otomatikleştiren, C# ile geliştirilmiş Windows masaüstü uygulaması.**

Bu projeyi Osi2 sunucusunda elle tekrarladığım simya işlemlerini otomatikleştirmek için geliştirdim. Kaynak kodunu, projeyi geliştirmek veya farklı sunucuların arayüzlerine uyarlamak isteyenlerin incelemesine ve katkısına açıyorum.

Aynı zamanda bu çalışma; masaüstü arayüz geliştirme, Windows girdi API'leri, görüntü karşılaştırma ve durum yönetimi konularındaki yaklaşımımı gösteren bir portfolyo projesidir.

## Tanıtım ve kullanım videosu

Hazırladığım tanıtım ve kullanım videosunu aşağıdaki görsele tıklayarak YouTube üzerinden izleyebilirsiniz.

[![Osi2 Simya — tanıtım ve kullanım videosunu izle](https://img.youtube.com/vi/ZF6uH7CcPsM/hqdefault.jpg)](https://youtu.be/ZF6uH7CcPsM)

## Özellikler

- Beyaz, Mavi, Yeşil, Siyah, Turuncu, Kırmızı ve Mor taş desteği.
- İşlenmemişten Tanrısala kadar hedef kademe seçimi. **Hedef kademedeki taşlar arındırılmaz.**
- Her kademe için ayrı minimum dolu yuva limiti.
- Ekrandaki noktaları **F6** ile kaydetme ve ayarları JSON dosyasında saklama.
- Boş yuva örnekleriyle görüntü karşılaştırmasına dayalı doluluk tespiti.
- Sağ tıktan sonra arındırma giriş yuvasında değişim görülmeden Hızlı Arındır göndermeme.
- Bir rengin sandığından yeni taş gelmeyene kadar devam edip sonraki seçili renge geçme.
- **F8** ile durdurma, pencere odağı kontrolü ve işlem günlüğü.

Güncel sürüm sohbet okumaz, OCR kullanmaz ve kademelerin sayılarını sohbetten tahmin etmez. Her kademe ekrandan kontrol edilir. Python bağımlılığı yoktur.

## Gereksinimler

- Windows ve .NET 10 Desktop Runtime.
- Kaynaktan derlemek için .NET 10'u hedefleyebilen bir .NET SDK.
- Pencere modunda çalışan oyun ve görünür Sandık Çantası, simya envanteri ve arındırma paneli.

Uygulama ekran görüntüsü karşılaştırması ve Windows'un standart fare/klavye girdisiyle çalışır. Bellek okuma, paket müdahalesi veya koruma aşma mekanizması içermez. Oyunun standart girdiyi kabul etmesi gerekir.

## Kaynaktan çalıştırma

Depoyu indirdikten sonra proje dizininde:

```powershell
dotnet restore
dotnet build -c Release
```

Oluşan uygulamayı çalıştırın:

```powershell
.\bin\Release\net10.0-windows\SimyaBot.exe
```

Mevcut görsel sayım ve limit testlerini çalıştırmak için:

```powershell
dotnet .\bin\Release\net10.0-windows\SimyaBot.dll --self-test
```

Bu kontroller canlı oyun testi yerine geçmez.

## İlk kurulum ve kalibrasyon

### 1. Oyun penceresini hazırlayın

Sandık Çantası, simya envanteri ve Arındırma Paneli'ni açın. Panelleri sabit yerlerine yerleştirin ve arındırma giriş alanını boş bırakın. Uygulama çalışırken bu panellerin üzerini kapatmayın.

### 2. Tıklama noktalarını kaydedin

Uygulamadaki listeden bir nokta seçin, **Seçili noktaya konum ata** düğmesine basın, fareyi oyun üzerindeki ilgili merkeze götürün ve **F6** tuşuna basın. Noktayı kaydetmek için oyunda tıklamanız gerekmez.

| Nokta | Kaydedilecek konum |
| --- | --- |
| Sandık: renk | Sandık Çantası'ndaki ilgili taşın merkezi |
| Renk: renk | Simya envanterindeki ilgili renk sekmesi |
| Kademe: ad | İşlenmemişten hedefe kadar ilgili kademe sekmesi |
| Izgara ilk yuva | Simya envanterindeki ilk eşya yuvasının merkezi |
| Hızlı Arındır | Arındırma panelindeki Hızlı Arındır düğmesi |
| Arındırma giriş yuvası | Arındırma panelinin sol taraftaki ilk eşya yuvasının merkezi |

**Arındırma giriş yuvası, simya envanterindeki ilk yuva veya panelin sağındaki sonuç yuvası değildir.**

Yalnız kullanacağınız renklerin Sandık ve Renk noktalarını kaydetmeniz yeterlidir. Kademe noktalarını hedef kademe dahil kaydedin.

### 3. Izgarayı ve boş örneği ayarlayın

Varsayılan ızgara **8 sütun × 4 satır**, yuva merkezleri arasındaki mesafe **32 piksel**dir. Kendi ekranınıza göre doğrulayın.

Tamamen boş bir simya sayfasında **Boş ızgara örneği al** düğmesine basın. Üç saniye içinde oyuna geçin ve fareyi ızgaradan uzaklaştırın. Ardından **Yuva sayımını dene** ile boş sayfanın **0**, tamamen dolu sayfanın **32** sayıldığını kontrol edin.

**Izgara başlangıcını Beyaz sekmesine hizala** düğmesi Osi2 yerleşimine göre yardımcı bir konum önerir; otomatik görüntü tespiti yapmaz. Sonucu kontrol edin. Bu düğme boş örnekleri temizlediği için sonrasında boş örneğini yeniden alın.

### 4. Hedefi ve limitleri seçin

Kullanılacak renkleri ve ulaşmak istediğiniz hedef kademeyi seçin. **Kademe doluluk limitleri** düğmesinden minimum sayıları belirleyin.

Örneğin İşlenmemiş için limit **25** ise:

- 24 dolu yuvada arındırma yapılmaz; taşlar biriktirilir.
- 25 veya daha fazla dolu yuvada arındırma yapılır.

Limit, yığın adedini değil **dolu yuva sayısını** ifade eder ve tüm seçili renklerde aynı kademeye uygulanır. Başlangıç limitleri 2'dir. Örneğin hedef **Mitsi** ise Efsaneviye kadar işlem yapılır; Mitsi ve üzerindeki taşlar korunur.

### 5. Başlatın

**Ayarları kaydet** ve ardından **BAŞLAT** düğmesine basın. Üç saniye içinde oyun penceresine geçin. **F8** veya **DURDUR** düğmesiyle işlemi durdurabilirsiniz.

## İşlem akışı

1. Seçili rengin İşlenmemiş sayfası kontrol edilir. Limit karşılanmıyorsa sandığa Ctrl + sağ tıkla yeni taş eklenir.
2. Kademeler sırayla sayılır. Limit altındaki kademelere arındırma uygulanmaz.
3. Uygun bir yuva yeniden kontrol edilip sağ tıklanır. Arındırma giriş yuvasındaki değişim doğrulanırsa bir kez Hızlı Arındır yapılır.
4. Sonuç hemen görünmezse ek bekleme ve yeniden sayım yapılır. Aynı turda aynı kademeye tekrar Hızlı Arındır gönderilmez.
5. Aynı renkte sandıktan taş gelmeyene kadar devam edilir. Üç açma denemesinde artış yoksa o renk bitmiş kabul edilir ve sonraki renge geçilir. Tüm seçili renkler tamamlanınca uygulama durur.

Sandığın bitişi doğrudan sandık adedi okunarak değil, yeni taş gelişiyle belirlenir. Yanlış koordinatlar veya oyunun girdiyi kabul etmemesi de artışı engelleyebilir. Üç tam turda ilerleme doğrulanamazsa uygulama durur.

## Sık karşılaşılan durumlar

| Durum | Kontrol |
| --- | --- |
| Boş yuvalar dolu sayılıyor | İlk yuva konumunu, ızgara mesafesini ve boş örneğini yeniden kontrol edin. |
| Parlak taşlar yanlış sayılıyor | Boş/dolu sayfalarda sayım testi yapın. Önce konum ve örnekleri doğrulayın, ardından gerekirse görüntü farkı eşiğini ayarlayın. |
| Taşa sağ tıklanıyor ama arındırma başlamıyor | Arındırma giriş yuvası noktasını ve panelde önceki işlemden taş kalıp kalmadığını kontrol edin. |
| Oyunda fare girdisi alınmıyor | Pencere modunu kontrol edin. Oyun yönetici olarak çalışıyorsa uygulamayı da aynı yetki düzeyinde çalıştırın. |
| İşlem arada kaçıyor | İşlem beklemesini artırın ve oyun penceresinin önde kaldığını kontrol edin. |

Ekran çözünürlüğü, Windows ölçeği veya panel konumları değişirse kalibrasyonu yenileyin. Ayrıntılı işlem kayıtları uygulamanın yanındaki `bot.log` dosyasına yazılır. Kişisel koordinatlar ve boş yuva örnekleri `ayarlar.json` içinde saklanır; bu dosyalar Git'e dahil edilmez.

## Teknik yapı

| Dosya | Sorumluluk |
| --- | --- |
| `Program.cs` | Ana arayüz, kalibrasyon, işlem döngüsü ve Windows girdi işlemleri |
| `SlotDetector.cs` | Boş örneğine göre yuva görüntülerini karşılaştırma |
| `TierLimits.cs` | Kademe limitleri, hedef kontrolü ve ilgili testler |
| `LimitsEditor.cs` | Kademe limitlerini düzenleyen pencere |
| `SimyaBot.csproj` | .NET ve Windows Forms proje yapılandırması |

## Farklı sunuculara uyarlama ve katkı

Projeyi **Osi2 sunucusunun arayüzünü ve Hızlı Arındır davranışını** temel alarak geliştirdim. Başka sunucularda renkler, kademeler, ızgara yapısı, işlem süreleri ve toplu arındırmanın çalışma biçimi farklı olabilir. Koordinat kalibrasyonu bazı farkları karşılar; davranış değişiklikleri için kodun uyarlanması gerekir.

Hata düzeltmeleri, daha kararlı görüntü tespiti, arayüz iyileştirmeleri ve farklı sunucu uyarlamaları için katkılarınızı memnuniyetle karşılarım. Bir sorun bildirirken sunucu/arayüz sürümünü, ekran çözünürlüğünü, Windows ölçeğini, tekrar adımlarını ve ilgili günlük bölümünü paylaşabilirsiniz. Kişisel bilgilerinizi paylaşmadan önce kayıtlardan çıkarın.

Bu projeyi bağımsız olarak geliştirdim; Osi2 veya Metin2 ile resmî bir bağlantım yok. Kaynak kodunu inceleyebilir ve geliştirme önerilerinizi benimle paylaşabilirsiniz. Henüz bir lisans dosyası eklemedim. Yeniden kullanım ve dağıtım koşullarını belirlediğimde depoya `LICENSE` dosyasını da ekleyeceğim.
