using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;
#if !UNITY_EDITOR && UNITY_ANDROID
using Google.XR.Cardboard;
#endif

// Liga o VR na ordem certa: primeiro grava os parametros do visor, so depois
// inicializa o XR.
//
// A ordem nao e detalhe. Com "Initialize XR on Startup" marcado, o renderizador
// nativo do Cardboard sobe ANTES de qualquer script rodar. Sem parametros de
// lente gravados ele chama CardboardLensDistortion_create com um objeto Java
// nulo e o processo morre com SIGABRT, antes do nosso Start() ter chance de
// gravar nada. Por isso a inicializacao do XR e feita aqui, na mao.
//
// Em vez de abrir o scanner de QR na primeira execucao, como faz o sample do
// Google, gravamos o perfil padrao do Cardboard V2. O sample exigiria permissao
// de camera e prenderia o jogador numa tela de leitura antes do jogo comecar.
// O botao de engrenagem continua abrindo o scanner para outros visores.
public class ControleCardboard : MonoBehaviour
{
    // Google Cardboard V2 (I/O 2015), o visor de papelao mais comum.
    private const string PERFIL_PADRAO =
        "https://google.com/cardboard/cfg?p=CgZHb29nbGUSEkNhcmRib2FyZCBJL08gMjAxNR0rGBU9JQHegj0qEAAAcEIAAHBCAABwQgAAcEJYADUpXA89OggeZnc-Ej6aPlAAYAM";

    private bool vrLigado;

    private IEnumerator Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        var settings = XRGeneralSettings.Instance;
        if (settings == null || settings.Manager == null)
        {
            Debug.LogWarning("XR nao configurado: rodando sem VR.");
            yield break;
        }

        // A segunda cena herda o XR ja ligado pela primeira.
        if (settings.Manager.isInitializationComplete)
        {
            vrLigado = true;
            yield break;
        }

        // 1. Inicializa o loader. So depois disto as chamadas de Api funcionam:
        //    HasDeviceParams e SaveDeviceParams comecam com uma checagem de
        //    XRLoader._isInitialized e saem caladas se ele for falso.
        yield return settings.Manager.InitializeLoader();

        if (settings.Manager.activeLoader == null)
        {
            Debug.LogError("Falhou ao inicializar o XR do Cardboard.");
            yield break;
        }

#if !UNITY_EDITOR && UNITY_ANDROID
        // 2. Agora sim grava o perfil do visor, ANTES de ligar os subsistemas.
        //    E em StartSubsystems que o renderizador cria a distorcao das lentes;
        //    sem parametros gravados ele aborta o processo com SIGABRT.
        if (!Api.HasDeviceParams())
        {
            Api.SaveDeviceParams(PERFIL_PADRAO);
        }
#endif

        // 3. So agora comeca a renderizar em estereo.
        settings.Manager.StartSubsystems();
        vrLigado = true;
    }

    private void Update()
    {
        if (!vrLigado) return;

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

        // Todo frame: e o que mantem a distorcao correta se a tela girar.
        Api.UpdateScreenParams();
#endif
    }
}
