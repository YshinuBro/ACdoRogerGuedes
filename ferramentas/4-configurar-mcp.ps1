# O VIGIA - aponta o MCP For Unity para o executavel do Claude Code
#
# Preenche a EditorPref "MCPForUnity.ClaudeCliPath", que e o campo
# "Claude CLI Path" da janela Window > MCP For Unity.
#
# So use isto se o botao Browse da janela nao resolver. O caminho normal e:
#   Browse > colar o caminho completo no campo "Nome do arquivo" > Enter
#
# A Unity PRECISA estar fechada: ela mantem as EditorPrefs em memoria e
# reescreve tudo ao sair, apagando o que for gravado com ela aberta.

$ErrorActionPreference = 'Stop'

$REG   = "HKCU:\Software\Unity Technologies\Unity Editor 5.x"
$CHAVE = "MCPForUnity.ClaudeCliPath"

# ---------------------------------------------------------------- achar o CLI

$raiz = "$env:APPDATA\Claude\claude-code"
if (-not (Test-Path $raiz)) {
  Write-Host "ABORTADO: nao achei $raiz" -ForegroundColor Red
  Write-Host "O Claude Code parece nao estar instalado neste usuario."
  exit 1
}

# Pega a versao mais recente, para nao quebrar quando o Claude Code atualizar.
$versao = Get-ChildItem $raiz -Directory |
          Sort-Object { try { [version]$_.Name } catch { [version]"0.0.0" } } |
          Select-Object -Last 1

$cli = Join-Path $versao.FullName "claude.exe"
if (-not (Test-Path $cli)) {
  Write-Host "ABORTADO: nao achei claude.exe em $($versao.FullName)" -ForegroundColor Red
  exit 1
}

# Confirma que e mesmo o CLI, e nao o aplicativo de desktop.
$v = & $cli --version 2>&1 | Out-String
if ($v -notmatch "Claude Code") {
  Write-Host "ABORTADO: $cli nao respondeu como Claude Code." -ForegroundColor Red
  Write-Host "Resposta: $v"
  exit 1
}
Write-Host "[ok] CLI encontrado: $($v.Trim())" -ForegroundColor Green
Write-Host "     $cli"

# ---------------------------------------------------------------- Unity fechada?

$unity = Get-Process Unity -ErrorAction SilentlyContinue
if ($unity) {
  Write-Host ""
  Write-Host "ABORTADO: a Unity esta aberta (PID $($unity[0].Id))." -ForegroundColor Red
  Write-Host "Feche a Unity e rode de novo, senao ela apaga o que for gravado."
  exit 1
}
Write-Host "[ok] Unity fechada" -ForegroundColor Green

# ---------------------------------------------------------------- hash e validacao

function Get-UnityHash([string]$nome) {
  $h = [uint32]5381
  foreach ($c in [System.Text.Encoding]::ASCII.GetBytes($nome)) {
    $h = [uint32](((([uint64]$h * 33) -band 0xFFFFFFFFL) -bxor $c) -band 0xFFFFFFFFL)
  }
  return $h
}

if (-not (Test-Path $REG)) {
  Write-Host "ABORTADO: chave da Unity nao encontrada no registro." -ForegroundColor Red
  exit 1
}

# Valida a funcao de hash contra as chaves que a propria Unity ja escreveu.
$k = Get-Item $REG
$ok = 0; $ruim = 0
foreach ($n in $k.GetValueNames()) {
  if ($n -match '^(?<nome>.+)_h(?<hash>\d+)$') {
    if ((Get-UnityHash $Matches['nome']) -eq [uint32]$Matches['hash']) { $ok++ } else { $ruim++ }
  }
}
if ($ruim -gt 0 -or $ok -lt 10) {
  Write-Host "ABORTADO: hash nao confere ($ok ok, $ruim erros). Nada gravado." -ForegroundColor Red
  exit 1
}
Write-Host "[ok] hash validado contra $ok chaves existentes" -ForegroundColor Green

# ---------------------------------------------------------------- gravar

$nomeValor = "$CHAVE" + "_h" + (Get-UnityHash $CHAVE)
$bytes = [System.Text.Encoding]::UTF8.GetBytes($cli) + [byte]0
New-ItemProperty -Path $REG -Name $nomeValor -Value $bytes -PropertyType Binary -Force | Out-Null

# relê para provar
$lido = [System.Text.Encoding]::UTF8.GetString($k.GetValue($nomeValor)).TrimEnd([char]0)

Write-Host ""
if ($lido -eq $cli) {
  Write-Host "GRAVADO: $nomeValor" -ForegroundColor Green
  Write-Host "         $lido"
  Write-Host ""
  Write-Host "Abra a Unity, va em Window > MCP For Unity e confirme que o"
  Write-Host "indicador saiu de Not Configured. Se continuar vermelho, clique"
  Write-Host "em Configure."
} else {
  Write-Host "ERRO: releu '$lido'" -ForegroundColor Red
  exit 1
}
