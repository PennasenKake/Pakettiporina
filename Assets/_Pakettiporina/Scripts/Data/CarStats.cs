using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pakettiporina
{
    // Auton viisi ominaisuutta yhtena rakenteena.
    [System.Serializable]
    public struct CarStats
    {
        public int voima;
        public int pito;
        public int keveys;
        public int kestavyys;
        public int kylmyys;

        // Rajaa kaikki arvot valille [min, max].
        public void Clamp(int min, int max)
        {
            voima = Mathf.Clamp(voima, min, max);
            pito = Mathf.Clamp(pito, min, max);
            keveys = Mathf.Clamp(keveys, min, max);
            kestavyys = Mathf.Clamp(kestavyys, min, max);
            kylmyys = Mathf.Clamp(kylmyys, min, max);
        }

        public override string ToString()
        {
            return $"voima {voima}, pito {pito}, keveys {keveys}, kesto {kestavyys}, kylmyys {kylmyys}";
        }
    }
}