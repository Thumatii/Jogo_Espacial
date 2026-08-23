using UnityEngine;
using TMPro;
using UnityEngine.UI; // Usamos o UI para controlar o Slider

public class UIManager : MonoBehaviour
{
    [Header("UI do Combustível")]
    public Slider barraCombustivel; // Agora é um Slider, não uma Image
    public TextMeshProUGUI textoPercentual; 

    [Header("Referência da Nave")]
    public Nave nave;

    void Start()
    {
        if (nave == null) nave = FindObjectOfType<Nave>();
    }

    void Update()
    {
        if (nave != null && barraCombustivel != null)
        {
            // Calcula a porcentagem (0 a 100)
            float porcentagem = nave.combustivel / 100f; 
            
            // No Slider, o valor máximo padrão é 1. Então passamos a porcentagem direto (ex: 0.75)
            barraCombustivel.value = porcentagem;

            // Atualiza o texto com o número inteiro
            if (textoPercentual != null)
                textoPercentual.text = Mathf.RoundToInt(nave.combustivel).ToString() + "%";
        }
    }
}