# O VIGIA - passo 3 de 3: apontar o External Tools da Unity para as ferramentas
#
# As preferencias do editor ficam em HKCU (seu usuario), entao nao precisa de
# administrador. O nome de cada valor no registro e "<chave>_h<hash djb2-xor>".
#
# AVISO HONESTO: este script e um atalho. Na maquina onde ele foi escrito, os
# caminhos entraram corretamente no registro, mas a Unity so passou a aceitar o
# JDK depois de confirmar pela interface. Se depois de rodar isto o build ainda
# reclamar de "JDK not found", abra Edit > Preferences > External Tools e use o
# botao Browse nos tres campos. Leva 30 segundos e resolve de vez.

$ErrorActionPreference = 'Stop'

$REG  = "HKCU:\Software\Unity Technologies\Unity Editor 5.x"
$BASE = ($env:USERPROFILE -replace '\\', '/') + "/dev/android"   # a Unity grava com barra normal

# ---------------------------------------------------------------- 1. Unity fechada?

$unity = Get-Process Unity -ErrorAction SilentlyContinue
if ($unity) {
  Write-Host "ABORTADO: a Unity esta aberta (PID $($unity[0].Id))." -ForegroundColor Red
  Write-Host "Ela reescreve as preferencias ao fechar e apagaria o que este script gravar."
  exit 1
}
Write-Host "[ok] Unity fechada" -ForegroundColor Green

# ---------------------------------------------------------------- 2. a funcao de hash

function Get-UnityHash([string]$nome) {
  $h = [uint32]5381
  foreach ($c in [System.Text.Encoding]::ASCII.GetBytes($nome)) {
    $h = [uint32](((([uint64]$h * 33) -band 0xFFFFFFFFL) -bxor $c) -band 0xFFFFFFFFL)
  }
  return $h
}

# ---------------------------------------------------------------- 3. auto-validacao

# Confere a funcao contra TODOS os valores que a propria Unity ja escreveu.
# Se um so nao bater, o hash esta errado e nao se escreve nada.

if (-not (Test-Path $REG)) {
  Write-Host "ABORTADO: chave da Unity nao encontrada. Abra e feche a Unity uma vez antes." -ForegroundColor Red
  exit 1
}

$chave = Get-Item $REG
$conferidos = 0
$erros = 0

foreach ($n in $chave.GetValueNames()) {
  if ($n -match '^(?<nome>.+)_h(?<hash>\d+)$') {
    if ((Get-UnityHash $Matches['nome']) -eq [uint32]$Matches['hash']) { $conferidos++ } else { $erros++ }
  }
}

if ($erros -gt 0 -or $conferidos -lt 10) {
  Write-Host "ABORTADO: a funcao de hash nao confere ($conferidos ok, $erros erros)." -ForegroundColor Red
  Write-Host "Configure pela interface: Edit > Preferences > External Tools."
  exit 1
}
Write-Host "[ok] hash validado contra $conferidos chaves existentes" -ForegroundColor Green

# ---------------------------------------------------------------- 4. as pastas existem?

$caminhos = @{
  'JdkPath'            = "$BASE/jdk"
  'AndroidSdkRoot'     = "$BASE/SDK"
  'AndroidNdkRootR27C' = "$BASE/NDK"     # o sufixo muda com a versao do NDK
}

$provas = @{
  "$BASE/jdk" = 'bin/java.exe'
  "$BASE/SDK" = 'platform-tools/adb.exe'
  "$BASE/NDK" = 'source.properties'
}

foreach ($p in $provas.Keys) {
  if (-not (Test-Path (Join-Path $p $provas[$p]))) {
    Write-Host "ABORTADO: instalacao incompleta, faltou $($provas[$p]) em $p" -ForegroundColor Red
    Write-Host "Rode os passos 1 e 2 antes deste."
    exit 1
  }
}
Write-Host "[ok] as tres pastas existem e tem os executaveis certos" -ForegroundColor Green

# ---------------------------------------------------------------- 5. gravar

function Set-UnityString([string]$nome, [string]$valor) {
  $vn = "$nome" + "_h" + (Get-UnityHash $nome)
  # A Unity guarda string como binario UTF-8 terminado em nulo.
  $bytes = [System.Text.Encoding]::UTF8.GetBytes($valor) + [byte]0
  New-ItemProperty -Path $REG -Name $vn -Value $bytes -PropertyType Binary -Force | Out-Null
}

function Set-UnityBool([string]$nome, [int]$valor) {
  $vn = "$nome" + "_h" + (Get-UnityHash $nome)
  New-ItemProperty -Path $REG -Name $vn -Value $valor -PropertyType DWord -Force | Out-Null
}

Write-Host ""
foreach ($nome in $caminhos.Keys) {
  Set-UnityString $nome $caminhos[$nome]
  Write-Host "  $nome = $($caminhos[$nome])"
}

# Desmarca as tres caixas de "usar a versao que veio com a Unity".
foreach ($nome in @('JdkUseEmbedded', 'SdkUseEmbedded', 'NdkUseEmbedded')) {
  Set-UnityBool $nome 0
  Write-Host "  $nome = 0 (desmarcado)"
}

Write-Host ""
Write-Host "GRAVADO." -ForegroundColor Green
Write-Host ""
Write-Host "Abra a Unity e CONFIRME em Edit > Preferences > External Tools." -ForegroundColor Yellow
Write-Host "Se algum campo aparecer vazio ou com aviso, use o Browse nesse campo:"
Write-Host "  JDK -> $BASE/jdk"
Write-Host "  SDK -> $BASE/SDK"
Write-Host "  NDK -> $BASE/NDK"
