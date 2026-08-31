using UnityEngine;

// Monta a visao estereo do Cardboard: duas cameras, cada uma ocupando metade
// da tela, afastadas pela distancia entre os olhos.
//
// Nao usa o plugin do Google. Ele nao funciona no Unity 6: quebra com SIGABRT
// dentro de CardboardLensDistortion_create, em codigo nativo, porque nao e
// atualizado desde 2023 e e anterior a reestruturacao do player Android. O
// historico de commits registra as tres tentativas de conserto.
//
// O que se perde sem o plugin e a distorcao de barril, que compensa a curvatura
// das lentes. A imagem fica levemente esticada nas bordas quando vista pelo
// visor. O estereo em si e real: cada olho ve de um ponto diferente, e a
// sensacao de profundidade aparece.
//
// A camera original vira a "cabeca": ela para de renderizar, mas continua
// carregando a HUD, a reticula, o giroscopio e o raycast de mira. Os dois olhos
// nascem como filhos dela, entao herdam a rotacao sem precisar de codigo extra.
public class ControleCardboard : MonoBehaviour
{
    [Header("Estereo")]
    [SerializeField] private bool dividirTela = true;

    [Tooltip("Distancia entre os olhos, em metros. 0.064 e a media humana.")]
    [SerializeField] private float distanciaEntreOlhos = 0.064f;

    [Tooltip("Barra preta no meio, separando os dois olhos.")]
    [SerializeField] private float folgaCentral = 0.002f;

    private void Start()
    {
        // A tela nao pode apagar no meio de uma partida.
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        if (dividirTela) MontarEstereo();
    }

    private void MontarEstereo()
    {
        Camera cabeca = Camera.main;
        if (cabeca == null)
        {
            Debug.LogWarning("Sem camera principal: estereo nao montado.");
            return;
        }

        // Ja montado? Acontece se a cena recarregar.
        if (cabeca.transform.Find("OlhoEsquerdo") != null) return;

        float meia = distanciaEntreOlhos * 0.5f;

        CriarOlho(cabeca, "OlhoEsquerdo", -meia, new Rect(0f, 0f, 0.5f - folgaCentral, 1f));
        CriarOlho(cabeca, "OlhoDireito", meia, new Rect(0.5f + folgaCentral, 0f, 0.5f - folgaCentral, 1f));

        // A cabeca para de desenhar, mas segue existindo: e ela que tem a HUD,
        // a reticula, o giroscopio e o raycast da mira.
        cabeca.enabled = false;
    }

    private void CriarOlho(Camera cabeca, string nome, float deslocamentoX, Rect area)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(cabeca.transform, false);
        go.transform.localPosition = new Vector3(deslocamentoX, 0f, 0f);
        go.transform.localRotation = Quaternion.identity;

        var olho = go.AddComponent<Camera>();
        olho.CopyFrom(cabeca);          // herda fog, clear flags, clipping, culling
        olho.rect = area;
        olho.enabled = true;

        // Campo de visao alto: o visor fica perto do rosto.
        olho.fieldOfView = 60f;

        // Só a cabeca guarda o AudioListener; dois causariam aviso da Unity.
        var ouvido = go.GetComponent<AudioListener>();
        if (ouvido != null) Destroy(ouvido);
    }
}
