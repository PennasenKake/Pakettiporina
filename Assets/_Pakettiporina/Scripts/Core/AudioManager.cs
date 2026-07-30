using UnityEngine;

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

        [Header("Pelitapahtumien aanet (valinnaisia � tyhja = ei aanta)")]
        public AudioClip countdownBeep;   // 3, 2, 1
        public AudioClip goSound;         // "AJA!"
        public AudioClip starPickup;      // tahden kerays
        public AudioClip finishFanfare;   // maali
        public AudioClip bubblePop;       // saippuakupla poksahtaa (M4)
        public AudioClip buttonClick;     // yleinen napin klikkausaani (kytke halutessasi kasin nappeihin)

        [Header("Musiikki (valinnainen)")]
        [Range(0f, 1f)] public float musicVolume = 0.6f;
        [Range(0f, 1f)] public float sfxVolume = 1f;

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

            // Luo aanilahteet automaattisesti � ei tarvitse lisata kasin Inspectorissa.
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

        void OnEnable()
        {
            GameEvents.OnCountdown += HandleCountdown;
            GameEvents.OnGo += HandleGo;
            GameEvents.OnStarCollected += HandleStar;
            GameEvents.OnFinish += HandleFinish;
        }

        void OnDisable()
        {
            GameEvents.OnCountdown -= HandleCountdown;
            GameEvents.OnGo -= HandleGo;
            GameEvents.OnStarCollected -= HandleStar;
            GameEvents.OnFinish -= HandleFinish;
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
