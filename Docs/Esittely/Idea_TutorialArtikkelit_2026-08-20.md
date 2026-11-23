# Idea: tutorial-artikkelisarja + repo (varasuunnitelma, 20.8.2026)

Käyttäjän jatkosuunnitelma **jos peliä ei viedä Play-kauppaan** (tai sen rinnalle):
tehdä tästä projektista tutorial-artikkelisarja, joka yhdistää Blenderin, Unityn,
Androidin ja AI-avusteisen (Claude) kehitystyön. Ei aloitettu, ei kiireellinen —
talteen otettu idea myöhempää päätöstä varten.

## Miksi tämä kulma on vahva

Useimmat Unity-tutoriaalit käsittelevät yhtä palaa (vain Unity, tai vain Blender).
Tässä projektissa on koko ketju idea → Blender-mallit → Unity-peli → Android-julkaisu
→ AI-avusteinen työnkulku sen läpi — ja viimeinen on ajankohtainen aihe josta harvalla
on käytännön kokemusta jaettavana. Projektissa on myös aitoja, dokumentoituja
epäonnistumis-ja-korjaus-tarinoita (esim. Scale-bugit, väärät materiaalit,
ajoituskilpailut) jotka ovat tutoriaalikultaa: "näin korjasin" > "näin se vain toimi".

## Kolme vahvinta yksittäistä aihetta

1. **Editor-työkalut** — miten automatisoit scenen kytkennät ja datan tarkistuksen
   omilla Unity-työkaluilla (Tarkistus/Korjaus/Lisäys-malli). Harva aloittelija osaa
   tämän, ja se on projektin vahvin näyte.
2. **Palettitekstuuri-tekniikka** — miten Blender-värit siirtyvät Unityyn luotettavasti
   yhdellä materiaalilla + pienellä PNG:llä. Konkreettinen, yleinen ongelma.
3. **Proseduraalinen ratageneraattori** — pelikenttien tuottaminen parametreilla
   Blenderissä.

## Malli: ilmainen artikkeli, maksullinen materiaali

- **Ilmaiseksi** (näkyvyys/maine): artikkelit + julkinen GitHub-repo esimerkkikoodilla.
- **Maksullisena** (pieni tulo, esim. 5-15 €): koottu asset-paketti — FBX-mallit,
  palettitekstuurit, editorityökalut valmiina, Blender-generaattorit, pieni PDF-opas.
- Alustat: artikkelit dev.to tai Medium; repo GitHub (`PennasenKake/Pakettiporina`);
  maksulliset assetit itch.io tai Gumroad; vapaaehtoinen Ko-fi/GitHub Sponsors.
- Lisenssi: MIT koodille, oma "vapaa omiin projekteihin, ei jälleenmyyntiä" -lisenssi
  assetteille.

## Artikkelisarjan runko (7 osaa, ei pakko tehdä kaikkia)

1. Yleiskatsaus — koko matka, koukku ja liikenteen veturi.
2. Blender → Unity -työnkulku (palettitekstuurit, FBX-vientiasetukset, Scale-ansat).
3. Unity-editorityökalut (vahvin näyte osaamisesta).
4. Proseduraalinen ratageneraattori.
5. Pelimekaniikan rakentaminen (auto, palkkiosilmukka, lapsiystävällinen suunnittelu).
6. AI-avusteinen kehitys käytännössä — rehellisesti mihin AI:ta käytettiin ja mihin ei,
   painottaen että päätökset/integroinnit/debuggaus olivat käyttäjän, AI oli työkalu.
7. Android-julkaisu — Play-vaatimukset lastenpelille.

## Kaava jokaiseen artikkeliin

Koukku (konkreettinen ongelma) → ongelma auki + kuva → ratkaisu vaiheittain, lyhyet
koodipalat → ennen/jälkeen-kuva tai GIF → linkki repoon + maksulliseen pakettiin.
Kuvat/GIFit ratkaisevat vetävyyden - niitä pitää kerätä pelistä matkan varrella.

## Päivitys 22.8.2026 — konkreettiset artikkeliaiheet suoraan projektin M-listasta

Käyttäjä toimitti projektin täyden virstanpylväs-/kehityslokidokumentin
tutorial-idean materiaaliksi. Dokumentissa on jo useita valmiiksi kirjoitettuja,
aidosti dokumentoituja debug-tarinoita — nämä ovat parasta mahdollista
tutoriaalimateriaalia, koska koukku/ongelma/vaiheittainen ratkaisu on jo olemassa,
pitää vain muotoilla artikkeliksi. Priorisoitu lista vahvimmasta heikoimpaan:

