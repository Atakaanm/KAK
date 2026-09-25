/// <summary>Mermilerin oyuncuya uyguladığı etki (ProjectileData.effect).</summary>
public enum ProjectileEffect
{
    Damage,      // can götürür
    Slow,        // yavaşlatır (kartopu)
    Freeze,      // kısa süre dondurur (buz sarkıtı)
    YellowCard,  // sarı kart: yavaşlatır + sayaç (2 sarı = kırmızı)
    RedCard      // kırmızı kart: can götürür + topu düşürür
}
