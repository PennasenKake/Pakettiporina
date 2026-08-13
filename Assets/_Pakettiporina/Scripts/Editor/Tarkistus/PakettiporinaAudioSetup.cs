#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Pakettiporina.EditorTools
{
    // Aanien (M5.3/M6.3) tarkistus- ja korjaustyokalu. Toisin kuin muut Tarkistus/Korjaus-
    // tyokalut, tama kasittelee KAIKKI projektin pelisceneet peratysten (ei vain sen scenen
    // joka on juuri auki), koska AudioManager pitaa olla kytkettyna jokaisessa scenessa erikseen
    // (sama "duplikaatti per scene" -periaate kuin GameManager/SaveManagerilla, ks. PakettiporinaWashSetup).
    //
    // Sijoita: Assets/_Pakettiporina/Scripts/Editor/Tarkistus/
    // Kaytto: Pakettiporina -> Tarkistus/06 Tarkista aanet, Korjaus/08 KORJAA aanet
    //
    // HUOM: tyokalu vaihtaa avointa scenea jokaisen kasiteltavan scenen kohdalla ja
    // palauttaa lopuksi alunperin avoinna olleen scenen. Jos avoimessa scenessa on
    // tallentamattomia muutoksia, tyokalu keskeyttaa eika tee mitaan (ettei muutoksia
    // huku scenevaihdossa) - tallenna ensin (Ctrl+S).
    public static class PakettiporinaAudioSetup
    {
        const string SCENES_DIR = "Assets/Scenes";
        const string SFX_DIR = "Assets/_Pakettiporina/Audio/SFX";
        const string MUSIC_DIR = "Assets/_Pakettiporina/Audio/Music";

        [MenuItem("Pakettiporina/Tarkistus/06 - Tarkista aanet (kaikki scenet)")]
        public static void Diagnose() => Run(false);

        [MenuItem("Pakettiporina/Korjaus/08 - KORJAA aanet (kaikki scenet)")]
        public static void FixAll() => Run(true);

        static AudioClip Load(string path)
        {
            var c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            return c;
        }

        static void Run(bool fix)
        {
            var s = new StringBuilder();
            s.AppendLine(fix ? "=== AANIEN KORJAUS (kaikki scenet) ===" : "=== AANIEN TARKISTUS (kaikki scenet) ===");
            int problems = 0;
            void Prob(string msg) { s.AppendLine("! " + msg); problems++; }

            // ---------- 1. Lataa klipit kerran ----------
            var countdown = Load($"{SFX_DIR}/pakettiporina_aani_countdown.wav");
            var go = Load($"{SFX_DIR}/pakettiporina_aani_go.wav");
            var tahti = Load($"{SFX_DIR}/pakettiporina_aani_tahti.wav");
            var maali = Load($"{SFX_DIR}/pakettiporina_aani_maali.wav");
            var kupla = Load($"{SFX_DIR}/pakettiporina_aani_kupla.wav");
            var nappi = Load($"{SFX_DIR}/pakettiporina_aani_nappi.wav");
            var boost = Load($"{SFX_DIR}/pakettiporina_aani_boost.wav");
            var latakko = Load($"{SFX_DIR}/pakettiporina_aani_latakko.wav");
            var kolhu = Load($"{SFX_DIR}/pakettiporina_aani_kolhu.wav");
            var sopii = Load($"{SFX_DIR}/pakettiporina_aani_sopii.wav");
            var avaus = Load($"{SFX_DIR}/pakettiporina_aani_avaus.wav");
            var moottori = Load($"{SFX_DIR}/pakettiporina_aani_moottori.wav");
            var musValikko = Load($"{MUSIC_DIR}/pakettiporina_musiikki_valikko.wav");
            var musHalli = Load($"{MUSIC_DIR}/pakettiporina_musiikki_halli.wav");
            var musRata = Load($"{MUSIC_DIR}/pakettiporina_musiikki_rata.wav");
            var musPesu = Load($"{MUSIC_DIR}/pakettiporina_musiikki_pesu.wav");

            var clipFields = new (string name, AudioClip clip)[]
            {
                ("countdown", countdown), ("go", go), ("tahti", tahti), ("maali", maali),
                ("kupla", kupla), ("nappi", nappi), ("boost", boost), ("latakko", latakko),
                ("kolhu", kolhu), ("sopii", sopii), ("avaus", avaus), ("moottori", moottori),
                ("musiikki: valikko", musValikko), ("musiikki: halli", musHalli),
                ("musiikki: rata", musRata), ("musiikki: pesu", musPesu),
            };
            int missingAssets = clipFields.Count(c => c.clip == null);
            if (missingAssets > 0)
            {
                s.AppendLine($"VAROITUS: {missingAssets}/{clipFields.Length} aaniklippia puuttuu kansioista " +
                             $"'{SFX_DIR}' / '{MUSIC_DIR}' - tarkista tiedostonimet.");
                foreach (var c in clipFields.Where(c => c.clip == null)) s.AppendLine($"   puuttuu: {c.name}");
            }
            else
            {
                s.AppendLine($"Kaikki {clipFields.Length} aaniklippia loytyivat.");
            }

            // ---------- 2. Turvatarkistus: avoimessa scenessa ei saa olla tallentamattomia muutoksia ----------
            var originalScene = EditorSceneManager.GetActiveScene();
            string originalPath = originalScene.path;
            if (originalScene.isDirty)
            {
                s.AppendLine("\nKESKEYTETTY: avoimessa scenessa on tallentamattomia muutoksia. " +
                             "Tallenna ensin (Ctrl+S) ja aja tyokalu uudelleen - tama tyokalu vaihtaa " +
                             "avointa scenea useita kertoja eika halua hukata muokkauksiasi.");
                Debug.LogWarning(s.ToString());
                return;
            }

            // ---------- 3. Kaikki peliscenet Assets/Scenes-kansiosta ----------
            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { SCENES_DIR });
            var scenePaths = sceneGuids.Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p).ToList();
            if (scenePaths.Count == 0)
            {
                s.AppendLine($"VIRHE: yhtaan scenea ei loytynyt kansiosta {SCENES_DIR}.");
                Debug.LogError(s.ToString());
                return;
            }

            int scenesTouched = 0, managersCreated = 0, clipsAssigned = 0, buttonsFixed = 0, buttonsMissing = 0;

            foreach (var scenePath in scenePaths)
            {
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                s.AppendLine($"\n--- {sceneName} ---");

                // AudioManager: etsi tai luo
                var mgr = Object.FindObjectOfType<AudioManager>(true);
                if (mgr == null)
                {
                    Prob($"{sceneName}: AudioManager puuttuu.");
                    if (fix)
                    {
                        var mgrGo = new GameObject("AudioManager");
                        mgr = mgrGo.AddComponent<AudioManager>();
                        Undo.RegisterCreatedObjectUndo(mgrGo, "AudioManager");
                        s.AppendLine("  -> luotu");
                        managersCreated++;
                    }
                }

                if (mgr != null)
                {
                    void Assign(string label, ref AudioClip field, AudioClip clip)
                    {
                        if (field != null) return; // ei ylikirjoiteta kasin asetettua
                        if (clip == null) return;
                        if (fix) { field = clip; clipsAssigned++; }
                        else Prob($"{sceneName}: AudioManager.{label} ei kytketty.");
                    }

                    Assign("Countdown Beep", ref mgr.countdownBeep, countdown);
                    Assign("Go Sound", ref mgr.goSound, go);
                    Assign("Star Pickup", ref mgr.starPickup, tahti);
                    Assign("Finish Fanfare", ref mgr.finishFanfare, maali);
                    Assign("Bubble Pop", ref mgr.bubblePop, kupla);
                    Assign("Button Click", ref mgr.buttonClick, nappi);
                    Assign("Boost Sound", ref mgr.boostSound, boost);
                    Assign("Splash Sound", ref mgr.splashSound, latakko);
                    Assign("Bump Sound", ref mgr.bumpSound, kolhu);
                    Assign("Fit Good Sound", ref mgr.fitGoodSound, sopii);
                    Assign("Unlock Sound", ref mgr.unlockSound, avaus);
                    Assign("Engine Loop", ref mgr.engineLoop, moottori);
                    Assign("Menu Music", ref mgr.menuMusic, musValikko);
                    Assign("Garage Music", ref mgr.garageMusic, musHalli);
                    Assign("Race Music", ref mgr.raceMusic, musRata);
                    Assign("Wash Music", ref mgr.washMusic, musPesu);

                    if (fix) EditorUtility.SetDirty(mgr);
                }

                // Nappien klikkausaani: lisaa ButtonClickSound-komponentti jokaiseen Buttoniin.
                // Tama EI koske napin olemassa olevaan OnClick-kytkentaan (ks. ButtonClickSound.cs).
                var buttons = Object.FindObjectsOfType<Button>(true);
                foreach (var btn in buttons)
                {
                    if (btn.GetComponent<ButtonClickSound>() != null) continue;
                    if (fix)
                    {
                        Undo.AddComponent<ButtonClickSound>(btn.gameObject);
                        buttonsFixed++;
                    }
                    else
                    {
                        buttonsMissing++;
                    }
                }
                if (buttons.Length > 0)
                    s.AppendLine(fix
                        ? $"  Napit: {buttons.Length} kpl kasitelty."
                        : $"  Napit: {buttons.Length} kpl, joista aanetta {buttons.Count(b => b.GetComponent<ButtonClickSound>() == null)}.");

                if (fix)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    s.AppendLine("  Tallennettu.");
                }
                scenesTouched++;
            }

            // ---------- 4. Palauta alunperin avoinna ollut scene ----------
            if (!string.IsNullOrEmpty(originalPath))
                EditorSceneManager.OpenScene(originalPath, OpenSceneMode.Single);

            s.AppendLine($"\nKasitelty {scenesTouched} scenea. " +
                         (fix
                             ? $"AudioManagereita luotu: {managersCreated}, klippeja kytketty: {clipsAssigned}, napteihin lisatty aani: {buttonsFixed}."
                             : $"Loytyi {problems} huomiota, {buttonsMissing} nappia ilman aanta."));
            s.AppendLine(fix
                ? "\nValmis. Palautettiin alkuperainen avoin scene."
                : "\nAja 'Pakettiporina/Korjaus/08 - KORJAA aanet (kaikki scenet)' korjataksesi.");
            Debug.Log(s.ToString());
        }
    }
}
#endif
