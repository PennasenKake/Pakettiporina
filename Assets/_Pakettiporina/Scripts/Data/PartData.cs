using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pakettiporina
{
    // Auton osan kategoria.
    public enum PartCategory { Kori, Renkaat, Moottori, Jouset, Lisat, Maali }

    // Yksi auton osa datana. Luo naista tiedostoja:
    // Assets > Create > Pakettiporina > Part
    [CreateAssetMenu(menuName = "Pakettiporina/Part", fileName = "Part")]
    public class PartData : ScriptableObject
    {
        [Header("Perustiedot")]
        public string displayName = "Uusi osa";
        public PartCategory category = PartCategory.Kori;

        [Header("Ominaisuusvaikutukset (+ / -)")]
        [Tooltip("Kiihtyvyys / mäkien nousu")] public int voima;
        [Tooltip("Mutkat ja liukkaat pinnat")] public int pito;
        [Tooltip("Ketteryys ja hyppyjen pituus")] public int keveys;
        [Tooltip("Töyssyjen sieto, särkyvän paketin suoja")] public int kestavyys;
        [Tooltip("Kylmälaatikon teho (jäätelö)")] public int kylmyys;

        [Header("Muut")]
        [Tooltip("Vain koriste (esim. kissankorvat) — ei muuta ominaisuuksia")]
        public bool cosmeticOnly;
        [Tooltip("Auton varille (Maali-kategoria)")]
        public Color color = Color.white;
        [Tooltip("Pistemaara jolla osa avautuu (0 = kaytettavissa heti)")]
        public int unlockPoints;
    }
}