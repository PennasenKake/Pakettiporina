# Julkaisu

Päivitetty 24.11.2026. Tila: **ei vielä julkaisuvalmis** (laitetesti, lisenssit, vanhempainportti ja allekirjoitus puuttuvat).

## Julkaisua edeltävä tarkistuslista
- [x] Tarkistus/01–08 ajettu ilman ongelmia (viimeksi 24.11.2026)
- [x] Android: IL2CPP, vain ARM64, minSdk 31
- [x] Muistivuotoa ei havaittu editorimittauksessa; Maatila raskain (257 piirtokutsua ka)
- [x] Offline, ei mainoksia, ei datankeruuta, ei ostoksia
- [ ] **Laitetesti oikealla puhelimella** (FPS, lämpö, akku 10 min, kosketusalueet, tekstien koko)
- [ ] Debug-lokit ja Development Build -overlay pois tuotantobuildista
- [ ] HDR pois URP-Balancedista tai todettu laitteella ok
- [ ] Tallennuksen siirtymälogiikka (`saveVersion`) ennen ensimmäistä julkaisua, jotta päivitykset eivät riko tallennuksia
- [ ] Pelitestaus lapsilla (tähti- ja lukitusrytmi)

## Kauppasivun materiaalit
Kuvakaappaukset: `Docs/Esittely` (päävalikko, halli 3 vaihetta, ajo, maali, pesu; Simulator Galaxy S10e, 24.11.2026). Puuttuu: ikoni, kuvakaappaukset oikealta laitteelta, lyhyt ja pitkä kuvaus, mahdollinen video. Vanhemmalle: ei mainoksia, ei ostoksia, ei datankeruuta, toimii offline.

## Ikäluokitus
Kohderyhmä 6–10 v. Sisältö: ei väkivaltaa, ei tekstichattia. Haetaan sopiva luokitus (PEGI 3 / ESRB Everyone) kun sisältö on vakaa.

## Lapsiturvallisuus ja yksityisyys
Offline, ei mainoksia, ei analytiikkaa. Aikuisille-osio (tietoja pelistä, tietosuoja, edistymisen nollaus kahdella painalluksella). Puuttuu: varsinainen vanhempainportti, tietosuojaseloste verkossa, Google Play Families -ilmoitukset. Tarkista voimassa olevat käytännöt (GDPR/COPPA).

## Lokalisointi
Tekstit ovat koodissa suomeksi (~300 merkkijonoa). Englanti vaatisi tekstien siirron avaintauluun; ei vielä aloitettu.

## Versiointi
`bundleVersion` 0.1.0. Muutosloki: [07_DEVLOG.md](07_DEVLOG.md).

## Build ja allekirjoitus
Keystore luomatta. AAB-paketointi ja lataus Play Consoleen tekemättä. Ks. [01_SETUP_JA_BUILD.md](01_SETUP_JA_BUILD.md).

## Julkaisun jälkeen
Palautteen seuranta, kaatumisraportit (ei analytiikkaa → Play Console), päivityssuunnitelma: Kylä-rata ja uudet paketit.
