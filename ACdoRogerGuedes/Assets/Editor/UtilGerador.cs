using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Funcoes comuns aos geradores de cena. Nada aqui vai para a build.
public static class UtilGerador
{
    public const string PASTA_CENAS = "Assets/Scenes";
    public const string PASTA_MATERIAIS = "Assets/Materiais";
    public const string CENA_MENU = "Assets/Scenes/00_Menu.unity";
    public const string CENA_FASE = "Assets/Scenes/01_Fase.unity";

    // ---------- layers ----------

    public static int GarantirLayer(string nome)
    {
        Object[] ativos = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (ativos == null || ativos.Length == 0) return 0;

        SerializedObject so = new SerializedObject(ativos[0]);
        SerializedProperty layers = so.FindProperty("layers");

        for (int i = 8; i < layers.arraySize; i++)
        {
            if (layers.GetArrayElementAtIndex(i).stringValue == nome) return i;
        }

        for (int i = 8; i < layers.arraySize; i++)
        {
            if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue))
            {
                layers.GetArrayElementAtIndex(i).stringValue = nome;
                so.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                Debug.Log("Layer criada: " + nome + " (indice " + i + ")");
                return i;
            }
        }

        Debug.LogWarning("Nao sobrou espaco para criar a layer " + nome);
        return 0;
    }

    public static void AplicarLayerEmTudo(GameObject alvo, int layer)
    {
        alvo.layer = layer;
        foreach (Transform filho in alvo.transform) AplicarLayerEmTudo(filho.gameObject, layer);
    }

    // ---------- materiais ----------

    public static Material Fosco(string nome, Color cor)
    {
        return Fosco(nome, cor, Color.black);
    }

    public static Material Fosco(string nome, Color cor, Color emissao)
    {
        Directory.CreateDirectory(PASTA_MATERIAIS);
        string caminho = PASTA_MATERIAIS + "/" + nome + ".mat";

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Material material = AssetDatabase.LoadAssetAtPath<Material>(caminho);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, caminho);
        }

        material.shader = shader;
        material.SetColor("_BaseColor", cor);
        material.color = cor;
        material.SetFloat("_Smoothness", 0.1f);

        if (emissao.maxColorComponent > 0f)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emissao);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            material.DisableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    public static Material SemLuz(string nome, Color cor)
    {
        Directory.CreateDirectory(PASTA_MATERIAIS);
        string caminho = PASTA_MATERIAIS + "/" + nome + ".mat";

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");

        Material material = AssetDatabase.LoadAssetAtPath<Material>(caminho);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, caminho);
        }

        material.shader = shader;
        material.SetColor("_BaseColor", cor);
        material.color = cor;

        EditorUtility.SetDirty(material);
        return material;
    }

    // ---------- objetos ----------

    public static GameObject Vazio(string nome, Transform pai, Vector3 posicao)
    {
        GameObject go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        go.transform.localPosition = posicao;
        return go;
    }

    public static GameObject Cubo(string nome, Transform pai, Vector3 posicao, Vector3 escala, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = nome;
        go.transform.SetParent(pai, false);
        go.transform.localPosition = posicao;
        go.transform.localScale = escala;
        if (material != null) go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }

    public static GameObject Quadrado(string nome, Transform pai, Vector3 posicao, Vector3 escala, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = nome;
        go.transform.SetParent(pai, false);
        go.transform.localPosition = posicao;
        go.transform.localScale = escala;
        if (material != null) go.GetComponent<Renderer>().sharedMaterial = material;

        Collider colisor = go.GetComponent<Collider>();
        if (colisor != null) Object.DestroyImmediate(colisor);

        return go;
    }

    public static Light Ponto(string nome, Transform pai, Vector3 posicao, Color cor, float alcance, float intensidade)
    {
        GameObject go = Vazio(nome, pai, posicao);
        Light luz = go.AddComponent<Light>();
        luz.type = LightType.Point;
        luz.color = cor;
        luz.range = alcance;
        luz.intensity = intensidade;
        luz.shadows = LightShadows.None;   // o celular renderiza duas vezes: sem sombra
        return luz;
    }

    // ---------- interface world space ----------

    public static Font FonteDoSistema()
    {
        Font fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (fonte == null) fonte = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return fonte;
    }

    public static Canvas Painel(string nome, Transform pai, Vector3 posicao, Vector2 tamanhoEmPixels, float escala)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        go.transform.SetParent(pai, false);

        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;   // Screen Space nao funciona em estereo

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = tamanhoEmPixels;
        rt.localPosition = posicao;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one * escala;

        return canvas;
    }

    public static Text Texto(string nome, Transform pai, Vector2 posicao, Vector2 tamanho, string conteudo, int corpo, Color cor)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(pai, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = posicao;
        rt.sizeDelta = tamanho;
        rt.localScale = Vector3.one;

        Text texto = go.GetComponent<Text>();
        texto.font = FonteDoSistema();
        texto.text = conteudo;
        texto.fontSize = corpo;
        texto.color = cor;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.horizontalOverflow = HorizontalWrapMode.Wrap;
        texto.verticalOverflow = VerticalWrapMode.Overflow;

        return texto;
    }

    // ---------- escrita em campos [SerializeField] private ----------

    public static void Campo(Object componente, string nome, Object valor)
    {
        SerializedObject so = new SerializedObject(componente);
        SerializedProperty p = so.FindProperty(nome);
        if (p == null) { Debug.LogWarning("Campo nao encontrado: " + nome); return; }
        p.objectReferenceValue = valor;
        so.ApplyModifiedProperties();
    }

    public static void CampoInt(Object componente, string nome, int valor)
    {
        SerializedObject so = new SerializedObject(componente);
        SerializedProperty p = so.FindProperty(nome);
        if (p == null) { Debug.LogWarning("Campo nao encontrado: " + nome); return; }
        p.intValue = valor;
        so.ApplyModifiedProperties();
    }

    public static void CampoTexto(Object componente, string nome, string valor)
    {
        SerializedObject so = new SerializedObject(componente);
        SerializedProperty p = so.FindProperty(nome);
        if (p == null) { Debug.LogWarning("Campo nao encontrado: " + nome); return; }
        p.stringValue = valor;
        so.ApplyModifiedProperties();
    }

    // ---------- build ----------

    public static void RegistrarCenasNoBuild()
    {
        bool temMenu = File.Exists(CENA_MENU);
        bool temFase = File.Exists(CENA_FASE);

        if (!temMenu || !temFase)
        {
            Debug.Log("Build Settings ainda incompleto: gere as duas cenas para ordenar os indices.");
            return;
        }

        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(CENA_MENU, true),
            new EditorBuildSettingsScene(CENA_FASE, true)
        };

        Debug.Log("Build Settings: 00_Menu (indice 0) e 01_Fase (indice 1).");
    }

    // ---------------------------------------------------------------- cabeca VR
    // Veio do antigo GeradorDaGaleria, que foi substituido pela cena montada
    // com os modelos do Blender. O gerador do menu ainda precisa dela.
    // A camera nunca e rotacionada por codigo: quem gira e o head tracking.
    public static Transform CriarCabecaVR(Transform pai, bool comHud, GameObject gerenciador)
    {
        GameObject go = Vazio("Camera", pai, new Vector3(0f, 0.65f, 0f));
        go.tag = "MainCamera";

        Camera camera = go.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.02f, 0.02f, 0.03f);
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 60f;
        go.AddComponent<AudioListener>();

        Material corDaReticula = SemLuz("MatReticula", Color.white);
        GameObject reticula = Quadrado("Reticula", go.transform, new Vector3(0f, 0f, 1.5f), Vector3.one * 0.022f, corDaReticula);

        InteracaoReticula interacao = go.AddComponent<InteracaoReticula>();
        Campo(interacao, "rendererDaReticula", reticula.GetComponent<Renderer>());
        CampoInt(interacao, "camadasDaMira", ~0);

        if (comHud && gerenciador != null) Hud(go.transform, gerenciador);

        return go.transform;
    }

    private static void Hud(Transform cabeca, GameObject gerenciador)
    {
        Canvas hud = Painel("HUD", cabeca, new Vector3(0f, 0f, 2f), new Vector2(1600f, 900f), 0.0015f);

        Text contador = Texto("TextoContador", hud.transform, new Vector2(0f, 380f), new Vector2(1500f, 120f), "Relíquias: 0 / 5", 60, new Color(1f, 0.9f, 0.6f));
        Text mensagem = Texto("TextoMensagem", hud.transform, new Vector2(0f, -330f), new Vector2(1500f, 300f), "", 56, Color.white);

        GerenciadorJogo cerebro = gerenciador.GetComponent<GerenciadorJogo>();
        Campo(cerebro, "textoContador", contador);
        Campo(cerebro, "textoMensagem", mensagem);
    }
}
