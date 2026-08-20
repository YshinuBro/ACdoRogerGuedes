using UnityEngine;
using UnityEngine.InputSystem;

// Move o CharacterController do Player na direcao para onde a cabeca esta olhando.
// A camera NUNCA e rotacionada por codigo: quem gira a cabeca e o head tracking.
[RequireComponent(typeof(CharacterController))]
public class MovimentoPlayer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform cabeca;

    [Header("Movimento")]
    [SerializeField] private float velocidade = 2.2f;
    [SerializeField] private float zonaMorta = 0.2f;
    [SerializeField] private float gravidade = -12f;

    private CharacterController controlador;
    private float velocidadeVertical;

    private void Awake()
    {
        controlador = GetComponent<CharacterController>();
        if (cabeca == null && Camera.main != null) cabeca = Camera.main.transform;
    }

    private void Update()
    {
        Vector2 entrada = LerEntrada();

        if (GerenciadorJogo.Instancia != null && GerenciadorJogo.Instancia.JogoTerminou) entrada = Vector2.zero;

        Vector3 frente = Vector3.forward;
        Vector3 lado = Vector3.right;

        if (cabeca != null)
        {
            frente = cabeca.forward;
            lado = cabeca.right;
        }

        // Achata no plano do chao para nao voar nem afundar ao olhar para cima ou para baixo.
        frente.y = 0f;
        lado.y = 0f;
        frente.Normalize();
        lado.Normalize();

        Vector3 direcao = frente * entrada.y + lado * entrada.x;
        if (direcao.sqrMagnitude > 1f) direcao.Normalize();

        if (controlador.isGrounded && velocidadeVertical < 0f) velocidadeVertical = -2f;
        velocidadeVertical += gravidade * Time.deltaTime;

        Vector3 passo = direcao * velocidade;
        passo.y = velocidadeVertical;

        controlador.Move(passo * Time.deltaTime);
    }

    private Vector2 LerEntrada()
    {
        Vector2 entrada = Vector2.zero;

        Gamepad controle = Gamepad.current;
        if (controle != null)
        {
            entrada = controle.leftStick.ReadValue();
            if (entrada.magnitude < zonaMorta) entrada = Vector2.zero;
        }

        // Fallback de teclado, so para testar dentro do Editor.
        if (entrada == Vector2.zero)
        {
            Keyboard teclado = Keyboard.current;
            if (teclado != null)
            {
                if (teclado.wKey.isPressed || teclado.upArrowKey.isPressed) entrada.y += 1f;
                if (teclado.sKey.isPressed || teclado.downArrowKey.isPressed) entrada.y -= 1f;
                if (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) entrada.x += 1f;
                if (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) entrada.x -= 1f;
            }
        }

        return entrada;
    }
}
