using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pakettiporina
{
    // Maaliportti: kun auto ajaa lapi, laukaisee maalin kerran.
    // Liita objektiin, jolla on Collider (Is Trigger paalla).
    [RequireComponent(typeof(Collider))]
    public class FinishTrigger : MonoBehaviour
    {
        bool triggered;

        // Ajetaan kun komponentti lisataan editorissa: asetetaan collider triggeriksi.
        void Reset() { GetComponent<Collider>().isTrigger = true; }

        void OnEnable() { GameEvents.OnRaceStart += ResetTrigger; }
        void OnDisable() { GameEvents.OnRaceStart -= ResetTrigger; }
        void ResetTrigger() { triggered = false; } // nollaa uuden keikan alussa

        void OnTriggerEnter(Collider other)
        {
            if (triggered) return;

            // Vain auto laukaisee maalin (etsii kontrollerin myos lapsiobjektista).
            if (other.GetComponentInParent<ArcadeCarController>() == null) return;

            // Maali kelpaa vain kun keikka on kaynnissa.
            if (RaceManager.Instance == null || !RaceManager.Instance.IsRacing) return;

            triggered = true;
            Debug.Log("[Finish] Auto osui maaliin!");
            GameEvents.Finish();
        }
    }
}