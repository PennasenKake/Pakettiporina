#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Pakettiporina.EditorTools
{
    // Kun olet siirtanyt/kiertanyt/skaalannut koristeita tai hienosaatanut Boost/
    // Latakko/Kartio-sijainteja kasin Hierarkiassa, tama tulostaa NYKYISET sijainnit
    // valmiina C#-koodina - kopioi konsolin tuloste ja liita se chattiin, niin
    // lahdekoodi (PakettiporinaDecor.cs / PakettiporinaTracks.cs) paivitetaan
    // vastaamaan scenen nykytilaa. Talla tavalla "12 - Lisaa teemakoristeet" ja
    // "11 - Lisaa pelielementit" eivat enaa ylikirjoita kasintehtyja hienosaatoja
    // seuraavalla ajokerralla, koska ne itse ASETTAVAT sijainnit koodista - koodin
    // pitaa siis vastata sita mita Hierarkiassa oikeasti on.
    //
    // HUOM: jos scenessa on Objektit-koristeita jotka on luotu ENNEN 7.8.2026
    // (ennen kuin PakettiporinaDecorMarker.modelName lisattiin), aja ensin kerran
    // "12 - Lisaa teemakoristeet talle radalle" (se tekee aina puhtaan uudelleen-
    // luonnin) jotta modelName-tieto on tallessa - vasta sen jalkeen kasin siirrellyt
    // sijainnit + tama vientityokalu antavat oikean tuloksen.
    //
    // Kaytto: Pakettiporina -> 16 - Vie sijainnit koodiksi (konsoli)
    public static class PakettiporinaExportPlacement
    {
        [MenuItem("Pakettiporina/16 - Vie sijainnit koodiksi (konsoli)")]
        public static void Export()
        {
            var s = new StringBuilder();
            string sceneName = EditorSceneManager.GetActiveScene().name;
            var preset = PakettiporinaTracks.Find(sceneName);
            s.AppendLine($"=== VIE SIJAINNIT KOODIKSI: {sceneName} ===");
            s.AppendLine("(Kopioi tarvittavat lohkot ja liita chattiin - lahdekoodi paivitetaan niin");
            s.AppendLine(" etta nama sijainnit sailyvat myos KORJAA-komentojen jalkeen.)\n");

            // --- Objektit: PakettiporinaDecorMarker -> PakettiporinaDecor.cs DecorItem-lista ---
            var markers = Object.FindObjectsOfType<PakettiporinaDecorMarker>(true)
                .OrderBy(m => m.transform.position.z)
                .ToList();
            if (markers.Count > 0)
            {
                s.AppendLine($"--- OBJEKTIT ({markers.Count} kpl) - liita PakettiporinaDecor.cs:n '{sceneName}'-presetin items-tauluun: ---");
                foreach (var m in markers)
                {
                    var t = m.transform;
                    string model = string.IsNullOrEmpty(m.modelName) ? t.name : m.modelName;
                    string instAttr = t.name != model ? $", instanceName = \"{t.name}\"" : "";
                    float rotY = NormAngle(t.eulerAngles.y);
                    float rotX = NormAngle(t.eulerAngles.x);
                    // HUOM: rotX tulostetaan vain jos poikkeaa nollasta (esim. veneet joilla
                    // on korjauskierto) - useimmilla objekteilla se on 0 eika sotke lohkoa.
                    // Signed-arvo (-90 eika 270) on luettavampi kun se on lahella nollaa.
                    string rotXAttr = "";
                    if (Mathf.Abs(rotX) > 0.5f && Mathf.Abs(rotX - 360f) > 0.5f)
                    {
                        float signedRotX = rotX > 180f ? rotX - 360f : rotX;
                        rotXAttr = $"rotX = {F(signedRotX,0)}f, ";
                    }
                    s.AppendLine($"new DecorItem {{ modelName = \"{model}\"{instAttr}, pos = new Vector3({F(t.position.x,1)}f, {F(t.position.y,1)}f, {F(t.position.z,1)}f), {rotXAttr}rotY = {F(rotY,0)}f, scale = {F(t.localScale.x,2)}f }},");
                }
                s.AppendLine();
            }
            else
            {
                s.AppendLine("(Ei koristeita loytynyt - aja ensin '12 - Lisaa teemakoristeet talle radalle'.)\n");
            }

            // --- Tehosteet: Boost_XX -> boostZ ---
            var boosts = AllNamed("Boost_").OrderBy(t => t.position.z).ToList();
            if (boosts.Count > 0)
            {
                s.AppendLine($"--- TEHOSTEET: Boostit ({boosts.Count} kpl) - liita '{sceneName}'-TrackDefin boostZ-tauluun (PakettiporinaTracks.cs): ---");
                s.AppendLine("boostZ = new[] { " + string.Join(", ", boosts.Select(t => $"{F(t.position.z,0)}f")) + " },\n");
            }

            // --- Esteet: Latakko_XX -> puddleZ, Kartio_XX -> cones ---
            var puddles = AllNamed("Latakko_").OrderBy(t => t.position.z).ToList();
            if (puddles.Count > 0)
            {
                s.AppendLine($"--- ESTEET: Latakot ({puddles.Count} kpl) - liita puddleZ-tauluun: ---");
                s.AppendLine("puddleZ = new[] { " + string.Join(", ", puddles.Select(t => $"{F(t.position.z,0)}f")) + " },\n");
            }

            var cones = AllNamed("Kartio_").OrderBy(t => t.position.z).ToList();
            if (cones.Count > 0 && preset != null)
            {
                s.AppendLine($"--- ESTEET: Kartiot ({cones.Count} kpl) - liita cones-tauluun: ---");
                var parts = cones.Select(t =>
                {
                    float roadX = PakettiporinaTracks.RoadX(t.position.z, preset);
                    float offset = t.position.x - roadX;
                    return $"({F(t.position.z,0)}f, {F(offset,1)}f)";
                });
                s.AppendLine("cones = new[] { " + string.Join(", ", parts) + " },\n");
            }
            else if (cones.Count > 0)
            {
                s.AppendLine("(Kartioita loytyi mutta radalle ei ole tunnettua presettia - offsetia ei voi laskea.)\n");
            }

            if (markers.Count == 0 && boosts.Count == 0 && puddles.Count == 0 && cones.Count == 0)
                s.AppendLine("Scenessa ei ole yhtaan tunnistettua radan elementtia.");

            Debug.Log(s.ToString());
        }

        static float NormAngle(float a)
        {
            a %= 360f;
            if (a < 0) a += 360f;
            return a;
        }

        // KRIITTINEN: kayttajan Windows/Unity-asetuksissa on suomalainen alue-asetus,
        // jossa desimaalierotin on PILKKU ("16,0") - oletus-string-interpolaatio
        // ({x:F1}) kayttaa jarjestelman kulttuuria ja olisi tuottanut PILKKUJA
        // desimaalierottimena, mika tekee tulostetusta C#-koodista rikkinaista
        // (esim. "Vector3(16,0f, 0f, -100,0f)" - vaarat pilkut, ei kaanny). Tama
        // pakottaa pisteen aina riippumatta kayttajan alue-asetuksesta.
        static string F(float v, int decimals) => v.ToString("F" + decimals, CultureInfo.InvariantCulture);

        static IEnumerable<Transform> AllNamed(string prefix)
        {
            foreach (var t in Object.FindObjectsOfType<Transform>(true))
                if (t.name.StartsWith(prefix)) yield return t;
        }
    }
}
#endif