1. **"Miksi lätäköt eivät näkyneet radalla" (17.–19.8. saaga)** — vahvin yksittäinen
   kandidaatti koko materiaalissa. Monivaiheinen etsintä: ensin epäiltiin normaalien
   suuntaa (oikea osittain — Latakko.fbx:n pinnat osoittivat alaspäin), sitten
   sijaintia/rotaatiota (väärä johtopäätös, peruttiin), lopulta paljastui ettei
   maaston pintakorkeus ollutkaan tasan Y=0 vaan vaihteli muutaman sentin —
   ohuet objektit hautautuivat näkymättömiin. Ratkaisu: raycast-pohjainen
   `GroundY()`-apuri joka lukee maaston oikean korkeuden joka pisteessä. Koukku:
   "peli sanoi kaiken olevan kunnossa (sijainti, aktiivisuus, Layer, materiaali) —
   silti mitään ei näkynyt". Opettaa systemaattista poissulkemista ja sitä että
   omaan diagnostiikkatyökaluun ei pidä luottaa sokeasti (ks. kohta 3).
2. **CanvasScaler-referenssiresoluutio ja skaalausmatematiikka** — tiivis, itsenäinen
   pätkä: `scaleFactor = 2^lerp(...)`, miten eri referenssiresoluutiot tuottavat
   vertailukelpoisen skaalaussuhteen (`sqrt(pinta-ala-suhde)`), ja miksi UI:n
   koon "korjaaminen" yhdenmukaistamalla referenssiarvo saattaa rikkoa olemassa
   olevan asettelun. Hyvä, itsenäinen "Unity UI -matematiikka" -artikkeli, ei vaadi
   koko projektin kontekstia.
3. **"Kun oma debug-työkalu valehtelee" — bends-etumerkkisaaga** — sama
   ratakäyrä-bugi (Huvipuisto, Game, Puisto) diagnosoitiin Blender-tekstuurianalyysillä
   kolmesti, ja kahdesti korjaus oli väärä suuntaan päinvastoin kuin analyysi väitti
   — vain oikea Unity-pelitesti paljasti totuuden. Meta-artikkeli luottamuksesta
   epäsuoriin diagnostiikkamenetelmiin AI-avusteisessa kehityksessä; nivoutuu hyvin
   yhteen kohdan 6 (AI-artikkeli) kanssa.
4. **Unity-scenetiedoston käsivarainen muokkaus (YAML-taso)** — koko tekniikka jota
   tässä projektissa käytettiin toistuvasti: `.unity`-tiedostojen suora editointi
   ohi Editorin, `SceneRoots`-sentinel-dokumentin rooli juuriobjekteille, fileID-
   viittausten eheystarkistus (duplikaatit, roikkuvat viittaukset). Niche mutta
   ainutlaatuinen aihe — harva kirjoittaa tästä, koska harva tekee sitä.
5. **"MonoScript-fileID-ansa"** (19.8., DecorMarker-tapaus) — kun samassa .cs-
   tiedostossa on useampi luokka, Unityn oletus-fileID (11500000) osoittaa AINA
   tiedoston NIMEÄ vastaavaan luokkaan, ei siihen jota luulisi. Konkreettinen,
   opettavainen "gotcha"-artikkeli jonka moni Unity-kehittäjä osaisi tunnistaa
   omasta virheestään.
6. **Palettitekstuuri + FBX-tuontiasetukset -kokoelma** — `materialLocation`
   (External vs. InPrefab), `useFileScale`-bugi (uusi FBX saa joskus `1` vaikka
   pitäisi olla `0` → malli ~100x liian pieni), `useSRGBMaterialColor`. Käytännön
   tarkistuslista muille jotka tuovat Blender-malleja Unityyn.

**Suositus**: aloita listalta kohdasta 1 tai 2 — kumpikin toimii itsenäisenä
artikkelina ilman että lukijan pitää tuntea koko projektia, ja molemmilla on
selkeä pelastava koukku ("ongelma jonka kaikki merkit sanoivat olevan olemassa,
mutta ei näkynyt" / "matematiikka joka selittää miksi korjaus rikkoi asettelun").
Kohdat 3–6 sopivat paremmin sarjan myöhemmiksi osiksi tai yhdeksi kokoomaosaksi
("Unity-gotchat joihin törmäsin").

## Käytännön muistilista kun/jos tähän palataan

- Kerää kuvakaappauksia ja lyhyt pelivideo jo nyt, ei vasta kirjoitusvaiheessa.
- Aloita yleiskatsausartikkelista (#1) - se ohjaa loput ja on helpoin koukku.
- Ei kaikkea kerralla - yksi artikkeli, katso jatkuuko into.
- Peliä kannattaa pitää edistymässä rinnalla - valmiimpi peli tekee artikkeleistakin
  vakuuttavampia.
