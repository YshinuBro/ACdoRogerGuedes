# O VIGIA — AC2

Jogo em primeira pessoa para Android. Você está numa galeria de museu escura:
colete as 5 relíquias mirando nelas com a retícula e fuja pela porta. Uma estátua,
o Vigia, **só se move quando você não está olhando para ela** — e fica mais rápida
a cada relíquia que você pega.

**Feito por JP e Maria Letícia**, com Luigi, Maria Julia, Rafael e Lucca Lago.

---

## Como abrir o projeto

O projeto Unity fica na pasta **`ACdoRogerGuedes/`**, não na raiz deste
repositório. No Unity Hub, use *Add project from disk* e selecione:

```
<pasta onde você clonou>/ACdoRogerGuedes
```

É a pasta que contém `Assets/`, `Packages/` e `ProjectSettings/`.

**Versão da Unity: 6000.0.38f1.** A primeira abertura demora alguns minutos,
porque a Unity precisa reimportar todos os assets e gerar a pasta `Library/`,
que não é versionada de propósito.

### Se o clone der erro de "Filename too long"

É o limite de 260 caracteres do Windows. Clone para um caminho curto, como
`C:\Unity\OVigia`, ou habilite caminhos longos no Git:

```
git config --global core.longpaths true
```

---

## Onde está cada coisa

| Caminho | O que é |
|---|---|
| `ACdoRogerGuedes/Assets/Scripts/` | a lógica do jogo, 11 scripts |
| `ACdoRogerGuedes/Assets/Scenes/` | `00_Menu` (build 0) e `01_Fase` (build 1) |
| `ACdoRogerGuedes/Assets/Modelos/` | FBX modelados no Blender |
| `ACdoRogerGuedes/Assets/Editor/` | gerador da tela de entrada |
| `ACdoRogerGuedes/README.md` | documentação técnica completa |
| `ACdoRogerGuedes/PLANO-VR.md` | estudo de adaptação para Quest e PSVR2 |
| `ferramentas/` | scripts que montam o ferramental Android sem admin |

Abra a cena `00_Menu` e aperte Play. No Editor: **WASD** anda, **botão direito do
mouse** olha em volta, **espaço ou clique** interage.

---

## Controles no celular

| Ação | Comando |
|---|---|
| Olhar | girar o aparelho (giroscópio) |
| Andar | arrastar o dedo na **metade esquerda** da tela |
| Interagir | tocar na **metade direita** |

Um gamepad Bluetooth também funciona: stick esquerdo anda, A/B/R1/R2 interagem.

---

## Uma limitação conhecida, e honesta

O jogo foi construído para **Google Cardboard**, com tela dividida em dois olhos.
Essa parte **não funciona**: o plugin oficial do Google não é atualizado desde 2023
e quebra no Unity 6, que reestruturou o player Android. O app fecha ao abrir, com
`SIGABRT` em código nativo dentro de `CardboardLensDistortion_create`.

Isso foi investigado a fundo, com log capturado de um aparelho real, e o histórico
de commits registra as três tentativas de conserto e o ponto exato onde parou.

**O que foi preservado:** a visão em primeira pessoa pelo movimento do celular
continua funcionando, agora lendo o giroscópio direto pelo Input System em vez de
depender do Cardboard. Todo o resto do jogo — objetivo, inimigo, retícula,
vitória, derrota, mensagens — está completo e funcional.

O `PLANO-VR.md` documenta o caminho para Meta Quest, que não tem esse problema.
