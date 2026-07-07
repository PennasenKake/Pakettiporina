using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pakettiporina
{
    // Kosketus-/hiiripainike, joka pysyy paalla niin kauan kuin sita painetaan
    // (tavallinen Button reagoi vain klikkaukseen). Liita UI-nappiin.
    // Aseta Car (auton CarInput) ja Action.
    public class TouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public enum Action { Gas, Brake, Left, Right }

        [Tooltip("Auton CarInput. Jos tyhja, etsitaan automaattisesti scenesta.")]
        public CarInput car;
        public Action action = Action.Gas;

        void Start()
        {
            if (car == null) car = FindObjectOfType<CarInput>();
            if (car == null) Debug.LogWarning("[TouchButton] CarInput puuttuu — aseta Car-kentta.");
        }

        public void OnPointerDown(PointerEventData e) { Set(true); }
        public void OnPointerUp(PointerEventData e) { Set(false); }
        void OnDisable() { Set(false); } // ei jaa jumiin jos nappi piilotetaan

        void Set(bool v)
        {
            if (car == null) return;
            switch (action)
            {
                case Action.Gas: car.touchGas = v; break;
                case Action.Brake: car.touchBrake = v; break;
                case Action.Left: car.touchLeft = v; break;
                case Action.Right: car.touchRight = v; break;
            }
        }
    }
}
