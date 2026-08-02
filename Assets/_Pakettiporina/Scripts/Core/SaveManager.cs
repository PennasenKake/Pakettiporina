using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pakettiporina
{
    // Tallentaa/lataa pisteet + valitun auton (osat+paketti) levylle JSONina
    // (Application.persistentDataPath). Singleton, DontDestroyOnLoad kuten GameManager.
    //
    // HUOM AJOITUKSESTA: lataus tehdaan Start()issa, ei Awakessa. Unity takaa etta
    // KAIKKI scenen Awake()t (myos GameManagerin) ovat valmiit ennen kuin YKSIKAAN
    // Start() ajetaan - talla varmistetaan etta GameManager.Instance on aina olemassa
    // kun tama yrittaa ladata siihen tallennuksen, riippumatta Script Execution Orderista.
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Tooltip("Kaikki osat, joista tallennettu nimi etsitaan takaisin PartDataksi. Tayttyy automaattisesti tarkistustyokalulla.")]
        public List<PartData> allParts = new List<PartData>();
        [Tooltip("Kaikki paketit, joista tallennettu nimi etsitaan takaisin PackageDataksi. Tayttyy automaattisesti.")]
        public List<PackageData> allPackages = new List<PackageData>();

        string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[Save] Toinen SaveManager loytyi - tuhotaan duplikaatti.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            Load();
        }

        public void Save()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            var data = new SaveData
            {
                points = gm.Points,
                selectedPackageName = gm.SelectedPackage != null ? gm.SelectedPackage.name : "",
            };
            foreach (var p in gm.SelectedParts)
                if (p != null) data.selectedPartNames.Add(p.name);

            try
            {
                File.WriteAllText(SavePath, JsonUtility.ToJson(data));
                Debug.Log($"[Save] Tallennettu: {data.points} pistetta, {data.selectedPartNames.Count} osaa.");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Save] Tallennus epaonnistui: " + e.Message);
            }
        }

        public void Load()
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log("[Save] Ei aiempaa tallennusta - aloitetaan tyhjalta.");
                return;
            }

            SaveData data;
            try
            {
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Save] Tallennuksen lukeminen epaonnistui: " + e.Message);
                return;
            }
            if (data == null) return;

            var gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.LogWarning("[Save] GameManageria ei loydy - lataus ohitettu.");
                return;
            }

            gm.SetPoints(data.points);

            PackageData pkg = null;
            if (!string.IsNullOrEmpty(data.selectedPackageName))
                pkg = allPackages.Find(p => p != null && p.name == data.selectedPackageName);

            var parts = new List<PartData>();
            foreach (var name in data.selectedPartNames)
            {
                var part = allParts.Find(p => p != null && p.name == name);
                if (part != null) parts.Add(part);
            }

            gm.ApplyLoadedSelection(pkg, parts);
            Debug.Log($"[Save] Ladattu: {data.points} pistetta, {parts.Count}/{data.selectedPartNames.Count} osaa loytyi.");
        }

        // Mobiilissa sovellus harvoin sulkeutuu siististi (kayttaja vain vaihtaa
        // sovellusta) - OnApplicationPause on tarkein tallennuskoukku, ei OnApplicationQuit.
        void OnApplicationPause(bool paused)
        {
            if (paused) Save();
        }

        void OnApplicationQuit()
        {
            Save();
        }
    }
}
