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

    [Header("UI do Nome da Nave")]
    public TMP_InputField inputNomeNave;
    public TextMeshProUGUI textoNomeNave;

    [Header("Referência da Nave")]
    public Nave nave;

    private string ultimoTextoDigitado = "";

    void Start()
    {
        if (nave == null) nave = FindObjectOfType<Nave>();

        // Carrega o nome salvo dos arquivos internos
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

            // ===== ACELERAÇÃO (NOVO) =====
            if (barraAceleracao != null)
            {
                // Calcula a porcentagem (0 a 100)
                float porcentagemAceleracao = (nave.velocidadeAtual / nave.velocidadeMaxima) * 100f;

                // Atualiza a barra
                barraAceleracao.value = nave.velocidadeAtual / nave.velocidadeMaxima;

                // Atualiza o texto (arredondado para número inteiro)
                if (textoAceleracao != null)
                    textoAceleracao.text = Mathf.RoundToInt(porcentagemAceleracao).ToString() + "km/h²";
            }
        }

        // Salvamento do nome da nave (nao nos arquivos do jogo), AIND
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