using UnityEngine;
using UnityEngine.UI;

namespace Pakettiporina
{
    // M5.15/20.8.2026 (kayttaja: "Yotila toggle ei tee mitaan taalla hetkella,
    // toteutetaan se loppuun"): SaveManager.darkMode tallentui jo aiemmin, mutta ei
    // vaikuttanut mihinkaan visuaalisesti (kayttajan silloinen pyynto: "pohja nyt").
    //
    // TOTEUTUSTAPA: yksi tumma, puoliläpinäkyvä kalvo KOKO ruudun paalla, jokaisessa
    // scenessa oma instanssi (sama "yksi per scene" -periaate kuin DirtSpots/DentSpots).
    // EI yksittaisten korttien/paneelien uudelleenvarittamista - peli koostuu 9
    // scenesta joissa on kymmenia eri paneeleita/kortteja (Asetukset, Tietoja,
    // Tietosuoja, Nollaa-vahvistus, Halli, Pesu, jokaisen radan HUD...) ja jokaisen
    // yksittainen uudelleenvarittaminen olisi iso, virhealtis tyo joka voisi rikkoa
    // jo hiottuja kontrasteja. Yksi kalvo Canvasin PAALIMMAISENA lapsena kattaa
    // AINA koko nakyman (myos 3D-pelimaailman: auto/rata), tuottaa yhtenaisen
    // "ilta/yo"-tunnelman kaikkialla, ja on taattu toimimaan riippumatta scenen
    // sisaisesta UI-rakenteesta.
    //
    // Kalvo EI koskaan sieppaa kosketusta (raycastTarget=false aina) - se on pelkkä
    // visuaalinen suodin, ei koskaan esta pelaamista tai napautuksia.
    [RequireComponent(typeof(Image))]
    public class DarkModeOverlay : MonoBehaviour
    {
        [Tooltip("Kalvon lapinakymattomyys kun Yotila on paalla (0=ei nay, 1=musta ruutu). " +
                 "0.55 tummentaa selvasti mutta jattaa kaiken silti nakyviin/luettavaksi.")]
        [Range(0f, 1f)] public float nightAlpha = 0.55f;

        Image img;

        void Awake()
        {
            img = GetComponent<Image>();
            img.raycastTarget = false; // ei koskaan esta kosketusta
            Refresh();
        }

        void OnEnable() => Refresh();

        // Kutsutaan SaveManager.ApplyToScene():sta (ajetaan aina scenen latautuessa,
        // seka valittomasti kun kayttaja togglaa Yotilan Asetuksista - sama kaava
        // kuin ohjaustapa/nopeustaso, ks. SettingsManager.OnDarkModeToggled).
        public void Refresh()
        {
            if (img == null) img = GetComponent<Image>();
            if (img == null) return;
            bool on = SaveManager.Instance != null && SaveManager.Instance.darkMode;
            var c = img.color;
            c.a = on ? nightAlpha : 0f;
            img.color = c;
        }
    }
}
