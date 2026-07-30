using UnityEngine;

namespace Pakettiporina
{
    // Maalaa ajossa olevan auton hallissa valitun Maali-osan varilla.
    // Liita samaan objektiin kuin ArcadeCarController (auton juureen).
    //
    // HUOM: tama etsii lapsista Renderer-komponentin, jonka materiaalin nimessa
    // on "kori" (esim. "Auto_kori") � se on auton korin materiaali, JOKA EI KAYTA
    // tekstuuria (paletti-materiaalit kuten Auto_Paletti jatetaan koskematta,
    // etteivat silmat/renkaat/muut yksityiskohdat vari mukana).
    //
    // Kayttaa MaterialPropertyBlockia, jotta materiaalia ei tarvitse instansioida
    // erikseen jokaiselle autolle (parempi suorituskyky, ei materiaalivuotoja).
    public class CarPainter : MonoBehaviour
    {
        [Tooltip("Auton korin Renderer (materiaali jonka nimessa on 'kori'). Jos tyhja, etsitaan automaattisesti.")]
        public Renderer bodyRenderer;

        [Tooltip("Osuma materiaalin nimesta joka tunnistaa korin (pieni/iso kirjain ei valita)")]
        public string materialNameContains = "kori";

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        MaterialPropertyBlock mpb;

        void Start()
        {
            if (bodyRenderer == null) bodyRenderer = FindBodyRenderer();
            ApplySelectedColor();
        }

        Renderer FindBodyRenderer()
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true))
            {
                if (r.sharedMaterial != null &&
                    r.sharedMaterial.name.ToLower().Contains(materialNameContains.ToLower()))
                    return r;
            }
            Debug.LogWarning("[CarPainter] Kori-materiaalia ei loytynyt (' " + materialNameContains + "' ei tasmaa yhteenkaan materiaaliin).");
            return null;
        }

        // Hakee hallissa valitun Maali-osan varin GameManagerista ja maalaa autoon.
        public void ApplySelectedColor()
        {
            if (bodyRenderer == null) return;

            var gm = GameManager.Instance;
            if (gm == null || gm.SelectedParts == null) return;

            Color? chosen = null;
            foreach (var part in gm.SelectedParts)
            {
                if (part != null && part.category == PartCategory.Maali)
                {
                    chosen = part.color;
                    break;
                }
            }
            if (chosen == null) return; // ei maalivalintaa (esim. testataan peliscenea suoraan) -> jatetaan oletusvari

            if (mpb == null) mpb = new MaterialPropertyBlock();
            bodyRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColorId, chosen.Value);
            bodyRenderer.SetPropertyBlock(mpb);

            Debug.Log("[CarPainter] Auton vari asetettu: " + chosen.Value);
        }
    }
}
