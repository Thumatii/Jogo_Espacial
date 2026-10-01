using System.Collections;
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
    public float velocidadeOrbita = 8f; // mais devagar que antes (era 20)

    [Header("Órbita — Altitude Ajustável e Risco")]
    public float distanciaOrbitaMinima = 2f;
    public float distanciaOrbitaMaxima = 10f;
    public float velocidadeAjusteAltitude = 1f; // W aproxima, S afasta (mais devagar que antes, era 3)
    public float consumoCombustivelOrbitaBase = 0.5f; // por segundo, na altitude mais segura
    public float multiplicadorRiscoCombustivel = 3f; // extra de consumo na altitude mínima
    public float chancePerigoPorSegundoNoMinimo = 0.05f;
    public float danoPerigoOrbital = 5f;

    [Header("Órbita — Detritos Visuais (no impacto do perigo)")]
    public int quantidadeDetritosPorPerigo = 4;
    public float duracaoDetritos = 1.5f;
    public float velocidadeDetritos = 3f;
    public Color corDetritos = new Color(0.45f, 0.4f, 0.35f);

    public float velocidadeAtual = 0f;
    private Rigidbody2D rb;
    private Vector2 direcaoMouse;
    private Planet[] planetasCache;

    private bool emOrbita = false;
    private Planet planetaOrbitando;
    private float anguloOrbita = 0f;

    public bool EstaEmOrbita => emOrbita;

    // Rastreia o anel visual (OrbitaVisual) da órbita atual, pra redesenhar
    // sempre que a altitude mudar — antes ele só era desenhado UMA VEZ ao
    // entrar, então altitude ajustável fazia a nave "sair" visualmente do
    // anel mesmo estando numa órbita válida.
    private OrbitaVisual orbitaVisualAtual;
    private float ultimaDistanciaDesenhada = -1f;

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

        // Planetas não mudam em runtime — busca uma vez só aqui em vez de
        // escanear a cena inteira toda FixedUpdate (rodava ~50x por segundo à toa).
        planetasCache = FindObjectsByType<Planet>(FindObjectsSortMode.None);

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
                    orbitaVisualAtual = orbitaVisual;
                    ultimaDistanciaDesenhada = distanciaOrbita;
                }
                else
                {
                    // Planeta salvo não existe mais nessa cena (nome mudou, foi removido, etc).
                    // Sem isso a nave ficava largada onde o Editor deixou, sem reposicionar nada.
                    transform.position = DadosGlobais.posicaoSalvaDaNave;
                    rb.linearVelocity = Vector2.zero;
                    DadosGlobais.estaEmOrbita = false;
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
            Time.timeScale = 1f; // nunca troca de cena com o jogo pausado
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

            // Altitude ajustável: W aproxima (mais arriscado, gasta mais), S afasta (mais seguro)
            if (Input.GetKey(KeyCode.W))
                distanciaOrbita = Mathf.Clamp(distanciaOrbita - velocidadeAjusteAltitude * Time.deltaTime, distanciaOrbitaMinima, distanciaOrbitaMaxima);
            else if (Input.GetKey(KeyCode.S))
                distanciaOrbita = Mathf.Clamp(distanciaOrbita + velocidadeAjusteAltitude * Time.deltaTime, distanciaOrbitaMinima, distanciaOrbitaMaxima);

            // Redesenha o anel SÓ quando a distância muda de verdade — não todo
            // frame à toa (Ativar() recalcula todos os pontos do anel).
            if (orbitaVisualAtual != null && !Mathf.Approximately(distanciaOrbita, ultimaDistanciaDesenhada))
            {
                orbitaVisualAtual.Ativar(distanciaOrbita);
                ultimaDistanciaDesenhada = distanciaOrbita;
            }

            AtualizarRiscoOrbital();

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

        foreach (Planet planeta in planetasCache)
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

    // Quanto mais perto do mínimo, mais arriscado: gasta mais combustível e
    // tem chance de sofrer dano (detritos/radiação). Na altitude máxima, risco = 0.
    void AtualizarRiscoOrbital()
    {
        float proporcaoRisco = 1f - Mathf.InverseLerp(distanciaOrbitaMinima, distanciaOrbitaMaxima, distanciaOrbita);

        combustivel -= consumoCombustivelOrbitaBase * (1f + proporcaoRisco * multiplicadorRiscoCombustivel) * Time.deltaTime;
        combustivel = Mathf.Clamp(combustivel, 0f, 100f);
    }

    // Chamado pelo DetritoEspacial quando a nave colide com um detrito de verdade —
    // substitui o dano por chance invisível de antes, agora tem causa visível.
    public void ReceberDano(float quantidade)
    {
        vida -= quantidade;
        vida = Mathf.Clamp(vida, 0f, 100f);
        NotificacaoUI.Instancia?.Mostrar($"Colisão com detrito espacial! -{quantidade:F0} vida");
        SpawnarDetritos(); // reaproveita o efeito visual de impacto (faíscas)
    }

    // Uns quadradinhos coloridos que saem voando da nave e somem — feedback
    // visual simples do impacto, sem precisar de sprite/partícula pronta.
    void SpawnarDetritos()
    {
        for (int i = 0; i < quantidadeDetritosPorPerigo; i++)
        {
            GameObject detrito = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(detrito.GetComponent<Collider>());

            detrito.transform.position = transform.position;
            detrito.transform.localScale = Vector3.one * Random.Range(0.1f, 0.25f);

            Renderer renderer = detrito.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.material.color = corDetritos;
            renderer.sortingOrder = 5;

            Vector2 direcao = Random.insideUnitCircle.normalized;
            StartCoroutine(MoverDetrito(detrito, direcao * velocidadeDetritos));
        }
    }

    IEnumerator MoverDetrito(GameObject detrito, Vector2 velocidade)
    {
        float tempo = 0f;
        while (tempo < duracaoDetritos && detrito != null)
        {
            detrito.transform.position += (Vector3)(velocidade * Time.deltaTime);
            tempo += Time.deltaTime;
            yield return null;
        }

        if (detrito != null) Destroy(detrito);
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

        // Sempre começa numa distância padrão — antes carregava o valor que
        // tinha ficado de uma órbita anterior (podia começar já no máximo).
        distanciaOrbita = Mathf.Clamp(5f, distanciaOrbitaMinima, distanciaOrbitaMaxima);

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
        orbitaVisualAtual = orbitaVisual;
        ultimaDistanciaDesenhada = distanciaOrbita;
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