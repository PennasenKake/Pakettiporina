using UnityEngine;

namespace Pakettiporina
{
    // Auton moottorin ajoaani: loopattava klippi jonka sävelkorkeus ja voimakkuus
    // seuraavat kaasua/nopeutta. Luo oman AudioSourcen itse Awakessa (sama tapa kuin
    // AudioManager.cs) - EI tarvitse valmiiksi AudioSourcea objektilla, joten tama
    // voidaan lisata autoon pelkkana komponenttina (esim. Editor-tyokalulla, samaan
    // tapaan kuin CarPainter lisataan PakettiporinaSetup.cs:ssa).
    //
    // Klippi luetaan AudioManager.Instance.engineLoop:sta Startissa (ei Awakessa,
    // koska AudioManager saattaa herata henkiin vasta samalla framella - sama
    // Script Execution Order -periaate kuin SaveManagerilla/StickerPanelilla).
    public class CarEngineAudio : MonoBehaviour
    {
        [Tooltip("Jos tyhja, etsitaan automaattisesti samasta objektista.")]
        public ArcadeCarController car;
        public CarInput input;

        [Header("Sävelkorkeus/voimakkuus kaasun+nopeuden mukaan")]
        [Range(0.5f, 2f)] public float minPitch = 0.8f;
        [Range(0.5f, 3f)] public float maxPitch = 1.55f;
        [Range(0f, 1f)] public float minVolume = 0.10f;
        [Range(0f, 1f)] public float maxVolume = 0.42f;
        [Tooltip("Kuinka nopeasti pitch/volume seuraa muutosta (pienempi = pehmeampi)")]
        public float smoothing = 6f;

        AudioSource src;
        Rigidbody rb;
        float currentBlend;

        void Awake()
        {
            if (car == null) car = GetComponent<ArcadeCarController>();
            if (input == null) input = GetComponent<CarInput>();
            rb = GetComponent<Rigidbody>();

            src = gameObject.AddComponent<AudioSource>();
            src.loop = true;
            src.playOnAwake = false;
            src.spatialBlend = 0f; // 2D - yksinkertainen kuunteluetaisyydesta riippumaton aani
        }

        void Start()
        {
            var clip = AudioManager.Instance != null ? AudioManager.Instance.engineLoop : null;
            if (clip == null) return; // ei klippia -> hiljaista, ei virhetta (sama periaate kuin muualla)
            src.clip = clip;
            src.Play();
        }

        void Update()
        {
            if (src.clip == null || rb == null) return;

            float throttle = input != null ? Mathf.Abs(input.Throttle) : 0f;
            float maxSpeed = car != null ? Mathf.Max(car.maxSpeed, 0.01f) : 12f;
            Vector3 flat = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            float speedFrac = Mathf.Clamp01(flat.magnitude / maxSpeed);

            // Kaasu reagoi valittomasti (kuulostaa "revittelylta"), nopeus tuo pohjan
            // kun auto jo liikkuu vaikka kaasu hetkeksi vapautettaisiin.
            float target = Mathf.Clamp01(Mathf.Max(throttle * 0.7f, speedFrac));
            currentBlend = Mathf.Lerp(currentBlend, target, Time.deltaTime * smoothing);

            src.pitch = Mathf.Lerp(minPitch, maxPitch, currentBlend);
            float sfxVol = AudioManager.Instance != null ? AudioManager.Instance.sfxVolume : 1f;
            src.volume = Mathf.Lerp(minVolume, maxVolume, currentBlend) * sfxVol;
        }
    }
}
