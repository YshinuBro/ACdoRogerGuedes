using UnityEngine;

// Ajustes de tela para jogar no celular.
//
// Este script ja tentou inicializar o Google Cardboard. O plugin nao funciona
// no Unity 6: ele quebra com SIGABRT em CardboardLensDistortion_create, dentro
// do proprio codigo nativo, porque nao e atualizado desde 2023 e e anterior a
// reestruturacao do player Android. O historico de commits registra as tres
// tentativas de conserto e onde cada uma parou.
//
// O pacote foi removido do projeto, e quem faz a visao em primeira pessoa agora
// e o OlharPeloGiroscopio, lendo o sensor de atitude direto pelo Input System.
// Ver PLANO-VR.md para o caminho de adaptacao ao Meta Quest, que nao tem esse
// problema.
public class ControleCardboard : MonoBehaviour
{
    private void Start()
    {
        // A tela nao pode apagar no meio de uma partida.
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
}
