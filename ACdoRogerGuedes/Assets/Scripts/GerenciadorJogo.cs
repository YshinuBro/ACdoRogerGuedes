using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Cerebro da fase: conta reliquias, decide vitoria/derrota e escreve na HUD.
// Acesso sempre por GerenciadorJogo.Instancia, com checagem de null.
public class GerenciadorJogo : MonoBehaviour
{
    public static GerenciadorJogo Instancia;

    [Header("Objetivo")]
    [SerializeField] private int totalDeReliquias = 5;

    [Header("Interface World Space")]
    [SerializeField] private Text textoContador;
    [SerializeField] private Text textoMensagem;

    [Header("Tela de fim")]
    [SerializeField] private GameObject painelDeFim;
    [SerializeField] private Image fundoDeFim;
    [SerializeField] private Text tituloDeFim;
    [SerializeField] private Text subtituloDeFim;
    [SerializeField] private float duracaoDoEscurecimento = 1.2f;

    [Header("Tempos")]
    [SerializeField] private float duracaoDaMensagem = 3f;
    [SerializeField] private float esperaAposDerrota = 5f;
    [SerializeField] private float esperaAposVitoria = 6f;

    [Header("Cenas")]
    [SerializeField] private string cenaDoMenu = "00_Menu";

    private int coletadas;
    private float tempoDaMensagem;
    private float tempoDaTroca;
    private float tempoDeFim;
    private bool trocandoDeCena;
    private bool voltarParaOMenu;

    public int Coletadas { get { return coletadas; } }
    public bool TodasColetadas { get { return coletadas >= totalDeReliquias; } }
    public bool JogoTerminou { get { return trocandoDeCena; } }
    public int Faltando { get { return Mathf.Max(0, totalDeReliquias - coletadas); } }

    private void Awake()
    {
        Instancia = this;
    }

    private void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    private void Start()
    {
        if (painelDeFim != null) painelDeFim.SetActive(false);
        AtualizarContador();
        MostrarMensagem("Colete as " + totalDeReliquias + " relíquias e fuja pela porta.");
    }

    private void Update()
    {
        if (tempoDaMensagem > 0f)
        {
            tempoDaMensagem -= Time.deltaTime;
            if (tempoDaMensagem <= 0f && textoMensagem != null) textoMensagem.text = "";
        }

        if (!trocandoDeCena) return;

        // escurece a tela por cima de tudo, para o fim ter peso
        tempoDeFim += Time.deltaTime;
        if (fundoDeFim != null)
        {
            float a = Mathf.Clamp01(tempoDeFim / Mathf.Max(0.01f, duracaoDoEscurecimento));
            Color c = fundoDeFim.color;
            fundoDeFim.color = new Color(c.r, c.g, c.b, a);
        }

        tempoDaTroca -= Time.deltaTime;
        if (tempoDaTroca <= 0f)
        {
            trocandoDeCena = false;
            if (voltarParaOMenu) SceneManager.LoadScene(cenaDoMenu);
            else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void ColetarReliquia(string nome)
    {
        if (trocandoDeCena) return;

        coletadas++;
        AtualizarContador();

        if (TodasColetadas) MostrarMensagem("Todas as relíquias! A saída está liberada.");
        else MostrarMensagem(nome + " coletada. Faltam " + Faltando + ".");
    }

    public void Vitoria()
    {
        Terminar(
            "VOCÊ ESCAPOU",
            "As " + totalDeReliquias + " relíquias saíram com você.\nO Vigia voltou a ser só uma estátua.",
            new Color(1f, 0.85f, 0.4f),
            true,
            esperaAposVitoria);
    }

    public void Derrota()
    {
        Terminar(
            "O VIGIA TE ALCANÇOU",
            "Ele só se move quando ninguém está olhando.\nDesta vez, você olhou para o lado.",
            new Color(0.9f, 0.3f, 0.25f),
            false,
            esperaAposDerrota);
    }

    // Fim de jogo: congela o jogador, escurece a tela e mostra o desfecho.
    private void Terminar(string titulo, string subtitulo, Color cor, bool venceu, float espera)
    {
        if (trocandoDeCena) return;

        trocandoDeCena = true;   // MovimentoPlayer e Vigia param de agir a partir daqui
        voltarParaOMenu = venceu;
        tempoDaTroca = espera;
        tempoDeFim = 0f;

        if (textoMensagem != null) textoMensagem.text = "";
        if (textoContador != null) textoContador.text = "";

        if (painelDeFim != null) painelDeFim.SetActive(true);
        if (fundoDeFim != null) fundoDeFim.color = new Color(0f, 0f, 0f, 0f);
        if (tituloDeFim != null) { tituloDeFim.text = titulo; tituloDeFim.color = cor; }
        if (subtituloDeFim != null) subtituloDeFim.text = subtitulo;
    }

    public void MostrarMensagem(string texto)
    {
        MostrarMensagem(texto, duracaoDaMensagem);
    }

    public void MostrarMensagem(string texto, float duracao)
    {
        if (trocandoDeCena) return;
        if (textoMensagem != null) textoMensagem.text = texto;
        tempoDaMensagem = duracao;
    }

    private void AtualizarContador()
    {
        if (textoContador != null) textoContador.text = "Relíquias: " + coletadas + " / " + totalDeReliquias;
    }
}
