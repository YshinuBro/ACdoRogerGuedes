using UnityEngine;
#if !UNITY_EDITOR && UNITY_ANDROID
using Google.XR.Cardboard;
#endif

// Cuida dos parametros do visor Cardboard.
//
// Com "Initialize XR on Startup" marcado, o renderizador nativo sobe junto com o
// app. Se nao houver parametros de dispositivo gravados, ele nao sabe a geometria
// das lentes e o app fecha sozinho ao abrir. O proprio sample do Google avisa que
// esta checagem "so e necessaria se o plugin de XR e inicializado no startup",
// que e o nosso caso.
//
// Em vez de abrir o scanner de QR na primeira execucao, que exige permissao de
// camera e deixa o jogador travado numa tela de leitura, gravamos o perfil padrao
// do Cardboard V2. O jogo abre direto. Quem tiver um visor diferente pode trocar
// pelo botao de engrenagem, que continua funcionando.
public class ControleCardboard : MonoBehaviour
{
    // Perfil do Google Cardboard V2 (I/O 2015), o de papelao mais comum.
    private const string PERFIL_PADRAO =
        "https://google.com/cardboard/cfg?p=CgZHb29nbGUSEkNhcmRib2FyZCBJL08gMjAxNR0rGBU9JQHegj0qEAAAcEIAAHBCAABwQgAAcEJYADUpXA89OggeZnc-Ej6aPlAAYAM";

    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

#if !UNITY_EDITOR && UNITY_ANDROID
        if (!Api.HasDeviceParams())
        {
            Api.SaveDeviceParams(PERFIL_PADRAO);
        }
#endif
    }

    private void Update()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        // Engrenagem: troca de visor lendo o QR do proprio aparelho.
        if (Api.IsGearButtonPressed)
        {
            Api.ScanDeviceParams();
        }

        if (Api.HasNewDeviceParams())
        {
            Api.ReloadDeviceParams();
        }

        // Precisa ser chamado todo frame: e o que mantem a distorcao das lentes
        // correta quando a tela gira ou muda de resolucao.
        Api.UpdateScreenParams();
#endif
    }
}
