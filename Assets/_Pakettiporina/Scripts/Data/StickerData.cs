using UnityEngine;

namespace Pakettiporina
{
    // Yksi keraittava tarra datana. Luo naista tiedostoja:
    // Assets > Create > Pakettiporina > Sticker
    [CreateAssetMenu(menuName = "Pakettiporina/Sticker", fileName = "Sticker")]
    public class StickerData : ScriptableObject
    {
        [Header("Perustiedot")]
        public string displayName = "Uusi tarra";
        public Sprite image;

        [Header("Avautuminen")]
        [Tooltip("Pistemaara jolla tarra avautuu (GameManager.Points >= tama)")]
        public int unlockPoints = 50;
    }
}
