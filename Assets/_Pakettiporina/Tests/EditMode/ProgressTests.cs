using System.Linq;
using NUnit.Framework;

namespace Pakettiporina.Tests
{
    // Lisatty 24.11.2026: Progress.cs:n puhtaat saannot. Ajo: Window > General > Test Runner > EditMode.
    public class ProgressTests
    {
        // --- RateRun: 1 = perilla, 2 = paketti ehjana, 3 = lisaksi kaikki bonustahdet (tai nopea aika jos bonuksia ei ole)
        [Test] public void RateRun_FinishWithDents_IsOneStar() { Assert.AreEqual(1, Progress.RateRun(3, 5, 5, 20f)); }
        [Test] public void RateRun_NoDentsButMissingBonus_IsTwoStars() { Assert.AreEqual(2, Progress.RateRun(0, 3, 5, 20f)); }
        [Test] public void RateRun_NoDentsAllBonus_IsThreeStars() { Assert.AreEqual(3, Progress.RateRun(0, 5, 5, 99f)); }
        [Test] public void RateRun_DentsNeverGiveThree_EvenWithAllBonus() { Assert.AreEqual(1, Progress.RateRun(1, 5, 5, 10f)); }
        [Test] public void RateRun_NoBonusesOnTrack_FastTimeGivesThree() { Assert.AreEqual(3, Progress.RateRun(0, 0, 0, 60f)); }
        [Test] public void RateRun_NoBonusesOnTrack_SlowTimeGivesTwo() { Assert.AreEqual(2, Progress.RateRun(0, 0, 0, 61f)); }
        [Test] public void RateRun_AlwaysBetweenOneAndThree()
        {
            for (int d = 0; d < 4; d++)
                for (int b = 0; b < 6; b++)
                    foreach (float t in new[] { 0f, 30f, 120f })
                    {
                        int r = Progress.RateRun(d, b, 5, t);
                        Assert.GreaterOrEqual(r, 1); Assert.LessOrEqual(r, 3);
                    }
        }

        // --- Radat
        [Test] public void TrackKey_StripsScandinavianLetters() { Assert.AreEqual("kyla", Progress.TrackKey("Kylä")); }
        [Test] public void TrackKey_Empty_IsEmpty() { Assert.AreEqual("", Progress.TrackKey(null)); }
        [Test] public void Satama_IsOpenFromStart() { Assert.AreEqual(0, Progress.TrackNeeded("Satama")); Assert.IsFalse(Progress.TrackLocked("Satama")); }
        [Test] public void UnknownTrack_IsOpen() { Assert.AreEqual(0, Progress.TrackNeeded("EiOleOlemassa")); }
        [Test] public void Puisto_IsLockedWithoutStars() { Assert.IsTrue(Progress.TrackLocked("Puisto")); Assert.AreEqual(Progress.TrackNeeded("Puisto"), Progress.TrackLeft("Puisto")); }
        [Test] public void TrackThresholds_AreIncreasingInDifficultyOrder()
        {
            Assert.Less(Progress.TrackNeeded("Satama"), Progress.TrackNeeded("Puisto"));
            Assert.Less(Progress.TrackNeeded("Puisto"), Progress.TrackNeeded("Huvipuisto"));
            Assert.Less(Progress.TrackNeeded("Huvipuisto"), Progress.TrackNeeded("Maatila"));
            Assert.Less(Progress.TrackNeeded("Maatila"), Progress.TrackNeeded("Kyla"));
        }

        // --- Avautumiset
        [Test] public void NewlyUnlocked_NoGrowth_IsEmpty() { Assert.AreEqual(0, Progress.NewlyUnlocked(5, 5).Count); Assert.AreEqual(0, Progress.NewlyUnlocked(7, 3).Count); }
        [Test] public void NewlyUnlocked_ReportsTrackWhenCrossingThreshold()
        {
            var list = Progress.NewlyUnlocked(Progress.TrackNeeded("Puisto") - 1, Progress.TrackNeeded("Puisto"));
            Assert.IsTrue(list.Contains("Uusi rata: Puisto"), string.Join(",", list));
        }
        [Test] public void NewlyUnlocked_DoesNotRepeatAlreadyPassedThreshold()
        {
            var list = Progress.NewlyUnlocked(Progress.TrackNeeded("Puisto"), Progress.TrackNeeded("Puisto") + 1);
            Assert.IsFalse(list.Contains("Uusi rata: Puisto"));
        }

        // --- Taulukkojen terveys (sama logiikka kuin Tarkistus/08, mutta ilman assetteja)
        [Test] public void Tables_HaveNoNegativeOrNegativeKeys()
        {
            foreach (var kv in Progress.PartTable.Concat(Progress.PackageTable).Concat(Progress.TrackTable))
            { Assert.GreaterOrEqual(kv.Value, 0, kv.Key); Assert.IsFalse(string.IsNullOrEmpty(kv.Key)); }
        }
        [Test] public void GoldenParts_AreStarGatedAndLate()
        {
            foreach (var kv in Progress.PartTable.Where(k => k.Key.StartsWith("Kulta")))
                Assert.GreaterOrEqual(kv.Value, 40, kv.Key);
        }
        [Test] public void RaceScenes_ContainEveryTrackInTable()
        {
            foreach (var kv in Progress.TrackTable)
                Assert.IsTrue(Progress.RaceScenes.Any(s => Progress.TrackKey(s) == kv.Key), kv.Key);
        }
        [Test] public void PlannedTracks_AreKnownTracks()
        {
            foreach (var k in Progress.PlannedTracks) Assert.IsTrue(Progress.TrackTable.Any(t => t.Key == k), k);
        }
        [Test] public void NextReward_ExistsAtStart_AndIsInTheFuture()
        {
            int need; string text;
            Assert.IsTrue(Progress.NextReward(out need, out text));
            Assert.Greater(need, Progress.RunStars); Assert.IsNotEmpty(text);
        }
    }
}
