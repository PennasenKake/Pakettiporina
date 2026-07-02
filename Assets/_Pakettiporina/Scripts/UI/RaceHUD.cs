using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Pakettiporina
{
    // Yksinkertainen ajon HUD: tahtimaara, kuluva aika ja maali-paneeli.
    // Liita Canvasiin ja kytke kentat Inspectorissa.
    public class RaceHUD : MonoBehaviour
    {
        [Header("Viittaukset")]
        public RaceManager race;
        [Tooltip("Paneeli joka nakyy vasta maalissa (piilota oletuksena)")]
        public GameObject finishPanel;
        public TMP_Text messageText; // "Maali!"
        public TMP_Text starText;    // "Tahdet: 0"
        public TMP_Text timeText;    // kuluva aika (valinnainen)

        void OnEnable()
        {
            GameEvents.OnRaceStart += HandleStart;
            GameEvents.OnStarCollected += HandleStar;
            GameEvents.OnFinish += HandleFinish;
        }

        void OnDisable()
        {
            GameEvents.OnRaceStart -= HandleStart;
            GameEvents.OnStarCollected -= HandleStar;
            GameEvents.OnFinish -= HandleFinish;
        }

        void HandleStart()
        {
            if (finishPanel != null) finishPanel.SetActive(false);
            if (starText != null) starText.text = "Tahdet: 0";
        }

        void HandleStar(int total)
        {
            if (starText != null) starText.text = "Tahdet: " + total;
        }

        void HandleFinish()
        {
            if (finishPanel != null) finishPanel.SetActive(true);
            if (messageText != null) messageText.text = "Maali!";
        }

        void Update()
        {
            if (timeText != null && race != null && race.IsRacing)
                timeText.text = race.Elapsed.ToString("F1") + " s";
        }

        // Kytke tama "Uudestaan"-napin OnClickiin.
        public void OnRestartButton()
        {
            if (race != null) race.Restart();
        }
    }
}
