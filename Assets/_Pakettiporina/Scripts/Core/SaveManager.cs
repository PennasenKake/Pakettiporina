using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        // Asetukset (M5.11, 13.8.2026): varahtely/ohjaustapa/nopeustaso pidetaan tassa muistissa
        // (ei vain SaveData-luokassa) koska muu koodi (CarInput, ArcadeCarController, VibrationManager)
        // lukee nama SaveManager.Instance:n kautta - sama "yksi yhteinen lahde" -periaate kuin
        // AudioManagerin musicVolume/sfxVolume-kentilla.
        [HideInInspector] public bool vibrationOn = true;
        [HideInInspector] public int controlMode = 2;   // ks. CarInput.ControlMode
        [HideInInspector] public int carSpeedTier = 1;  // 0=Hidas,1=Keski,2=Nopea

        // M5.15, 14.8.2026: "Yotila" - mieltymys tallennetaan ja luetaan, mutta EI VIELA
        // vaikuta mihinkaan visuaalisesti (kayttajan pyynto: pohja nyt, teema myohemmin).
        [HideInInspector] public bool darkMode = false;

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

        void OnEnable() { SceneManager.sceneLoaded += HandleSceneLoaded; }
        void OnDisable() { SceneManager.sceneLoaded -= HandleSceneLoaded; }

        void Start()
        {
            Load();
            ApplyToScene();
        }

        // Kutsutaan aina kun uusi scene latautuu (myos ensimmaisen kerran Start()issa) -
        // kytkee varahtely/ohjaus/nopeus-asetukset scenen autoon/ohjaimeen ilman etta jokaista
        // rataa tarvitsee erikseen muistaa kytkea (sama periaate kuin AudioManager.HandleSceneLoaded).
        void HandleSceneLoaded(Scene scene, LoadSceneMode mode) => ApplyToScene();

        public void ApplyToScene()
        {
            var input = FindObjectOfType<CarInput>();
            if (input != null) input.SetControlMode(controlMode);

            var car = FindObjectOfType<ArcadeCarController>();
            if (car != null) car.ApplySpeedTier(carSpeedTier);
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
            data.unlockedStickerNames = new List<string>(gm.UnlockedStickerNames);

            var audio = AudioManager.Instance;
            if (audio != null)
            {
                data.musicVolume = audio.musicVolume;
                data.sfxVolume = audio.sfxVolume;
            }
            data.vibrationOn = vibrationOn;
            data.controlMode = controlMode;
            data.carSpeedTier = carSpeedTier;
            data.darkMode = darkMode;

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
            gm.ApplyLoadedStickers(data.unlockedStickerNames);

            var audio = AudioManager.Instance;
            if (audio != null)
            {
                audio.SetMusicVolume(data.musicVolume);
                audio.SetSfxVolume(data.sfxVolume);
            }
            vibrationOn = data.vibrationOn;
            controlMode = data.controlMode;
            carSpeedTier = data.carSpeedTier;
            darkMode = data.darkMode;
            ApplyToScene();

            Debug.Log($"[Save] Ladattu: {data.points} pistetta, {parts.Count}/{data.selectedPartNames.Count} osaa loytyi, {data.unlockedStickerNames?.Count ?? 0} tarraa avattu, aanet {data.musicVolume:0.00}/{data.sfxVolume:0.00}, varahtely {data.vibrationOn}, ohjaus {data.controlMode}, nopeus {data.carSpeedTier}, yotila {data.darkMode} (ei viela visuaalinen).");
        }

        // "Nollaa edistyminen" (M5.11) -asetusnapin kutsuma: poistaa tallennustiedoston ja
        // palauttaa pelin ajonaikaisen tilan (pisteet/osat/tarrat) tyhjaksi. EI koske
        // aani-/varahtely-/ohjausasetuksia (ne ovat kayttajan laiteasetuksia, ei pelietenemista).
        public void ResetProgress()
        {
            try
            {
                if (File.Exists(SavePath)) File.Delete(SavePath);
                Debug.Log("[Save] Tallennustiedosto poistettu (Nollaa edistyminen).");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Save] Tallennustiedoston poisto epaonnistui: " + e.Message);
            }

            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.SetPoints(0);
                gm.ApplyLoadedSelection(null, new List<PartData>());
                gm.ApplyLoadedStickers(new List<string>());
            }
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
