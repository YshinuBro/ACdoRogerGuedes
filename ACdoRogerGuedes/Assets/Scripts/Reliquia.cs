using UnityEngine;

// Item coletavel. A luz e filha deste objeto, entao some junto quando coletado.
public class Reliquia : MonoBehaviour, IInteragivel
{
    [Header("Identificacao")]
    [SerializeField] private string nomeVisivel = "Relíquia";

    [Header("Giro decorativo")]
    [SerializeField] private float velocidadeDeGiro = 40f;

    private void Update()
    {
        if (velocidadeDeGiro != 0f) transform.Rotate(Vector3.up, velocidadeDeGiro * Time.deltaTime, Space.World);
    }

    public void Interagir()
    {
        if (GerenciadorJogo.Instancia != null) GerenciadorJogo.Instancia.ColetarReliquia(nomeVisivel);
        gameObject.SetActive(false);
    }
}
