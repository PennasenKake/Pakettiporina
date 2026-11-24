using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Pakettiporina.Tests
{
    // Lisatty 24.11.2026: tallennusdata (JSON) ja ajotahtien kirjanpito (GameManager) ilman scenea.
    public class SaveAndRunStarsTests
    {
        GameObject go; GameManager gm;

        [SetUp] public void SetUp() { go = new GameObject("TestGM"); gm = go.AddComponent<GameManager>(); }
        [TearDown] public void TearDown() { Object.DestroyImmediate(go); }

        [Test] public void SaveData_RoundTripKeepsProgress()
        {
            var a = new SaveData { points = 123, runStarsTotal = 17, selectedTrack = "Puisto" };
            a.trackBestNames.Add("Satama"); a.trackBestStars.Add(3);
            var b = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(a));
            Assert.AreEqual(123, b.points); Assert.AreEqual(17, b.runStarsTotal); Assert.AreEqual("Puisto", b.selectedTrack);
            CollectionAssert.AreEqual(new[] { "Satama" }, b.trackBestNames); CollectionAssert.AreEqual(new[] { 3 }, b.trackBestStars);
        }

        [Test] public void SaveData_OldSaveWithoutNewFields_LoadsWithSafeDefaults()
        {
            var b = JsonUtility.FromJson<SaveData>("{\"saveVersion\":1,\"points\":450}");
            Assert.AreEqual(450, b.points); Assert.AreEqual(0, b.runStarsTotal);
            Assert.IsNotNull(b.trackBestNames); Assert.IsNotNull(b.selectedPartNames);
            Assert.AreEqual("", b.selectedTrack); Assert.IsTrue(b.vibrationOn); Assert.AreEqual(2, b.controlMode); Assert.AreEqual(1, b.carSpeedTier);
        }

        [Test] public void SaveData_CorruptJson_Throws_SoSaveManagerCanBackItUp()
        {
            Assert.Throws<System.ArgumentException>(() => JsonUtility.FromJson<SaveData>("{ei json"));
        }

        [Test] public void AwardRun_AddsStarsAndKeepsBestPerTrack()
        {
            gm.AwardRun("Satama", 2);
            Assert.AreEqual(2, gm.RunStars); Assert.AreEqual(2, gm.BestFor("Satama"));
            gm.AwardRun("Satama", 1);                      // huonompi ajo: tahdet kertyvat, paras ei laske
            Assert.AreEqual(3, gm.RunStars); Assert.AreEqual(2, gm.BestFor("Satama"));
            gm.AwardRun("Satama", 3);
            Assert.AreEqual(3, gm.BestFor("Satama"));
        }

        [Test] public void AwardRun_NegativeStars_DoNotReduceTotal() { gm.AwardRun("Satama", -5); Assert.AreEqual(0, gm.RunStars); }

        [Test] public void AwardRun_ReturnsTrackUnlockedByThisRun()
        {
            gm.ApplyLoadedProgress(Progress.TrackNeeded("Puisto") - 1, null, null, "");
            var unlocked = gm.AwardRun("Satama", 1);
            Assert.IsTrue(unlocked.Contains("Uusi rata: Puisto"), string.Join(",", unlocked));
        }

        [Test] public void ApplyLoadedProgress_ClampsAndHandlesMismatchedLists()
        {
            gm.ApplyLoadedProgress(-4, new List<string> { "Satama", "Puisto", "" }, new List<int> { 9, 2 }, null);
            Assert.AreEqual(0, gm.RunStars);
            Assert.AreEqual(3, gm.BestFor("Satama"));      // 9 -> 3
            Assert.AreEqual(2, gm.BestFor("Puisto"));
            Assert.AreEqual("", gm.SelectedTrack);
        }

        [Test] public void BestFor_UnknownScene_IsZero() { Assert.AreEqual(0, gm.BestFor("Tuntematon")); }
    }
}
