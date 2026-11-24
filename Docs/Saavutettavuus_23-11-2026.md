# Saavutettavuustarkistus 23.11.2026 (ajon HUD ja maali)

Perusta: WCAG 2.1 AA (teksti 4,5:1, iso teksti 3:1, kosketuskohde vähintään 48 dp). Laskettu pahimmalle taustalle (valkoinen taivas / valkoinen ruutulippuruutu). Ei vielä mitattu laitteella.

## Kontrastit

| Elementti | Ennen | Nyt | Tulos |
|-----------|-------|-----|-------|
| Tähdet / aika (valkoinen teksti, ohut reunus) valkoista taivasta vasten | noin 1,1:1 | tumma laatta (alpha 0,66) tekstin takana: noin 6,4:1 | OK (RaceHUD.AddPlate) |
| Lika/Kolhut-selitteet | valkoinen, 5 px, ei taustaa | valkoinen tummalla kontilla (alpha 0,72): yli 8:1, fontti suurempi | OK |
| "Hienoa, perillä!" (keltainen) valkoisella ruudulla, himmennys 0,45 | noin 2,5:1 | himmennys 0,6: noin 4,2:1 (iso teksti, vaatimus 3:1) | OK |
| Palkkio-teksti (valkoinen) maalipaneelissa | noin 4,5:1 | noin 5,7:1 | OK |
| Lähtölaskenta (keltainen, reunus) | ei taustaa | ei laattaa; iso (yli 80 px) teksti | Tarkista laitteella |

## Kosketuskohteet (Galaxy S10e -simulaatio, noin 1,44 dp/px)

| Elementti | Koko | Arvio |
|-----------|------|-------|
| Pause (keltainen soikio) | noin 72 x 36 dp | Korkeus alle 48 dp: suurenna |
| Ohjausnapit (vasen/oikea/jarru) | noin 58 dp | OK |
| Maali-paneelin napit | noin 62 dp | OK |

## Muut havainnot
- Väri ei ole ainoa tieto: mittareilla on nyt sana (Lika/Kolhut) ja ikoni, ei vain väri (WCAG 1.4.1). Palkin väri (keltainen -> punainen) on lisätieto.
- Jokaisella HUD-tekstillä on tumma reunus; ilman laattaa reunus yksin ei riitä vaaleaa taivasta vasten.
- Liike: konfetti 4 s, ei välkkyvää (alle 3 vilahdusta/s). Lisää asetus "vähennä liikettä" harkittavaksi.
- Värisokeus: Lika (ruskea) ja Kolhut (keltainen-punainen) erottuvat sanalla ja ikonilla, ei pelkällä värillä.

## Avoin
- [ ] Pause-napin korkeus vähintään 48 dp.
- [ ] Lähtölaskennan taustalaatta tarvittaessa.
- [ ] Hallin, Pesun ja valikon kontrastit (ei vielä laskettu).
- [ ] Mittaa oikeilla pikselikuvilla (kuvakaappauksista) ja laitteella ulkovalossa.
