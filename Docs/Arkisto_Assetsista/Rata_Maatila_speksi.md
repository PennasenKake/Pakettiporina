# Maatila — ratadata + suunnittelumuisti

Maatila on pelin ensimmäinen "hengähdys"-rata: helppo, rauhallinen, ei
kartioita eikä lätäköitä. Luvut tässä täsmäävät suoraan
`PakettiporinaTracks.cs`/`PakettiporinaDecor.cs`:ään. Dokumentin loppuosa
(”Suunnittelumuisti”) tiivistää 9.8.2026 läpikäydystä ulkopuolisesta
Blender-generaattori-materiaalista sen, mikä on OIKEASTI hyödyllistä meidän
pipelinellemme — ei koko materiaalia sellaisenaan.

## Maaston kokonaismitat

| Ulottuvuus | Arvo | Rajat |
|---|---|---|
| Leveys (X) | ~71.5 (GROUND_SCALE_X=7.2) | −35.75 … +35.75 |
| Pituus (Z) | 300 (start −150 → finish +150) | −150 … +150 |
| Korkeus (Y) | 0 (tasainen) | ajopinta tasolla 0 |

## Tie

- `roadBaseX = 0` (tie kaartaa keskilinjan MOLEMMIN puolin, ei sivuun
  siirretty kuten Satamassa).
- Muoto: kaksi hyvin loivaa peltotien kaarretta, symmetriset amplitudiltaan
  (14 / −14) mutta vastakkaisiin suuntiin.
- Ei kartioita, ei lätäköitä, ei boosteja (`boostZ`/`puddleZ`/`cones` kaikki
  tyhjiä) — tarkoituksella, opettaa ajamisen ja tähtien keräämisen ilman
  rangaistusriskiä.

## Tien keskilinja (X kullakin Z:lla)

`roadX(z) = 14·sin²(π(z+110)/90)` kun `−110 ≤ z ≤ −20`
`roadX(z) = −14·sin²(π(z−20)/90)` kun `20 ≤ z ≤ 110`
`roadX(z) = 0` muuten (suorat osuudet)

| Z | X (keskilinja) | Osuus |
|---|---|---|
| −150 → −110 | 0 | suora (lähtö) |
| **−65** | **+14** | 1. mutkan huippu (oikealle) |
| −20 | 0 | suora väli |
| 0 → 20 | 0 | suora väli |
| **65** | **−14** | 2. mutkan huippu (vasemmalle) |
| 110 → 150 | 0 | suora (maali) |

## Nykyiset koristeobjektit (PakettiporinaDecor.cs)

| Kohde | Sijainti (X, Z) | Kierto | Skaala | Huom |
|---|---|---|---|---|
| Navetta (Maatila_Navetta) | (−18, −70) | 90° | 1.0 | 1. mutkan ulkolaidalla |
| Heinäpaali A/B/C | (−13,−62) / (−14,−76) / (−11,−55) | 20°/60°/−30° | 1.0/1.0/0.9 | klusteri navetan lähellä |
| Aita A–I (9 kpl) | ks. koodi, Z −95…80 | vaihtelee | 1.0 | reunustaa tietä molemmin puolin koko matkan |
| Lehmä A/B | (15,−5) / (18,10) | 200°/160° | 1.0 | laidun keskiosassa |
| Lammas A/B/C | (16,3) / (20,15) / (14,20) | 90°/250°/30° | 1.0/1.0/0.9 | sama laidun |
| Variksenpelätin | (10, 45) | 0° | 1.2 | 2. mutkan lähellä, EI ajolinjalla |
| Tuulimylly | (−20, 90) | 0° | 1.2 | loppusuoralla, kaukaa näkyvä maamerkki |

## Suunnittelumuisti — mikä ulkopuolisesta materiaalista oli oikeasti hyödyllistä

9.8.2026 käytiin läpi laaja ulkopuolinen Blender-Python-farm-generaattori-
materiaali (bpy.ops-pohjainen, Root/Empty-hierarkiat, FARM_CONFIG-dict, LOD/
collision-collectionit). Suurin osa siitä ratkaisee ERI pipelinen ongelmaa
kuin meillä on — se olettaa että Blender-scene ON lopputuote, kun taas
meillä Blender on vain FBX-generaattori ja Unity (DecorItem) hoitaa
sijoittelun ja pelilogiikan erillisenä kerroksena. Näitä osia EI oteta
käyttöön: bpy.ops-primitiivipohjainen rakennustapa (hauraampi tool-call-
generoinnissa kuin oma bmesh-tapamme), Root/Empty-parent-hierarkiat (turhia,
me viemme yhden yhdistetyn meshin per objekti), LOD/collision-collectionit
(turhia näin pienillä polygonimäärillä), gameplay-custom-propertyt
mesh-objekteissa (Unityn MonoBehaviour-komponentit, esim. `Cone.cs`,
hoitavat tämän paremmin).

Nämä periaatteet OVAT oikeasti hyödyllisiä ja kannattaa pitää mielessä
seuraavan kerran kun Maatilaa laajennetaan (esim. traktori, lisää eläimiä,
portti):

**1. Standardoidut todelliset koot ENNEN mallinnusta, ei jälkikäteen audit-
skriptillä.** Tässä sessiossa löysimme ylikokoisia objekteja (Penkki,
Lyhtypylväs, Kiipeilyteline) vasta jälkikäteen Python-audit-skriptillä.
Referenssitaulukko seuraavaa eläin/objektilaajennusta varten:

