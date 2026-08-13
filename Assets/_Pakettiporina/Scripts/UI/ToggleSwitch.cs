using UnityEngine;
using UnityEngine.UI;

namespace Pakettiporina
{
    // iOS-tyylinen liukukytkin: kytkee Unityn Togglen visuaalisesti liukuvaksi kytkimeksi
    // (soikea/pilleri-muotoinen rata + pyorea nuppi, joka liukuu ja rata vaihtaa varia
    // paalla/pois-tilan mukaan). Kaytetaan Asetuksissa "Aanet"-rivilla kahden erillisen
    // radiopainikkeen ("Aanet paalla"/"Aanet pois") sijaan.
    //
    // Ei kuuntele Toggle.onValueChanged-tapahtumaa erikseen - Update() lukee suoraan
    // toggle.isOn joka framella. Tama toimii aina oikein riippumatta siita asetettiinko
    // tila napautuksella (SetIsOn) vai koodista ilman tapahtumaa (SetIsOnWithoutNotify,
    // jota SettingsManager.Refresh() kayttaa) - ei tarvitse erillista "paivita nappi"-kutsua.
    [RequireComponent(typeof(Toggle))]
    public class ToggleSwitch : MonoBehaviour
    {
        [Tooltip("Rata (soikea tausta) - varitetaan onColor/offColor:lla.")]
        public Image track;
        [Tooltip("Nuppi, joka liukuu radan sisalla.")]
        public RectTransform knob;

        public Color onColor = new Color(0.21568628f, 0.5411765f, 0.8666667f, 1f);
        public Color offColor = new Color(0.55f, 0.53f, 0.56f, 1f);

        [Tooltip("Nupin X-anchoredPosition kun kytkin on POIS PAALTA.")]
        public float offKnobX = -55f;
        [Tooltip("Nupin X-anchoredPosition kun kytkin on PAALLA.")]
        public float onKnobX = 55f;

        [Tooltip("Kuinka nopeasti nuppi/vari liukuu kohti tavoitetilaa.")]
        public float animSpeed = 10f;

        Toggle toggle;

        void Awake()
        {
            toggle = GetComponent<Toggle>();
        }

        void OnEnable()
        {
            // Napauta heti oikeaan tilaan kun paneeli avataan - ei animaatiota framen 0 aikana.
            SnapToState(toggle != null && toggle.isOn);
        }

        public void SnapToState(bool isOn)
        {
            if (track != null) track.color = isOn ? onColor : offColor;
            if (knob != null)
            {
                var p = knob.anchoredPosition;
                p.x = isOn ? onKnobX : offKnobX;
                knob.anchoredPosition = p;
            }
        }

        void Update()
        {
            if (toggle == null) return;
            bool isOn = toggle.isOn;
            float targetX = isOn ? onKnobX : offKnobX;
            Color targetColor = isOn ? onColor : offColor;
            float t = Time.unscaledDeltaTime * animSpeed;

            if (track != null) track.color = Color.Lerp(track.color, targetColor, t);
            if (knob != null)
            {
                var p = knob.anchoredPosition;
                p.x = Mathf.Lerp(p.x, targetX, t);
                knob.anchoredPosition = p;
            }
        }
    }
}
