using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Pakettiporina.Tests
{
    // Lisatty 24.11.2026: ajaa EditMode-testit valikosta ja kirjoittaa raportin Docs/Testit_viimeisin.md
    // (sama raportointitapa kuin muissa tyokaluissa; konsoli ei aina ole luettavissa).
    // Vaihtoehto: Window > General > Test Runner > EditMode > Run All.
    public static class TestRunnerMenu
    {
        [MenuItem("Pakettiporina/Tarkistus/10 - Aja automaattiset testit (EditMode)")]
        public static void RunAll()
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new Report(api));
            api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode }));
            Debug.Log("[Testit] Ajetaan EditMode-testit...");
        }

        class Report : ICallbacks
        {
            readonly TestRunnerApi api;
            readonly List<string> failures = new List<string>();
            int passed, failed, skipped;
            public Report(TestRunnerApi a) { api = a; }

            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }

            public void TestFinished(ITestResultAdaptor r)
            {
                if (r.HasChildren) return;
                switch (r.TestStatus)
                {
                    case TestStatus.Passed: passed++; break;
                    case TestStatus.Skipped: case TestStatus.Inconclusive: skipped++; break;
                    default: failed++; failures.Add("- " + r.Test.FullName + ": " + (r.Message ?? "").Replace("\n", " ")); break;
                }
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                var sb = new StringBuilder();
                sb.AppendLine("# Automaattiset testit (EditMode) " + System.DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                sb.AppendLine();
                sb.AppendLine("Läpi: " + passed + ", epäonnistui: " + failed + ", ohitettu: " + skipped);
                sb.AppendLine();
                if (failures.Count == 0) sb.AppendLine("Kaikki testit läpäisivät.");
                else { sb.AppendLine("## Epäonnistuneet"); foreach (var f in failures) sb.AppendLine(f); }
                try
                {
                    File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/Testit_viimeisin.md")), sb.ToString(), new UTF8Encoding(true));
                }
                catch (System.Exception e) { Debug.LogWarning("[Testit] Raportin kirjoitus epäonnistui: " + e.Message); }
                if (failed == 0) Debug.Log("[Testit] OK: " + passed + " läpi."); else Debug.LogWarning("[Testit] " + failed + " epäonnistui.");
                Object.DestroyImmediate(api);
            }
        }
    }
}
