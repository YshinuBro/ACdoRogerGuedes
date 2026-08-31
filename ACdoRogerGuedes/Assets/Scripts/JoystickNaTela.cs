using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Joystick virtual desenhado na tela, para jogar sem gamepad.
//
// Metade esquerda da tela: arrastar o dedo anda. O circulo aparece onde o dedo
// encostou, entao nao ha posicao fixa para acertar.
// Metade direita: toque interage, que e o que a InteracaoReticula ja fazia.
//
// A interface e Screen Space Overlay, o que so vale porque esta build nao e
// estereo. Se o Cardboard voltar a funcionar, este script precisa sair: em
// estereo o Overlay nao renderiza direito.
public class JoystickNaTela : MonoBehaviour
{
    public static JoystickNaTela Instancia;

    [SerializeField] private float raioEmPixels = 140f;
    [SerializeField] private float zonaMorta = 0.15f;

    private RectTransform baseDoJoystick;
    private RectTransform manopla;
    private Vector2 centro;
    private int dedo = -1;

    // Direcao normalizada, no mesmo formato do stick do gamepad.
    public Vector2 Direcao { get; private set; }

    private void Awake()
    {
        Instancia = this;
        Montar();
        Esconder();
    }

    // O sprite embutido da Unity nem sempre carrega em build, e sem ele o
    // joystick vira um quadrado. Desenhar o circulo na mao nao depende de nada.
    private static Sprite CriarCirculo(int lado = 128)
    {
        var tex = new Texture2D(lado, lado, TextureFormat.RGBA32, false);
        float r = lado * 0.5f;
        for (int y = 0; y < lado; y++)
        {
            for (int x = 0; x < lado; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                // borda suave, para nao ficar serrilhado
                float a = Mathf.Clamp01((r - d) / 2f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        return Sprite.Create(tex, new Rect(0, 0, lado, lado), new Vector2(0.5f, 0.5f));
    }

    private void Montar()
    {
        var sprite = CriarCirculo();

        var goCanvas = new GameObject("CanvasDoJoystick", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        goCanvas.transform.SetParent(transform, false);
        var canvas = goCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = goCanvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

        baseDoJoystick = NovaImagem("Base", goCanvas.transform, sprite, new Color(1f, 1f, 1f, 0.22f), raioEmPixels * 2f);
        manopla = NovaImagem("Manopla", baseDoJoystick, sprite, new Color(1f, 1f, 1f, 0.55f), raioEmPixels * 0.9f);
    }

    private static RectTransform NovaImagem(string nome, Transform pai, Sprite sprite, Color cor, float tamanho)
    {
        var go = new GameObject(nome, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(pai, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = cor;
        img.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(tamanho, tamanho);
        return rt;
    }

    private void Esconder()
    {
        if (baseDoJoystick != null) baseDoJoystick.gameObject.SetActive(false);
        Direcao = Vector2.zero;
        dedo = -1;
    }

    private void Update()
    {
        var tela = Touchscreen.current;
        if (tela == null) { Direcao = Vector2.zero; return; }

        // ainda seguindo o mesmo dedo?
        if (dedo >= 0)
        {
            foreach (var t in tela.touches)
            {
                if (t.touchId.ReadValue() != dedo) continue;

                var fase = t.phase.ReadValue();
                if (fase == UnityEngine.InputSystem.TouchPhase.Ended ||
                    fase == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    Esconder();
                    return;
                }

                Vector2 pos = t.position.ReadValue();
                Vector2 delta = pos - centro;
                float d = delta.magnitude;

                manopla.anchoredPosition = d > raioEmPixels ? delta.normalized * raioEmPixels : delta;

                Vector2 bruto = delta / raioEmPixels;
                Direcao = bruto.magnitude < zonaMorta ? Vector2.zero
                        : Vector2.ClampMagnitude(bruto, 1f);
                return;
            }

            // o dedo sumiu da lista
            Esconder();
            return;
        }

        // procura um dedo novo comecando na metade esquerda
        foreach (var t in tela.touches)
        {
            if (t.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.Began) continue;

            Vector2 pos = t.position.ReadValue();
            if (pos.x > Screen.width * 0.5f) continue;   // metade direita e para interagir

            dedo = t.touchId.ReadValue();
            centro = pos;
            baseDoJoystick.gameObject.SetActive(true);
            baseDoJoystick.position = pos;
            manopla.anchoredPosition = Vector2.zero;
            Direcao = Vector2.zero;
            return;
        }

        Direcao = Vector2.zero;
    }

    // A metade direita e livre para interagir.
    public static bool ToqueDeInteracao(Vector2 posicao)
    {
        return posicao.x > Screen.width * 0.5f;
    }
}
