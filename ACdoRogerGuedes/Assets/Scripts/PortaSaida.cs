using UnityEngine;

// Saida da galeria. So abre com todas as reliquias na mao.
public class PortaSaida : MonoBehaviour, IInteragivel
{
    public void Interagir()
    {
        if (GerenciadorJogo.Instancia == null) return;

        if (GerenciadorJogo.Instancia.TodasColetadas)
        {
            GerenciadorJogo.Instancia.Vitoria();
        }
        else
        {
            GerenciadorJogo.Instancia.MostrarMensagem("A porta está trancada. Faltam " + GerenciadorJogo.Instancia.Faltando + " relíquias.");
        }
    }
}
