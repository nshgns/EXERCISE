USE [GunesMallDB]
GO

INSERT INTO [dbo].[Urunler] ([Ad], [Fiyat], [Kategori], [ResimYolu], [Stok]) VALUES
-- Elektronik Kategorisi
('Akýllý Telefon X1', 45000.00, 'Elektronik', '/images/products/akilli-telefon-x1.jpg', 50),
('Kablosuz Kulaküstü Kulaklýk', 3200.50, 'Elektronik', '/images/products/kablosuz-kulaklik.jpg', 120),
('Oyuncu Mouse v2', 1450.00, 'Elektronik', '/images/products/oyuncu-mouse.jpg', 85),
('Mekanik Klavye RGB', 2750.00, 'Elektronik', '/images/products/mekanik-klavye.jpg', 40),
('UltraWide Monitör 34"', 18900.90, 'Elektronik', '/images/products/ultrawide-monitor.jpg', 15),

-- Giyim Kategorisi
('Pamuklu Erkek Tiþört', 450.00, 'Giyim', '/images/products/erkek-tisort.jpg', 300),
('Oversize Kadýn Sweatshirt', 750.00, 'Giyim', '/images/products/kadin-sweatshirt.jpg', 150),
('Klasik Deri Ceket', 3499.99, 'Giyim', '/images/products/deri-ceket.jpg', 25),
('Spor Koþu Ayakkabýsý', 2200.00, 'Giyim', '/images/products/kosu-ayakkabisi.jpg', 70),
('Keten Klasik Pantolon', 890.00, 'Giyim', '/images/products/keten-pantolon.jpg', 110),

-- Ev & Yaþam Kategorisi
('Filtre Kahve Makinesi', 4200.00, 'Ev & Yaþam', '/images/products/kahve-makinesi.jpg', 35),
('Akýllý Robot Süpürge', 13500.00, 'Ev & Yaþam', '/images/products/robot-supurge.jpg', 20),
('Çift Kiþilik Nevresim Takýmý', 1250.00, 'Ev & Yaþam', '/images/products/nevresim-takimi.jpg', 95),
('Porselen 24 Parça Yemek Takýmý', 3800.50, 'Ev & Yaþam', '/images/products/yemek-takimi.jpg', 40),
('Dekoratif Ahþap Lambader', 1150.00, 'Ev & Yaþam', '/images/products/lambader.jpg', 60),

-- Kozmetik & Kiþisel Bakým Kategorisi
('Nemlendirici Yüz Kremi 50ml', 320.00, 'Kozmetik', '/images/products/yuz-kremi.jpg', 200),
('Erkek Parfüm EDP 100ml', 1850.00, 'Kozmetik', '/images/products/erkek-parfum.jpg', 80),
('Þarjlý Diþ Fýrçasý', 1600.00, 'Kozmetik', '/images/products/sarjli-dis-fircasi.jpg', 140),

-- Kitap & Hobi Kategorisi
('Distopya Romaný - Yeni Dünya', 145.00, 'Kitap & Hobi', '/images/products/yeni-dunya-kitap.jpg', 500),
('1000 Parça Manzara Puzzle', 350.00, 'Kitap & Hobi', '/images/products/manzara-puzzle.jpg', 180);
GO