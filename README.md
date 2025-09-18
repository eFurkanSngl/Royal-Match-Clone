
[RoyalMatch-Clone.webm](https://github.com/user-attachments/assets/1ede6509-835d-4858-b2a8-731764113788)

Bu proje, Royal Match benzeri bir Match-3 oyun prototipidir. Amacım sadece core mekanikleri değil, aynı zamanda temiz kod, mimari tasarım ve game feel odaklı bir yapı kurmaktı.

🔑 Özellikler

Temel Mekanikler:

Swap sistemi (DOTween animasyonlu)

Eşleşme algılama: yatay/dikey 3’lü, 2x2 kare, L ve T şekilleri

Cascade (zincirleme) eşleşme sistemi

Power-Up’lar: Horizontal/Vertical Rocket, Bomb, Light Bomb, Propeller

Obstacle sistemi (taşların düşüşünü engelleyen yapılar)

Mimari & Teknik Yapı:

Zenject ile bağımlılık enjeksiyonu

Object Pooling ile performanslı Tile & Obstacle yönetimi

Event-driven mimari (UnityAction) ile skor ve UI güncellemeleri

SRP & OCP prensiplerine uygun modüler ve ölçeklenebilir kod yapısı

Yalnızca hareket eden taşlar üzerinden eşleşme kontrolü ile performans optimizasyonu

Game Feel & UI:

DOTween tabanlı squash-stretch ve smooth animasyonlar

Tüm Power-Up türleri için özel animasyonlar

Ses entegrasyonu

Safe Area & Aspect Ratio uyumlu dinamik kamera ve UI

🛠 Kullanılan Teknolojiler

Unity (C#)

Zenject (Dependency Injection)

DOTween (Animasyon Sistemi)

📌 Sonuç

Bu proje, Royal Match’in temel mekaniklerini barındıran; performanslı, modüler ve genişletilebilir bir Match-3 prototipidir. Hem oyun hissi (game feel) hem de mimari tasarım açısından sektöre uygun bir demo olarak tasarlanmıştır.
