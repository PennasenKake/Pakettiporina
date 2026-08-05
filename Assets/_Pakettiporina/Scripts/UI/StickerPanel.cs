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
            [Tooltip("Valinnainen: koko slotin nappi. Napautus yrittaa ostaa lukossa olevan tarran.")]
            public Button button;
        }

        [Tooltip("Koko paneeli, joka avataan/suljetaan napista. Piilotetaan automaattisesti alussa.")]
        public GameObject panelRoot;

        [Tooltip("Valinnainen: nayttaa nykyiset pisteet paneelin sisalla (esim. 'Pisteet: 340').")]
        public TMP_Text pointsText;

        public List<Slot> slots = new List<Slot>();

        void Start()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        void OnEnable()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnPointsChanged += HandlePointsChanged;
        }

        void OnDisable()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnPointsChanged -= HandlePointsChanged;
        }

        // Paivitetaan vain jos paneeli on parhaillaan nakyvissa - ei tarvitse piirtaa
        // suljettua paneelia joka pistemuutoksella.
        void HandlePointsChanged(int p)
        {
            if (panelRoot != null && panelRoot.activeSelf) Refresh();
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
            if (pointsText != null) pointsText.text = $"Pisteet: {pts}";

            foreach (var s in slots)
            {
                if (s.sticker == null || s.image == null) continue;
                s.image.sprite = s.sticker.image;

                bool unlocked = GameManager.Instance != null && GameManager.Instance.IsStickerUnlocked(s.sticker);
                s.image.color = unlocked ? Color.white : new Color(1f, 1f, 1f, 0.25f);

                if (s.lockOverlay != null) s.lockOverlay.SetActive(!unlocked);
                if (s.lockText != null)
                    s.lockText.text = unlocked ? "" : $"{s.sticker.unlockPoints} p";
            }
            Debug.Log($"[Stickers] Paivitetty ({slots.Count} tarraa, pisteet={pts}).");
        }

        // Slotin napin OnClick (indeksi kytketaan Editor-tyokalulla). Yrittaa ostaa
        // lukossa olevan tarran nykyisilla pisteilla; jo avattuun ei koske.
        public void OnSlotClicked(int index)
        {
            if (index < 0 || index >= slots.Count) return;
            var s = slots[index];
            if (s.sticker == null || GameManager.Instance == null) return;

            if (GameManager.Instance.IsStickerUnlocked(s.sticker))
            {
                Refresh();
                return;
            }

            bool bought = GameManager.Instance.TryUnlockSticker(s.sticker);
            if (bought)
            {
                Debug.Log($"[Stickers] Ostettu: {s.sticker.displayName}");
                Refresh();
            }
            else
            {
                Debug.Log($"[Stickers] Ei tarpeeksi pisteita: {s.sticker.displayName} ({s.sticker.unlockPoints} p tarvitaan).");
                StartCoroutine(FlashInsufficientFunds(s));
            }
        }

        System.Collections.IEnumerator FlashInsufficientFunds(Slot s)
        {
            if (s.lockText == null) yield break;
            var original = s.lockText.color;
            s.lockText.color = Color.red;
            yield return new WaitForSeconds(0.4f);
            if (s.lockText != null) s.lockText.color = original;
        }
    }
}
