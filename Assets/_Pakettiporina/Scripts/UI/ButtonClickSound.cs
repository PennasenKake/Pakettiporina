using UnityEngine;
using UnityEngine.UI;

namespace Pakettiporina
{
    // Lisaa napin klikkausaanen ilman etta koskee napin olemassa olevaan OnClick-kytkentaan.
    // Kytkeytyy AWAKESSA suoraan koodilla (ei Inspector-listener), joten se ei koskaan
    // riko tai tuplaa Editor-tyokalujen (esim. PakettiporinaWashSetup) jo kytkemia
    // pelitoimintoja — tama vain LISAA aanen paalle.
    // Lisataan automaattisesti kaikkiin Buttoneihin tyokalulla:
    // Pakettiporina/Korjaus/08 - KORJAA aanet (kaikki scenet)
    [RequireComponent(typeof(Button))]
    public class ButtonClickSound : MonoBehaviour
    {
        void Awake()
        {
            var btn = GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(PlayClick);
        }

        void PlayClick()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        }
    }
}
