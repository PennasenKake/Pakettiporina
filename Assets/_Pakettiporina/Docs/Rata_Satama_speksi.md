# Satama v2 — ratadata

Sataman rata ja koko elementtiasettelu suunniteltiin täysin uudelleen. Luvut
tässä täsmäävät suoraan `PakettiporinaElements.cs`/`PakettiporinaStars.cs`/
`PakettiporinaDecor.cs`/`PakettiporinaSetup.cs`:ään ja Blenderissä rakennettuun
`Satama.fbx`-maastoon — eivät ole arvioita.

## Maaston kokonaismitat

| Ulottuvuus | Arvo | Rajat |
|---|---|---|
| Leveys (X) | 71.5 | −35.75 … +35.75 |
| Pituus (Z) | 340 | −170 … +170 |
| Korkeus (Y) | 0 (tasainen) | ajopinta tasolla 0 |

Origo on maaston keskellä ja ajopinnan tasolla — maasto asetetaan Unityssä kohtaan (0, 0, 0).

## Tie

- Leveys: 12 (ajokaista), lisäksi molemmin puolin 1.8 leveä piennar
- Koko tie siirretty +15 sivuun (`roadBaseX`) — ks. alla miksi
- Tien alku: Z = −150, loppu: Z = +150 (StartPoint/Finish)
- Muoto: **EPÄSYMMETRINEN kaari** — ei enää yksi symmetrinen kupera mutka.
  Ensin iso loiva mutka OIKEALLE (Z −100…0, huippu Z=−50), sitten loivempi
  mutka VASEMMALLE (Z 0…100, pohja Z=50). Rantaviiva vaihtelee siis
  luonteeltaan kahdessa puoliskossa: leveämpi "lahti" alkupuolella, kapeampi
  "salmi" loppupuolella.

## Tien keskilinja (X kullakin Z:lla)

`roadX(z) = 15 + 10·sin²(π(z+100)/100)` kun `−100 ≤ z ≤ 0`
`roadX(z) = 15 − 6·sin²(πz/100)` kun `0 < z ≤ 100`
`roadX(z) = 15` muuten (suorat osuudet)

| Z | X (keskilinja) | Osuus |
|---|---|---|
| −170 → −100 | 15 | suora (lähtö) |
| −75 | 20 | kaartuu oikealle |
| **−50** | **25** | 1. mutkan huippu (oikealla) |
| −25 | 20 | palaa kohti keskustaa |
| 0 | 15 | käännekohta (suunta vaihtuu) |
| 25 | 12 | kaartuu vasemmalle |
| **50** | **9** | 2. mutkan pohja (vasemmalla) |
| 75 | 12 | palaa kohti keskustaa |
| 100 → 170 | 15 | suora (maali) |

## Vesi / rantaviiva

Meri seuraa tien kaarretta koko radan matkan tien VASEMMALLA (pienemmän X:n)
puolella, vakioetäisyydellä roadX(z):sta — tästä syystä vesileveys pysyy
~34 yksikkönä (~48 % 71.5-levyisestä maastosta) KOKO matkan riippumatta
mutkan suunnasta tai jyrkkyydestä.

| Ulottuvuus | Kaava | Selitys |
|---|---|---|
| Piennar | roadX(Z) ± (6…7.8) | ajoradan reuna + 1.8 piennar |
| Hiekka | roadX(Z) − 10.3 … roadX(Z) − 7.8 | 2.5 leveä kaistale piennarin ja veden välissä |
| Vesi | roadX(Z) − 44.3 … roadX(Z) − 10.3 | 34 leveä, koko matkan vakio |
| Maapuoli (oikea) | roadX(Z) + 7.8 … 35.75 | vaihtelee 3–20 leveän välillä riippuen kohdasta |

Maapuolen leveys on PIENIMMILLÄÄN 1. mutkan huipulla (Z=−50, vain ~3 yks.) ja
SUURIMMILLAAN 2. mutkan pohjalla (Z=45…75, ~18–19 yks.) — siksi kaikki isot
maaelementit (nosturi, kontit, varasto) on sijoitettu juuri sinne.

