# Virstanpylväät ja niiden sisältö (M-viittaukset)

Tämä dokumentti selittää koodin kommenteissa ja tarkistuslistassa käytetyt
M-viittaukset (M1, M2, M3…). Jokaisella alivaiheella on tila, sisältö ja
keskeiset tiedostot. Juuri kaikille poluille: `Assets/_Pakettiporina/`

**Tilat:** ✅ valmis · 🔨 kesken · ⬜ suunniteltu

---

## M1 — Ajettava auto ✅
Tavoite: auto liikkuu maalla, kamera seuraa. Hauskuuden perusta.

- **M1.1 Projekti & asetukset** ✅ — Unity 2022.3 (3D URP), Active Input Handling = Both, build-perusteet (IL2CPP/ARM64).
- **M1.2 Hakemistorakenne & versionhallinta** ✅ — `_Pakettiporina`-kansiot, `.gitignore`, `Pakettiporina.asmdef`, Git-repo.
- **M1.3 Syöte** ✅ — `Scripts/Vehicle/CarInput.cs` (aluksi näppäimistö; myöhemmin + kosketus/kallistus).
- **M1.4 Autofysiikka** ✅ — `Scripts/Vehicle/ArcadeCarController.cs` (arcade: kiihdytys, kääntö, pito, huippunopeus).
- **M1.5 Kamera** ✅ — `Scripts/Core/CameraFollow.cs` (pehmeä seuraava kamera).
- **M1.6 Maa & auto-prefab** ✅ — Ground (Plane + vihreä materiaali), Car (Rigidbody + Body-collider) → `Prefabs/Car`.

## M2 — Rata + maali (yksi pelattava keikka) ✅
Tavoite: keikka lähdöstä maaliin — tähdet, maali, restart, valikko, tauko.

- **M2.1 GameManager** ✅ — `Scripts/Core/GameManager.cs` (singleton: pelin vaihe + pisteet, DontDestroyOnLoad).
- **M2.2 GameEvents** ✅ — `Scripts/Core/GameEvents.cs` (tapahtumaväylä: OnRaceStart / OnFinish / OnStarCollected).
- **M2.3 RaceManager** ✅ — `Scripts/Gameplay/RaceManager.cs` (keikan kulku, kuluva aika, tähtilaskenta, putoamispalautus Fall Y).
- **M2.4 FinishTrigger** ✅ — `Scripts/Gameplay/FinishTrigger.cs` (maaliportti, laukaisee maalin kerran).
- **M2.5 Pickup** ✅ — `Scripts/Gameplay/Pickup.cs` (kerättävä tähti) → `Prefabs/Pickups`.
- **M2.6 HUD & Canvas** ✅ — `Scripts/UI/RaceHUD.cs` (tähtimäärä, aika, FinishPanel, restart), Canvas + Canvas Scaler + EventSystem.
- **M2.7 StartPoint & minimirata** ✅ — StartPoint (lähtö + suunta), Finish-portti, tähdet sceneen.
- **M2.8 Aloitusnäkymä** ✅ — `Scripts/UI/MainMenu.cs`, MainMenu-scene, taustakuva (`Art/UI`), Pelaa/Lopeta, Build Settings -scenejärjestys.
- **M2.9 Kosketusohjaus** ✅ — `Scripts/UI/TouchButton.cs` (pidä-pohjassa-napit) + CarInput-päivitys (näppäimistö + kaasu/vasen/oikea yhtä aikaa).
- **M2.10 Viimeistely** ✅ — ajonapit piiloon maalissa, päävalikko-napit maali- ja taukopaneelissa, `Scripts/UI/PauseMenu.cs` (tauko: Tauko-nappi / Esc / Android-takaisin), putoamispalautuksen varmistus.

## M3 — Rakentelu + paketit 🔨
Tavoite: opettava ydin — osat, ominaisuusmittarit, paketin ja auton yhteensovitus.

