# O VIGIA

Jogo VR-Cardboard para Android, controlado por gamepad Bluetooth.
Trabalho AC2 — Unity 6 (6000.0.38f1), URP, Input System novo.

Você está numa galeria de museu escura. Colete as 5 relíquias mirando nelas com a
retícula e depois mire na porta de saída para escapar. Uma estátua — o Vigia —
**só se move quando você não está olhando para ela**. Se ela te alcançar, é derrota.

---

## Controles

| Ação | Comando |
|---|---|
| Olhar | Mover a cabeça (head tracking do Cardboard) |
| Andar | Stick esquerdo do gamepad |
| Interagir | Botão A / B / R1 / R2 do gamepad, ou toque na tela |
| Testar no Editor | WASD para andar, Espaço ou clique para interagir |

Os quatro botões são aceitos porque o mapeamento muda de controle para controle no Android.

---

## As cenas

O jogo tem duas cenas prontas e versionadas. Abra qualquer uma e aperte Play.

| Cena | Build index | O que é |
|---|---|---|
| `Assets/Scenes/00_Menu.unity` | 0 | tela de entrada: título, integrantes, instruções e o botão iniciar |
| `Assets/Scenes/01_Fase.unity` | 1 | a galeria jogável: 5 relíquias, a porta de saída e o Vigia |

A `01_Fase` tem cerca de 40 objetos — chão, quatro paredes, o bloco central, duas
vitrines, cinco pedestais com suas relíquias e luzes, a porta, o player VR e o Vigia.
Tudo cubo padrão, bem abaixo do teto de 50 mil triângulos que um celular aguenta
renderizando duas vezes por frame.

### Como as cenas foram construídas

O cenário não foi montado arrastando cubos no Editor: ele é descrito em código, em
`Assets/Editor/GeradorDaGaleria.cs` e `GeradorDoMenu.cs`, e montado pelo menu
**O Vigia** na barra superior da Unity.

A vantagem é reprodutibilidade. Um ajuste de layout — mover uma relíquia, mudar a
largura do corredor, redimensionar o painel do menu — é uma linha no gerador e um
clique, em vez de posicionar objeto por objeto e torcer para não esquecer nenhum.
Também garante que os dois integrantes gerem exatamente a mesma cena, sem divergência
de quem arrastou o quê.

| Item do menu | O que faz |
|---|---|
| 1. Criar cena da fase | cria e salva a `01_Fase` do zero |
| 2. Gerar cenário | regera a fase na cena já aberta |
| 3. Criar cena do menu | cria e salva a `00_Menu` do zero |
| 4. Gerar menu | regera o menu na cena já aberta |
| 5. Registrar cenas no Build Settings | põe `00_Menu` no índice 0 e `01_Fase` no 1 |

Os geradores também criam a layer `Interactive`, os materiais em `Assets/Materiais/`
e a iluminação. Podem rodar quantas vezes quiser: apagam o grupo anterior
(`GaleriaGerada` / `MenuGerado`) antes de recriar, então nunca duplicam.

**Como as cenas são geradas, edite-as pelos scripts, não à mão** — uma edição manual
se perde na próxima vez que alguém rodar o gerador.

### Créditos

As constantes ficam no topo de `Assets/Editor/GeradorDoMenu.cs`:

- **Integrantes** — JP e Maria Letícia em fonte 54, mais Luigi, Maria Julia,
  Rafael e Lucca Lago logo abaixo em fonte 42. Todos contam como integrantes;
  o tamanho é que separa quem fez mais de quem fez menos.
- **Participação EXTREMAMENTE Especial** — a piada

Para mudar qualquer um deles, edite as constantes e rode
**O Vigia > 4. Gerar menu** com a `00_Menu` aberta, depois salve a cena (Ctrl+S).

---

## O que ainda precisa ser feito na Unity (passo a passo)

Nada disso pode ser feito por script — são cliques no Editor.

### 1. Google Cardboard XR Plugin — já está no repositório

O pacote vive **embutido** em `Packages/com.google.xr.cardboard/` (versão 1.34.0).
Pacote embutido é detectado sozinho pela Unity: não precisa fazer nada no Package
Manager, e não há entrada dele no `manifest.json`.

Foi feito assim de propósito, em vez do Git URL: o repositório fica autossuficiente,
então quem clonar não depende de rede nem de acesso ao GitHub para compilar.

Os binários de iOS (`Runtime/iOS`, 63 MB) foram removidos — o projeto é Android-only
e o `.gitattributes` não cobre `.a`/`.aar` com LFS, então esse arquivo entraria cru
no histórico do Git.

Na primeira vez que abrir o projeto, a Unity vai baixar sozinha as dependências
declaradas pelo pacote: `com.unity.xr.management` e `com.unity.xr.legacyinputhelpers`.
Isso precisa de internet, mas só uma vez.

### 2. Trocar a plataforma para Android

1. `File > Build Profiles`
2. Selecione **Android** na lista da esquerda
3. **Switch Platform** (demora alguns minutos na primeira vez)

### 3. Ligar o Cardboard no XR Plug-in Management

1. `Edit > Project Settings > XR Plug-in Management`
2. Se pedir, clique em **Install XR Plugin Management**
3. Abra a aba com o ícone do **Android**
4. Marque **Google Cardboard**
5. Confirme que **Initialize XR on Startup** está marcado

