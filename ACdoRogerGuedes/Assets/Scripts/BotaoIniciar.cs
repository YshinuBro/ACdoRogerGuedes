using UnityEngine;
using UnityEngine.SceneManagement;

// Botao da tela de entrada. Mirar nele e apertar o gatilho comeca a fase.
public class BotaoIniciar : MonoBehaviour, IInteragivel
{
    [SerializeField] private string cenaDaFase = "01_Fase";

    private bool jaCarregou;

    public void Interagir()
    {
        if (jaCarregou) return;
        jaCarregou = true;
        SceneManager.LoadScene(cenaDaFase);
    }
}
