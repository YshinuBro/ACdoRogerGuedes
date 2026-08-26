# O VIGIA - passo 2 de 3: extrair e montar a estrutura que a Unity espera
#
# Cada zip do Google traz uma pasta interna com nome proprio, e quase nunca e o
# nome que a Unity procura. Exemplos reais que nos custaram tempo:
#   build-tools 34  ->  pasta interna "android-14"
#   platform 33     ->  pasta interna "android-13"
#   cmdline-tools   ->  precisa se chamar "6.0", nao "latest"
#
# Aquele ultimo e o mais traicoeiro: com o nome errado a Unity nao acha o
# sdkmanager, nao consegue listar as API levels, e o erro que ela mostra fala de
# "sdk tools version 25 or higher", que nao aponta para o problema real.

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

$base = "$env:USERPROFILE\dev\android"
$dl   = "$base\_zips"
$tmp  = "$base\_tmp"      # curto de proposito: o NDK tem caminhos internos longos

function Instalar($zipNome, $destino) {
  $zip = Join-Path $dl $zipNome
  if (-not (Test-Path $zip)) { throw "zip ausente: $zipNome - rode o 1-baixar.ps1 primeiro" }

  if (Test-Path $tmp) { Remove-Item $tmp -Recurse -Force }
  New-Item -ItemType Directory -Force $tmp | Out-Null

  [System.IO.Compression.ZipFile]::ExtractToDirectory($zip, $tmp)

  $itens = Get-ChildItem $tmp
  if ($itens.Count -eq 1 -and $itens[0].PSIsContainer) {
    $origem = $itens[0].FullName
    $interna = $itens[0].Name
  } else {
    $origem = $tmp
    $interna = "(raiz do zip)"
  }

  $pai = Split-Path $destino -Parent
  New-Item -ItemType Directory -Force $pai | Out-Null
  if (Test-Path $destino) { Remove-Item $destino -Recurse -Force }

  Move-Item $origem $destino
  if (Test-Path $tmp) { Remove-Item $tmp -Recurse -Force -ErrorAction SilentlyContinue }

  Write-Host "[ok] $zipNome : $interna -> $(Split-Path $destino -Leaf)" -ForegroundColor Green
}

Write-Host "montando em $base" -ForegroundColor Cyan
Write-Host ""

Instalar 'jdk.zip'            "$base\jdk"
Instalar 'platform-tools.zip' "$base\SDK\platform-tools"
Instalar 'build-tools.zip'    "$base\SDK\build-tools\34.0.0"
Instalar 'platform-33.zip'    "$base\SDK\platforms\android-33"
Instalar 'platform-34.zip'    "$base\SDK\platforms\android-34"
Instalar 'platform-35.zip'    "$base\SDK\platforms\android-35"
Instalar 'cmdline-tools.zip'  "$base\SDK\cmdline-tools\6.0"
Instalar 'cmake.zip'          "$base\SDK\cmake\3.22.1"
Instalar 'ndk.zip'            "$base\NDK"

# ---------------------------------------------------------------- conferencia

Write-Host ""
Write-Host "conferindo os executaveis..." -ForegroundColor Cyan

$provas = @{
  "$base\jdk\bin\java.exe"                          = 'JDK'
  "$base\SDK\platform-tools\adb.exe"                = 'Platform Tools'
  "$base\SDK\build-tools\34.0.0\aapt2.exe"          = 'Build Tools'
  "$base\SDK\platforms\android-34\android.jar"      = 'Platform 34'
  "$base\SDK\cmdline-tools\6.0\bin\sdkmanager.bat"  = 'Command Line Tools'
  "$base\NDK\source.properties"                     = 'NDK'
}

$falhou = $false
foreach ($p in $provas.Keys) {
  if (Test-Path $p) {
    Write-Host ("  OK    " + $provas[$p]) -ForegroundColor Green
  } else {
    Write-Host ("  FALTA " + $provas[$p] + " -> " + $p) -ForegroundColor Red
    $falhou = $true
  }
}

Write-Host ""
if ($falhou) {
  Write-Host "Algo nao ficou no lugar. Nao siga para o passo 3." -ForegroundColor Red
  exit 1
}

Write-Host "PRONTO." -ForegroundColor Green
Write-Host "Agora rode: powershell -ExecutionPolicy Bypass -File ferramentas\3-configurar-unity.ps1"
