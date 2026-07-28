using UnityEngine;

namespace Pakettiporina
{
    // Maaliportti: kun auto osuu, ilmoittaa RaceManagerille (joka laskee palkkion ja lahettaa OnFinish).
    [RequireComponent(typeof(Collider))]
    public class FinishTrigger : MonoBehaviour
    {
        bool triggered;

        void OnTriggerEnter(Collider other)
        {
            if (triggered) return;
            // tunnistaa auton Rigidbodyn kautta (auton collider voi olla lapsessa)
            var rb = other.attachedRigidbody;
            if (rb == null) return;
            if (RaceManager.Instance == null || !RaceManager.Instance.IsRacing) return;

            triggered = true;
            Debug.Log("[Finish] Auto osui maaliin!");
            RaceManager.Instance.FinishRace();
        }

        // Sallii uuden maalin, kun keikka alkaa uudelleen.
        void OnEnable() { GameEvents.OnRaceStart += Reset; }
        void OnDisable() { GameEvents.OnRaceStart -= Reset; }
        void Reset() { triggered = false; }
    }
}