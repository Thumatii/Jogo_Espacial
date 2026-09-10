using UnityEngine;

public class Satellite : MonoBehaviour
{
    [Header("Configurações")]
    public Planet planetaAlvo;
    public float tempoDeVida = 60f; 
    public float velocidadeOrbita = 30f; 
    public float distanciaExtra = 3f;

    private float anguloAtual;
    private float tempoRestante;

    void Start()
    {
        if (planetaAlvo == null)
        {
            Destroy(gameObject);
            return;
        }

        // ===== IMPEDE SATÉLITES DUPLICADOS =====
        Satellite[] satelitesExistentes = FindObjectsOfType<Satellite>();
        foreach (Satellite s in satelitesExistentes)
        {
            if (s != this && s.planetaAlvo == planetaAlvo)
            {
                Destroy(gameObject);
                return;
            }
        }
        // =======================================

        // ===== MARCA O PLANETA COMO JÁ SATELITADO =====
        planetaAlvo.sateliteLancado = true;
        // ==============================================

        // Pega a posição da NAVE
        Nave nave = FindObjectOfType<Nave>();
        if (nave != null)
        {
            Vector2 direcao = ((Vector2)nave.transform.position - (Vector2)planetaAlvo.transform.position).normalized;
            anguloAtual = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        }
        else
        {
            anguloAtual = Random.Range(0f, 360f);
        }

        tempoRestante = tempoDeVida;
    }

    void Update()
    {
        if (planetaAlvo == null)
        {
            Destroy(gameObject);
            return;
        }

        tempoRestante -= Time.deltaTime;
        if (tempoRestante <= 0f)
        {
            planetaAlvo.explorado = true; // Marca como explorado quando expira
            Destroy(gameObject);
            return;
        }

        anguloAtual += velocidadeOrbita * Time.deltaTime;
        Vector2 pos = (Vector2)planetaAlvo.transform.position + 
                     new Vector2(Mathf.Cos(anguloAtual * Mathf.Deg2Rad), Mathf.Sin(anguloAtual * Mathf.Deg2Rad)) * 
                     (planetaAlvo.raio + distanciaExtra);
        transform.position = pos;
    }
}