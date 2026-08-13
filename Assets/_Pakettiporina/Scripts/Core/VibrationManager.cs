using UnityEngine;

namespace Pakettiporina
{
    // Varahtelypalaute (M5.11, Asetukset: "Varahtely"-liukukytkin). Staattinen apuluokka -
    // ei tarvitse omaa GameObjectia/Instancea, koska tama ei sailyta mitaan omaa tilaa
    // (asetus itse asuu SaveManager.Instance.vibrationOn:issa, sama "yksi yhteinen lahde"
    // -periaate kuin AudioManager.musicVolume/sfxVolume-kentilla).
    //
    // Kaytto: kutsu VibrationManager.Vibrate() siina kohtaa koodia jossa jotain
    // "tuntuvaa" tapahtuu (esim. auto kolahtaa kartioon). Handheld.Vibrate() toimii
    // vain Android/iOS-laitteilla - Editorissa/Standalonessa kutsu ei tee mitaan (ei virhetta).
    public static class VibrationManager
    {
        public static void Vibrate()
        {
            if (SaveManager.Instance != null && !SaveManager.Instance.vibrationOn) return;
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}