## Kiintopisteet radalla

| Kohde | Sijainti (X, Y, Z) | Kierto | Huom |
|---|---|---|---|
| StartPoint (lähtö) | (15, 1, −150) | — | suoralla |
| Finish (maali) | (15, 0, +150) | — | suoralla |
| Majakka (Satama_Majakka) | (6, 0, −100) | 0° | sataman "suulla", hiekalla |
| Laituri (Satama_Laituri) | (11.2, 0, −50) | 90° | 1. mutkan huipulla, ulottuu rannalta veteen |
| Laiva (Satama_Laiva) | (0, 0, −50) | 190° | laiturin vesipään tuntumassa |
| Vene (Satama_Vene) | (6, 0, −35) | 45° | lahdessa |
| Poiju A/B/C (Satama_Poiju) | (−5,−80) / (−8,−15) / (−10,30) | 0° | avovedessä |
| Rantalyhty A/B | (14,−65) / (6,140) | 0° | lahden reunalla / uloskäynnin suoralla |
| Konttipino B | (24, 0, 45) | 20° | lastauspiha, 2. mutkan kohdalla |
| Nosturi | (24, 0, 55) | 160° | lastauspiha |
| Konttipino A | (23, 0, 65) | 0° | lastauspiha |
| Varasto | (28, 0, 75) | −10° | lastauspiha |
| Lätäkkö | (15, 0.03, 0) | — | käännekohdassa, tiellä |
| Kartio × 2 | (z=−50, offset +3.5) / (z=50, offset −3.5) | — | mutkien huipuilla/pohjilla |
| Boost | — | — | ei boosteja tällä radalla (opettaa pidon merkitystä) |
| Vene B/C (Satama_Vene) | (9,−58) / (8.5,−42) | −25° / 150° | pikkuvenesatama laiturin ympärillä |
| Purjevene / Purjevene B | (2.5,−60) / (−4,−25) | 110° / 200° | uudet mallit, mastolla+purjeella |
| Moottorivene / B | (10,−44) / (−6,20) | 255° / 60° | uudet mallit, ohjaamolla |

## Pikkuvenesatama (lisätty)

Laiturin (Z=−50) ympärille lisättiin 6 uutta venettä referenssikuvien marina-tunnelman
mukaan: 2 lisää Satama_Vene-kopiota, ja kaksi uutta mallia — **Satama_Purjevene**
(matala runko + masto + purje + viiri, dims 1.2×3.7×4.0) ja **Satama_Moottorivene**
(runko + ohjaamo, dims 1.4×1.25×4.5) — molemmat rakennettu samalla paletti-tekstuuri-
tekniikalla kuin muut Satama-mallit. Kaikki 6 tarkistettu KIERRETYLLA bounding boxilla
pysymään veden vyöhykkeen sisällä ja ≥2,5 yksikön päässä laiturista/laivasta/toisistaan.

Kaikki yllä olevat sijainnit on tarkistettu ohjelmallisesti jokaisen mallin
todellista (scale-kerrottua) ja KIERRETTYÄ bounding boxia vasten — ei pelkkää
pivot-pisteen etäisyyttä — jotta mikään ei ulotu tielle, piennarelle tai
väärälle puolelle vettä/maata.

## Käytännön hyöty

Radan tarina etenee järjestyksessä: sataman suu (majakka) → lahti (laituri,
laiva, vene, poijuja vedessä) → lastauspiha (nosturi, kontit, varasto) →
uloskäynti (toinen rantalyhty). Koristeiden sijoittelussa muista kaksi rajaa
yhtä aikaa: **etäisyys tien keskilinjasta** JA **kummalla puolella vesi on**
— maaelementit roadX(Z):n POSITIIVISELLE (oikealle) puolelle, vesielementit
NEGATIIVISELLE (vasemmalle) puolelle.
