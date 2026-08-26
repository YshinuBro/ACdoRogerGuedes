using UnityEngine;

// A estatua so anda quando esta fora do campo de visao do jogador
// ou escondida atras de alguma parede.
//
// Para chegar ate o jogador ela usa duas estrategias. Se houver linha livre,
// vai direto. Se houver parede no meio, contorna pelo anel de corredores que
// circunda o bloco central, escolhendo o sentido mais curto. Sem isso ela
// empurrava a parede eternamente e nunca alcancava ninguem.
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

    [Header("Rota de contorno")]
    // Encostados na parede de proposito: o meio do corredor e onde estao os
    // pedestais, e um canto dentro de um pedestal trava a estatua ali.
    [Tooltip("Os cantos do corredor em anel, na ordem em que se ligam.")]
    [SerializeField] private Vector3[] rota =
    {
        new Vector3(-9f, 0f, -6f),
        new Vector3(-9f, 0f,  6f),
        new Vector3( 9f, 0f,  6f),
        new Vector3( 9f, 0f, -6f)
    };

    [Tooltip("Distancia para considerar que chegou num canto.")]
    [SerializeField] private float raioDoCanto = 1.2f;

    [Tooltip("Altura em que a linha de visao e testada. Acima dos pedestais.")]
    [SerializeField] private float alturaDaLinha = 1.2f;

    private CharacterController controlador;
    private float velocidadeVertical;
    private int cantoAtual = -1;

    private void Awake()
    {
        controlador = GetComponent<CharacterController>();
        if (cabecaDoJogador == null && Camera.main != null) cabecaDoJogador = Camera.main.transform;
    }

    private void Update()
    {
        if (alvo == null) return;
        if (GerenciadorJogo.Instancia != null && GerenciadorJogo.Instancia.JogoTerminou) return;

        Vector3 ateOJogador = Achatar(alvo.position - transform.position);
        Vector3 passo = Vector3.zero;

        if (!EstouSendoObservado() && ateOJogador.magnitude > distanciaDeToque)
        {
            Vector3 direcao = ParaOndeIr();
            if (direcao.sqrMagnitude > 0.001f)
            {
                passo = direcao.normalized * velocidade;
                Encarar(direcao);
            }
        }
        else
        {
            // parado: encara o jogador, que e mais assustador do que ficar de lado
            Encarar(ateOJogador);
        }

        if (controlador.isGrounded && velocidadeVertical < 0f) velocidadeVertical = -2f;
        velocidadeVertical += gravidade * Time.deltaTime;
        passo.y = velocidadeVertical;

        controlador.Move(passo * Time.deltaTime);

        Vector3 depois = Achatar(alvo.position - transform.position);
        if (depois.magnitude <= distanciaDeToque && GerenciadorJogo.Instancia != null)
        {
            GerenciadorJogo.Instancia.Derrota();
        }
    }

    // ---------------------------------------------------------------- rota

    private Vector3 ParaOndeIr()
    {
        // Passagem livre: vai direto. O canto comprometido NAO e descartado de
        // proposito. A passagem fica piscando entre livre e bloqueada quando ela
        // passa raspando numa quina, e recalcular a entrada no anel a cada
        // piscada fazia ela voltar pelo caminho, sem nunca chegar.
        if (PassagemLivre(transform.position, alvo.position))
        {
            return Achatar(alvo.position - transform.position);
        }

        if (rota == null || rota.Length < 2) return Achatar(alvo.position - transform.position);

        // Sem destino ainda: entra no anel pelo canto mais proximo. Mirar direto
        // num canto distante fazia ela atravessar o bloco central em linha reta.
        if (cantoAtual < 0)
        {
            int meu = CantoMaisProximo(transform.position);
            cantoAtual = Achatar(rota[meu] - transform.position).magnitude > raioDoCanto
                ? meu
                : VizinhoNaDirecaoDoJogador(meu);
        }

        // Chegou no canto atual: compromete-se com o proximo. So troca aqui,
        // senao ela fica oscilando entre dois cantos no meio do trecho.
        if (Achatar(rota[cantoAtual] - transform.position).magnitude < raioDoCanto)
        {
            cantoAtual = VizinhoNaDirecaoDoJogador(cantoAtual);
        }

        return Achatar(rota[cantoAtual] - transform.position);
    }

    // Vizinho de 'origem' no sentido que chega antes ao canto do jogador.
    private int VizinhoNaDirecaoDoJogador(int origem)
    {
        int dele = CantoMaisProximo(alvo.position);
        int n = rota.Length;

        if (origem == dele) return dele;

        int horario = (dele - origem + n) % n;
        int antiHorario = (origem - dele + n) % n;

        return horario <= antiHorario ? (origem + 1) % n : (origem - 1 + n) % n;
    }

    private int CantoMaisProximo(Vector3 ponto)
    {
        int melhor = 0;
        float menor = float.MaxValue;
        for (int i = 0; i < rota.Length; i++)
        {
            float d = Achatar(rota[i] - ponto).sqrMagnitude;
            if (d < menor) { menor = d; melhor = i; }
        }
        return melhor;
    }

    // ---------------------------------------------------------------- visao

    private bool EstouSendoObservado()
    {
        if (cabecaDoJogador == null) return false;

        Vector3 doOlhoAteMim = transform.position - cabecaDoJogador.position;
        if (Vector3.Angle(cabecaDoJogador.forward, doOlhoAteMim) > anguloDeVisao * 0.5f) return false;

        // dentro do cone, mas pode ter parede no meio
        return CaminhoLivre(cabecaDoJogador.position, transform.position);
    }

    // Para VER: linha de espessura zero, que e o que o olho faz.
    private bool CaminhoLivre(Vector3 a, Vector3 b)
    {
        a.y = alturaDaLinha;
        b.y = alturaDaLinha;

        RaycastHit toque;
        if (!Physics.Linecast(a, b, out toque, camadasQueBloqueiam, QueryTriggerInteraction.Ignore)) return true;

        return EhCorpoConhecido(toque.collider.transform);
    }

    // Para ANDAR: esfera da largura do corpo. Uma linha passa por vaos onde a
    // estatua nao cabe, e ai ela ia raspar na quina em vez de contornar.
    private bool PassagemLivre(Vector3 a, Vector3 b)
    {
        a.y = alturaDaLinha;
        b.y = alturaDaLinha;

        Vector3 delta = b - a;
        float distancia = delta.magnitude;
        if (distancia < 0.01f) return true;

        float raio = controlador != null ? controlador.radius : 0.35f;

        RaycastHit toque;
        if (!Physics.SphereCast(a, raio, delta / distancia, out toque, distancia, camadasQueBloqueiam, QueryTriggerInteraction.Ignore)) return true;

        return EhCorpoConhecido(toque.collider.transform);
    }

    // O proprio corpo da estatua e o do jogador nao contam como obstaculo.
    private bool EhCorpoConhecido(Transform t)
    {
        return t.IsChildOf(transform) || (alvo != null && t.IsChildOf(alvo));
    }

    // ---------------------------------------------------------------- util

    private static Vector3 Achatar(Vector3 v)
    {
        v.y = 0f;
        return v;
    }

    private void Encarar(Vector3 direcao)
    {
        direcao = Achatar(direcao);
        if (direcao.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(direcao);
    }

    private void OnDrawGizmosSelected()
    {
        if (rota == null || rota.Length < 2) return;
        Gizmos.color = Color.red;
        for (int i = 0; i < rota.Length; i++)
        {
            Gizmos.DrawWireSphere(rota[i], raioDoCanto);
            Gizmos.DrawLine(rota[i], rota[(i + 1) % rota.Length]);
        }
    }
}
