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

    [Header("Tempos")]
    [SerializeField] private float duracaoDaMensagem = 3f;
    [SerializeField] private float esperaAposDerrota = 4f;
    [SerializeField] private float esperaAposVitoria = 5f;

    [Header("Cenas")]
    [SerializeField] private string cenaDoMenu = "00_Menu";

    private int coletadas;
    private float tempoDaMensagem;
    private float tempoDaTroca;
    private bool trocandoDeCena;
    private bool voltarParaOMenu;

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

        if (trocandoDeCena)
        {
            tempoDaTroca -= Time.deltaTime;
            if (tempoDaTroca <= 0f)
            {
                trocandoDeCena = false;
                if (voltarParaOMenu) SceneManager.LoadScene(cenaDoMenu);
                else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
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
        if (trocandoDeCena) return;

        MostrarMensagem("VOCÊ ESCAPOU DA GALERIA!", esperaAposVitoria);
        voltarParaOMenu = true;
        trocandoDeCena = true;
        tempoDaTroca = esperaAposVitoria;
    }

    public void Derrota()
    {
        if (trocandoDeCena) return;

        MostrarMensagem("O VIGIA TE ALCANÇOU...", esperaAposDerrota);
        voltarParaOMenu = false;
        trocandoDeCena = true;
        tempoDaTroca = esperaAposDerrota;
    }

    public void MostrarMensagem(string texto)
    {
        MostrarMensagem(texto, duracaoDaMensagem);
    }

    public void MostrarMensagem(string texto, float duracao)
    {
        if (textoMensagem != null) textoMensagem.text = texto;
        tempoDaMensagem = duracao;
    }

    private void AtualizarContador()
    {
        if (textoContador != null) textoContador.text = "Relíquias: " + coletadas + " / " + totalDeReliquias;
    }
}
