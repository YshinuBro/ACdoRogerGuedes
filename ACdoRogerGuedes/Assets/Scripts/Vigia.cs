using UnityEngine;

// A estatua so anda quando esta fora do campo de visao do jogador
// ou escondida atras de alguma parede.
[RequireComponent(typeof(CharacterController))]
public class Vigia : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform alvo;
    [SerializeField] private Transform cabecaDoJogador;

    [Header("Perseguicao")]
    [SerializeField] private float velocidade = 1.3f;
    [SerializeField] private float distanciaDeToque = 1.3f;
    [SerializeField] private float gravidade = -12f;

    [Header("Campo de visao do jogador")]
    [SerializeField] private float anguloDeVisao = 65f;
    [SerializeField] private LayerMask camadasQueBloqueiam = 1;

    private CharacterController controlador;
    private float velocidadeVertical;

    private void Awake()
    {
        controlador = GetComponent<CharacterController>();
        if (cabecaDoJogador == null && Camera.main != null) cabecaDoJogador = Camera.main.transform;
    }

    private void Update()
    {
        if (alvo == null) return;
        if (GerenciadorJogo.Instancia != null && GerenciadorJogo.Instancia.JogoTerminou) return;

        EncararOJogador();

        Vector3 ateOJogador = alvo.position - transform.position;
        ateOJogador.y = 0f;

        Vector3 passo = Vector3.zero;
        if (!EstouSendoObservado() && ateOJogador.magnitude > distanciaDeToque) passo = ateOJogador.normalized * velocidade;

        if (controlador.isGrounded && velocidadeVertical < 0f) velocidadeVertical = -2f;
        velocidadeVertical += gravidade * Time.deltaTime;
        passo.y = velocidadeVertical;

        controlador.Move(passo * Time.deltaTime);

        Vector3 depoisDoPasso = alvo.position - transform.position;
        depoisDoPasso.y = 0f;
        if (depoisDoPasso.magnitude <= distanciaDeToque && GerenciadorJogo.Instancia != null) GerenciadorJogo.Instancia.Derrota();
    }

    private bool EstouSendoObservado()
    {
        if (cabecaDoJogador == null) return false;

        Vector3 doOlhoAteMim = transform.position - cabecaDoJogador.position;

        if (Vector3.Angle(cabecaDoJogador.forward, doOlhoAteMim) > anguloDeVisao * 0.5f) return false;

        // Dentro do cone, mas pode ter parede no meio: ai ele continua andando.
        // O proprio corpo da estatua nao conta como parede.
        RaycastHit toque;
        if (Physics.Linecast(cabecaDoJogador.position, transform.position, out toque, camadasQueBloqueiam, QueryTriggerInteraction.Ignore))
        {
            if (!toque.collider.transform.IsChildOf(transform)) return false;
        }

        return true;
    }

    private void EncararOJogador()
    {
        Vector3 direcao = alvo.position - transform.position;
        direcao.y = 0f;
        if (direcao.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(direcao);
    }
}
