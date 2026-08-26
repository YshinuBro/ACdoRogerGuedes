# O VIGIA - passo 1 de 3: baixar o ferramental Android
#
# Baixa JDK, SDK e NDK para dentro do seu perfil de usuario.
# Nao escreve em Program Files, entao NAO precisa de administrador.
#
# As versoes abaixo nao sao escolha nossa: sao exatamente as que a API de releases
# da Unity indica para a build 6000.0.38f1. Trocar qualquer uma quebra o build.
#   OpenJDK 17.0.9+9 | NDK r27c | Build Tools 34.0.0 | Platform Tools 34.0.5
#   Platforms 33/34/35 | Command Line Tools 6.0 | CMake 3.22.1
#
# Total: ~1,2 GB de download, ~3 GB em disco depois de extrair.

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'   # sem isso o Invoke-WebRequest fica lentissimo

$dl = "$env:USERPROFILE\dev\android\_zips"
New-Item -ItemType Directory -Force $dl | Out-Null

$items = @(
  @{ n = 'jdk.zip';            u = 'https://download.unity3d.com/download_unity/open-jdk/open-jdk-win-x64/jdk17.0.9-9_f12c2989c2f749b13282640a12d7d624097f6c2d45144d87331f21ad352ab63e.zip' },
  @{ n = 'platform-tools.zip'; u = 'https://dl.google.com/android/repository/platform-tools_r34.0.5-windows.zip' },
  @{ n = 'build-tools.zip';    u = 'https://dl.google.com/android/repository/build-tools_r34-windows.zip' },
  @{ n = 'platform-33.zip';    u = 'https://dl.google.com/android/repository/platform-33_r02.zip' },
  @{ n = 'platform-34.zip';    u = 'https://dl.google.com/android/repository/platform-34-ext7_r02.zip' },
  @{ n = 'platform-35.zip';    u = 'https://dl.google.com/android/repository/platform-35_r01.zip' },
  @{ n = 'cmdline-tools.zip';  u = 'https://dl.google.com/android/repository/commandlinetools-win-8092744_latest.zip' },
  @{ n = 'cmake.zip';          u = 'https://dl.google.com/android/repository/cmake-3.22.1-windows.zip' },
  @{ n = 'ndk.zip';            u = 'https://dl.google.com/android/repository/android-ndk-r27c-windows.zip' }
)

Write-Host "baixando para $dl" -ForegroundColor Cyan
Write-Host ""

$total = 0
foreach ($i in $items) {
  $out = Join-Path $dl $i.n
  if (Test-Path $out) {
    $mb = [math]::Round((Get-Item $out).Length / 1MB, 1)
    Write-Host "[ja tinha] $($i.n) - $mb MB"
    $total += $mb
    continue
  }
  Write-Host "[baixando] $($i.n) ..."
  Invoke-WebRequest -Uri $i.u -OutFile $out -UseBasicParsing
  $mb = [math]::Round((Get-Item $out).Length / 1MB, 1)
  $total += $mb
  Write-Host "[ok] $($i.n) - $mb MB" -ForegroundColor Green
}

Write-Host ""
Write-Host "TOTAL: $total MB" -ForegroundColor Green
Write-Host "Agora rode: powershell -ExecutionPolicy Bypass -File ferramentas\2-arrumar.ps1"
