# Asennus ja build

Päivitetty 24.11.2026.

## Vaatimukset
Unity **2022.3.23f1** (LTS) Unity Hubin kautta, moduulit: Android Build Support, Android SDK & NDK Tools, OpenJDK. Scripting Backend IL2CPP, kohdearkkitehtuuri **vain ARM64** (ARMv7 poistettu 24.11.2026 valikosta Pakettiporina → Suorituskyky → 03). Minimum API Level 31 (Android 12; rajaa vanhat puhelimet pois). Versio `0.1.0`.

## Projektin avaaminen
Avaa Unity Hubista projekti `Pakettiporina`, valitse versio 2022.3.23f1. Ensimmäinen avaus tuo kaikki assetit ja kääntää koodin (kestää useita minuutteja). Git LFS tarvitaan: `*.png`, `*.fbx`, `*.wav` ym. ovat LFS-tiedostoja (ks. `.gitattributes`); asenna LFS ennen kloonausta.

## Pakettiriippuvuudet
Universal RP, TextMeshPro, Unity UI. Ei kolmannen osapuolen paketteja ajossa. Active Input Handling = Both.

## Projektiasetukset
Graphics → SRP-asettina URP. Laatutasot: Performant, Balanced (Androidin oletus), High Fidelity. HDR on tällä hetkellä päällä Balancedissa ja High Fidelityssä (ks. TODO osio 8).

## Ajaminen editorissa
Avaa `MainMenu`-scene ja paina Play. Ohjaus: nuolet/WASD tai kosketusnapit. Tallennus kirjoittaa laitteen tallennuskansioon (editorissa omaan käyttäjäkansioon, joten testiajot muuttavat omaa tallennusta). Skriptimuutosten jälkeen Ctrl+R kääntää; odota käännös ennen Playta.

## Editorityökalut
Valikko **Pakettiporina**: Tarkistus (01–08), Korjaus, Lisäys, Työkalut, Esittely, Suorituskyky. Aja Tarkistus/08 (edistys) ja /02 (halli) ennen committia. Ks. [04_TESTAUS.md](04_TESTAUS.md).

## Android-build
File → Build Settings → Android → Switch Platform. Scenet: MainMenu, Garage, Wash, Game, Puisto, Satama, Huvipuisto, Maatila (Kyla pois, tehdään myöhemmin). Development Build näyttää FPS/GC/muisti-overlayn (neljän sormen kosketus 1 s). Build → APK/AAB.

## Allekirjoitus (julkaisu)
Keystore luodaan ja säilytetään turvassa; **älä koskaan commitoi keystorea** eikä salasanoja. Ei vielä luotu.

## Yleiset ongelmat
- Pinkki/magenta materiaali → Render Pipeline Converter (Built-in → URP).
- Käännös ei käynnisty → klikkaa Unityä ja paina Ctrl+R.
- Käyttöliittymä näyttää vanhalta → tarkista, että Garage-scenessä `GarageScreen.useWizard` on päällä.
- Hallin kuvat puuttuvat → Korjaus/10 (synkkaa `Resources/WizardUI`).
- Unity-editori voi jumittua muistin loppuessa pitkissä sessioissa: käynnistä uudelleen.
