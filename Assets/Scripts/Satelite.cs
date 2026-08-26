using UnityEngine;

public class Satellite : MonoBehaviour
{
    [Header("Configurações")]
    public Planet planetaAlvo;
    public float tempoDeVida = 60f; // 1 minuto
    public float velocidadeOrbita = 30f; // graus por segundo
    public float distanciaExtra = 3f; // distância além do raio do planeta

    private float anguloAtual;
    private float tempoRestante;

    void Start()
    {
        if (planetaAlvo == null)
        {
            Debug.LogError("Satélite sem planeta alvo! Destruindo.");
            Destroy(gameObject);
            return;
        }

        // Começa em um ângulo aleatório
        anguloAtual = Random.Range(0f, 360f);
        tempoRestante = tempoDeVida;
    }

    void Update()
    {
        if (planetaAlvo == null)
        {
            Destroy(gameObject);
            return;
        }

        // Conta o tempo
        tempoRestante -= Time.deltaTime;
        if (tempoRestante <= 0f)
        {
            // Marca o planeta como explorado
            planetaAlvo.explorado = true;
            Destroy(gameObject);
            return;
        }

        // Orbita o planeta
        anguloAtual += velocidadeOrbita * Time.deltaTime;
        Vector2 pos = (Vector2)planetaAlvo.transform.position + 
                     new Vector2(Mathf.Cos(anguloAtual * Mathf.Deg2Rad), Mathf.Sin(anguloAtual * Mathf.Deg2Rad)) * 
                     (planetaAlvo.raio + distanciaExtra);
        transform.position = pos;
    }
}