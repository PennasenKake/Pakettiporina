# Arkkitehtuuri

Päivitetty 24.11.2026. Koodin ja järjestelmien yleiskuva: miten osat liittyvät toisiinsa.

## Yleiskuva (kerrokset)

UI (halli, HUD, valikot) → pelilogiikka (keikka, maali, pesu) → auto ja fysiikka (ohjaus, Rigidbody) → data (ScriptableObjectit) → Unity (URP, Android). Kaikki koodi on yhdessä assemblyssa (`Scripts/Pakettiporina.asmdef`). Suurin osa käyttöliittymän lisäyksistä rakennetaan **ajonaikaisesti koodilla**, jotta scene-tiedostoihin ei tarvitse koskea (ja jotta muutokset ovat versionhallinnassa luettavia).

## Hakemistokartta

| Kansio | Sisältö |
|---|---|
| `Scripts/Core` | GameManager, SaveManager/SaveData, AudioManager, Progress, PerformanceManager, PerfBenchmark, SettingsManager, SceneFader, BackButtonHandler, DarkModeOverlay, CutoutAvoider (kameran reikä), ResetConfirmManager, CarColor, **Juice** (tehosteet), **SoundSynth** (syntetisoidut äänet) |
| `Scripts/Vehicle` | ArcadeCarController, CarInput, CarRecovery, CarPainter, CarEngineAudio |
| `Scripts/Builder` | CarBuilder (osista auto), FitChecker (sopiiko auto pakettiin) |
| `Scripts/Gameplay` | RaceManager, FinishTrigger, TrackManager, esteet ja tehosteet (Puddle, Cone, Laatikko, Boost, Pickup), WashScreen, RepairTool, Bubble |
| `Scripts/UI` | **GarageScreen** (halli-logiikka) ja **GarageWizard** (kolmivaiheinen halli), RaceHUD, PauseMenu, MainMenu, StickerPanel, VilkkuController, MascotAnimator |
| `Scripts/Data` | PartData, PackageData, StickerData, TrackData, CarStats |
| `Scripts/Editor` | Tarkistus, Korjaus, Lisays, Tyokalut, Common (jaetut apurit), EsittelyKaappaus |
| `Tests/EditMode` | EditMode-testit (Progress, tallennus, ajotähdet) |
| `Resources/WizardUI` | Hallin kolmivaiheen kuvat (kopioita `Art/UI`-kuvista; synkkaa Korjaus/10) |

## Keskeiset järjestelmät

| Järjestelmä | Vastuu | Sijainti |
|---|---|---|
| GameManager | keskustila: pisteet, valittu paketti, osat ja auto | Core |
| SaveManager / SaveData | JSON-tallennus laitteelle | Core |
| RaceManager | keikan kulku (lähtölaskenta, aika, maali) | Gameplay |
| CarBuilder + FitChecker | auton osat → CarStats; sopiiko paketille | Builder |
| GarageScreen | osien/pakettien selaus, lukitus (tähdillä), tallennus, "Aja" | UI |
| GarageWizard | Paketti → Rata → Auto -näkymät GarageScreenin päälle (alla) | UI |
| Ratalista | `Progress.RaceScenes` (ajoscenet; AudioManager ja PerformanceManager lukevat) ja `Progress.PlannedTracks` (Kylä: myöhemmin, pois Build Settingsista). Uusi rata: myös `tracks`/`trackNames` ja `PakettiporinaTracks.ALL`; Tarkistus/08 valvoo | Core |
| Progress | Ajotähtien (1-3/ajo) avaustaulukko: värit, osat, paketit, radat; GameManager.RunStars/BestStars tallentuvat SaveDataan | Core |
| RaceHUD | tähdet, aika, Lika- ja Kolhut-mittarit, maalipaneeli | UI |
| Juice | pomput, hiukkaset, konfetti, välähdys, leijuvat tekstit, `Juice.Play(avain)` | Core |
| SoundSynth | äänet koodilla (`confetti`, `warning`, `whoosh`, `arm`, `pluck`) | Core |

### GarageWizard

