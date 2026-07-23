using UnityEngine;

namespace Pakettiporina
{
    // Pyorittaa ja kellutaa objektia (esim. kerattava tahti).
    // Liita Star-prefabin VISUAALIIN (tai suoraan Star-objektiin).
    public class Spinner : MonoBehaviour
    {
        [Header("Pyoriminen")]
        [Tooltip("Astetta sekunnissa Y-akselin ympari")]
        public float spinSpeed = 90f;

        [Header("Kellunta")]
        [Tooltip("Kuinka korkealle keinuu (0 = ei kellu)")]
        public float bobHeight = 0.25f;
        [Tooltip("Keinunnan nopeus")]
        public float bobSpeed = 2f;

        Vector3 startPos;

        void Start()
        {
            startPos = transform.localPosition;
        }

        void Update()
        {
            // pyorii
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
            // kelluu pehmeasti ylos-alas
            if (bobHeight > 0f)
            {
                float y = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                transform.localPosition = startPos + new Vector3(0f, y, 0f);
            }
        }
    }
}