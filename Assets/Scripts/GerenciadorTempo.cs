using UnityEngine;

public class GerenciadorTempo : MonoBehaviour
{
    public static GerenciadorTempo Instancia;

    [Header("Configuração")]
    [Tooltip("Quantos segundos reais equivalem a 1 hora de jogo.")]
    public float segundosPorHora = 60f;
    public int diasPorAno = 365;

    public int Ano { get; private set; } = 1;
    public int Dia { get; private set; } = 1;
    public int Hora { get; private set; } = 0;

    private float acumulador = 0f;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        DontDestroyOnLoad(gameObject);

        CarregarDoDisco();
    }

    void Update()
    {
        acumulador += Time.deltaTime;

        while (acumulador >= segundosPorHora)
        {
            acumulador -= segundosPorHora;
            AvancarHora();
        }
    }

    void AvancarHora()
    {
        Hora++;

        if (Hora >= 24)
        {
            Hora = 0;
            Dia++;

            if (Dia > diasPorAno)
            {
                Dia = 1;
                Ano++;
            }
        }

        SalvarNoDisco();
    }

    public string TextoFormatado()
    {
        return $"Ano {Ano} | Dia {Dia} | {Hora:00}h";
    }

    // ===== SAVE =====

    void SalvarNoDisco()
    {
        PlayerPrefs.SetInt("Tempo_Ano", Ano);
        PlayerPrefs.SetInt("Tempo_Dia", Dia);
        PlayerPrefs.SetInt("Tempo_Hora", Hora);
        PlayerPrefs.Save();
    }

    void CarregarDoDisco()
    {
        Ano = PlayerPrefs.GetInt("Tempo_Ano", 1);
        Dia = PlayerPrefs.GetInt("Tempo_Dia", 1);
        Hora = PlayerPrefs.GetInt("Tempo_Hora", 0);
    }

    // Usado pelo comando de debug "reset_time"
    public void ResetarTempo()
    {
        Ano = 1;
        Dia = 1;
        Hora = 0;
        acumulador = 0f;
        SalvarNoDisco();
    }
}
