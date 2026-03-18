using UnityEngine;

/// <summary>
/// Karakter verilerini tutan ScriptableObject.
/// Unity Editorde: Assets > Create > KacAtaKac > Player Data
/// Her karakter icin (Boy, Girl vb.) ayri bir PlayerData olusturulur.
/// </summary>
[CreateAssetMenu(fileName = "New PlayerData", menuName = "KacAtaKac/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Genel Bilgi")]
    public string playerName = "Boy";
    public PlayerType playerType = PlayerType.Boy;

    [Header("Istatistikler")]
    public int maxHealth = 3;
    public float moveSpeed = 5f;
    public float invincibilityDuration = 0.35f;

    // ── 8 yonlu animasyon yapisi ──────────────────────────────────────────
    // Her yon icin tek bir blok: 1 idle sprite + istedigin kadar run frame.
    // PlayerDirectionSprite.cs ile birebir eslesiyor.
    // Inspector'da her yon katlanabilir blok olarak gorunur.
    [System.Serializable]
    public class DirectionData
    {
        public Sprite idle;        // Duruyorken gozukecek sprite
        public Sprite[] runFrames; // Kosarken donecek kareler (istedigin kadar ekle)
    }

    [Header("8 Yon Animasyonlari")]
    public DirectionData north;
    public DirectionData south;
    public DirectionData east;
    public DirectionData west;
    public DirectionData northEast;
    public DirectionData northWest;
    public DirectionData southEast;
    public DirectionData southWest;

    [Header("Prefab (opsiyonel)")]
    public GameObject playerPrefab;

    [Header("Magaza / Kilit")]
    public bool isLocked = false;
    public int unlockPrice = 0;
}

public enum PlayerType
{
    Boy,
    Girl,
    Monster,
    Custom
}