| Eläin/objekti | Korkeus (maasta selkään/huippuun) |
|---|---|
| Hevonen | 2.2 m |
| Lehmä | 1.6–1.7 m |
| Vuohi | 1.1 m |
| Lammas | 1.0 m |
| Possu | 0.8–0.9 m |
| Kana | 0.4–0.5 m |
| Kani | 0.3–0.4 m |
| Tipu | 0.2 m |
| Variksenpelätin | 2.0–2.2 m |
| Traktori | 2.0–2.5 m |
| Navetta | 8–12 m (meidän nykyinen huomattavasti pienempi — tarkista jos "iso maamerkki" -tuntuma puuttuu) |
| Tuulimylly | 8–10 m |

**2. Siluetti-periaate ja "symboli, ei realismi".** Lapsi tunnistaa lehmän
sekunnissa jos siinä on: valkoinen vartalo + mustat täplät + musta nenä +
neljä jalkaa + pää. EI kannata mallintaa turkkia, sorkkia, realistisia
kasvonpiirteitä — meidän matalapoly-paletti-atlas-tekniikka on jo
lähtökohtaisesti tätä periaatetta noudattava, mutta muista pitää se kurissa
myös JATKOSSA (esim. jos lisätään possu/vuohi/kana, älä lähde
yksityiskohtaistamaan yli tarpeen).

**3. Aita ei ole pelkkä koriste — se on pelaajalle suunnistussignaali**
("tähän suuntaan ei kuulu ajaa"). Meidän 9 aitasegmenttiä ympäröivät jo
tien koko matkan — tämä on hyvä, pidä sama periaate jos aitaa laajennetaan.

**4. Maamerkit (Tuulimylly, Navetta) opettavat sijaintia ilman karttaa** —
pelaaja oppii "kun näen tuulimyllyn, olen loppusuoralla". Meidän
Tuulimylly (Z=90, loppusuora) ja Navetta (Z=−70, alkupuoli) toteuttavat jo
tätä ideaa hyvin — säilytä tämä looginen etäisyys jos lisää maamerkkejä
tulee (älä ryhmitä kaikkia samaan kohtaan rataa).

**5. Variksenpelätin: ystävällinen, ei pelottava.** Pyöreä vaalea pää,
isot silmät, hymy, ruskea hattu — EI rasti-silmiä (näyttää helposti
oudolta/pelottavalta pieneltä lapselta). Tarkista tämä jos
Variksenpelätin-mallia joskus päivitetään.

**6. Pipeline-sääntö, joka löytyi vasta bugien kautta Satamassa — sovella
Maatilaan alusta asti:** `finish_export`-apufunktiota (tai muuta jaettua
export-koodia) EI SAA muokata yhden objektin erikoistarpeen takia.
Automaattiset bmesh-siivousoperaatiot kuten `recalc_face_normals` voivat
kääntää normaalit väärinpäin avoimissa/ei-manifold-meshessä (tapahtui
Satama_Venéelle — näytti "ylösalaiselta"). Jos jokin malli tarvitsee
siivousta, tee se ymmärretyllä, manuaalisella topologialla, ei
automaattisilla arvaus-operaattoreilla.

**7. Matalat/litteät objektit näkyvät huonosti ajopelin seurantakamerasta
jos ne osoittavat "kameraa kohti".** Kolmannen persoonan seurantakamera
katsoo suunnilleen radan suuntaisesti (Z-akseli meillä). Jos objektilla ei
ole pystysuoraa, aina-tunnistettavaa piirrettä (masto, hytti, pää), sen
PITKÄ akseli kannattaa suunnata POIKITTAIN kameran katsesuuntaan nähden,
ei sen suuntaisesti — muuten se näyttää typistyneeltä läiskältä. Tämä
löytyi Satama_Venéen kolmannella korjauskierroksella; sovella suoraan jos
Maatilaan lisätään matalia objekteja (esim. makaava eläin, kärry).

**8. Kokostandardi + selkeä sijainti ENNEN Blender-työtä säästää iteraatio-
kierroksia.** Kirjaa aina etukäteen tähän dokumenttiin: kohteen
todellinen tavoitekoko (taulukko yllä), tarkka (X,Z)-sijainti ja
`roadX(z)`-etäisyys tiestä/piennarrasta, ennen kuin avaat Blenderin. Tämä
olisi säästänyt useita kierroksia sekä Satamassa (koko/sijoitteluauditit
jälkikäteen) että Puistossa (Kiipeilytelineen 6 uudelleenrakennusta).

## Mahdolliset seuraavat laajennukset (ei vielä tehty)

Referenssimateriaalin eläinroosteri oli laajempi kuin meidän nykyinen
(lehmä+lammas): possu, vuohi, hevonen, kana+tipu, kani. Jos Maatilaa
laajennetaan, käytä yllä olevaa kokotaulukkoa ja samaa "symboli, ei
realismi" -periaatetta. Myös traktori (2.0–2.5 m, decorative tai
myöhemmin ajettava) ja aitausportti (`gate["openable"]`-tyylinen
custom-data — meillä tämä toteutettaisiin C#-puolella, ei Blenderissä)
olivat hyviä ideoita jos Maatilaan halutaan lisää "farmi elää" -tunnelmaa.
