using UnityEngine;

public class Nave : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float velocidadeMaxima = 10f;    
    public float aceleracao = 15f;          
    public float freio = 25f;               
    // (Removemos a velocidade de rotação, pois o Transform não vai mais girar)

    [Header("Gravidade (calculada manualmente)")]
    public float constanteGravitacional = 20f; 
    public float distanciaMinima = 0.5f;

    [Header("Status da Nave")]
    public float combustivel = 100f;        
    public float vida = 100f;               

    [Header("Sprites da Nave (40 frames)")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] spritesNave; 

    private float velocidadeAtual = 0f;
    private Rigidbody2D rb;
    private Vector2 direcaoMouse;

        // ===== VARIÁVEIS DE ÓRBITA =====
    private bool emOrbita = false;
    private Planet planetaOrbitando;
    private float anguloOrbita = 0f;
    public float velocidadeOrbita = 20f; // Graus por segundo
    public float distanciaOrbita = 2f; // Distância extra acima da superfície do planeta

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; 
    }

    void Update()
    {
                // ===== INÍCIO DO BLOCO DE ÓRBITA =====
        if (emOrbita)
        {
            // 1. Atualiza o ângulo da órbita
            anguloOrbita += velocidadeOrbita * Time.deltaTime;

            // 2. Calcula a posição na órbita
            float rad = anguloOrbita * Mathf.Deg2Rad;
            Vector2 pos = (Vector2)planetaOrbitando.transform.position + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * (planetaOrbitando.raio + distanciaOrbita);
            transform.position = pos;

            // 3. CALCULO ESTÁVEL DA DIREÇÃO (TANGENTE)
            // A tangente é perpendicular ao vetor do raio.
            // Se a velocidade for positiva, andamos no sentido anti-horário:
            // direção = (-sin(angulo), cos(angulo))
            // Se a velocidade for negativa, invertemos o sinal:
            float sinal = (velocidadeOrbita > 0) ? 1f : -1f;
            Vector2 direcaoTangente = new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad)) * sinal;
            direcaoTangente.Normalize();

            // 4. Converte a tangente em ângulo
            float anguloMovimento = Mathf.Atan2(direcaoTangente.y, direcaoTangente.x) * Mathf.Rad2Deg;

            // 5. Troca o sprite (com uma suavização para evitar piscadas)
            // Em vez de trocar o sprite em cada frame para o valor exato,
            // usamos uma rotação "lerp" (interpolação linear) para o sprite ir
            // suavemente até a direção correta.
            spriteRenderer.sprite = spritesNave[ObterIndiceSpriteEstavel(anguloMovimento)];

            return; // Sai do Update para não executar o controle normal
        }
        // ===== FIM DO BLOCO DE ÓRBITA =====

        // Calcula a direção do mouse em relação à nave
        Vector3 posicaoMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        posicaoMouse.z = 0f;
        direcaoMouse = (posicaoMouse - transform.position).normalized;

        // Se o mouse não estiver em cima da nave, calcula o ângulo e troca o sprite
        if (direcaoMouse.sqrMagnitude > 0.01f)
        {
            float angulo = Mathf.Atan2(direcaoMouse.y, direcaoMouse.x) * Mathf.Rad2Deg;
            TrocarSpritePeloAngulo(angulo);
        }

            // Função auxiliar para pegar o índice do sprite de forma estável
        int ObterIndiceSpriteEstavel(float angulo)
        {
            if (spritesNave == null || spritesNave.Length == 0) return 0;

            // Normaliza o ângulo para 0-360
            if (angulo < 0) angulo += 360f;
            if (angulo >= 360f) angulo -= 360f;

            // Calcula o índice baseado no ângulo (40 sprites = 9 graus cada)
            int indice = Mathf.FloorToInt((angulo + 90f) / 9f) % 40;
            
            // A função retorna o índice, mas o importante é que agora o ângulo
            // é calculado matematicamente (tangente), o que gera um valor SEM instabilidade.
            return indice;
        }
    }

    void TrocarSpritePeloAngulo(float angulo)
    {
        if (spritesNave == null || spritesNave.Length == 0) return;

        // Normaliza o ângulo para 0-360
        if (angulo < 0) angulo += 360f;

        // 40 sprites = 360 / 40 = 9 graus por sprite
        int indice = (Mathf.FloorToInt((angulo + 90f) / 9f) % 40);

        spriteRenderer.sprite = spritesNave[indice];
        print("Valores, " + angulo +", "+ indice);
    }

    void FixedUpdate()
    {
        // Acelerar (W) e Frear (S) usando a direção do mouse
        if (Input.GetKey(KeyCode.W))
        {
            velocidadeAtual += aceleracao * Time.fixedDeltaTime;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            velocidadeAtual -= freio * Time.fixedDeltaTime;
        }
        velocidadeAtual = Mathf.Clamp(velocidadeAtual, 0f, velocidadeMaxima);

        // Cálculo da gravidade (igual ao seu GravitySystem)
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

        // Define a velocidade da nave APONTANDO para o mouse (sem girar o Transform)
        // Isso mantém a mecânica de "sempre se move aonde aponta", mas sem girar o objeto
        Vector2 velocidadeFrente = direcaoMouse * velocidadeAtual;
        rb.linearVelocity = velocidadeFrente + forcaGravidadeTotal;
    }

    public void ResetarNave(Vector2 posicao)
    {
        rb.linearVelocity = Vector2.zero;
        velocidadeAtual = 0f;
        transform.position = posicao;
    }

        // ===== FUNÇÕES DE ÓRBITA =====
    public void IniciarOrbita(Planet planeta)
    {
        emOrbita = true;
        planetaOrbitando = planeta;
        Vector2 dir = ((Vector2)transform.position - (Vector2)planeta.transform.position).normalized;
        anguloOrbita = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        // Zera a velocidade para não ter inércia
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    public void SairDaOrbita()
    {
        emOrbita = false;
        planetaOrbitando = null;
        // Dá um pequeno impulso para fora (ou zera a velocidade)
        rb.linearVelocity = Vector2.zero;
    }
}