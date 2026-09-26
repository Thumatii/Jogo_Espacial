using UnityEngine;

public class Satellite : MonoBehaviour
{
    [Header("Identidade (pro painel de status)")]
    public string nomeSatelite = "Sonda-01";
    public string tipoSatelite = "Scanner Básico";

    [Header("Configurações")]
    public Planet planetaAlvo;
    public float tempoDeVida = 60f;
    public float velocidadeOrbita = 12f; // mais devagar que antes (era 30)
    public float distanciaExtra = 3f;

    private float anguloAtual;
    private float progresso = 0f; // 0 a 1 — lido pela barra de progresso no GameController

    public float Progresso => progresso;

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
    }

    void Update()
    {
        if (planetaAlvo == null)
        {
            Destroy(gameObject);
            return;
        }

        progresso += Time.deltaTime / tempoDeVida;

        if (progresso >= 1f)
        {
            planetaAlvo.explorado = true; // Marca como explorado quando o scan termina
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