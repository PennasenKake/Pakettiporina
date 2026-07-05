# Asennus ja build

_(Kuinka kehitysympäristö pystytetään ja peli buildataan laitteelle.)_

## Vaatimukset
_(Unity-versio (esim. 2022.3.x LTS) ja asennetut moduulit: Android Build Support + Android SDK & NDK + OpenJDK. Mainitse IL2CPP.)_

## Projektin avaaminen
_(Avaa Unity Hubista, valitse oikea Unity-versio, avaa projektikansio. Ensimmäinen avaus tuo paketit — voi kestää.)_

## Pakettiriippuvuudet (Package Manager)
_(Listaa: Universal RP, Input System (tai legacy "Both"), TextMeshPro, mahdolliset muut. Versiot.)_

## Projektiasetukset
_(Active Input Handling = Both (Project Settings → Player → Other Settings → Configuration). Graphics → SRP-asset osoittaa URP:hen.)_

## Ajaminen editorissa
_(Avaa scene, paina Play, ohjaus WASD/nuolet. Konsolista näkee lokit [Car], [Race] jne.)_

## Android-build
_(File → Build Settings → Android → Switch Platform. Player Settings: Scripting Backend IL2CPP, Target Architectures ARM64, näytön suunta, Minimum API Level. Build → AAB/APK.)_

## Allekirjoitus (julkaisu)
_(Keystore-tiedoston luonti ja säilytys turvassa. Älä commitoi keystorea versionhallintaan.)_

## Yleiset ongelmat
_(Pinkki/magenta materiaali → aja Render Pipeline Converter (Built-in → URP). Input-virhe → Active Input Handling = Both. Auto putoaa → tarkista collider/StartPoint.)_