Lisää itsensä Garage-sceneen (`GarageScreen.useWizard`, oletuksena päällä). Rakentaa täyden ruudun juuren Canvasin päälle, siirtää auton esikatselukuvan (`GarageScreen.carPreview`) uuteen kehykseen ja piilottaa vanhan käyttöliittymän CanvasGroupilla (alpha 0, ei kosketuksia; olio pysyy aktiivisena, jotta GarageScreen toimii). Kaikki logiikka kulkee GarageScreenin julkisen API:n kautta (`PartsIn`, `SelectPartIndex`, `NextPackage`, `CurrentFit`, `OnDrive` jne.). **Rata ei ole erillinen valinta**: se tulee paketin `trackScene`-kentästä, ja vaihe 2 on esikatselu (kuva `Resources/WizardUI/rata_<scene>.png`, oletus `rata_perus`).

Sudenkuoppia: ensimmäisen aktivoinnin `Start`; `??` ei toimi Unity-olioille (käytä `== null`); älä piilota CanvasGroupilla ennen kuin siirrettävät lapset on siirretty pois; `Time.timeScale = 0`:ssa käytä `unscaledDeltaTime`.

### Tiedostojako (partial-luokat, 24.11.2026)
Suuret UI-luokat on jaettu `partial`-tiedostoihin sisältöä muuttamatta: `Juice.cs` + `Juice.UIEffects.cs`, `Juice.HudSprites.cs`, `Juice.Feedback.cs`; `GarageWizard.cs` + `.Helpers.cs`, `.Build.cs`; `RaceHUD.cs` + `RaceHUD.Meters.cs`; `WashScreen.cs` + `WashScreen.Repair.cs`. Uusi koodi lisätään sopivaan osaan; älä tee uutta yli 500 rivin tiedostoa.

### Putoamissuoja
`CarRecovery` (auton komponentti) tallentaa turvallisia paikkoja 0,5 s välein (6 s historia, maa-tarkistus raycastilla) ja palauttaa putoamisen jälkeen vähintään 1,5 s taaksepäin; toistuva putoaminen siirtää yhä vanhempaan paikkaan. Radan sivuilla on näkymättömät kaiteet (`Ratakaiteet`, Korjaus/11).

## Tapahtumat (GameEvents)

`OnRaceStart`, `OnCountdown(int)`, `OnGo`, `OnStarCollected(int)`, `OnFinish` (+ `GameEvents.Finish()`, jota Editorin "Maalin efektit (testi)" kutsuu). RaceHUD ja ääni/efektit kuuntelevat näitä.

## Datavirta

Paketti (PackageData: `trackScene`, `requiredPart`, `rewardPoints`, `unlockPoints`) + valitut osat (PartData → CarStats) → FitChecker → "Aja" tallentaa valinnan GameManageriin → rata (scene) → maali → pisteet → pesu (Wash) → halli.

## Tallennus

`SaveData` JSON laitteen tallennuskansioon (`SaveManager.Save()` kutsutaan mm. kun keikka lähtee). Sisältää pisteet, avaukset, auton osat, paketin, asetukset sekä Lika/Kolhut-mittarit. Nollaus vaatii kaksi painallusta (`ResetConfirmManager`).

Rikkinäinen tallennustiedosto kopioidaan `.rikki.bak`-tiedostoksi ennen kuin uusi tallennus ylikirjoittaa sen. `saveVersion` on mukana mutta siirtymälogiikka puuttuu.

## Riippuvuudet ja kokoonpano

Unity 2022.3.23f1, URP, TextMeshPro. Ei kolmannen osapuolen paketteja ajossa. Äänet ja osa kuvista syntetisoidaan ajonaikaisesti (ei binäärejä).

## Laajennuspisteet

- Uusi näkymä: rakenna UI koodilla samaan tapaan kuin `GarageWizard` tai `Juice`-apureilla.
- Uusi rata: uusi scene + `PackageData.trackScene` + ratakuva `rata_<scene>.png` (ks. 03_SISALLON_LISAYS).
- Uusi osa/paketti: datana (ks. 03).
- Uusi tehoste: `Juice.*`; uusi ääni: `SoundSynth.Get(avain)`.
