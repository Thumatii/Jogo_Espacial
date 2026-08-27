using UnityEngine;
using TMPro;
using UnityEngine.UI; // Necessário para Slider

public class UIManager : MonoBehaviour
{
    [Header("UI do Combustível")]
    public Slider barraCombustivel;      
    public TextMeshProUGUI textoPercentual; 

    [Header("UI da Aceleração (Novo!)")]
    public Slider barraAceleracao; // Arraste a BarraAceleracao aqui
    public TextMeshProUGUI textoAceleracao; // Arraste o texto que mostra o número

    [Header("Referência da Nave")]
    public Nave nave; 

    void Start()
    {
        if (nave == null) nave = FindObjectOfType<Nave>();
    }

    void Update()
    {
        if (nave != null)
        {
            // ===== COMBUSTÍVEL =====
            if (barraCombustivel != null)
            {
                barraCombustivel.value = nave.combustivel / 100f;
                if (textoPercentual != null)
                    textoPercentual.text = Mathf.RoundToInt(nave.combustivel).ToString() + "%";
            }

            // ===== ACELERAÇÃO (NOVO) =====
            if (barraAceleracao != null)
            {
                // Calcula a porcentagem (0 a 100)
                float porcentagemAceleracao = (nave.velocidadeAtual / nave.velocidadeMaxima) * 100f;

                // Atualiza a barra
                barraAceleracao.value = nave.velocidadeAtual / nave.velocidadeMaxima;

                // Atualiza o texto (arredondado para número inteiro)
                if (textoAceleracao != null)
                    textoAceleracao.text = Mathf.RoundToInt(porcentagemAceleracao).ToString() + "%";
            }
        }
    }
}