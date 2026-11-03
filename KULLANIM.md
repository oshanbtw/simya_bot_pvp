# Osi2 Simya — Görsel sürüm

Uygulama: `bin/Gorsel/SimyaBot.exe`. C# / .NET 10 Windows Forms kullanır. Sohbet okuma, OCR ve hafızadan sayfa atlama kaldırılmıştır. Her kademe doğrudan ekrandan sayılır. Mor dahil yedi renk, kademe doluluk limitleri ve hedef kademe korunur. Önceki sürümler kendi klasörlerinde durur.

## Yeni gerekli konum

Listeden **Arındırma giriş yuvası** seç, konum ata düğmesine bas. Fareyi **Arındırma Paneli'nin sol taraftaki ilk eşya yuvasının ortasına** götür ve F6 bas. Bu, simya envanterindeki ilk yuva veya sağdaki sonuç yuvası değildir. Başlatırken arındırma panelinin giriş alanında önceki işlemden taş kalmamalı.

Bot bir taş seçmeden önce ve seçtikten sonra bu giriş yuvasının görüntüsünü karşılaştırır. Değişim doğrulanmazsa Hızlı Arındır'a basmaz. En fazla üç farklı, yeniden kontrol edilmiş yuva denenir. Hiçbiri doğrulanmazsa o kademe atlanır. Bu bir görsel değişim kontrolüdür; eşya türünü okumaz.

## Kalibrasyon

1. Oyun pencere modunda, Sandık Çantası, simya ve arındırma panelleri açık olmalı.
2. Kullanılacak her rengin **Sandık** ve **Renk** noktalarını, İşlenmemişten hedefe kadar **Kademe** noktalarını, **Hızlı Arındır**, **Izgara ilk yuva** ve yeni **Arındırma giriş yuvası** noktalarını kaydet. Listeden seç → Konum ata → fareyi ilgili merkeze götür → F6.
3. Izgara başlangıcı yanlışsa Beyaz sekmesine hizalama düğmesi mevcut yerleşime göre +4,+80 piksel önerir. İlk yuva merkezini kendin kontrol edip gerekirse F6 ile düzelt. Bu işlem boş örnekleri siler.
4. Sütun, satır ve yuva merkezleri arasındaki mesafeyi ayarla. Standart değerler 8 × 4 ve 32 pikseldir.
5. **Tamamen boş bir simya sayfasında** boş örneği al. Üç saniye içinde oyuna geç ve fareyi ızgaradan uzak tut.
6. Sayım testiyle boş sayfanın **0**, tamamen dolu sayfanın **32** sayıldığını doğrula. Boş sayfada dolu yuva sayılması yanlış konum veya yanlış boş örnek belirtisi olabilir; önce bunları düzelt. Örnek yanlışsa eşik değiştirmek tek başına çözüm değildir.

## İşleyiş

Kademe doluluk limitlerinden her kademe için minimum dolu yuva sayısı seçilir. Örneğin 25, **25 ve üzeri** demektir. Limit altındaki taşlar sonraki turlarda birikir. Hedef kademe ve üstü işlenmez. Limitler tüm renkler için geçerlidir; taş yığını adedi değil dolu yuva sayısı ölçülür.

İşlenmemiş sayfası zaten limitteyse sandık yeniden açılmaz. Limit altındaysa Ctrl+sağ tıkla doldurulur. Üç denemede yeni taş gelmezse renk bitmiş kabul edilip kalan kademeler son kez kontrol edilir ve sonraki renge geçilir. Bu tespit, sandık adedini değil yeni taş gelişini ölçer. Yanlış konum veya oyunun girdiyi kabul etmemesi de artışı engelleyebilir.

Her kademede önce sayım yapılır; limit karşılanıyorsa belirgin ve yeniden doğrulanmış bir yuva seçilir. Arındırma girişindeki değişim doğrulanırsa bir kez Hızlı Arındır yapılır. Ardından sayım kontrol edilir. Azalma hemen görülmezse ek bekleme ve ikinci ölçüm yapılır. Hâlâ doğrulanmazsa aynı kademede tekrar arındırma yapılmadan sonraki kademeye geçilir. Üç tam tur boyunca ilerleme doğrulanamazsa bot durur; yanlış tıklamalar sürekli tekrarlanmaz. Kalibrasyon hatalarını bu doğrulama tamamen telafi edemez.

Seçili ilk renk sandıktan yeni taş gelmeyene kadar sürdürülür; sonra sıradaki renge geçilir. Son renk bitince bot durur. F8 anında durdurur. Oyun odağı kaybolduğunda da işlem durur.

Ayarlar uygulamanın yanındaki `ayarlar.json`, kayıtlar `bot.log` içindedir. Önceki kalibrasyon ve limitler yeni sürüme taşınmıştır; yalnız yeni giriş yuvası noktası gereklidir. Panel yerleri veya ekran ölçeği değişirse konumları ve boş örnekleri yeniden kaydet.

## Doğrulama ve derleme

`dotnet build -c Release --no-restore -o bin/Gorsel`

`dotnet bin/Gorsel/SimyaBot.dll --self-test`

Derleme ve mevcut görsel sayım/limit testleri çalıştırıldı. Yeni panele aktarım kontrolü canlı oyunda henüz doğrulanmadı.
