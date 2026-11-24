using UnityEngine;

namespace Pakettiporina
{
    // LISATTY 23.11.2026 (Play-testi Satamassa: auto ajoi laiturilta mereen eika palannut,
    // kamera jai tuijottamaan tyhjaa vetta). Tallentaa viimeisen turvallisen paikan ja
    // palauttaa auton siihen, jos se putoaa selvasti alle lahtokorkeuden.
    // CarPainter lisaa taman autolle automaattisesti.
    [RequireComponent(typeof(Rigidbody))]
    public class CarRecovery : MonoBehaviour
    {
        [Tooltip("Kuinka monta metria lahtokorkeuden alapuolelle auto saa pudota ennen palautusta.")]
        public float fallThreshold = 2.5f;
        [Tooltip("Turvallisen paikan tallennusvali (s).")]
        public float saveInterval = 0.5f;

        Rigidbody rb;
        float startY;
        Vector3 safePos;
        Quaternion safeRot;
        float nextSave;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            startY = transform.position.y;
            safePos = transform.position;
            safeRot = transform.rotation;
        }

        void FixedUpdate()
        {
            if (rb == null) return;
            float y = transform.position.y;

            if (y < startY - fallThreshold || y < -50f)
            {
                transform.SetPositionAndRotation(safePos + Vector3.up * 0.5f, safeRot);
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                nextSave = Time.time + 1f;
                Debug.Log("[CarRecovery] Auto putosi - palautettu viimeiseen turvalliseen paikkaan.");
                return;
            }

            // Turvallinen = suunnilleen lahtokorkeudella ja ei kaatuneena.
            if (Time.time >= nextSave && y > startY - 0.8f && Vector3.Dot(transform.up, Vector3.up) > 0.6f)
            {
                safePos = transform.position;
                safeRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
                nextSave = Time.time + saveInterval;
            }
        }
    }
}
