# Ferramental Android sem administrador

Estes três scripts reconstroem, em qualquer PC Windows, o ambiente necessário para
compilar o APK de **O VIGIA** — sem precisar de direitos de administrador.

Servem para quando o Unity Hub não oferece "Add modules" (caso de instalação manual
da Unity) ou quando você simplesmente não tem admin na máquina.

## Uso

A partir da raiz do repositório, com a **Unity fechada**:

```
powershell -ExecutionPolicy Bypass -File ferramentas\1-baixar.ps1
powershell -ExecutionPolicy Bypass -File ferramentas\2-arrumar.ps1
powershell -ExecutionPolicy Bypass -File ferramentas\3-configurar-unity.ps1
```

Tudo é instalado em `%USERPROFILE%\dev\android\`. Nada é escrito em `Program Files`.

| Script | O que faz | Custo |
|---|---|---|
| `1-baixar` | baixa JDK, SDK, NDK e CMake das URLs oficiais | ~1,2 GB |
| `2-arrumar` | extrai e renomeia para o layout que a Unity espera | ~3 GB em disco |
| `3-configurar-unity` | aponta o External Tools da Unity para essas pastas | segundos |

Cada script valida antes de agir e aborta sem escrever nada se algo não bater.

## Versões

Não são arbitrárias: são exatamente as que a API de releases da Unity indica para a
build **6000.0.38f1**. Trocar qualquer uma quebra o build.

```
OpenJDK 17.0.9+9        NDK r27c (27.2.12479018)
Build Tools 34.0.0      Platform Tools 34.0.5
Platforms 33/34/35      Command Line Tools 6.0      CMake 3.22.1
```

Se o projeto migrar para outra versão da Unity, essas versões mudam — e o sufixo da
chave `AndroidNdkRootR27C` no script 3 muda junto com o NDK.

## Armadilhas que estes scripts já resolvem

**A pasta do `cmdline-tools` precisa se chamar `6.0`.** O zip vem com uma pasta
`cmdline-tools` por fora, e a tentação é renomear para `latest`, que é a convenção do
Android. A Unity não acha o `sdkmanager` assim, e o erro que ela mostra fala em
*"sdk tools version 25 or higher"* — que não aponta para o problema real.

**Os zips do Google trazem nomes internos que não batem com o destino.** O Build Tools
34 vem numa pasta `android-14`; o Platform 33 vem como `android-13`. Quem extrai na mão
renomeia errado.

**O `-ProgressPreference` do PowerShell.** Sem desligar a barra de progresso, o
`Invoke-WebRequest` fica ordens de grandeza mais lento. O script 1 já desliga.

## O passo 3 é um atalho, não uma garantia

Ele escreve direto nas EditorPrefs da Unity, que no Windows ficam no registro do
usuário com um hash no nome da chave. O script descobre esse hash e se valida contra
as chaves que a própria Unity já escreveu, abortando se não conferir.

Ainda assim: na máquina onde foi escrito, os caminhos entraram, mas a Unity só passou
a aceitar o JDK depois de confirmação pela interface. Se o build reclamar de
**"JDK not found"**, abra `Edit > Preferences > External Tools` e use o **Browse** nos
três campos. É o caminho que sempre funciona.
