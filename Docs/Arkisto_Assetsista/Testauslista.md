# Pakettiporina — Pelattavuuden testauslista

*Luotu 16.8.2026. Käytä tätä listaa läpikäydessäsi peliä Unity Play-tilassa tai buildissa.
Merkitse ✅/❌ jokaisen kohdan perään.*

## 1. Päävalikko (MainMenu)
- [ ] Käynnistyy oikeaan näkymään, tausta+Vilkku-maskotti näkyvät
- [ ] "Halli"-nappi vie Garageen
- [ ] Teemapuiston kartta näkyy ja radat valittavissa sen kautta (jos käytössä)
- [ ] Asetukset-paneeli avautuu/sulkeutuu, ei jää päälle taustalle
- [ ] Äänenvoimakkuus-liu'ut (musiikki/tehosteet) vaikuttavat kuultavasti
- [ ] Värähtely päällä/pois -kytkin
- [ ] Ohjaustapa: Napit / Kallistus / Molemmat — kokeile kaikkia radalla
- [ ] Auton nopeustaso: Hidas / Keski / Nopea — **selvästi erilaiset** (ks. avoin bugi)
- [ ] Yötila-kytkin (huom: ei vielä visuaalista vaikutusta, tarkoituksella)
- [ ] "Nollaa edistyminen" kysyy vahvistuksen, tyhjentää pisteet/osat/tarrat/lika-kolhumittarit
- [ ] Tietosuoja-paneeli avautuu/sulkeutuu
- [ ] Ohjeet & palaute -paneeli avautuu/sulkeutuu

## 2. Halli (Garage)
- [ ] Kategoriavälilehdet (Kori/Maali/Renkaat/...) vaihtavat osajoukon
- [ ] Osan Edellinen/Seuraava-navigointi toimii, esikatselu päivittyy
- [ ] Lukitut osat näkyvät lukittuina, avautuvat pisteillä
- [ ] Paketin valinta (Edellinen/Seuraava) päivittää palkkion/vaatimuksen
- [ ] "Sopii autoon" -kuittaus (ääni+teksti) kun oikea osa valittuna
- [ ] Tilastopaneeli (nopeus/kiihtyvyys/pito) päivittyy osan mukaan
- [ ] Tarrat: osto onnistuu riittävillä pisteillä, ei-riittävillä ei
- [ ] "Aja keikka" vie oikealle radalle valitun paketin mukaan
- [ ] Auto säilyy samana kun palataan Hallista takaisin (SaveManager)

## 3. Radat (Game, Puisto, Satama, Huvipuisto, Maatila)
Jokaiselle radalle erikseen:
- [ ] Lähtölaskenta 3-2-1-AJA toimii, auto lukittu laskennan ajan
- [ ] Ohjaus toimii valitulla ohjaustavalla (napit/kallistus/molemmat)
- [ ] Tähdet kerääntyvät, HUD-lukema päivittyy
- [ ] Boost kiihdyttää havaittavasti
- [ ] Kartio/Laatikko: osuma leikkaa nopeutta, EI pysäytä peliä
- [ ] Latakko: osuma heikentää pitoa hetkeksi (huomaa luiston)
- [ ] Lika-/kolhumittarit (HUD-palkit) kasvavat HETI kun osuu esteeseen/lätäkköön (ks. avoin bugi)
- [ ] Auto ei putoa radalta ilman palautumista (fallY-turvaverkko)
- [ ] Maali: palkkiopaneeli näyttää oikean summan (paketti+tähdet)
- [ ] Maalin "Pesuun"-nappi: teksti/ikoni oikein, vie Pesu-sceneen
- [ ] Tauko (Pause) avautuu/sulkeutuu kesken ajon, aika pysähtyy
- [ ] Radan koristeet/objektit sijoittuvat järkevästi, eivät tuki tietä

## 4. Pesu (Wash)
- [ ] Likatahrat autossa vastaavat oikeasti kertynyttä lika-mittaria
- [ ] Kuplien poksautus poistaa likatahroja yksi kerrallaan, ääni soi
- [ ] Puhtausprosentti päivittyy oikein
- [ ] Kun kaikki lika poistettu, siirtyy automaattisesti korjausvaiheeseen
- [ ] Jos ei likaa eikä kolhuja, ohittaa molemmat suoraan Valmis-paneeliin

## 5. Korjaus (osa Pesu-sceneä)
- [ ] Kolhu-merkit autossa vastaavat kolhumittaria
- [ ] RepairTool-työkalut kelluvat, napautus korjaa yhden kolhun, vasarointi-animaatio+ääni
- [ ] "Korjattu: X %" -teksti näkyy ja päivittyy (ks. avoin bugi "ei näytä osumia")
- [ ] Kaikkien kolhujen jälkeen: efekti+ääni, siirtyy Valmis-paneeliin
- [ ] Valmis-paneelin "Jatkoon"-nappi vie Halliin
- [ ] "Uudestaan"-nappi ei anna ilmaisia lisäpisteitä jo tehdystä pesusta
- [ ] "Ohita"-nappi (jos käytössä) vie suoraan Halliin

## 6. Ääni
- [ ] Musiikki vaihtuu scenen mukaan (valikko/halli/rata/pesu), pehmeä häivytys
- [ ] Kaikki tehosteäänet kuuluvat: lähtölaskenta, AJA, tähti, maali, kupla, nappi,
      boost, läträkkö, kolhu, korjaus, sopii, avaus, moottori
- [ ] Toistuvat äänet (kolhu/roiske/kupla/korjaus) kuulostavat hieman vaihtelevilta, ei robottimaisilta
- [ ] Moottoriääni voimistuu/kiihtyy kaasutettaessa

## 7. Tallennus
- [ ] Pisteet/osat/tarrat säilyvät sovelluksen sulkemisen yli
- [ ] Lika-/kolhumittari säilyy sovelluksen sulkemisen yli
- [ ] Asetukset (äänet/värähtely/ohjaus/nopeus) säilyvät

## 8. Maatila-scenen sisältö (erikseen tarkistettava tämän kierroksen jälkeen)
- [ ] Maissipelto oikeassa kulmassa, ei tuki tietä
- [ ] Tuulimylly oikeinpäin
- [ ] Hevonen näyttää hyvältä läheltä katsottuna
- [ ] Eläinaitaukset (lampaat/kanat) siistissä, loogisessa asetelmassa
- [ ] Kaikki objektien värit/materiaalit näyttävät oikeilta (ei mustia/violetteja "puuttuva materiaali" -objekteja)

## 9. Yleinen
- [ ] Ei konsolivirheitä (punaisia) normaalin pelikierroksen aikana
- [ ] Kaikki napit reagoivat kosketukseen/klikkaukseen
- [ ] Ei paneeleita jotka jäävät vahingossa päälle scenen alussa
