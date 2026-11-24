# Opetussarjan aihepaketti: lue ensin

Tämä kansio on siirtopaketti uuteen keskusteluun, jossa luodaan opetussarjan ("How to code your own game") materiaalit Pakettipörinän pohjalta. Se sisältää kaiken, mitä uusi keskustelu tarvitsee.

## Tiedostot
| Tiedosto | Sisältö |
|---|---|
| 01_projektin_tiivistelma.md | mikä peli on, tekniikka, koko, tila |
| 02_paatokset_ja_avoimet.md | tehdyt päätökset, blokkerit, avoimet kysymykset, riskit |
| 03_osaluettelo_v2.md | 21 osan suunnitelma (tavoite, sisältö, tehtävä, vastine) |
| 04_koodikartta.md | kaikki skriptit ja rivimäärät (generoitu) |
| 05_lisenssi_inventaario.md | FBX-, ääni-, kuva- ja paketti-inventaario täytettäväksi |
| 06_kirjoitusohje_ja_osamalli.md | sävy, osamalli, laatusäännöt |
| 07_tyojarjestys.md | vaiheet 0-3 ja osa -> tiedosto -kartta |
| 08_vertailu.md | kilpailijavertailu |

## Aloituskehote uuteen keskusteluun (kopioi sellaisenaan)
> Työskentelemme Pakettipörinä-opetussarjan materiaalien parissa. Lue ensin kansion Docs/Opetussarja tiedostot 00-08 (aloita 00_LUE_ENSIN.md). Tavoite tässä keskustelussa: [valitse yksi: (a) lisenssi-inventaarion täyttö, (b) repo-jaon suunnittelu, (c) osan 1 materiaalit, (d) osien 1-3 materiaalit]. Käytä osamallia (06). Kysy ensin vain asiat, jotka estävät työn. Älä muuta pelin koodia ellen pyydä.

## Muistettavat rajoitteet
- Ei git push --force. Älä committaa LFS-binäärejä sandboxista. Älä lähetä käyttäjän sähköpostia ulkopuolelle.
- Opetuskoodi kirjoitetaan erikseen yksinkertaistettuna; projektin koodi on vain lähde.
- Älä myy tai lupaa mallien myyntiä ennen kuin lisenssit (05) on täytetty.
- Käyttäjä (Muumi) kirjoittaa suomeksi ja lyhyesti; vastaa tiiviisti.

## Tämän pakettiin johtanut keskustelu (tiivistetysti)
Peli sai 3-vaiheisen Hallin, suorituskykytestit ja -työkalut, vapaan radan valinnan, 1-3 tähden arvostelun ja porrastetut lukitukset. Sitten suunniteltiin opetussarja, vertailtiin kilpailijoihin (Unity Learn, Catlike Coding, Kodeco, Udemy, GameDev.tv) ja arvioitiin GitHub-repo: neljä estettä (julkinen repo sisältää myytävän sisällön, lisenssit dokumentoimatta, GitHub jäljessä, koodi liian pitkää opetukseen).

---
## Päivitys 24.11.2026 (illan lopputilanne)
- Kylä-rata siirretty selvästi myöhemmäksi (oma ratamalli + paljon FBX-elementtejä); `Progress.PlannedTracks`. Älä käsittele sitä sarjan sisältönä nyt.
- Ratalista on keskitetty osittain (`Progress.RaceScenes`); uusi rata vaatii yhä Progress-, PakettiporinaTracks- ja Decor/Obstacles-rivit.
- Valmistuneet: ARM64, HDR-tarkistus kaikista laatutasoista, Tarkistus/08 laajennus, Docs siirretty pois Assetsista (`Docs/Arkisto_Assetsista`), Lika/Kolhut-palkit piiloon maalissa, tallennuksen `.rikki.bak`.
- Uudet kuvat `Docs/Esittely` (paavalikko, halli_1_paketti, halli_2_rata, halli_3_auto, ajo, maali, wash) – käyttökelpoisia artikkelien kuvituksena.
- Tunnetut heikkoudet ja avoimet: README.md (kohta "Tunnetut heikkoudet") ja TODO_Kiillotus_ja_UX.md osio 12. Sarjan estäjät ennallaan: lisenssi-inventaario, repon jako (myytävä vs. avoin), testit, sanasto.
- Dokumentit 01–09 on täytetty ja päivitetty; vertailu on nyt `12_ARTIKKELISARJA_vertailu.md`.

- 24.11.2026 myöhemmin: lisätty 29 EditMode-testiä (`Tests/EditMode`, Tarkistus/10), UI-tiedostot pilkottu partial-luokiksi, ratakaiteet + CarRecovery-parannus, Pause 48 dp. Testit ja partial-jako ovat hyvää opetussarja-materiaalia (osa: testaus ja koodin rakenne).
