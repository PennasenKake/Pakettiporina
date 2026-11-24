# Suorituskyky: staattinen tarkistus 24.11.2026 21.20

## Player-asetukset (Android)
- Arkkitehtuurit: ARM64
- Scripting backend: IL2CPP
- Managed stripping: Minimal
- minSdk: AndroidApiLevel31 (Android 12+: vanhat lapsen kierrätyspuhelimet jäävät pois)
- Graphics API:t: Vulkan, OpenGLES3
- Development Build -overlay: Debug-buildissa neljän sormen kosketus 1 s näyttää FPS/GC/muisti.

## Laatu ja renderöinti
- Nykyinen laatutaso editorissa: High Fidelity
- Performant: vSync 0, AA 0, varjot Disable, pikselivalot 0
- Balanced: vSync 1, AA 0, varjot HardOnly, pikselivalot 1
- High Fidelity: vSync 1, AA 4, varjot All, pikselivalot 2
- Android käyttää oletuksena laatutasoa, joka on määritelty QualitySettingsissa (Android: Balanced). Balanced: HDR päällä, varjot päällä.
- Laatutaso 'Performant' (URP-profiili 'URP-Performant'): HDR pois tai ei luettavissa.
- ! Laatutaso 'Balanced' (URP-profiili 'URP-Balanced'): HDR on päällä (kallis mobiilissa, ei tarvita tässä tyylissä).
- ! Laatutaso 'High Fidelity' (URP-profiili 'URP-HighFidelity'): HDR on päällä (kallis mobiilissa, ei tarvita tässä tyylissä).

## Scenet ja projektin siisteys
- Scene 'Kyla' on tarkoituksella pois päältä (suunniteltu rata, valmistuu myöhemmin).

## Tekstuurit
- Iso tekstuuri: Assets/_Pakettiporina/Art/UI/pakettiporina_aloitusnakyma_tausta.png (1080x1920, pakkaus Compressed)
- Iso tekstuuri: Assets/_Pakettiporina/Art/UI/pakettiporina_halli_tausta.png (1080x1920, pakkaus Compressed)
- Iso tekstuuri: Assets/_Pakettiporina/Art/UI/pakettiporina_halli_tausta_uusi.png (1080x1920, pakkaus Compressed)
- Iso tekstuuri: Assets/_Pakettiporina/Art/UI/pakettiporina_taivas_skybox.png (2048x1024, pakkaus Compressed)
- 222 tekstuuria projektissa; yli 1,5 Mpx yhteensä 8,317952 Mpx; luettavia 0.

## Mallit
- 73 mallia; luettavia (Read/Write) 0.

## Äänet
- pakettiporina_musiikki_halli.wav: 21,3 s, 24000 Hz, 1 kanavaa, load DecompressOnLoad, Vorbis (puretaan muistiin kokonaan: 1,0 MB)
- pakettiporina_musiikki_pesu.wav: 16,6 s, 24000 Hz, 1 kanavaa, load DecompressOnLoad, Vorbis (puretaan muistiin kokonaan: 0,8 MB)
- pakettiporina_musiikki_rata.wav: 13,0 s, 24000 Hz, 1 kanavaa, load DecompressOnLoad, Vorbis (puretaan muistiin kokonaan: 0,6 MB)
- pakettiporina_musiikki_valikko.wav: 16,0 s, 32000 Hz, 1 kanavaa, load DecompressOnLoad, Vorbis (puretaan muistiin kokonaan: 1,0 MB)

## Koodi (tiedossa olevat kuumat kohdat)
- Juice: nappien automaattiskannaus: 0,25 s vain 3 s scenen latauksen jälkeen, muuten 1,5 s välein ja vain aktiiviset (korjattu 23.11.).
- CutoutAvoider: skannaus 0,1 s vain 3 s scenen latauksen jälkeen, muuten 0,25 s (korjattu 23.11.).
- RaceHUD.LateUpdate: laattojen koko ja paikka päivitettiin joka ruutu (korjattu: vain tekstin muuttuessa).
- Application.targetFrameRate / Screen.sleepTimeout: ei hallittu (korjattu: PerformanceManager).

**2 varoitusta.**
