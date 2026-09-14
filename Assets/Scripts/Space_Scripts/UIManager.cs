
using UnityEngine;
using TMPro;
using UnityEngine.UI; // Necessário para Slider

public class UIManager : MonoBehaviour
{
    [Header("UI do Combustível")]
    public Slider barraCombustivel;
    public TextMeshProUGUI textoPercentual;

    [Header("UI da Aceleração")]
    public Slider barraAceleracao;
    public TextMeshProUGUI textoAceleracao;

    [Header("UI do Nome da Nave")]
    public TMP_InputField inputNomeNave;
    public TextMeshProUGUI textoNomeNave;

    [Header("UI de Coordenadas (Novo!)")]
    public TextMeshProUGUI textoCoordenadas; // Arraste o texto das coordenadas aqui

    [Header("Referência da Nave")]
    public Nave nave;

    private string ultimoTextoDigitado = "";

    void Start()
    {
        if (nave == null) nave = FindObjectOfType<Nave>();

        // Carrega o nome
        string nomeSalvo = PlayerPrefs.GetString("NomeDaNave", "FENG-01");

        if (textoNomeNave != null)
            textoNomeNave.text = ":// " + nomeSalvo;

        if (inputNomeNave != null)
        {
            inputNomeNave.text = nomeSalvo;
            ultimoTextoDigitado = nomeSalvo;
        }
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
                    textoPercentual.text = Mathf.RoundToInt(nave.combustivel).ToString() + "m³";
            }

            // ===== ACELERAÇÃO =====
            if (barraAceleracao != null)
            {
                float porcentagemAceleracao = (nave.velocidadeAtual / nave.velocidadeMaxima) * 100f;
                barraAceleracao.value = nave.velocidadeAtual / nave.velocidadeMaxima;

                if (textoAceleracao != null)
                    textoAceleracao.text = Mathf.RoundToInt(porcentagemAceleracao).ToString() + "km/h²";
            }

            // Coordenadas legais
            if (textoCoordenadas != null)
            {
                Vector3 pos = nave.transform.position;
                // (use :F1 para 1 casa decimal ou remova para número inteiro)
                textoCoordenadas.text = $"X: {pos.x:F1} | Y: {pos.y:F1}";
            }
        }

        // Salvamento do nome da nave
        if (inputNomeNave != null && inputNomeNave.text != ultimoTextoDigitado)
        {
            ultimoTextoDigitado = inputNomeNave.text;

            string nomeParaSalvar = string.IsNullOrEmpty(ultimoTextoDigitado) ? "FENG-01" : ultimoTextoDigitado;

            PlayerPrefs.SetString("NomeDaNave", nomeParaSalvar);
            PlayerPrefs.Save();

            if (textoNomeNave != null)
                textoNomeNave.text = ":// " + nomeParaSalvar;
        }
    }
}