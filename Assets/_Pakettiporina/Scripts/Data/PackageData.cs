using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pakettiporina
{
    // Yksi toimitettava paketti datana. Luo naista tiedostoja:
    // Assets > Create > Pakettiporina > Package
    [CreateAssetMenu(menuName = "Pakettiporina/Package", fileName = "Package")]
    public class PackageData : ScriptableObject
    {
        [Header("Perustiedot")]
        public string displayName = "Uusi paketti";
        [Tooltip("Lyhyet kuvailevat tagit, esim. sarkyva, kylma, iso")]
        public string[] tags;

        [Header("Vaatimus autolle")]
        [Tooltip("Osa, joka autossa pitaa olla jotta paketti sopii (veda osatiedosto tahan)")]
        public PartData requiredPart;

        [Header("Ominaisuudet")]
        [Tooltip("Paketin massa — lisataan auton painoon, vaikuttaa ajoon")]
        public float mass = 5f;
        [Tooltip("Palkkio pisteina onnistuneesta toimituksesta")]
        public int rewardPoints = 30;
        [Tooltip("Sulamisaika sekunteina kylmalle paketille (0 = ei sula)")]
        public float meltTime = 0f;
    }
}