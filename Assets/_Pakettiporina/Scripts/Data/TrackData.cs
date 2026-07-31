using UnityEngine;

namespace Pakettiporina
{
    // Yksi ajorata datana. Luo naista tiedostoja:
    // Assets > Create > Pakettiporina > Track
    [CreateAssetMenu(menuName = "Pakettiporina/Track", fileName = "Track")]
    public class TrackData : ScriptableObject
    {
        [Header("Perustiedot")]
        public string displayName = "Uusi rata";
        [Tooltip("Pieni esikatselukuva, valinnainen (esim. tulevaa rata-valintanaytto varten)")]
        public Sprite thumbnail;

        [Header("Sisalto")]
        [Tooltip("Prefab joka sisaltaa koko radan: Maasto/GroundCollider, 'StartPoint'-lapsi, " +
                 "'Finish'-lapsi (FinishTrigger + BoxCollider Trigger paalla), seka mahd. koristeet/tahdet.")]
        public GameObject trackPrefab;

        [Header("Ajo-ominaisuudet (valinnainen, ei viela kaytossa RaceSetupissa)")]
        [Tooltip("Kerroin auton pitoon taman radan ajaksi, esim. 0.7 = liukkaampi (lumi/jaa)")]
        public float gripMultiplier = 1f;
    }
}
