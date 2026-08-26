using UnityEngine;

// Puxa a paleta da fase para o vermelho conforme o tempo passa. A galeria
// comeca fria e dourada e vai ficando sangrenta, sem que nada seja dito.
//
// Cada luz caminha a partir da SUA cor original, entao a luz da saida, que ja
// nasce avermelhada, continua distinguivel das luzes das reliquias.
public class ClimaDaFase : MonoBehaviour
{
    [Header("Ritmo")]
    [Tooltip("Segundos ate a paleta chegar no vermelho total.")]
    [SerializeField] private float tempoAteOVermelho = 150f;

    [Tooltip("Quantos segundos cada reliquia coletada adianta o relogio.")]
    [SerializeField] private float adiantoPorReliquia = 12f;

    [Header("Destino da paleta")]
    [SerializeField] private Color corFinalDasLuzes = new Color(1f, 0.18f, 0.10f);
    [SerializeField] private Color fogFinal = new Color(0.09f, 0.008f, 0.008f);
    [SerializeField] private Color ambienteFinal = new Color(0.11f, 0.02f, 0.02f);

    [Tooltip("O quanto cada luz chega perto do vermelho final. 1 = totalmente.")]
    [SerializeField, Range(0f, 1f)] private float intensidadeMaxima = 0.85f;

    private Light[] luzes;
    private Color[] coresOriginais;

    private Color fogOriginal;
    private Color ambienteOriginal;

    private float tempo;

    private void Awake()
    {
        luzes = FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        coresOriginais = new Color[luzes.Length];
        for (int i = 0; i < luzes.Length; i++) coresOriginais[i] = luzes[i].color;

        fogOriginal = RenderSettings.fogColor;
        ambienteOriginal = RenderSettings.ambientLight;
    }

    // Sem isto, rodar no Editor deixaria a cena salva com a paleta do fim
    // da partida, e a proxima abriria vermelha.
    private void OnDestroy()
    {
        RenderSettings.fogColor = fogOriginal;
        RenderSettings.ambientLight = ambienteOriginal;
        if (luzes == null) return;
        for (int i = 0; i < luzes.Length; i++)
        {
            if (luzes[i] != null) luzes[i].color = coresOriginais[i];
        }
    }

    private void Update()
    {
        if (GerenciadorJogo.Instancia != null && GerenciadorJogo.Instancia.JogoTerminou) return;

        tempo += Time.deltaTime;

        float adianto = 0f;
        if (GerenciadorJogo.Instancia != null) adianto = GerenciadorJogo.Instancia.Coletadas * adiantoPorReliquia;

        float t = Mathf.Clamp01((tempo + adianto) / Mathf.Max(1f, tempoAteOVermelho));

        RenderSettings.fogColor = Color.Lerp(fogOriginal, fogFinal, t);
        RenderSettings.ambientLight = Color.Lerp(ambienteOriginal, ambienteFinal, t);

        float f = t * intensidadeMaxima;
        for (int i = 0; i < luzes.Length; i++)
        {
            if (luzes[i] == null) continue;
            luzes[i].color = Color.Lerp(coresOriginais[i], corFinalDasLuzes, f);
        }
    }
}
