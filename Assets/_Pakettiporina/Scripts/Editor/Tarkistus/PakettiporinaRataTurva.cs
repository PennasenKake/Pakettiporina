#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Pakettiporina.EditorTools
{
    // LISATTY 24.11.2026 (TODO osio 2: "auto ajaa mereen / ei paase maaliin; maali-portti ja koristeiden koot").
    // Ajetaan AUKI OLEVALLE ratascenelle.
    //   Pakettiporina -> Tarkistus/09 - Tarkista rata (kaiteet, maaliportti, koristeet)   (vain raportti)
    //   Pakettiporina -> Korjaus/11 - Lisaa ratakaiteet ja rajoita maaliportti            (muuttaa scenea, tallentamaton)
    // Kaiteet: nakymattomat, kitkattomat BoxColliderit radan molemmin puolin ('Ratakaiteet'-juuri), jotta auto ei
    // voi ajaa mereen/maailman reunalle. Tie on 12 yks. leveä; kaide on tien keskilinjasta +-(6 + marginaali).
    // Marginaali: Satama 8 (meri lahella), muut 22 (ruohoa saa ajaa, mutta ei maailman reunalle).
    // Maaliportti: skaalataan tasaisesti jos se on suurempi kuin MAX_GATE_W x MAX_GATE_H (kamera jai portin sisaan).
    // Idempotentti: vanha 'Ratakaiteet' korvataan.
    public static class PakettiporinaRataTurva
    {
        const float ROAD_HALF = 6f;
        const float WALL_H = 3f, WALL_T = 1f, STEP = 8f;
        const float MAX_GATE_W = 18f, MAX_GATE_H = 9f;
        const string ROOT = "Ratakaiteet";
        const string MAT_PATH = "Assets/_Pakettiporina/Data/Kaide_ei_kitkaa.physicMaterial";

        [MenuItem("Pakettiporina/Tarkistus/09 - Tarkista rata (kaiteet, maaliportti, koristeet)")]
        public static void Diagnose() { Run(false); }

        [MenuItem("Pakettiporina/Korjaus/11 - Lisaa ratakaiteet ja rajoita maaliportti")]
        public static void Fix() { Run(true); }

        static Bounds BoundsOf(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>(false);
            if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        static void Run(bool fix)
        {
            var sceneName = EditorSceneManager.GetActiveScene().name;
            var s = new StringBuilder(fix ? "=== RATA: KORJAUS (" + sceneName + ") ===\n" : "=== RATA: TARKISTUS (" + sceneName + ") ===\n");
            int problems = 0;
            void Prob(string m) { s.AppendLine("! " + m); problems++; }
            void Info(string m) { s.AppendLine("i " + m); }

            var track = PakettiporinaTracks.Find(sceneName);
            if (track == null) { Debug.LogWarning("[RataTurva] Scene '" + sceneName + "' ei ole ratalistalla (PakettiporinaTracks.ALL). Avaa ratascene."); return; }

            // --- 1) kaiteet
            var root = GameObject.Find(ROOT);
            int existing = root != null ? root.transform.childCount : 0;
            if (existing == 0) Prob("Radalla ei ole kaiteita: auto voi ajaa maailman reunalle/mereen (CarRecovery palauttaa, mutta kaide on parempi).");
            else Info("Kaiteita " + existing + " kpl.");
            if (fix)
            {
                if (root != null) Undo.DestroyObjectImmediate(root);
                root = new GameObject(ROOT);
                Undo.RegisterCreatedObjectUndo(root, "Ratakaiteet");
                var mat = AssetDatabase.LoadAssetAtPath<PhysicMaterial>(MAT_PATH);
                if (mat == null)
                {
                    mat = new PhysicMaterial("Kaide_ei_kitkaa") { dynamicFriction = 0f, staticFriction = 0f, frictionCombine = PhysicMaterialCombine.Minimum, bounciness = 0f };
                    Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Application.dataPath, "_Pakettiporina/Data")));
                    AssetDatabase.CreateAsset(mat, MAT_PATH);
                }
                float margin = sceneName == "Satama" ? 8f : 22f;
                float off = ROAD_HALF + margin;
                float z0 = track.startZ - 12f, z1 = track.finishZ + 30f;
                int made = 0;
                for (int side = -1; side <= 1; side += 2)
                    for (float z = z0; z < z1; z += STEP)
                    {
                        float za = z, zb = Mathf.Min(z + STEP, z1);
                        var a = new Vector3(PakettiporinaTracks.RoadX(za, track) + side * off, 0f, za);
                        var b = new Vector3(PakettiporinaTracks.RoadX(zb, track) + side * off, 0f, zb);
                        var seg = new GameObject("Kaide_" + (side < 0 ? "V" : "O") + "_" + made);
                        seg.transform.SetParent(root.transform, false);
                        seg.transform.position = (a + b) * 0.5f + Vector3.up * (WALL_H * 0.5f);
                        var d = b - a;
                        seg.transform.rotation = Quaternion.LookRotation(d.normalized, Vector3.up);
                        var col = seg.AddComponent<BoxCollider>();
                        col.size = new Vector3(WALL_T, WALL_H, d.magnitude + 0.5f);   // pieni paallekkaisyys: ei rakoja kaarteissa
                        col.sharedMaterial = mat;
                        made++;
                    }
                // paatykaiteet (estavat ajon taaksepain maailman reunalle)
                foreach (float zEnd in new[] { z0, z1 })
                {
                    var cap = new GameObject("Kaide_paaty_" + (zEnd < 0 ? "alku" : "loppu"));
                    cap.transform.SetParent(root.transform, false);
                    cap.transform.position = new Vector3(PakettiporinaTracks.RoadX(zEnd, track), WALL_H * 0.5f, zEnd);
                    var col = cap.AddComponent<BoxCollider>();
                    col.size = new Vector3((ROAD_HALF + margin) * 2f + WALL_T, WALL_H, WALL_T);
                    col.sharedMaterial = mat;
                    made++;
                }
                Info("Luotu " + made + " kaidetta (marginaali " + margin + " yks. tien reunasta, korkeus " + WALL_H + ").");
            }

            // --- 2) maaliportti
            var finishes = Object.FindObjectsOfType<FinishTrigger>(true);
            if (finishes.Length == 0) Prob("FinishTrigger puuttuu.");
            foreach (var f in finishes)
            {
                // portin visuaali: lahin vanhempi jolla on rendereita (Finish-juuri), muuten itse objekti
                var gateRoot = f.transform;
                while (gateRoot.parent != null && gateRoot.GetComponentsInChildren<Renderer>(false).Length == 0) gateRoot = gateRoot.parent;
                var b = BoundsOf(gateRoot.gameObject);
                Info($"Maaliportti '{gateRoot.name}': leveys {b.size.x:F1}, korkeus {b.size.y:F1}, syvyys {b.size.z:F1} (rajat {MAX_GATE_W}x{MAX_GATE_H}).");
                float k = Mathf.Min(1f, MAX_GATE_W / Mathf.Max(0.01f, b.size.x), MAX_GATE_H / Mathf.Max(0.01f, b.size.y));
                if (k < 0.999f)
                {
                    Prob($"Maaliportti '{gateRoot.name}' on liian suuri (kerroin {k:F2}): kamera jaa portin sisaan.");
                    if (fix)
                    {
                        Undo.RecordObject(gateRoot, "Maaliportin koko");
                        gateRoot.localScale *= k;
                        Info($"  -> skaalattu x{k:F2}. Tarkista nakyma ja etta tie mahtuu portin alle (>= {ROAD_HALF * 2:F0} yks.).");
                    }
                }
                var col = f.GetComponent<Collider>();
                if (col != null && col.bounds.size.x < ROAD_HALF * 2f - 0.1f)
                    Prob($"Maalin laukaisin on kapeampi ({col.bounds.size.x:F1}) kuin tie ({ROAD_HALF * 2:F0}): auto voi ohittaa maalin sivusta.");
            }

            // --- 3) koristeiden koot: suurimmat renderit (ei maastoa)
            var big = new List<(string name, Vector3 size)>();
            foreach (var m in Object.FindObjectsOfType<PakettiporinaDecorMarker>(true))
            {
                var b = BoundsOf(m.gameObject);
                if (b.size.y > 25f || Mathf.Max(b.size.x, b.size.z) > 30f) big.Add((m.name, b.size));
            }
            foreach (var e in big.OrderByDescending(x => x.size.y).Take(8))
                Prob($"Iso koriste: {e.name} ({e.size.x:F0} x {e.size.y:F0} x {e.size.z:F0}) - tarkista skaala (Decor-presetti) ettei peita rataa tai kameraa.");
            if (big.Count == 0) Info("Koristeet: ei yli 25 m korkeita tai 30 m leveita.");

            s.AppendLine(problems == 0 ? "OK: ei ongelmia." : problems + " huomiota.");
            if (fix) { EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); s.AppendLine("Scene muutettu: tallenna (Ctrl+S) kun olet tarkistanut nakyman."); }
            try
            {
                string file = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/Rata_turva_" + sceneName + ".md"));
                File.WriteAllText(file, "# Radan turvatarkistus " + sceneName + " " + System.DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "\n\n```\n" + s + "```\n", new UTF8Encoding(true));
            }
            catch (System.Exception e) { Debug.LogWarning("[RataTurva] Raportin kirjoitus epäonnistui: " + e.Message); }
            Debug.Log(s.ToString());
        }
    }
}
#endif
