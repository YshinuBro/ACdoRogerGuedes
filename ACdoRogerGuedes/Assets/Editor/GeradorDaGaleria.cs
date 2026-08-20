using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Monta a fase inteira do zero: galeria em anel, reliquias, porta, player VR,
// Vigia, HUD e iluminacao. Pode rodar quantas vezes quiser: apaga o grupo antigo antes.
public static class GeradorDaGaleria
{
    private const string RAIZ = "GaleriaGerada";
    private const string LAYER_INTERATIVA = "Interactive";

    // ---------------------------------------------------------------- menus

    [MenuItem("O Vigia/1. Criar cena da fase (01_Fase)", false, 20)]
    public static void CriarCenaDaFase()
    {
        // Em modo batch nao ha ninguem para responder o dialogo de salvar.
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Scene cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Gerar();

        Directory.CreateDirectory(UtilGerador.PASTA_CENAS);
        EditorSceneManager.SaveScene(cena, UtilGerador.CENA_FASE);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        UtilGerador.RegistrarCenasNoBuild();
        Debug.Log("01_Fase criada em " + UtilGerador.CENA_FASE);
    }

    [MenuItem("O Vigia/2. Gerar cenario (na cena aberta)", false, 21)]
    public static void Gerar()
    {
        int layerInterativa = UtilGerador.GarantirLayer(LAYER_INTERATIVA);

        GameObject antigo = GameObject.Find(RAIZ);
        if (antigo != null) Object.DestroyImmediate(antigo);

        GameObject raiz = new GameObject(RAIZ);

        Ambiente();
        Estrutura(raiz.transform);
        GameObject gerenciador = Cerebro(raiz.transform);
        GameObject player = Jogador(raiz.transform, gerenciador);
        Interativos(raiz.transform, layerInterativa, gerenciador);
        Inimigo(raiz.transform, player.transform, player.transform.Find("Camera"));

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Cenario gerado. Layer '" + LAYER_INTERATIVA + "' no indice " + layerInterativa + ".");
    }

    // ---------------------------------------------------------------- partes

