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
    // Velocidade por numero de reliquias ja coletadas. O jogador anda a 2.2, e
    // a estatua e sempre mais rapida de proposito: com um perseguidor mais lento
    // da para so correr e nunca olhar para tras, e a mecanica vira enfeite.
    //
    // A curva nao e linear. Ela abre folga no comeco, dobra a velocidade do
    // jogador na terceira reliquia e fica assustadora na quinta, que e quando
    // so falta atravessar a galeria ate a porta.
    [Tooltip("Indice = reliquias coletadas. Jogador anda a 2.2 m/s.")]
    [SerializeField] private float[] velocidadePorReliquia =
    {
        2.6f,   // 0 coletadas  +18%
        3.2f,   // 1            +45%
        3.8f,   // 2            +73%
        4.4f,   // 3            dobro do jogador
        5.6f,   // 4            +155%
        7.2f    // 5            mais de tres vezes
    };

    [SerializeField] private float distanciaDeToque = 1.3f;
    [SerializeField] private float gravidade = -12f;

    [Header("Campo de visao do jogador")]
    [SerializeField] private float anguloDeVisao = 65f;
    [SerializeField] private LayerMask camadasQueBloqueiam = 1;

    [Header("Rota de contorno")]
    // Encostados na parede de proposito: o meio do corredor e onde estao os
    // pedestais, e um canto dentro de um pedestal trava a estatua ali.
    //
    // O z de 6.45 e apertado por motivo: o Pedestal3 vai ate z=5.9 e a face
    // interna da parede esta em z=7.0. Com raio de corpo 0.35, so passa entre
    // 6.25 e 6.65. Em z=6 ela raspava no pedestal a cada volta.
    [Tooltip("Os cantos do corredor em anel, na ordem em que se ligam.")]
    [SerializeField] private Vector3[] rota =
    {
        new Vector3(-9f, 0f, -6.45f),
        new Vector3(-9f, 0f,  6.45f),
        new Vector3( 9f, 0f,  6.45f),
        new Vector3( 9f, 0f, -6.45f)
    };

    [Tooltip("Distancia para considerar que chegou num canto.")]
    [SerializeField] private float raioDoCanto = 1.2f;

    [Tooltip("Altura em que a linha de visao e testada. Acima dos pedestais.")]
    [SerializeField] private float alturaDaLinha = 1.2f;

    [Header("Destravamento")]
    [Tooltip("Quanto tempo empurrando sem sair do lugar antes de tentar desviar.")]
    [SerializeField] private float tempoParaDestravar = 0.3f;
    [Tooltip("Quanto tempo anda de lado ao desviar.")]
    [SerializeField] private float duracaoDoDesvio = 1.0f;

    private CharacterController controlador;
    private float velocidadeVertical;
    private int cantoAtual = -1;

    private Vector3 posicaoAnterior;
    private float tempoTravado;
    private float tempoDesviando;
    private float ladoDoDesvio = 1f;

    private int sentido = 1;          // +1 ou -1: para que lado esta dando a volta
    private int cantoDoJogador = -1;  // ultimo canto de referencia do jogador

    // Acelera conforme o jogador coleta: quanto menos falta para escapar,
    // mais perto ela chega.
    private float VelocidadeAtual
    {
        get
        {
            if (velocidadePorReliquia == null || velocidadePorReliquia.Length == 0) return 2.6f;
            int coletadas = GerenciadorJogo.Instancia != null ? GerenciadorJogo.Instancia.Coletadas : 0;
            int i = Mathf.Clamp(coletadas, 0, velocidadePorReliquia.Length - 1);
            return velocidadePorReliquia[i];
        }
    }

    private void Awake()
    {
        controlador = GetComponent<CharacterController>();
        if (cabecaDoJogador == null && Camera.main != null) cabecaDoJogador = Camera.main.transform;
        posicaoAnterior = transform.position;
    }

    private void Update()
    {
        if (alvo == null) return;
        if (GerenciadorJogo.Instancia != null && GerenciadorJogo.Instancia.JogoTerminou) return;

        Vector3 ateOJogador = Achatar(alvo.position - transform.position);
        Vector3 passo = Vector3.zero;
        bool queriaAndar = false;

        if (!EstouSendoObservado() && ateOJogador.magnitude > distanciaDeToque)
        {
            Vector3 direcao = ParaOndeIr();

            // Encravou em quina de pedestal: anda de lado por um instante ate soltar.
            if (tempoDesviando > 0f)
            {
                tempoDesviando -= Time.deltaTime;
                direcao = Vector3.Cross(Vector3.up, direcao.normalized) * ladoDoDesvio;
            }

            if (direcao.sqrMagnitude > 0.001f)
            {
                queriaAndar = true;
                passo = direcao.normalized * VelocidadeAtual;
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

        VerificarTravamento(queriaAndar);

        Vector3 depois = Achatar(alvo.position - transform.position);
        if (depois.magnitude <= distanciaDeToque && GerenciadorJogo.Instancia != null)
        {
            GerenciadorJogo.Instancia.Derrota();
        }
    }

    // Compara o quanto ela queria andar com o quanto andou de fato. Empurrar
    // uma quina sem sair do lugar dispara um desvio lateral, alternando o lado
    // a cada tentativa para nao insistir sempre no mesmo.
    private void VerificarTravamento(bool queriaAndar)
    {
        float andou = Achatar(transform.position - posicaoAnterior).magnitude;
        posicaoAnterior = transform.position;

        if (!queriaAndar || tempoDesviando > 0f)
        {
            tempoTravado = 0f;
            return;
        }

        float esperado = VelocidadeAtual * Time.deltaTime * 0.4f;
        if (andou < esperado) tempoTravado += Time.deltaTime;
        else tempoTravado = 0f;

        if (tempoTravado >= tempoParaDestravar)
        {
            tempoTravado = 0f;
            tempoDesviando = duracaoDoDesvio;
            ladoDoDesvio = -ladoDoDesvio;
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
    //
    // O sentido da volta so e reescolhido quando o jogador muda de canto. Sem
    // isso, um passo do jogador dentro do mesmo trecho podia inverter o sentido
    // no meio de uma curva, e a estatua ficava indo e voltando na quina.
    private int VizinhoNaDirecaoDoJogador(int origem)
    {
        int dele = CantoMaisProximo(alvo.position);
        int n = rota.Length;

        if (origem == dele) return dele;

        if (dele != cantoDoJogador)
        {
            int horario = (dele - origem + n) % n;
            int antiHorario = (origem - dele + n) % n;
            sentido = horario <= antiHorario ? 1 : -1;
            cantoDoJogador = dele;
        }

        return ((origem + sentido) % n + n) % n;
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
