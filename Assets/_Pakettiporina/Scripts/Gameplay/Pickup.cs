using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pakettiporina
{
    // Kerattava tahti. Liita objektiin, jolla on Collider (Is Trigger paalla).
    [RequireComponent(typeof(Collider))]
    public class Pickup : MonoBehaviour
    {
        void Reset() { GetComponent<Collider>().isTrigger = true; }

        void OnTriggerEnter(Collider other)
        {
            // Vain auto kerää tahden.
            if (other.GetComponentInParent<ArcadeCarController>() == null) return;

            if (RaceManager.Instance != null) RaceManager.Instance.AddStar();
            Debug.Log("[Pickup] Tahti kerätty!");
            gameObject.SetActive(false); // piilota kerätty tahti
        }
    }
}