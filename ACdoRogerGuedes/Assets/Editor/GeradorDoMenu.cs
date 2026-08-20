using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Monta a tela de entrada: titulo, integrantes, instrucoes e o cubo do botao iniciar.
// Tudo em Canvas World Space, porque Screen Space nao renderiza em estereo.
public static class GeradorDoMenu
{
    private const string RAIZ = "MenuGerado";
    private const string LAYER_INTERATIVA = "Interactive";

    // >>> TROQUE OS NOMES AQUI ANTES DE ENTREGAR <<<
    private const string NOME_DO_JOGO = "O VIGIA";
    private const string SUBTITULO = "Uma noite na galeria";
    private const string INTEGRANTES = "Integrantes: Nome do Aluno 1  -  Nome do Aluno 2";

    private const string INSTRUCOES =
        "Olhe ao redor movendo a cabeça.\n" +
        "Stick esquerdo do controle: andar.\n" +
        "Mire com a retícula no centro da visão.\n" +
        "Botão A ou gatilho direito: interagir.\n" +
        "\n" +
        "Colete as 5 relíquias e fuja pela porta.\n" +
        "O Vigia só se move quando você não está olhando para ele.";

    // ---------------------------------------------------------------- menus

    [MenuItem("O Vigia/3. Criar cena do menu (00_Menu)", false, 22)]
    public static void CriarCenaDoMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Scene cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Gerar();

        Directory.CreateDirectory(UtilGerador.PASTA_CENAS);
        EditorSceneManager.SaveScene(cena, UtilGerador.CENA_MENU);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        UtilGerador.RegistrarCenasNoBuild();
        Debug.Log("00_Menu criada em " + UtilGerador.CENA_MENU);
    }

    [MenuItem("O Vigia/4. Gerar menu (na cena aberta)", false, 23)]
    public static void Gerar()
    {
        int layerInterativa = UtilGerador.GarantirLayer(LAYER_INTERATIVA);

        GameObject antigo = GameObject.Find(RAIZ);
        if (antigo != null) Object.DestroyImmediate(antigo);

        GameObject raiz = new GameObject(RAIZ);

        Ambiente();
        Cenario(raiz.transform);
        Jogador(raiz.transform);
        Painel(raiz.transform);
        Botao(raiz.transform, layerInterativa);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Menu gerado.");
    }

    [MenuItem("O Vigia/5. Registrar cenas no Build Settings", false, 40)]
    public static void RegistrarCenas()
    {
        UtilGerador.RegistrarCenasNoBuild();
    }

    // ---------------------------------------------------------------- partes

    private static void Ambiente()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.12f, 0.12f, 0.16f);
        RenderSettings.skybox = null;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.05f;
        RenderSettings.fogColor = new Color(0.02f, 0.02f, 0.03f);

    }

    private static void Cenario(Transform raiz)
    {
        Material piso = UtilGerador.Fosco("MatPiso", new Color(0.16f, 0.15f, 0.14f));
        UtilGerador.Cubo("Chao", raiz, new Vector3(0f, -0.1f, 2f), new Vector3(16f, 0.2f, 16f), piso);

        UtilGerador.Ponto("LuzDoTitulo", raiz, new Vector3(0f, 3f, 3f), new Color(1f, 0.9f, 0.7f), 10f, 2.5f);
    }

    private static void Jogador(Transform raiz)
    {
        // Sem CharacterController e sem MovimentoPlayer: no menu ninguem anda.
        GameObject player = UtilGerador.Vazio("Player", raiz, new Vector3(0f, 0.95f, 0f));
        GeradorDaGaleria.CriarCabecaVR(player.transform, false, null);
    }

    private static void Painel(Transform raiz)
    {
        Canvas painel = UtilGerador.Painel("PainelDoMenu", raiz, new Vector3(0f, 2.3f, 4f), new Vector2(1800f, 1200f), 0.0022f);

        UtilGerador.Texto("Titulo", painel.transform, new Vector2(0f, 470f), new Vector2(1700f, 220f), NOME_DO_JOGO, 130, new Color(1f, 0.82f, 0.35f));
        UtilGerador.Texto("Subtitulo", painel.transform, new Vector2(0f, 330f), new Vector2(1700f, 100f), SUBTITULO, 56, new Color(0.8f, 0.8f, 0.85f));
        UtilGerador.Texto("Integrantes", painel.transform, new Vector2(0f, 190f), new Vector2(1700f, 100f), INTEGRANTES, 48, new Color(0.75f, 0.75f, 0.8f));
        UtilGerador.Texto("Instrucoes", painel.transform, new Vector2(0f, -180f), new Vector2(1700f, 600f), INSTRUCOES, 46, Color.white);
    }

    private static void Botao(Transform raiz, int layerInterativa)
    {
        Material verde = UtilGerador.Fosco("MatBotao", new Color(0.15f, 0.45f, 0.2f), new Color(0.15f, 0.6f, 0.25f));

        GameObject botao = UtilGerador.Cubo("BotaoIniciar", raiz, new Vector3(0f, 1f, 3.6f), new Vector3(1.8f, 0.6f, 0.2f), verde);
        BotaoIniciar script = botao.AddComponent<BotaoIniciar>();
        UtilGerador.CampoTexto(script, "cenaDaFase", "01_Fase");
        UtilGerador.AplicarLayerEmTudo(botao, layerInterativa);

        // O texto fica num canvas separado para nao herdar a escala achatada do cubo.
        Canvas rotulo = UtilGerador.Painel("RotuloDoBotao", raiz, new Vector3(0f, 1f, 3.48f), new Vector2(900f, 300f), 0.0018f);
        UtilGerador.Texto("Texto", rotulo.transform, Vector2.zero, new Vector2(880f, 280f), "INICIAR", 90, Color.white);

        UtilGerador.Ponto("LuzDoBotao", raiz, new Vector3(0f, 1.4f, 3f), new Color(0.6f, 1f, 0.7f), 4f, 1.5f);
    }
}
