using UnityEngine;
using TMPro;

public class SistemaExploracao : MonoBehaviour
{
    public TextMeshProUGUI txtRecursos;

    private int recursosColetados = 0;
    private bool ativo = false;

    public void IniciarExploracao()
    {
        ativo = true;
        recursosColetados = 0;
        AtualizarUI();
    }

    public void ColetarRecurso()
    {
        if (!ativo) return;
        recursosColetados++;
        AtualizarUI();

        if (recursosColetados >= 3)
        {
            ativo = false;
            txtRecursos.text = "Exploração concluída!";
            // (Aqui você pode adicionar um comando para voltar ao espaço automaticamente, se quiser)
        }
    }

    void AtualizarUI()
    {
        if (txtRecursos != null)
            txtRecursos.text = $"Recursos: {recursosColetados} / 3";
    }
}