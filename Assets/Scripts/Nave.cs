using UnityEngine;

public class Nave : MonoBehaviour
{
    
    [Header("Configurações de Movimento")]
    public float velocidadeMaxima = 10f;    
    public float aceleracao = 15f;          
    public float freio = 25f;               
    public float velocidadeRotacao = 10f;   // Usada para suavizar o controle normal

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

    // Variáveis do deslize
    private bool emTransicao = false;
    private Vector3 posicaoInicial;
    private Vector3 posicaoAlvo;
    private float progressoTransicao = 0f;
    private float tempoTransicao = 1.5f; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; 
    }

    void Update()
    {
        // ===== BLOCO DE ÓRBITA =====
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
                
                // Durante o deslize, use a direção do movimento (sem suavizar para não travar)
                Vector2 direcaoDeslize = (posicaoAlvo - posicaoInicial).normalized;
                if (direcaoDeslize.sqrMagnitude > 0.01f)
                {
                    float anguloAlvo = Mathf.Atan2(direcaoDeslize.y, direcaoDeslize.x) * Mathf.Rad2Deg;
                    TrocarSpritePeloAngulo(anguloAlvo);
                }
                return;
            }

            // Depois do deslize, órbita normal
            anguloOrbita += velocidadeOrbita * Time.deltaTime;
            Vector2 pos = (Vector2)planetaOrbitando.transform.position + new Vector2(Mathf.Cos(anguloOrbita * Mathf.Deg2Rad), Mathf.Sin(anguloOrbita * Mathf.Deg2Rad)) * (planetaOrbitando.raio + distanciaOrbita);
            transform.position = pos;

            // ===== TROCA DE SPRITE ESTÁVEL USANDO A TANGENTE =====
            // O ângulo da tangente é sempre +90° (ou -90°) em relação ao raio.
            // Isso é matemática pura, não depende de posição anterior, então é 100% estável.
            float anguloTangente = anguloOrbita + 90f; // Ajuste o sinal se a órbita for ao contrário
            TrocarSpritePeloAngulo(anguloTangente);
            // ====================================================

            return;
        }
        // ===== FIM DO BLOCO DE ÓRBITA =====

        // ===== CONTROLE NORMAL =====
        Vector3 posicaoMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        posicaoMouse.z = 0f;
        direcaoMouse = (posicaoMouse - transform.position).normalized;

        if (direcaoMouse.sqrMagnitude > 0.01f)
        {
            float angulo = Mathf.Atan2(direcaoMouse.y, direcaoMouse.x) * Mathf.Rad2Deg;
            // Usamos suavização apenas no controle normal (fora da órbita)
            anguloSpriteAtual = Mathf.LerpAngle(anguloSpriteAtual, angulo, Time.deltaTime * velocidadeRotacao);
            TrocarSpritePeloAngulo(anguloSpriteAtual);
        }
    }

    // Variável para suavizar o controle normal
    private float anguloSpriteAtual = 0f;

    void TrocarSpritePeloAngulo(float angulo)
    {
        if (spritesNave == null || spritesNave.Length == 0) return;

        // Pega o angulo
        float anguloNormalizado = Mathf.Repeat(angulo + 90f, 360f);

        // Calcula o "índice" e usamos o .Length em vez do 40, nn entendi pq 40
        // se você adicionar ou remover sprites no Inspector toma cuidado
        int indice = Mathf.FloorToInt(anguloNormalizado / 9f) % spritesNave.Length;

        spriteRenderer.sprite = spritesNave[indice];
    }

    void FixedUpdate()
    {
        if (emOrbita || emTransicao) return;

        if (velocidadeAtual > 0.1f)
        {
            combustivel -= 0.05f * Time.fixedDeltaTime;
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
}