using UnityEngine;

public class GameController : MonoBehaviour
{
    public enum Estado { Mapa, Decisao, MinigamePouso, ExplorandoPlaneta }

    [Header("Referências de Objetos")]
    public Nave nave;
    public Camera cameraPrincipal;
    
    [Header("UI (Painéis)")]
    public GameObject painelDecisaoPouso; // Painel dos botões
    public GameObject painelPouso;        // Painel do minigame (Slider)
    public GameObject painelExploracao;   // Painel da exploração

    [Header("Cenário (Planeta)")]
    public GameObject espacoCenario;      // Objeto pai de todo o espaço (onde estão os planetas e a nave)
    public GameObject planetaInterior;    // Objeto pai do chão do planeta e personagem
    public Camera cameraPlaneta;          // Uma segunda câmera só para o planeta

    private Estado estadoAtual = Estado.Mapa;
    private Planet planetaAlvo;

    void Start()
    {
        // Estado inicial
        painelDecisaoPouso.SetActive(false);
        painelPouso.SetActive(false);
        painelExploracao.SetActive(false);
        planetaInterior.SetActive(false);
        cameraPlaneta.gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    void Update()
    {
        // Verifica colisão APENAS se estiver no espaço
        if (estadoAtual == Estado.Mapa && nave != null)
        {
            Planet[] planetas = FindObjectsOfType<Planet>();
            foreach (Planet p in planetas)
            {
                float dist = Vector2.Distance(nave.transform.position, p.transform.position);
                if (dist < p.raio + 0.5f) // Raio de colisão fixo para simplificar
                {
                    planetaAlvo = p;
                    AbrirPainelDecisao();
                    break;
                }
            }
        }
    }

    void AbrirPainelDecisao()
    {
        estadoAtual = Estado.Decisao;
        painelDecisaoPouso.SetActive(true);
        Time.timeScale = 0; // Pausa o jogo
    }

    // Chamado pelo botão "Voltar ao Espaço"
    public void VoltarParaEspaco()
    {
    Time.timeScale = 1;
    painelDecisaoPouso.SetActive(false);
    estadoAtual = Estado.Mapa;

    if (planetaAlvo != null && nave != null)
    {
        Vector2 direcaoParaLonge = ((Vector2)nave.transform.position - (Vector2)planetaAlvo.transform.position).normalized;
        
        // Mudei o 5f para 2.5f aqui. O teleporte será bem menor!
        Vector2 novaPos = (Vector2)nave.transform.position + (direcaoParaLonge * 2.5f);

        nave.ResetarNave(novaPos);
    }
    }

    // Chamado pelo botão "Pousar"
    public void IniciarMinigamePouso()
    {
        Time.timeScale = 1;
        painelDecisaoPouso.SetActive(false);
        estadoAtual = Estado.MinigamePouso;
        painelPouso.SetActive(true);

        // Encontra o SistemaPouso e ativa o minigame
        SistemaPouso sistema = FindObjectOfType<SistemaPouso>();
        if (sistema != null) sistema.IniciarPouso(planetaAlvo);
    }

    // Chamado pelo SistemaPouso se o jogador PERDER
    public void PousoFalhou()
    {
        Time.timeScale = 1;
        painelPouso.SetActive(false);
        
        Vector2 pos = Random.insideUnitCircle * 12f;
        nave.ResetarNave(pos);
        nave.GetComponent<Rigidbody2D>().linearVelocity = Random.insideUnitCircle * 20f;
        estadoAtual = Estado.Mapa;
    }

    // Chamado pelo SistemaPouso se o jogador GANHAR
    public void PousoBemSucedido()
    {
        Time.timeScale = 1;
        painelPouso.SetActive(false);
        EntrarNoPlaneta();
    }

    void EntrarNoPlaneta()
    {
        estadoAtual = Estado.ExplorandoPlaneta;

        // Troca o cenário
        espacoCenario.SetActive(false);
        planetaInterior.SetActive(true);
        cameraPrincipal.gameObject.SetActive(false);
        cameraPlaneta.gameObject.SetActive(true);

        painelExploracao.SetActive(true);
    }

    // Chamado quando o personagem aperta "E" na nave parada
    public void SairDoPlaneta()
    {
        // Volta o cenário
        espacoCenario.SetActive(true);
        planetaInterior.SetActive(false);
        cameraPrincipal.gameObject.SetActive(true);
        cameraPlaneta.gameObject.SetActive(false);
        painelExploracao.SetActive(false);

        // Coloca a nave perto do planeta
        Vector2 pos = (Vector2)planetaAlvo.transform.position + new Vector2(6f, 6f);
        nave.ResetarNave(pos);
        
        estadoAtual = Estado.Mapa;
    }
}