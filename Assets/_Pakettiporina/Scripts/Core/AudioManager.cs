using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pakettiporina
{
    // Pelin aanikeskus. Yksi kappale koko pelissa (DontDestroyOnLoad, sama tapa kuin GameManager).
    // Kytkeytyy automaattisesti GameEvents-vayllaan: lahtolaskenta, AJA, tahti ja maali soivat
    // ITSESTAAN ilman etta yhtakaan nappia tarvitsee kytkea uudelleen.
    // Muut aanet (kuplan poksahdus, napin klikkaus) kutsutaan skriptista suoraan, esim.
    // AudioManager.Instance.PlayBubblePop();
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Pelitapahtumien aanet (valinnaisia — tyhja = ei aanta)")]
        public AudioClip countdownBeep;   // 3, 2, 1
        public AudioClip goSound;         // "AJA!"
        public AudioClip starPickup;      // tahden kerays
        public AudioClip finishFanfare;   // maali
        public AudioClip bubblePop;       // saippuakupla poksahtaa (M4)
        public AudioClip buttonClick;     // yleinen napin klikkausaani (kytke halutessasi kasin nappeihin)

        [Header("Radan elementit (valinnaisia)")]
        public AudioClip boostSound;      // Boost-tapa
        public AudioClip splashSound;     // Lätäkkö
        public AudioClip bumpSound;       // Kartio-kolhu

        [Header("Halli (valinnaisia)")]
        public AudioClip fitGoodSound;    // paketti sopii autoon (kuittaus)
        public AudioClip unlockSound;     // osa/paketti/tarra avautuu tai ostetaan

        [Header("Auton moottori (valinnainen, loopattava)")]
        public AudioClip engineLoop;      // CarEngineAudio lukee taman ja soittaa loopina

        [Header("Musiikki (valinnainen)")]
        [Range(0f, 1f)] public float musicVolume = 0.6f;
        [Range(0f, 1f)] public float sfxVolume = 1f;

        [Header("Taustamusiikki scenettain (valinnaisia — tyhja = ei musiikkia sille scenelle)")]
        public AudioClip menuMusic;    // MainMenu
        public AudioClip garageMusic;  // Garage
        public AudioClip raceMusic;    // Game, Game2, Puisto, Satama, Huvipuisto, Maatila...
        public AudioClip washMusic;    // Wash

        // Scenet joissa raceMusic soi. Lisaa uuden radan scenen nimi tahan kun rata luodaan
        // (esim. "Maatila"), niin musiikki alkaa automaattisesti ilman koodimuutosta muualla.
        static readonly string[] RaceScenes = { "Game", "Game2", "Puisto", "Satama", "Huvipuisto", "Maatila" };

        AudioSource sfxSource;
        AudioSource musicSource;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Luo aanilahteet automaattisesti — ei tarvitse lisata kasin Inspectorissa.
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.volume = sfxVolume;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = musicVolume;

            Debug.Log("[Audio] Valmis.");
        }

        void Start()
        {
            // Soita heti oikea musiikki sille scenelle jossa AudioManager herasi henkiin
            // (esim. peli kaynnistetty suoraan Editorista jonkun radan scenessa).
            HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        void OnEnable()
        {
            GameEvents.OnCountdown += HandleCountdown;
            GameEvents.OnGo += HandleGo;
            GameEvents.OnStarCollected += HandleStar;
            GameEvents.OnFinish += HandleFinish;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        void OnDisable()
        {
            GameEvents.OnCountdown -= HandleCountdown;
            GameEvents.OnGo -= HandleGo;
            GameEvents.OnStarCollected -= HandleStar;
            GameEvents.OnFinish -= HandleFinish;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        // Valitsee ja soittaa oikean taustamusiikin scenen nimen perusteella. Kaikki musiikkikentat
        // ovat valinnaisia — jos klippia ei ole kytketty, ei tapahdu mitaan (ei virhetta).
        void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            string name = scene.name;
            if (name == "MainMenu") PlayMusic(menuMusic);
            else if (name == "Garage") PlayMusic(garageMusic);
            else if (name == "Wash") PlayMusic(washMusic);
            else if (System.Array.IndexOf(RaceScenes, name) >= 0) PlayMusic(raceMusic);
            // Tuntematon scene (esim. tuleva uusi rata jota ei viela lisatty RaceScenes-listaan):
            // ei vaihdeta musiikkia, jotta ei katkaista mahdollista jo soivaa kappaletta turhaan.
        }

        void HandleCountdown(int n) { PlaySfx(countdownBeep); }
        void HandleGo() { PlaySfx(goSound); }
        void HandleStar(int total) { PlaySfx(starPickup); }
        void HandleFinish() { PlaySfx(finishFanfare); }

        // --- Yleiset kutsut muualta koodista ---
        public void PlaySfx(AudioClip clip)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        public void PlayBubblePop() { PlaySfx(bubblePop); }
        public void PlayButtonClick() { PlaySfx(buttonClick); }
        public void PlayBoost() { PlaySfx(boostSound); }
        public void PlaySplash() { PlaySfx(splashSound); }
        public void PlayBump() { PlaySfx(bumpSound); }
        public void PlayFitGood() { PlaySfx(fitGoodSound); }
        public void PlayUnlock() { PlaySfx(unlockSound); }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource == null || clip == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return;
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null) musicSource.Stop();
        }

        public void SetSfxVolume(float v)
        {
            sfxVolume = Mathf.Clamp01(v);
            if (sfxSource != null) sfxSource.volume = sfxVolume;
        }

        public void SetMusicVolume(float v)
        {
            musicVolume = Mathf.Clamp01(v);
            if (musicSource != null) musicSource.volume = musicVolume;
        }
    }
}