    private static void Ambiente()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.10f, 0.10f, 0.13f);
        RenderSettings.skybox = null;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.04f;
        RenderSettings.fogColor = new Color(0.02f, 0.02f, 0.03f);

    }

    private static void Estrutura(Transform raiz)
    {
        Material piso = UtilGerador.Fosco("MatPiso", new Color(0.16f, 0.15f, 0.14f));
        Material parede = UtilGerador.Fosco("MatParede", new Color(0.24f, 0.23f, 0.22f));
        Material bloco = UtilGerador.Fosco("MatBloco", new Color(0.20f, 0.19f, 0.20f));
        Material vitrine = UtilGerador.Fosco("MatVitrine", new Color(0.32f, 0.34f, 0.38f));

        GameObject grupo = UtilGerador.Vazio("Estrutura", raiz, Vector3.zero);

        UtilGerador.Cubo("Chao", grupo.transform, new Vector3(0f, -0.1f, 0f), new Vector3(20f, 0.2f, 14f), piso);

        UtilGerador.Cubo("ParedeNorte", grupo.transform, new Vector3(0f, 1.75f, 7.2f), new Vector3(20.4f, 3.5f, 0.4f), parede);
        UtilGerador.Cubo("ParedeSul", grupo.transform, new Vector3(0f, 1.75f, -7.2f), new Vector3(20.4f, 3.5f, 0.4f), parede);
        UtilGerador.Cubo("ParedeLeste", grupo.transform, new Vector3(10.2f, 1.75f, 0f), new Vector3(0.4f, 3.5f, 14f), parede);
        UtilGerador.Cubo("ParedeOeste", grupo.transform, new Vector3(-10.2f, 1.75f, 0f), new Vector3(0.4f, 3.5f, 14f), parede);

        // O bloco central e o que quebra a linha de visao e da espaco para o Vigia.
        UtilGerador.Cubo("BlocoCentral", grupo.transform, new Vector3(0f, 1.75f, 0f), new Vector3(12f, 3.5f, 6f), bloco);

        UtilGerador.Cubo("Vitrine1", grupo.transform, new Vector3(-7f, 1f, 2f), new Vector3(2f, 2f, 0.6f), vitrine);
        UtilGerador.Cubo("Vitrine2", grupo.transform, new Vector3(7f, 1f, -2f), new Vector3(2f, 2f, 0.6f), vitrine);
    }

    private static GameObject Cerebro(Transform raiz)
    {
        GameObject go = UtilGerador.Vazio("Gerenciador", raiz, Vector3.zero);
        go.AddComponent<GerenciadorJogo>();
        return go;
    }

    private static GameObject Jogador(Transform raiz, GameObject gerenciador)
    {
        GameObject player = UtilGerador.Vazio("Player", raiz, new Vector3(0f, 0.9f, -5f));
        player.transform.rotation = Quaternion.Euler(0f, -90f, 0f);

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.7f;
        cc.radius = 0.3f;
        cc.center = Vector3.zero;
        cc.slopeLimit = 45f;
        cc.stepOffset = 0.3f;

        Transform cabeca = CriarCabecaVR(player.transform, true, gerenciador);

        MovimentoPlayer movimento = player.AddComponent<MovimentoPlayer>();
        UtilGerador.Campo(movimento, "cabeca", cabeca);

        return player;
    }

    private static void Interativos(Transform raiz, int layerInterativa, GameObject gerenciador)
    {
        Material pedestal = UtilGerador.Fosco("MatPedestal", new Color(0.28f, 0.27f, 0.26f));
        Material ouro = UtilGerador.Fosco("MatReliquia", new Color(0.85f, 0.68f, 0.18f), new Color(1.6f, 1.15f, 0.25f));
        Material porta = UtilGerador.Fosco("MatPorta", new Color(0.35f, 0.12f, 0.10f), new Color(0.45f, 0.08f, 0.05f));

        GameObject grupo = UtilGerador.Vazio("Interativos", raiz, Vector3.zero);
        GameObject pedestais = UtilGerador.Vazio("Pedestais", raiz, Vector3.zero);

        Vector3[] pontos =
        {
            new Vector3(-8f, 1.3f, -5f),
            new Vector3(-8f, 1.3f, 0f),
            new Vector3(-4f, 1.3f, 5.5f),
            new Vector3(8f, 1.3f, 4f),
            new Vector3(8f, 1.3f, -4f)
        };

        for (int i = 0; i < pontos.Length; i++)
        {
            Vector3 p = pontos[i];

            UtilGerador.Cubo("Pedestal" + (i + 1), pedestais.transform, new Vector3(p.x, 0.5f, p.z), new Vector3(0.8f, 1f, 0.8f), pedestal);

            GameObject reliquia = UtilGerador.Cubo("Reliquia" + (i + 1), grupo.transform, p, Vector3.one * 0.3f, ouro);
            reliquia.transform.rotation = Quaternion.Euler(35f, 25f, 0f);

            Reliquia script = reliquia.AddComponent<Reliquia>();
            UtilGerador.CampoTexto(script, "nomeVisivel", "Relíquia " + (i + 1));

            // A luz e filha: quando a reliquia some, a sala escurece um pouco.
            UtilGerador.Ponto("Luz", reliquia.transform, Vector3.zero, new Color(1f, 0.85f, 0.5f), 5f, 2f);

            UtilGerador.AplicarLayerEmTudo(reliquia, layerInterativa);
        }

        GameObject saida = UtilGerador.Cubo("PortaSaida", grupo.transform, new Vector3(0f, 1.5f, -7f), new Vector3(2f, 3f, 0.3f), porta);
        saida.AddComponent<PortaSaida>();
        UtilGerador.AplicarLayerEmTudo(saida, layerInterativa);

        // Uma luz fraca marca a saida para o objetivo ficar visivel de longe.
        UtilGerador.Ponto("LuzDaSaida", grupo.transform, new Vector3(0f, 2.2f, -6.3f), new Color(1f, 0.35f, 0.25f), 7f, 1.5f);
    }

    private static void Inimigo(Transform raiz, Transform player, Transform cabeca)
    {
        Material pedra = UtilGerador.Fosco("MatVigia", new Color(0.07f, 0.07f, 0.08f));

        GameObject vigia = UtilGerador.Vazio("Vigia", raiz, new Vector3(0f, 1f, 5f));

        CharacterController cc = vigia.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.35f;
        cc.center = Vector3.zero;
        cc.stepOffset = 0.3f;

        // Corpo so visual: quem colide e o CharacterController.
        GameObject corpo = UtilGerador.Cubo("Corpo", vigia.transform, Vector3.zero, new Vector3(0.8f, 2f, 0.8f), pedra);
        Object.DestroyImmediate(corpo.GetComponent<Collider>());

        Vigia script = vigia.AddComponent<Vigia>();
        UtilGerador.Campo(script, "alvo", player);
        UtilGerador.Campo(script, "cabecaDoJogador", cabeca);
        UtilGerador.CampoInt(script, "camadasQueBloqueiam", 1);   // so a layer Default bloqueia
    }

    // ---------------------------------------------------------------- cabeca VR

    // Camera + reticula (+ HUD opcional). Usada tambem pelo gerador do menu.
    // A camera nunca e rotacionada por codigo: quem gira e o head tracking.
    public static Transform CriarCabecaVR(Transform pai, bool comHud, GameObject gerenciador)
    {
        GameObject go = UtilGerador.Vazio("Camera", pai, new Vector3(0f, 0.65f, 0f));
        go.tag = "MainCamera";

        Camera camera = go.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.02f, 0.02f, 0.03f);
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 60f;
        go.AddComponent<AudioListener>();

        Material corDaReticula = UtilGerador.SemLuz("MatReticula", Color.white);
        GameObject reticula = UtilGerador.Quadrado("Reticula", go.transform, new Vector3(0f, 0f, 1.5f), Vector3.one * 0.022f, corDaReticula);

        InteracaoReticula interacao = go.AddComponent<InteracaoReticula>();
        UtilGerador.Campo(interacao, "rendererDaReticula", reticula.GetComponent<Renderer>());
        UtilGerador.CampoInt(interacao, "camadasDaMira", ~0);

        if (comHud && gerenciador != null) Hud(go.transform, gerenciador);

        return go.transform;
    }

    private static void Hud(Transform cabeca, GameObject gerenciador)
    {
        Canvas hud = UtilGerador.Painel("HUD", cabeca, new Vector3(0f, 0f, 2f), new Vector2(1600f, 900f), 0.0015f);

        Text contador = UtilGerador.Texto("TextoContador", hud.transform, new Vector2(0f, 380f), new Vector2(1500f, 120f), "Relíquias: 0 / 5", 60, new Color(1f, 0.9f, 0.6f));
        Text mensagem = UtilGerador.Texto("TextoMensagem", hud.transform, new Vector2(0f, -330f), new Vector2(1500f, 300f), "", 56, Color.white);

        GerenciadorJogo cerebro = gerenciador.GetComponent<GerenciadorJogo>();
        UtilGerador.Campo(cerebro, "textoContador", contador);
        UtilGerador.Campo(cerebro, "textoMensagem", mensagem);
    }
}
