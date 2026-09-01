using System.ComponentModel.DataAnnotations;
using EXERCISE_MVC.Models;

namespace EXERCISE_MVC.Models
{
    public class SiparisDetay
    {
        public int Id { get; set; }
        //Hangi siparişe ait olduğunu bağlıyoruz(foreign key)
        public int SiparisId { get; set; }
        public Siparis Siparis { get; set; } //Siparişe geri dönen bağ 

        // Hangi ürünün satıldığını bağlıyoruz.
        public int UrunId { get; set; }
        public Urun Urun { get; set; }
        //Kritik Nokta:Ürün Adedi
        public int Adet { get; set; }

        //Ürünün o anki satış fiyatı (İleride ürünün fiyatı değişse bile siparişteki eski fiyat sabit kalsın diye )
        public decimal AnlikFiyat { get; set; }
    }
}