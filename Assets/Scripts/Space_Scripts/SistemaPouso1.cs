using UnityEngine;
using UnityEngine.UI;

public class SistemaPouso : MonoBehaviour
{
    public Slider barraPouso; 
    public float velocidadeControle = 1.5f; // Velocidade que a barra mexe quando você aperta a seta
    public float zonaSegura = 0.15f;
    public float tempoParaPousar = 3f;

    private float valorAtual = 0f;
    private float tempoNoCentro = 0f;
    private bool minigameAtivo = false;

    public void IniciarPouso(Planet planeta)
    {
        minigameAtivo = true;
        tempoNoCentro = 0f;
        barraPouso.gameObject.SetActive(true);
        barraPouso.value = 0.5f; // Começa no meio
        valorAtual = 0f;
    }

    void Update()
    {
        if (!minigameAtivo) return;

        // CONTROLE MANUAL: O jogador usa as setas para mover
        float entrada = Input.GetAxisRaw("Horizontal"); // Retorna -1 (Esquerda), 0 (Nada) ou 1 (Direita)
        
        // Move a barra baseado na tecla pressionada
        valorAtual += entrada * velocidadeControle * Time.deltaTime;

        // Limita a barra para não sair da tela (entre -1 e 1)
        valorAtual = Mathf.Clamp(valorAtual, -1f, 1f);

        // Converte para o valor do Slider (0 a 1)
        float valorSlider = (valorAtual + 1f) / 2f; 
        barraPouso.value = valorSlider;

        // Verifica se está no centro
        if (Mathf.Abs(valorAtual) < zonaSegura)
        {
            tempoNoCentro += Time.deltaTime;
            if (tempoNoCentro >= tempoParaPousar)
            {
                minigameAtivo = false;
                FindObjectOfType<GameController>().PousoBemSucedido();
            }
        }
        else
        {
            tempoNoCentro = 0f; // Se sair do centro, zera o progresso
        }
    }
}