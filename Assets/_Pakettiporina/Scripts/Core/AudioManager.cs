using System.Collections;
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

        [Header("Pesu (valinnaisia)")]
        // LISATTY 15.8.2026 (kayttaja: "korjaamiseen tulisi liittya jokin aani ja
        // efekti") - kolhujen korjaus siirtyi Halli->Pesu-sceneen samalla, katso
        // WashScreen.OnRepairToolTapped() / RepairTool.cs.
        public AudioClip repairSound;     // "Korjaa kolhut" -napin aani Pesussa

        [Header("Auton moottori (valinnainen, loopattava)")]
        public AudioClip engineLoop;      // CarEngineAudio lukee taman ja soittaa loopina

        [Header("Musiikki (valinnainen)")]
        [Range(0f, 1f)] public float musicVolume = 0.6f;
        [Range(0f, 1f)] public float sfxVolume = 1f;

        // M6.3/16.8.2026 (kokonaisvaltainen aanien parannus): kaksi yleista "juice"-
        // parannusta jotka eivat vaadi uusia aaniklippeja.
        [Header("Aanen vaihtelu ja pehmeys (M6.3, 16.8.2026)")]
        [Tooltip("Kuinka paljon usein toistuvien aanien (kolhu/roiske/kupla/korjaus/tahti/nappi) " +
                 "sävelkorkeus vaihtelee satunnaisesti per toisto - estaa robottimaisen " +
                 "'aina tasan sama aani' -tunteen. 0 = ei vaihtelua.")]
        [Range(0f, 0.3f)] public float sfxPitchVariance = 0.06f;
        [Tooltip("Kuinka pitkaan musiikin vaihto (esim. Ajo -> Pesu) haivyttaa vanhan pois " +
                 "ja uuden sisaan yhteensa - pehmentaa aiemman kovan katkaisu+aloitus-vaihdon.")]
        public float musicCrossfadeDuration = 0.6f;

        Coroutine musicFadeRoutine;

        [Header("Taustamusiikki scenettain (valinnaisia — tyhja = ei musiikkia sille scenelle)")]
        public AudioClip menuMusic;    // MainMenu
        public AudioClip garageMusic;  // Garage
        public AudioClip raceMusic;    // Game, Game2, Puisto, Satama, Huvipuisto, Maatila...
        public AudioClip washMusic;    // Wash

        // Scenet joissa raceMusic soi. Lisaa uuden radan scenen nimi tahan kun rata luodaan
        // (esim. "Maatila"), niin musiikki alkaa automaattisesti ilman koodimuutosta muualla.
        // KORJATTU 19.8.2026 (koko pelin tarkistuksessa loydetty puute): "Kyla" lisatty -
        // uusi rata jai aiemmin listan ulkopuolelle, jolloin musiikki ei olisi vaihtunut
        // sielle (hiljainen puute, ei virhe - mutta turha silti korjata nyt kun rata on jo
        // olemassa, vaikka koristelu onkin viela kesken).
        static readonly string[] RaceScenes = { "Game", "Game2", "Puisto", "Satama", "Huvipuisto", "Maatila", "Kyla" };

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

        // Lahtolaskenta pysyy tarkoituksella KIINTEALLA sävelkorkeudella (3-2-1-rytmin
        // pitaa kuulostaa aina samalta, ei "vaihtelevalta") - samoin "AJA!" ja maalifanfaari
        // (kumpikin soi vain kerran per keikka, ei toistu tarpeeksi usein hyotyakseen
        // vaihtelusta). Tahti sen sijaan VOI toistua monta kertaa samalla radalla, joten
        // se saa saman pienen vaihtelun kuin muut usein toistuvat aanet.
        void HandleCountdown(int n) { PlaySfx(countdownBeep); }
        void HandleGo() { PlaySfx(goSound); }
        void HandleStar(int total) { PlaySfx(starPickup, sfxPitchVariance); }
        void HandleFinish() { PlaySfx(finishFanfare); }

        // --- Yleiset kutsut muualta koodista ---
        // PlaySfx(clip) sailyy ennallaan (kiintea sävelkorkeus) - vanhat kutsupaikat eivat
        // muutu. Uusi ylikuormitus PlaySfx(clip, pitchVariance) lisaa satunnaisen, pienen
        // sävelkorkeuden vaihtelun - kaytetaan usein toistuvissa aanissa alla.
        public void PlaySfx(AudioClip clip) => PlaySfx(clip, 0f);

        public void PlaySfx(AudioClip clip, float pitchVariance)
        {
            if (clip == null || sfxSource == null) return;
            // HUOM: AudioSource.PlayOneShot "ottaa talteen" senhetkisen source.pitch-arvon
            // taman yhden soiton ajaksi, joten peräkkäiset PlaySfx-kutsut eri pitch-arvoilla
            // eivat sotke toisiaan vaikka aanet menisivat paallekkain.
            sfxSource.pitch = pitchVariance > 0f ? 1f + Random.Range(-pitchVariance, pitchVariance) : 1f;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        // M6.3/16.8.2026: usein/moneen kertaan samalla keikalla toistuvat aanet (osumat,
        // napautukset) saavat pienen sävelkorkeuden vaihtelun - vahemman robottimaista.
        // Harvoin soivat/kertaluontoiset aanet (FitGood, Unlock) pysyvat kiinteina.
        public void PlayBubblePop() { PlaySfx(bubblePop, sfxPitchVariance); }
        public void PlayButtonClick() { PlaySfx(buttonClick, sfxPitchVariance); }
        public void PlayBoost() { PlaySfx(boostSound, sfxPitchVariance); }
        public void PlaySplash() { PlaySfx(splashSound, sfxPitchVariance); }
        public void PlayBump() { PlaySfx(bumpSound, sfxPitchVariance); }
        public void PlayFitGood() { PlaySfx(fitGoodSound); }
        public void PlayUnlock() { PlaySfx(unlockSound); }
        public void PlayRepair() { PlaySfx(repairSound, sfxPitchVariance); }

        // M6.3/16.8.2026: musiikin vaihto haivyttaa nyt pehmeasti (ei enaa kova
        // katkaisu+aloitus, esim. Ajo->Pesu-siirtymassa). Sama julkinen signatuuri kuin
        // ennen, joten mikaan olemassaoleva kutsupaikka ei muutu.
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (musicSource == null || clip == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return;

            if (musicFadeRoutine != null) StopCoroutine(musicFadeRoutine);
            musicFadeRoutine = StartCoroutine(CrossfadeMusicRoutine(clip, loop));
        }

        IEnumerator CrossfadeMusicRoutine(AudioClip clip, bool loop)
        {
            float half = Mathf.Max(0.05f, musicCrossfadeDuration * 0.5f);

            // Haivyta nykyinen kappale pois (jos jotain jo soi).
            if (musicSource.isPlaying)
            {
                float startVol = musicSource.volume;
                float t = 0f;
                while (t < half)
                {
                    t += Time.deltaTime;
                    musicSource.volume = Mathf.Lerp(startVol, 0f, t / half);
                    yield return null;
                }
            }

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.volume = 0f;
            musicSource.Play();

            // Haivyta uusi kappale sisaan.
            float t2 = 0f;
            while (t2 < half)
            {
                t2 += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, musicVolume, t2 / half);
                yield return null;
            }
            musicSource.volume = musicVolume;
            musicFadeRoutine = null;
        }

        public void StopMusic()
        {
            if (musicFadeRoutine != null) { StopCoroutine(musicFadeRoutine); musicFadeRoutine = null; }
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
