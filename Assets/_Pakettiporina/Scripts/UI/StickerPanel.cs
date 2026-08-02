using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Pakettiporina
{
    // Tarrapaneeli (esim. MainMenussa): nayttaa kaikki tarrat, avatut varillisina,
    // lukossa olevat himmeana + tarvittava pistemaara. Jokainen "slot" on kasin tehty
    // UI-objekti (Image + valinnainen Lock-peite + valinnainen LockText), jotka
    // tarkistustyokalu kytkee automaattisesti nimen perusteella vastaavaan StickerDataan.
    public class StickerPanel : MonoBehaviour
    {
        [Serializable]
        public class Slot
        {
            public StickerData sticker;
            public Image image;
            [Tooltip("Valinnainen: harmaa/lukko-peite joka nakyy kun tarra on lukossa")]
            public GameObject lockOverlay;
            [Tooltip("Valinnainen: nayttaa esim. '50 p' kun tarra on lukossa")]
            public TMP_Text lockText;
        }

        [Tooltip("Koko paneeli, joka avataan/suljetaan napista. Piilotetaan automaattisesti alussa.")]
        public GameObject panelRoot;

        public List<Slot> slots = new List<Slot>();

        void Start()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        // "Tarrat/Pisteet"-napin OnClick.
        public void Toggle()
        {
            if (panelRoot == null) return;
            bool show = !panelRoot.activeSelf;
            panelRoot.SetActive(show);
            if (show) Refresh();
        }

        public void Refresh()
        {
            int pts = GameManager.Instance != null ? GameManager.Instance.Points : 0;
            foreach (var s in slots)
            {
                if (s.sticker == null || s.image == null) continue;
                s.image.sprite = s.sticker.image;

                bool unlocked = pts >= s.sticker.unlockPoints;
                s.image.color = unlocked ? Color.white : new Color(1f, 1f, 1f, 0.25f);

                if (s.lockOverlay != null) s.lockOverlay.SetActive(!unlocked);
                if (s.lockText != null)
                    s.lockText.text = unlocked ? "" : $"{s.sticker.unlockPoints} p";
            }
            Debug.Log($"[Stickers] Paivitetty ({slots.Count} tarraa, pisteet={pts}).");
        }
    }
}
