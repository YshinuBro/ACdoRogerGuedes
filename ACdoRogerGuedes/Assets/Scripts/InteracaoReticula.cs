using UnityEngine;
using UnityEngine.InputSystem;

// Fica na Camera. Dispara um raio do centro da visao e aciona IInteragivel
// quando o jogador aperta o botao. A mascara inclui a layer Default de proposito,
// para que uma parede na frente bloqueie a mira.
public class InteracaoReticula : MonoBehaviour
{
    [Header("Mira")]
    [SerializeField] private float alcance = 6f;
    [SerializeField] private LayerMask camadasDaMira = ~0;

    [Header("Feedback da reticula")]
    [SerializeField] private Renderer rendererDaReticula;
    [SerializeField] private Color corNormal = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color corSobreAlvo = new Color(1f, 0.85f, 0.2f, 1f);

    private IInteragivel alvoAtual;

    private void Update()
    {
        alvoAtual = ProcurarAlvo();
        PintarReticula(alvoAtual != null);

        if (alvoAtual != null && BotaoFoiApertado()) alvoAtual.Interagir();
    }

    private IInteragivel ProcurarAlvo()
    {
        RaycastHit toque;
        Ray raio = new Ray(transform.position, transform.forward);

        if (!Physics.Raycast(raio, out toque, alcance, camadasDaMira, QueryTriggerInteraction.Collide)) return null;

        return toque.collider.GetComponentInParent<IInteragivel>();
    }

    private void PintarReticula(bool sobreAlvo)
    {
        if (rendererDaReticula == null) return;
        rendererDaReticula.material.color = sobreAlvo ? corSobreAlvo : corNormal;
    }

    // O mapeamento de botao muda de controle para controle no Android,
    // entao aceitamos varios. O toque na tela cobre o gatilho do Cardboard.
    private bool BotaoFoiApertado()
    {
        Gamepad controle = Gamepad.current;
        if (controle != null)
        {
            if (controle.buttonSouth.wasPressedThisFrame) return true;
            if (controle.buttonEast.wasPressedThisFrame) return true;
            if (controle.rightShoulder.wasPressedThisFrame) return true;
            if (controle.rightTrigger.wasPressedThisFrame) return true;
        }

        // Toque na metade DIREITA interage. A esquerda e do joystick na tela.
        Touchscreen tela = Touchscreen.current;
        if (tela != null)
        {
            foreach (var t in tela.touches)
            {
                if (t.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.Began) continue;
                if (JoystickNaTela.ToqueDeInteracao(t.position.ReadValue())) return true;
            }
        }

        Keyboard teclado = Keyboard.current;
        if (teclado != null && teclado.spaceKey.wasPressedThisFrame) return true;

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame) return true;

        return false;
    }
}
