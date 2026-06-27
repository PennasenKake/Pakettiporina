using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pakettiporina
{
    // Pehmeasti autoa seuraava kamera. Liita Main Cameraan ja aseta Target = Car.
    public class CameraFollow : MonoBehaviour
    {
        [Header("Kohde")]
        [Tooltip("Auto jota kamera seuraa")] public Transform target;

        [Header("Asetukset")]
        [Tooltip("Sijainti auton takana ja ylapuolella")] public Vector3 offset = new Vector3(0f, 5f, -9f);
        [Tooltip("Seurannan pehmeys")] public float smooth = 6f;
        [Tooltip("Mihin korkeuteen kamera katsoo")] public float lookHeight = 1f;

        bool warned;

        void Start()
        {
            // Kerro consoleen onko kohde asetettu — yleisin aloittelijan virhe.
            if (target == null)
                Debug.LogWarning("[Camera] Target on tyhja! Veda Car-objekti CameraFollow'n Target-kenttaan.");
            else
                Debug.Log($"[Camera] Seurataan kohdetta: {target.name}");
        }

        void LateUpdate()
        {
            if (target == null)
            {
                // Varoita vain kerran, ei joka ruutu.
                if (!warned) { Debug.LogWarning("[Camera] Ei kohdetta — kamera ei liiku."); warned = true; }
                return;
            }

            // Haluttu sijainti = auton takana/ylapuolella, sen kaantymisen mukaan.
            Vector3 desired = target.position + target.rotation * offset;
            // Pehmea siirtyminen kohti haluttua sijaintia.
            transform.position = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);
            // Katse autoon (hieman maan ylapuolelle).
            transform.LookAt(target.position + Vector3.up * lookHeight);
        }
    }
}