### 4. Player Settings

`Edit > Project Settings > Player > aba Android`

**Other Settings > Rendering**
- **Auto Graphics API**: desmarcar
- Na lista de Graphics APIs, deixar **apenas OpenGLES3** — o Cardboard não funciona com Vulkan, remova-o
- **Color Space**: Linear
- **Multithreaded Rendering**: desmarcar

**Other Settings > Configuration**
- **Scripting Backend**: IL2CPP
- **Target Architectures**: marcar **ARM64**, desmarcar ARMv7
- **Minimum API Level**: Android 8.0 (API 26). Não pode ser menor: o `GfxPluginCardboard.aar`
  declara 26 no manifesto dele, e o Gradle recusa o merge se o app prometer rodar em
  versão mais antiga que uma biblioteca sua. O celular precisa ser Android 8 ou superior.
- **Package Name**: algo como `com.suadupla.ovigia`

**Resolution and Presentation**
- **Default Orientation**: Landscape Left

### 5. Gerar o APK

1. `File > Build Profiles > Android`
2. Confira que as duas cenas aparecem na lista, com `00_Menu` em primeiro
3. **Build** (ou **Build And Run** com o celular conectado por USB e depuração USB ligada)

### 6. No celular

1. Instalar o APK
2. Parear o controle por Bluetooth **antes** de abrir o jogo
3. Abrir o jogo, encaixar no Cardboard
4. Mirar no botão **INICIAR** e apertar o botão do controle

Se o controle não responder, teste os quatro botões (A, B, R1, R2) — o jogo aceita todos.

---

## Estrutura

```
Assets/
  Scripts/
    IInteragivel.cs        interface com um metodo: Interagir()
    GerenciadorJogo.cs     singleton: conta reliquias, vitoria, derrota, mensagens
    MovimentoPlayer.cs     stick esquerdo -> CharacterController, na direcao da cabeca
    InteracaoReticula.cs   raycast do centro da visao + botao do gamepad
    Reliquia.cs            coletavel (5 na fase)
    PortaSaida.cs          vitoria se todas coletadas
    Vigia.cs               so anda quando nao esta sendo observado
    BotaoIniciar.cs        botao da tela de entrada
  Editor/
    UtilGerador.cs         funcoes comuns aos geradores
    GeradorDaGaleria.cs    monta a fase inteira
    GeradorDoMenu.cs       monta a tela de entrada
  Materiais/               gerados pelos scripts acima
  Scenes/
    00_Menu.unity          build index 0
    01_Fase.unity          build index 1
```

### Regras que o código segue

- A câmera **nunca** é rotacionada por código — quem gira é o head tracking.
  Todo movimento acontece no objeto pai `Player`, que tem o CharacterController.
- Toda interface é **Canvas World Space**. Screen Space Overlay não renderiza em estéreo.
- **Nenhuma luz projeta sombra** — a cena é renderizada duas vezes num celular.
- Sem post-processing, sem NavMesh, sem pacotes externos.
- Código sem acento; acento só em texto que o jogador lê.

### Cenário

Galeria em anel de 20 m × 14 m, corredores de 4 m, paredes de 3,5 m. O bloco central
maciço (12 × 6 m) é o que quebra a linha de visão e dá espaço para o Vigia avançar.
Cerca de 30 objetos, todos cubos — bem abaixo do teto de 50 mil triângulos.

---

## Checklist do professor

| Requisito | Estado |
|---|---|
| Tela de entrada antes da fase (build index 0) | pronto |
| Nome do jogo na tela de entrada | pronto |
| Nome dos integrantes | pronto (6 nomes, JP e Maria Letícia em destaque) |
| Botão para iniciar o jogo | pronto (cubo verde, mira + botão) |
| Instruções básicas de controle | pronto (painel do menu) |
| Uso do player VR/Cardboard | pacote no repo; falta ligar no XR Plug-in Management |
| Movimentação com o stick do gamepad | pronto |
| Visão em primeira pessoa pelo movimento do celular | **depende dos passos 1 a 4 acima** |
| Uso da retícula para mirar | pronto (muda de cor sobre o alvo) |
| Pelo menos uma interação com a retícula | pronto (5 relíquias, porta e botão do menu) |
| Uma fase 3D jogável | pronto |
| Objetivo principal claro | pronto (contador na HUD, mensagem inicial, luz na saída) |
| Um inimigo que ameaça o jogador | pronto (o Vigia) |
| Condição de vitória | pronto (mirar na porta com as 5) |
| Condição de derrota | pronto (o Vigia encosta) |
| Mensagens e feedbacks adequados para VR | pronto (HUD World Space a 2 m) |

---

## Ajuste fino depois do primeiro teste

Os valores do Vigia estão no Inspector do objeto `Vigia`, dentro de `GaleriaGerada`:

| Campo | Padrão | Se... |
|---|---|---|
| `velocidade` | 1.3 | fácil demais, suba para 1.6–1.8 |
| `anguloDeVisao` | 65 | ele avança quando já deveria estar visível, suba para 80 |
| `distanciaDeToque` | 1.3 | a derrota dispara longe demais, baixe para 1.0 |

Velocidade do jogador: campo `velocidade` no componente `MovimentoPlayer` do objeto `Player`
(padrão 2.2). Em VR, andar rápido demais causa enjoo — não passe muito de 2.5.
