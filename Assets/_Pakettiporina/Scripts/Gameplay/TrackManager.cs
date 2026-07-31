using System.Collections.Generic;
using UnityEngine;

namespace Pakettiporina
{
    // Arpoo ja instantioi yhden radan ennen kilpailun alkua.
    // Liita tyhjaan objektiin peliscenessa (esim. "TrackManager"), aseta Track Root
    // johonkin tyhjaan Transformiin jonka alle radat instantioidaan.
    //
    // Jokaisen radan prefabissa PITAA olla lapsi nimelta "StartPoint" (Transform,
    // maarittaa auton lahtopaikan ja -suunnan). "Finish"-objekti (FinishTrigger +
    // trigger-collider) toimii itsestaan, koska FinishTrigger kutsuu RaceManager.Instancea
    // suoraan - sita ei tarvitse kytkea erikseen.
    public class TrackManager : MonoBehaviour
    {
        public static TrackManager Instance { get; private set; }

        [Tooltip("Kaikki radat joita voidaan arpoa. Tayttyy automaattisesti tarkistustyokalulla.")]
        public List<TrackData> allTracks = new List<TrackData>();

        [Tooltip("Tyhja objekti johon rata instantioidaan. Jos tyhja, kaytetaan tata objektia.")]
        public Transform trackRoot;

        public TrackData CurrentTrack { get; private set; }
        GameObject spawnedInstance;

        void Awake()
        {
            Instance = this;
        }

        // Arpoo ja instantioi uuden radan. Palauttaa radan StartPointin, tai null jos epaonnistui.
        public Transform SpawnRandomTrack()
        {
            if (allTracks == null || allTracks.Count == 0)
            {
                Debug.LogError("[TrackManager] Ei yhtaan rataa 'All Tracks' -listassa.");
                return null;
            }
            var pick = allTracks[Random.Range(0, allTracks.Count)];
            return SpawnTrack(pick);
        }

        public Transform SpawnTrack(TrackData data)
        {
            if (data == null || data.trackPrefab == null)
            {
                Debug.LogError("[TrackManager] TrackDatalta puuttuu Track Prefab.");
                return null;
            }

            if (spawnedInstance != null) Destroy(spawnedInstance);

            CurrentTrack = data;
            var parent = trackRoot != null ? trackRoot : transform;
            spawnedInstance = Instantiate(data.trackPrefab, parent);
            Debug.Log($"[TrackManager] Rata kaytossa: {data.displayName}");

            var start = FindByName(spawnedInstance.transform, "StartPoint");
            if (start == null)
                Debug.LogWarning($"[TrackManager] Radalta '{data.displayName}' (prefab '{data.trackPrefab.name}') " +
                                  "ei loytynyt 'StartPoint'-nimista lasta.");
            return start;
        }

        static Transform FindByName(Transform root, string name)
        {
            if (root.name == name) return root;
            foreach (Transform child in root)
            {
                var r = FindByName(child, name);
                if (r != null) return r;
            }
            return null;
        }
    }
}
