#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Pakettiporina.EditorTools
{
    // LISATTY 23.11.2026: suorituskyvyn ja akun tyokalut.
    //  01 - Aja automaattimittaus: kay scenet lapi Play-tilassa ja kirjoittaa raportin (PerfBenchmark).
    //  02 - Staattinen tarkistus: asetukset ja resurssit (ei vaadi Playta), raportti Docs/Suorituskyky_<pvm>_staattinen.md.
    //  03 - Kayta akku- ja suorituskykysuositukset: ARM64, stripping (vaihtaa Player-asetuksia, kysyy ensin).
    public static class PakettiporinaPerf
    {
        [MenuItem("Pakettiporina/Suorituskyky/01 - Aja automaattimittaus (Play)")]
        public static void RunBenchmark()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("[Perf] Lopeta Play ensin."); return; }
            EditorPrefs.SetBool(PerfBenchmark.PrefKey, true);
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("Pakettiporina/Suorituskyky/02 - Staattinen tarkistus (asetukset ja resurssit)")]
        public static void StaticCheck()
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Suorituskyky: staattinen tarkistus " + System.DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
            sb.AppendLine();
            int warn = 0;
            void W(string m) { sb.AppendLine("- ! " + m); warn++; }
            void I(string m) { sb.AppendLine("- " + m); }

            sb.AppendLine("## Player-asetukset (Android)");
            var arch = PlayerSettings.Android.targetArchitectures;
            I("Arkkitehtuurit: " + arch);
            if ((arch & AndroidArchitecture.ARMv7) != 0) W("ARMv7 on mukana: kasvattaa kokoa; minSdk " + PlayerSettings.Android.minSdkVersion + " -laitteet ovat 64-bittisiä (Korjaus: Suorituskyky/03).");
            I("Scripting backend: " + PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android));
            if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) != ScriptingImplementation.IL2CPP) W("Käytä IL2CPP:tä.");
            I("Managed stripping: " + PlayerSettings.GetManagedStrippingLevel(BuildTargetGroup.Android));
            I("minSdk: " + PlayerSettings.Android.minSdkVersion + (PlayerSettings.Android.minSdkVersion >= (AndroidSdkVersions)31 ? " (Android 12+: vanhat lapsen kierrätyspuhelimet jäävät pois)" : ""));
            I("Graphics API:t: " + string.Join(", ", PlayerSettings.GetGraphicsAPIs(BuildTarget.Android)));
            I("Development Build -overlay: Debug-buildissa neljän sormen kosketus 1 s näyttää FPS/GC/muisti.");

            sb.AppendLine();
            sb.AppendLine("## Laatu ja renderöinti");
            int q = QualitySettings.GetQualityLevel();
            I("Nykyinen laatutaso editorissa: " + QualitySettings.names[q]);
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                I(string.Format("{0}: vSync {1}, AA {2}, varjot {3}, pikselivalot {4}", QualitySettings.names[i], QualitySettings.vSyncCount, QualitySettings.antiAliasing, QualitySettings.shadows, QualitySettings.pixelLightCount));
            }
            QualitySettings.SetQualityLevel(q, false);
            I("Android käyttää oletuksena laatutasoa, joka on määritelty QualitySettingsissa (Android: Balanced). Balanced: HDR päällä, varjot päällä.");
            // 24.11.2026: varoitus vain jos HDR on oikeasti paalla (aiemmin aina).
            // (reflektiolla: URP-tyyppia ei viitata suoraan, jotta tyokalu kaantyy vaikka URP-assembly puuttuisi viittauksista)
            // Kaydaan KAIKKI laatutasot lapi (editorin aktiivinen taso ei ole Androidin taso).
            for (int qi = 0; qi < QualitySettings.names.Length; qi++)
            {
                var rp = QualitySettings.GetRenderPipelineAssetAt(qi);
                if (rp == null) continue;
                var hdrProp = rp.GetType().GetProperty("supportsHDR");
                bool hdr = hdrProp != null && (bool)hdrProp.GetValue(rp, null);
                if (hdr) W("Laatutaso '" + QualitySettings.names[qi] + "' (URP-profiili '" + rp.name + "'): HDR on päällä (kallis mobiilissa, ei tarvita tässä tyylissä).");
                else I("Laatutaso '" + QualitySettings.names[qi] + "' (URP-profiili '" + rp.name + "'): HDR pois tai ei luettavissa.");
            }

            sb.AppendLine();
            sb.AppendLine("## Scenet ja projektin siisteys");
            foreach (var bs in EditorBuildSettings.scenes)
            {
                string sn = Path.GetFileNameWithoutExtension(bs.path);
                if (bs.enabled) continue;
                if (Progress.IsPlanned(Progress.TrackKey(sn))) I("Scene '" + sn + "' on tarkoituksella pois päältä (suunniteltu rata, valmistuu myöhemmin).");
                else W("Scene '" + sn + "' on Build Settingsissa pois päältä (Hallin ratalista ei näytä sitä).");
            }
            int docTex = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Pakettiporina/Docs" }).Length;
            if (docTex > 0) W(docTex + " kuvaa kansiossa Assets/_Pakettiporina/Docs (tuodaan Unityyn turhaan): siirrä kansio Assetsin ulkopuolelle.");

            sb.AppendLine();
            sb.AppendLine("## Tekstuurit");
            var texGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Pakettiporina" });
            long bigPixels = 0; int mipUi = 0, uiCount = 0, readable = 0;
            foreach (var g in texGuids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                var imp = AssetImporter.GetAtPath(p) as TextureImporter; if (imp == null) continue;
                if (imp.isReadable) { readable++; W("Luettava tekstuuri (tuplaa muistin): " + p); }
                if (p.Contains("/Art/UI/") || p.Contains("/Resources/"))
                {
                    uiCount++;
                    if (imp.mipmapEnabled && imp.textureType == TextureImporterType.Sprite) mipUi++;
                }
                var t = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                if (t != null && t.width * t.height >= 1500000) { bigPixels += t.width * t.height; I(string.Format("Iso tekstuuri: {0} ({1}x{2}, pakkaus {3})", p, t.width, t.height, imp.textureCompression)); }
            }
            I(texGuids.Length + " tekstuuria projektissa; yli 1,5 Mpx yhteensä " + bigPixels / 1000000f + " Mpx; luettavia " + readable + ".");
            if (mipUi > 0) W(mipUi + " sprite-tekstuurissa mipmapit päällä (turha muisti).");

            sb.AppendLine();
            sb.AppendLine("## Mallit");
            var fbx = AssetDatabase.FindAssets("t:Model", new[] { "Assets/_Pakettiporina" });
            int readableMeshes = 0;
            foreach (var g in fbx)
            {
                var m = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(g)) as ModelImporter;
                if (m != null && m.isReadable) readableMeshes++;
            }
            I(fbx.Length + " mallia; luettavia (Read/Write) " + readableMeshes + (readableMeshes > 0 ? " -> kytke pois jos ei tarvita." : "."));

            sb.AppendLine();
            sb.AppendLine("## Äänet");
            foreach (var g in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/_Pakettiporina/Audio" }))
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                var imp = AssetImporter.GetAtPath(p) as AudioImporter; if (imp == null) continue;
                var s = imp.defaultSampleSettings;
                var c = AssetDatabase.LoadAssetAtPath<AudioClip>(p);
                if (p.Contains("/Music/"))
                    I(string.Format("{0}: {1:0.0} s, {2} Hz, {3} kanavaa, load {4}, {5}", Path.GetFileName(p), c.length, c.frequency, c.channels, s.loadType, s.compressionFormat)
                      + (s.loadType == AudioClipLoadType.DecompressOnLoad ? " (puretaan muistiin kokonaan: " + (c.length * c.frequency * c.channels * 2 / 1048576f).ToString("0.0") + " MB)" : ""));
            }
            int lazySfx = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/_Pakettiporina/Audio/SFX" })
                .Count(g => { var a = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(g)) as AudioImporter; return a != null && !a.defaultSampleSettings.preloadAudioData; });
            if (lazySfx > 0) I(lazySfx + " tehosteääntä ilman Preload Audio Data: ensimmäinen toisto voi nykäistä (lataus levyltä). Lyhyet SFX kannattaa esiladata.");

            sb.AppendLine();
            sb.AppendLine("## Koodi (tiedossa olevat kuumat kohdat)");
            I("Juice: nappien automaattiskannaus: 0,25 s vain 3 s scenen latauksen jälkeen, muuten 1,5 s välein ja vain aktiiviset (korjattu 23.11.).");
            I("CutoutAvoider: skannaus 0,1 s vain 3 s scenen latauksen jälkeen, muuten 0,25 s (korjattu 23.11.).");
            I("RaceHUD.LateUpdate: laattojen koko ja paikka päivitettiin joka ruutu (korjattu: vain tekstin muuttuessa).");
            I("Application.targetFrameRate / Screen.sleepTimeout: ei hallittu (korjattu: PerformanceManager).");

            sb.AppendLine();
            sb.AppendLine(warn == 0 ? "**Ei varoituksia.**" : "**" + warn + " varoitusta.**");
            string file = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/Suorituskyky_" + System.DateTime.Now.ToString("yyyy-MM-dd") + "_staattinen.md"));
            File.WriteAllText(file, sb.ToString(), new UTF8Encoding(true));
            Debug.Log("[Perf] Staattinen tarkistus valmis (" + warn + " varoitusta): " + file);
        }

        // Lyhyiden tehosteaanien esilataus: ensimmainen toisto ei nykaise (lataus levyltä pelin aikana).
        [MenuItem("Pakettiporina/Suorituskyky/04 - Esilataa tehosteäänet (Preload Audio Data)")]
        public static void PreloadSfx()
        {
            int changed = 0;
            foreach (var g in AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/_Pakettiporina/Audio/SFX" }))
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                var imp = AssetImporter.GetAtPath(p) as AudioImporter; if (imp == null) continue;
                var s = imp.defaultSampleSettings;
                if (s.preloadAudioData) continue;
                s.preloadAudioData = true;
                imp.defaultSampleSettings = s;
                imp.SaveAndReimport();
                changed++;
            }
            Debug.Log("[Perf] Esiladataan " + changed + " tehosteääntä (preloadAudioData = true).");
        }

        [MenuItem("Pakettiporina/Suorituskyky/03 - Käytä akku- ja kokosuositukset (ARM64)")]
        public static void ApplyRecommendations()
        {
            if (!EditorUtility.DisplayDialog("Akku- ja kokosuositukset",
                "Asetetaan Androidille vain ARM64 (poistaa ARMv7:n; pienempi build, kaikki nykylaitteet 64-bittisiä). Jatketaanko?", "Kyllä", "Peruuta")) return;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            Debug.Log("[Perf] Android: IL2CPP + ARM64.");
        }
    }
}
#endif