- **M3.1 Data** ✅ — `Scripts/Data/PartData.cs`, `Scripts/Data/PackageData.cs` + esimerkkiassetit (`Data/Parts`, `Data/Packages`).
- **M3.2 Laskenta** ✅ — `Scripts/Data/CarStats.cs` (5 mittaria), `Scripts/Builder/CarBuilder.cs` (valitut osat → summattu CarStats).
- **M3.3 Sopivuus** ✅ — `Scripts/Builder/FitChecker.cs` + `CarBuilder.HasPart` ("sopii keikkaan / tarvitsee: …", ei-estävä).
- **M3.4 Halli-UI** 🔨 — `Scripts/UI/GarageScreen.cs`: osakategoriat, osavalinta, mittaripalkit, sopivuusmerkki, pakettilista, "Aja keikka". (harmaalaatikko, ilme myöhemmin)
- **M3.5 Kytkentä ajoon** ⬜ — valittu auto + paketti kulkevat GameManagerin kautta keikalle; CarStats vaikuttaa ajoon (voima→kiihtyvyys, pito→pito); paketin massa autoon; palkkio pisteisiin.

## M4 — Huoltoasema ⬜
Tavoite: maalin jälkeen korjaus + pesu saippuakuplilla.

- **M4.1 CarConditionTracker** ⬜ — kerää lian ja kolhut radalla (esteet/lätäköt) → `Scripts/Vehicle`.
- **M4.2 CarCareController** ⬜ — vaiheet korjaus → pesu → valmis → `Scripts/CarCare`.
- **M4.3 RepairStep** ⬜ — kolhun korjaus napauttamalla.
- **M4.4 WashStep + SoapBubble + CleanlinessMeter** ⬜ — pesu kuplia poksauttamalla, puhtausmittari.
- **M4.5 ScoreManager** ⬜ — pisteet = paketti + tähdet + kuplat + siisteys → `Scripts/Gameplay`.
- **M4.6 CarCareScreen** ⬜ — huoltoaseman UI (vaiheet + napit).

## M5 — Silmukka + meta ⬜
Tavoite: tallennus, eteneminen, tarrat, näkymäkierto, lapsiturva.

- **M5.1 SaveManager + SaveData** ⬜ — tallennus JSONiin (pisteet, avaukset, autot, asetukset).
- **M5.2 SettingsManager** ⬜ — ohjaustapa, kieli, musiikki/SFX.
- **M5.3 AudioManager** ⬜ — musiikki ja äänitehosteet.
- **M5.4 SavedCarsManager** ⬜ — talli / muistitoiminto (tallenna & lataa suosikkiauto).
- **M5.5 ProgressionManager + UnlockManager** ⬜ — arvonimet, pisterajat, avautuva sisältö.
- **M5.6 AchievementTracker** ⬜ — tarrojen avausehdot.
- **M5.7 StickerData + DistrictData** ⬜ — tarrat ja kaupunginosat datana.
- **M5.8 Näkymät & ScreenManager** ⬜ — HomeScreen, MapScreen, StickerBookScreen, BottomNav, näkymänvaihto.
- **M5.9 ParentalGate + Boot-scene** ⬜ — vanhempainportti, käynnistysscene.

## M6 — Kiillotus ⬜
Tavoite: ulkoasu, ääni, mehukkuus, lokalisointi, suorituskyky.

- **M6.1 Ulkoasu** ⬜ — mockupin paletti, fontit, pyöristetyt napit, Vilkku UI:hin.
- **M6.2 3D & valaistus** ⬜ — Kenneyn low-poly-mallit, materiaalit, URP-jälkikäsittely (bloom, tonemapping).
- **M6.3 Ääni** ⬜ — musiikki, SFX, Vilkun puhe (TTS/nauhoitus), haptiikka.
- **M6.4 Mehukkuus** ⬜ — partikkelit (konfetti, kuplat), animaatiot (DOTween, squash/stretch).
- **M6.5 Lokalisointi** ⬜ — käännösavaimet fi/en kovakoodattujen tekstien tilalle.
- **M6.6 Suorituskyky & laitetestaus** ⬜ — object pooling, static batching, targetFrameRate, testaus puhelimella.

---

*Koodin kommenttien `[M1]`, `[M2]`, `[M3]` viittaavat päävirstanpylvääseen; tämä
dokumentti purkaa ne alivaiheiksi. Päivitä tilat (✅/🔨/⬜) edetessäsi.*
