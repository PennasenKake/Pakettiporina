using UnityEngine;

namespace Pakettiporina
{
    // LISATTY 23.11.2026 (Play-testi Satamassa: auto ajoi laiturilta mereen eika palannut,
    // kamera jai tuijottamaan tyhjaa vetta). Tallentaa turvallisia paikkoja ja palauttaa auton,
    // jos se putoaa selvasti alle lahtokorkeuden. CarPainter lisaa taman autolle automaattisesti.
    //
    // PARANNETTU 24.11.2026 (TODO: "Recovery palauttaa loputtomasti samaan kohtaan"):
    //  - Turvalliset paikat tallennetaan HISTORIAKSI (viimeiset ~6 s), ja palautus tehdaan
    //    vahintaan 'minRewindSeconds' sekunnin takaiseen paikkaan, jotta auto ei palaa aivan
    //    reunalle jonka yli se juuri putosi.
    //  - Jos auto putoaa uudelleen pian palautuksen jalkeen, palataan aina VANHEMPAAN paikkaan
    //    (ja lopulta lahtopaikkaan), joten silmukka purkautuu.
    //  - Turvallinen paikka vaatii maan alta (raycast), ei vain oikeaa korkeutta.
    //  - Palautuksen jalkeen auto on hetken suojattu (ei uutta palautusta heti) ja saa pienen
    //    eteenpainvauhdin, jotta se ei jaa seisomaan reunalle.
    [RequireComponent(typeof(Rigidbody))]
    public class CarRecovery : MonoBehaviour
    {
        [Tooltip("Kuinka monta metria lahtokorkeuden alapuolelle auto saa pudota ennen palautusta.")]
        public float fallThreshold = 2.5f;
        [Tooltip("Turvallisen paikan tallennusvali (s).")]
        public float saveInterval = 0.5f;
        [Tooltip("Palautus tehdaan vahintaan nain monta sekuntia vanhempaan paikkaan.")]
        public float minRewindSeconds = 1.5f;
        [Tooltip("Jos putoaa uudelleen taman ajan (s) sisalla palautuksesta, palataan yha vanhempaan paikkaan.")]
        public float repeatWindow = 8f;
        [Tooltip("Pieni eteenpainvauhti palautuksen jalkeen (m/s).")]
        public float respawnSpeed = 3f;

        const int HistorySize = 12;                 // 12 x 0,5 s = 6 s

        Rigidbody rb;
        float startY;
        Vector3 startPos;
        Quaternion startRot;
        readonly Vector3[] posHistory = new Vector3[HistorySize];
        readonly Quaternion[] rotHistory = new Quaternion[HistorySize];
        int histCount, histHead;                     // histHead = seuraava kirjoituskohta
        float nextSave;
        float lastRecoverTime = -999f;
        int repeatCount;
        float protectedUntil;

        public int RecoveryCount { get; private set; }

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            startY = transform.position.y;
            startPos = transform.position;
            startRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            Push(startPos, startRot);
        }

        void Push(Vector3 p, Quaternion r)
        {
            posHistory[histHead] = p; rotHistory[histHead] = r;
            histHead = (histHead + 1) % HistorySize;
            histCount = Mathf.Min(histCount + 1, HistorySize);
        }

        // steps = montako tallennusta taaksepain (0 = tuorein)
        bool TryGet(int steps, out Vector3 p, out Quaternion r)
        {
            p = startPos; r = startRot;
            if (steps >= histCount) return false;
            int idx = ((histHead - 1 - steps) % HistorySize + HistorySize) % HistorySize;
            p = posHistory[idx]; r = rotHistory[idx];
            return true;
        }

        // Valitsee palautuspaikan: perusrewind + yksi askel per toisto; loppuu lahtopaikkaan.
        void ChooseRespawn(out Vector3 p, out Quaternion r)
        {
            int baseSteps = Mathf.CeilToInt(minRewindSeconds / Mathf.Max(0.1f, saveInterval));
            int steps = baseSteps + repeatCount * 2;
            if (!TryGet(steps, out p, out r))
            {
                // Historia ei riita: lahtopaikka (aina turvallinen).
                p = startPos; r = startRot;
            }
        }

        void FixedUpdate()
        {
            if (rb == null) return;
            float y = transform.position.y;

            if (Time.time >= protectedUntil && (y < startY - fallThreshold || y < -50f))
            {
                Recover();
                return;
            }

            // Turvallinen = suunnilleen lahtokorkeudella, ei kaatuneena ja maa jalkojen alla.
            if (Time.time >= nextSave && y > startY - 0.8f && Vector3.Dot(transform.up, Vector3.up) > 0.6f && HasGroundBelow())
            {
                Push(transform.position, Quaternion.Euler(0f, transform.eulerAngles.y, 0f));
                nextSave = Time.time + saveInterval;
            }
        }

        bool HasGroundBelow()
        {
            // Raycast alas: osuuko jokin ei-trigger-collider (ei auto itse) lahella. Vesi/tyhja ei osu.
            var hits = Physics.RaycastAll(transform.position + Vector3.up * 0.3f, Vector3.down, 2.5f, ~0, QueryTriggerInteraction.Ignore);
            foreach (var h in hits)
                if (h.collider != null && h.collider.attachedRigidbody != rb) return true;
            return false;
        }

        void Recover()
        {
            repeatCount = (Time.time - lastRecoverTime < repeatWindow) ? repeatCount + 1 : 0;
            lastRecoverTime = Time.time;
            RecoveryCount++;

            Vector3 p; Quaternion r;
            ChooseRespawn(out p, out r);
            transform.SetPositionAndRotation(p + Vector3.up * 0.5f, r);
            rb.velocity = r * Vector3.forward * respawnSpeed;
            rb.angularVelocity = Vector3.zero;
            protectedUntil = Time.time + 1.0f;
            nextSave = Time.time + 1.5f;
            Debug.Log($"[CarRecovery] Auto putosi - palautettu (toisto {repeatCount}, kertoja yhteensa {RecoveryCount}).");
        }
    }
}
