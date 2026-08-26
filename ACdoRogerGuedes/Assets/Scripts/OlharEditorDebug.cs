using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

// Permite olhar em volta com o mouse dentro do Editor, para dar de testar o jogo
// sem celular. Segure o botao DIREITO do mouse e mova.
//
// Nao viola a regra de nunca rotacionar a camera por codigo: todo o corpo desta
// classe so existe no Editor (#if UNITY_EDITOR), e mesmo la ela se desliga sozinha
// se houver um dispositivo XR ativo. Num APK a classe fica vazia e nao roda nada.
public class OlharEditorDebug : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Olhar com o mouse (so no Editor)")]
    [SerializeField] private float sensibilidade = 0.12f;
    [SerializeField] private float limiteVertical = 85f;

    private float giroX;
    private float giroY;

    private void Start()
    {
        // Com o Cardboard ativo quem manda e o head tracking. Este script sai de cena.
        if (UnityEngine.XR.XRSettings.isDeviceActive)
        {
            enabled = false;
            return;
        }

        Vector3 e = transform.localEulerAngles;
        giroY = e.y;
        giroX = e.x > 180f ? e.x - 360f : e.x;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || !mouse.rightButton.isPressed) return;

        Vector2 d = mouse.delta.ReadValue();
        giroY += d.x * sensibilidade;
        giroX = Mathf.Clamp(giroX - d.y * sensibilidade, -limiteVertical, limiteVertical);

        transform.localRotation = Quaternion.Euler(giroX, giroY, 0f);
    }
#endif
}
