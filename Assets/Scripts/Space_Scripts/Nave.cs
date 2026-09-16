using UnityEngine;
using UnityEngine.SceneManagement;

public class Nave : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float velocidadeMaxima = 10f;
    public float aceleracao = 15f;
    public float freio = 25f;
    public float velocidadeRotacao = 10f;

    [Header("Gravidade (calculada manualmente)")]
    public float constanteGravitacional = 20f;
    public float distanciaMinima = 0.5f;

    [Header("Status da Nave")]
    public float combustivel = 100f;
    public float vida = 100f;

    [Header("Sprites da Nave (40 frames)")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] spritesNave;

    [Header("Órbita")]
    public float distanciaOrbita = 5f;
    public float velocidadeOrbita = 20f;

    public float velocidadeAtual = 0f;
    private Rigidbody2D rb;
    private Vector2 direcaoMouse;

    private bool emOrbita = false;
    private Planet planetaOrbitando;
    private float anguloOrbita = 0f;

    private bool emTransicao = false;
    private Vector3 posicaoInicial;
    private Vector3 posicaoAlvo;
    private float progressoTransicao = 0f;
    private float tempoTransicao = 1.5f;

    private float anguloSpriteAtual = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        if (DadosGlobais.jaEntrouNoEspaco)
        {
            // Se veio de um Quit, ignora a contagem de tempo (tempo congelado)
            float tempoDecorrido = DadosGlobais.carregouDoQuit ? 0f : (Time.time - DadosGlobais.tempoSaida);

            if (DadosGlobais.estaEmOrbita)
            {
                GameObject objPlaneta = GameObject.Find(DadosGlobais.nomePlanetaOrbitado);

                if (objPlaneta != null)
                {
                    planetaOrbitando = objPlaneta.GetComponent<Planet>();
                    emOrbita = true;

                    anguloOrbita = DadosGlobais.carregouDoQuit
                        ? DadosGlobais.anguloOrbitaSalvo
                        : DadosGlobais.anguloOrbitaSalvo + (velocidadeOrbita * tempoDecorrido);

                    float raioOrbita = planetaOrbitando.raio + distanciaOrbita;
                    Vector2 pos = (Vector2)planetaOrbitando.transform.position +
                        new Vector2(Mathf.Cos(anguloOrbita * Mathf.Deg2Rad), Mathf.Sin(anguloOrbita * Mathf.Deg2Rad)) * raioOrbita;

                    transform.position = new Vector3(pos.x, pos.y, 0f);
                    rb.linearVelocity = Vector2.zero;

                    OrbitaVisual orbitaVisual = planetaOrbitando.GetComponentInChildren<OrbitaVisual>();
                    if (orbitaVisual != null) orbitaVisual.Ativar(distanciaOrbita);
                }
            }
            else
            {
                Vector3 deslocamentoEmInercia = (Vector3)(DadosGlobais.velocidadeVetorSalva * tempoDecorrido);
                transform.position = DadosGlobais.posicaoSalvaDaNave + deslocamentoEmInercia;

                rb.linearVelocity = DadosGlobais.velocidadeVetorSalva;
                velocidadeAtual = DadosGlobais.velocidadeAtualSalva;
            }

            DadosGlobais.carregouDoQuit = false;
        }
    }

    void Update()
    {
        // ATUALIZAÇÃO CONTÍNUA: Garante que o Carregador da UI tenha os dados certos
        DadosGlobais.posicaoSalvaDaNave = transform.position;
        DadosGlobais.velocidadeAtualSalva = velocidadeAtual;
        if (rb != null) DadosGlobais.velocidadeVetorSalva = rb.linearVelocity;

        bool consoleAberto = DebugConsole.Instancia != null && DebugConsole.Instancia.ConsoleEstaAberto;

        if (!consoleAberto && Input.GetKeyDown(KeyCode.E))
        {
            SalvarEstadoDaNave();
            SceneManager.LoadScene("InteriorNave");
        }

        if (emOrbita)
        {
            if (emTransicao)
            {
                progressoTransicao += Time.deltaTime / tempoTransicao;
                if (progressoTransicao >= 1f)
                {
                    progressoTransicao = 1f;
                    emTransicao = false;
                }
                transform.position = Vector3.Lerp(posicaoInicial, posicaoAlvo, progressoTransicao);

                Vector2 direcaoDeslize = (posicaoAlvo - posicaoInicial).normalized;
                if (direcaoDeslize.sqrMagnitude > 0.01f)
                {
                    float anguloAlvo = Mathf.Atan2(direcaoDeslize.y, direcaoDeslize.x) * Mathf.Rad2Deg;
                    TrocarSpritePeloAngulo(anguloAlvo);
                }
                return;
            }

            anguloOrbita += velocidadeOrbita * Time.deltaTime;
            Vector2 pos = (Vector2)planetaOrbitando.transform.position + new Vector2(Mathf.Cos(anguloOrbita * Mathf.Deg2Rad), Mathf.Sin(anguloOrbita * Mathf.Deg2Rad)) * (planetaOrbitando.raio + distanciaOrbita);
            transform.position = pos;

            float anguloTangente = anguloOrbita + 90f;
            TrocarSpritePeloAngulo(anguloTangente);

            return;
        }

        Vector3 posicaoMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        posicaoMouse.z = 0f;
        direcaoMouse = (posicaoMouse - transform.position).normalized;

        if (direcaoMouse.sqrMagnitude > 0.01f)
        {
            float angulo = Mathf.Atan2(direcaoMouse.y, direcaoMouse.x) * Mathf.Rad2Deg;
            anguloSpriteAtual = Mathf.LerpAngle(anguloSpriteAtual, angulo, Time.deltaTime * velocidadeRotacao);
            TrocarSpritePeloAngulo(anguloSpriteAtual);
        }
    }

    void TrocarSpritePeloAngulo(float angulo)
    {
        if (spritesNave == null || spritesNave.Length == 0) return;

        float anguloNormalizado = Mathf.Repeat(angulo + 90f, 360f);
        int indice = Mathf.FloorToInt(anguloNormalizado / 9f) % spritesNave.Length;

        spriteRenderer.sprite = spritesNave[indice];
    }

    void FixedUpdate()
    {
        if (emOrbita || emTransicao) return;

        if (velocidadeAtual > 0.1f)
        {
            combustivel -= 0.00005f * Time.fixedDeltaTime;
        }

        if (Input.GetKey(KeyCode.W))
        {
            velocidadeAtual += aceleracao * Time.fixedDeltaTime;
            combustivel -= (aceleracao * 0.5f) * Time.fixedDeltaTime;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            velocidadeAtual -= freio * Time.fixedDeltaTime;
        }

        velocidadeAtual = Mathf.Clamp(velocidadeAtual, 0f, velocidadeMaxima);
        combustivel = Mathf.Clamp(combustivel, 0f, 100f);

        Vector2 forcaGravidadeTotal = Vector2.zero;
        Planet[] planetas = FindObjectsOfType<Planet>();

        foreach (Planet planeta in planetas)
        {
            Vector2 direcaoPlaneta = ((Vector2)planeta.transform.position - rb.position).normalized;
            float distancia = Vector2.Distance(rb.position, planeta.transform.position);

            if (distancia > 0.01f)
            {
                float forca = constanteGravitacional * planeta.massa / (distancia * distancia);
                forcaGravidadeTotal += direcaoPlaneta * forca;
            }
        }

        Vector2 velocidadeFrente = direcaoMouse * velocidadeAtual;
        rb.linearVelocity = velocidadeFrente + forcaGravidadeTotal;
    }

    private void SalvarEstadoDaNave()
    {
        DadosGlobais.posicaoSalvaDaNave = transform.position;
        DadosGlobais.velocidadeVetorSalva = rb.linearVelocity;
        DadosGlobais.velocidadeAtualSalva = velocidadeAtual;
        DadosGlobais.tempoSaida = Time.time;
        DadosGlobais.jaEntrouNoEspaco = true;

        DadosGlobais.estaEmOrbita = emOrbita;
        if (emOrbita && planetaOrbitando != null)
        {
            DadosGlobais.nomePlanetaOrbitado = planetaOrbitando.gameObject.name;
            DadosGlobais.anguloOrbitaSalvo = anguloOrbita;
        }

        DadosGlobais.SalvarNoDisco();
    }

    public void IniciarOrbita(Planet planeta)
    {
        planetaOrbitando = planeta;
        anguloSpriteAtual = 0f;

        Vector2 dir = ((Vector2)transform.position - (Vector2)planeta.transform.position).normalized;
        anguloOrbita = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float raioOrbita = planeta.raio + distanciaOrbita;
        posicaoAlvo = (Vector2)planeta.transform.position + new Vector2(Mathf.Cos(anguloOrbita * Mathf.Deg2Rad), Mathf.Sin(anguloOrbita * Mathf.Deg2Rad)) * raioOrbita;
        posicaoAlvo.z = 0;

        posicaoInicial = transform.position;
        progressoTransicao = 0f;
        emTransicao = true;
        emOrbita = true;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        OrbitaVisual orbitaVisual = planeta.GetComponentInChildren<OrbitaVisual>();
        if (orbitaVisual != null) orbitaVisual.Ativar(distanciaOrbita);
    }

    public void SairDaOrbita()
    {
        Planet planetaSaindo = planetaOrbitando;

        emOrbita = false;
        emTransicao = false;
        planetaOrbitando = null;
        rb.linearVelocity = Vector2.zero;

        DadosGlobais.estaEmOrbita = false;

        if (planetaSaindo != null)
        {
            OrbitaVisual orbitaVisual = planetaSaindo.GetComponentInChildren<OrbitaVisual>();
            if (orbitaVisual != null) orbitaVisual.Desativar();
        }
    }

    public void ResetarNave(Vector2 posicao)
    {
        emOrbita = false;
        emTransicao = false;
        rb.linearVelocity = Vector2.zero;
        velocidadeAtual = 0f;
        transform.position = posicao;
    }

    void OnApplicationQuit()
    {
        SalvarEstadoDaNave();
    }
}