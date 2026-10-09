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

    [Header("Anel Visual (igual o da nave, mas só desse satélite)")]
    public int quantidadePontosAnel = 30;
    public float tamanhoPontoAnel = 0.15f;
    public Color corAnel = Color.cyan;

    private float anguloAtual;
    private float progresso = 0f; // 0 a 1 — lido pela barra de progresso no GameController
    private GameObject[] pontosAnel; // os pontinhos que desenham o círculo da órbita

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

        CriarAnelVisual();
    }

    // Desenha um círculo de pontinhos no raio da órbita — pertence só a
    // ESSE satélite (não é o mesmo objeto usado pela nave), então não tem
    // briga de "quem desenha por cima de quem" se tiver vários satélites.
    void CriarAnelVisual()
    {
        pontosAnel = new GameObject[quantidadePontosAnel];
        float raioTotal = planetaAlvo.raio + distanciaExtra;
        Vector3 centro = planetaAlvo.transform.position;

        for (int i = 0; i < quantidadePontosAnel; i++)
        {
            GameObject ponto = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(ponto.GetComponent<Collider>());
            ponto.name = "AnelSatelite_" + i;
            ponto.transform.localScale = Vector3.one * tamanhoPontoAnel;

            Renderer r = ponto.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Sprites/Default"));
            r.material.color = corAnel;
            r.sortingOrder = -10;

            float angulo = (i / (float)quantidadePontosAnel) * 360f * Mathf.Deg2Rad;
            float x = centro.x + Mathf.Cos(angulo) * raioTotal;
            float y = centro.y + Mathf.Sin(angulo) * raioTotal;
            ponto.transform.position = new Vector3(x, y, 0f);

            pontosAnel[i] = ponto;
        }
    }

    // Some JUNTO com o satélite — não antes, não depois. Era a falta
    // disso que provavelmente fazia o anel desaparecer sozinho sem motivo.
    void OnDestroy()
    {
        if (pontosAnel == null) return;
        foreach (GameObject p in pontosAnel)
        {
            if (p != null) Destroy(p);
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