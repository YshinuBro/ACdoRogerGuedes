using UnityEngine;
using UnityEngine.InputSystem;

// Gira a camera com o sensor de atitude do celular, sem depender de XR.
//
// Normalmente quem faz isso e o head tracking do Cardboard. Como o plugin do
// Cardboard nao sobe no Unity 6, este script assume o papel: le o giroscopio
// direto pelo Input System e aplica a rotacao na camera.
//
// So roda no aparelho. No Editor e se houver XR ativo ele sai de cena, para
// nao disputar a rotacao com quem tem prioridade.
public class OlharPeloGiroscopio : MonoBehaviour
{
    [Tooltip("Ajuste fino caso a imagem venha torta ou de lado.")]
    [SerializeField] private Vector3 correcao = new Vector3(90f, 0f, 0f);

    [Tooltip("Desliga em Editor, onde nao ha giroscopio.")]
    [SerializeField] private bool somenteNoAparelho = true;

    private bool ativo;
    private Quaternion rotacaoInicial;

    private void Start()
    {
        rotacaoInicial = transform.localRotation;

        if (UnityEngine.XR.XRSettings.isDeviceActive)
        {
            enabled = false;
            return;
        }

        if (somenteNoAparelho && Application.isEditor)
        {
            enabled = false;
            return;
        }

        if (AttitudeSensor.current == null)
        {
            Debug.LogWarning("Sem sensor de atitude neste aparelho.");
            enabled = false;
            return;
        }

        InputSystem.EnableDevice(AttitudeSensor.current);
        ativo = true;
    }

    private void Update()
    {
        if (!ativo || AttitudeSensor.current == null) return;

        Quaternion a = AttitudeSensor.current.attitude.ReadValue();

        // O sensor entrega num sistema destro com Z para cima; a Unity usa
        // canhoto com Y para cima. Inverter z e w faz a conversao.
        Quaternion naUnity = new Quaternion(a.x, a.y, -a.z, -a.w);

        transform.localRotation = Quaternion.Euler(correcao) * naUnity;
    }

    // Rechama no comeco para o jogador poder centralizar a mira olhando para
    // frente e reiniciando a referencia.
    public void Recentralizar()
    {
        transform.localRotation = rotacaoInicial;
    }
}
