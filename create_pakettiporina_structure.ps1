# Luo Pakettipörinä-Unity-projektin hakemistorakenteen (Windows).
# Aja täma Unity-projektin JUURESSA (kansio, jossa Assets\ sijaitsee):
#   powershell -ExecutionPolicy Bypass -File .\create_pakettiporina_structure.ps1
# Sulje Unity ennen ajoa ja avaa uudelleen jalkeen, niin Unity luo .meta-tiedostot.

$Root = "Assets\_Pakettiporina"

$Dirs = @(
  "Art\Models",
  "Art\Materials",
  "Art\Textures",
  "Art\VFX",
  "Art\UI\Icons",
  "Art\UI\Stickers",
  "Audio\Music",
  "Audio\SFX",
  "Prefabs\Car",
  "Prefabs\TrackBlocks",
  "Prefabs\Pickups",
  "Prefabs\Obstacles",
  "Prefabs\UI",
  "Scenes",
  "Scripts\Core",
  "Scripts\Vehicle",
  "Scripts\Builder",
  "Scripts\Gameplay",
  "Scripts\CarCare",
  "Scripts\UI",
  "Scripts\Data",
  "Data\Parts",
  "Data\Packages",
  "Data\Stickers",
  "Data\Districts",
  "Settings",
  "Localization",
  "Resources"
)

if (-not (Test-Path "Assets")) {
  Write-Host "VAROITUS: Assets-kansiota ei loydy. Aja tama Unity-projektin juuressa." -ForegroundColor Yellow
  exit 1
}

foreach ($d in $Dirs) {
  $path = Join-Path $Root $d
  New-Item -ItemType Directory -Force -Path $path | Out-Null
  New-Item -ItemType File -Force -Path (Join-Path $path ".gitkeep") | Out-Null
}

$Asmdef = Join-Path $Root "Scripts\Pakettiporina.asmdef"
if (-not (Test-Path $Asmdef)) {
@'
{
    "name": "Pakettiporina",
    "rootNamespace": "Pakettiporina",
    "references": [],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "autoReferenced": true,
    "noEngineReferences": false
}
'@ | Set-Content -Path $Asmdef -Encoding UTF8
}

Write-Host "Valmis! Rakenne luotu kansioon $Root" -ForegroundColor Green
Write-Host "Avaa Unity uudelleen, niin .meta-tiedostot syntyvat automaattisesti."
