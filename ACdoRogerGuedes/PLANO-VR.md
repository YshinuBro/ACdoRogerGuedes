# Plano: levar O VIGIA para headsets de verdade

Documento de estudo, escrito depois da entrega do AC2. Nada aqui foi implementado.

O jogo hoje roda em **Google Cardboard**: o celular encaixado num suporte de papelão,
rotação vinda do giroscópio, e um gamepad Bluetooth para andar e interagir. Funciona,
cumpre a disciplina, mas deixa na mesa o que headsets modernos dão de graça — controles
com posição no espaço, rastreamento dos seis eixos e uma tela decente.

---

## Resumo da viabilidade

| Alvo | Viável? | Por quê |
|---|---|---|
| **Meta Quest 2 / 3** | **Sim** | Continua Android + ARM64. Boa parte do que já montamos serve. |
| **PSVR2 no PC**, via adaptador | **Sim** | Vira um build de PC com OpenXR, pelo SteamVR. |
| **PSVR2 no PS5** | **Não** | Exige devkit e contrato com a Sony. Fora de alcance. |

O caminho mais curto é o Quest. É o único que aproveita o ferramental Android que já
está instalado e documentado no [README](README.md).

---

## Meta Quest 2 / 3

### O que sobrevive sem tocar

Quase tudo, e é por isso que esse caminho vale a pena:

- **Toda a cadeia de build** — JDK, SDK, NDK, IL2CPP, ARM64. O Quest é Android.
  Os scripts em [`ferramentas/`](../ferramentas/LEIAME.md) continuam servindo.
- **As duas cenas**, os modelos do Blender, os materiais e a iluminação.
- **A lógica do jogo inteira** — `GerenciadorJogo`, `Reliquia`, `PortaSaida`, `Vigia`
  com sua rota em anel, `ClimaDaFase`. Nada disso sabe que existe Cardboard.
- **O Input System novo**, que já é o que os controles Touch usam.

### O que muda

**Trocar o plugin de XR.** Sai o `com.google.xr.cardboard`, entra o
`com.unity.xr.openxr` com o grupo de funcionalidades da Meta. Isso também dispensa a
dependência do AndroidX AppCompat que hoje existe só por causa do manifesto do
Cardboard — o `mainTemplate.gradle` pode voltar a ser o padrão.

**Subir o Minimum API Level.** Hoje está em 26, exigência do `.aar` do Cardboard. O
Quest 2 roda Android 10 (API 29) e o Quest 3 roda Android 12.

**Repensar o `androidApplicationEntry`.** Ele está em `Activity` porque o Cardboard não
suporta GameActivity. Sem o Cardboard, essa amarra some.

### A decisão de design que importa

Hoje a retícula e o congelamento do Vigia são a **mesma coisa**: os dois saem da cabeça.
Você mira olhando, e olhar é o que paralisa a estátua.

Com controles, o natural seria mover a retícula para a mão. Isso separa os dois gestos —
e essa separação é uma oportunidade, não um problema:

> Você aponta para uma relíquia com a mão enquanto mantém a cabeça virada para a
> estátua. Coletar deixa de exigir desviar o olhar, mas passa a exigir coordenar duas
> coisas ao mesmo tempo.

Ou o contrário, mantendo a mira na cabeça e usando os controles só para andar, o que
preserva a tensão atual intacta. **Vale prototipar as duas** antes de decidir; é a
escolha que mais muda o jogo.

### Conforto

O Vigia chega a 7,2 m/s no fim da fase. Em Cardboard isso já é rápido; num headset com
locomoção suave, correr de costas nessa velocidade enjoa muita gente. Duas medidas
padrão resolvem:

- **Vinheta de conforto** — escurecer as bordas da visão durante o movimento
- **Giro em incrementos** no stick direito, em vez de giro contínuo

### Passo a passo

1. Instalar `com.unity.xr.openxr` e habilitar o feature group da Meta
2. Em XR Plug-in Management, aba Android: trocar Cardboard por OpenXR
3. Player Settings: Minimum API Level para 29
4. Substituir o rig do `Player` pelo XR Origin, mantendo o `CharacterController`
5. Reescrever a entrada de `MovimentoPlayer` para os sticks dos controles
6. Decidir mira: cabeça ou mão — prototipar as duas
7. Adicionar vinheta de conforto e giro por incrementos
8. Testar no aparelho pelo Link antes de gerar APK

---

## PSVR2

São dois cenários bem diferentes, e vale não confundir.

**No PlayStation 5** o desenvolvimento passa pelo PlayStation Partner Program: é
preciso ser aprovado pela Sony, receber um devkit e assinar acordo de confidencialidade.
As ferramentas de Unity para PS5 são distribuídas sob esse contrato. Não é caro nem
difícil — é simplesmente fechado. Para um trabalho de faculdade, está fora de alcance.

**No PC**, com o *PlayStation VR2 PC Adapter*, o headset aparece como um dispositivo
SteamVR comum. Aí o alvo deixa de ser Android e passa a ser um build de Windows com
OpenXR — o mesmo caminho de qualquer headset de PC. Perde-se o rastreamento ocular e o
retorno háptico do headset, que são exclusivos do PS5, mas o jogo roda.

Se o objetivo é ver O VIGIA num PSVR2, o caminho é esse.

---

## Esforço e risco

O grosso do trabalho é **input e conforto**, não conteúdo. Cenário, modelos, mecânica e
progressão de dificuldade já existem e não sabem em que headset estão rodando.

O maior risco não é técnico: é decidir a mira de mão sem prototipar e descobrir tarde
que ela dissolveu a tensão do jogo. A mecânica inteira depende de olhar ser custoso.

**Recomendação: só depois da entrega.** O que existe hoje cumpre a checklist do AC2 e
está verificado. Trocar de plataforma antes disso arrisca o que já funciona, em troca de
algo que ninguém pediu.
