using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public enum Estado { Mapa, EmOrbita }

    [Header("Referências de Objetos")]
    public Nave nave;
    public Camera cameraPrincipal;

    [Header("Aviso de Proximidade (só 'Pressione O', só fora da órbita)")]
    public TextMeshProUGUI textoOrbita;
    public GameObject painelTextoOrbita;
    public float raioOrbita = 5f;

    [Header("Satélite")]
    public GameObject satellitePrefab;
    public GameObject painelTabela;
    public TextMeshProUGUI textoInfoPlanetas;
    public TMP_InputField inputNomeSatelite; // opcional — mesmo esquema do nome da nave (UIManager)

    [Header("Barra de Progresso do Scan (só aparece com o painel de status aberto)")]
    public GameObject painelProgressoScan;
    public UnityEngine.UI.Slider barraProgressoScan;

    [Header("Painel de Status (satélite + órbita) — toggle com a tecla I")]
    public GameObject painelStatus;
    public TextMeshProUGUI textoStatus;

    private Estado estadoAtual = Estado.Mapa;
    private Planet planetaAlvo;
    private float tempoParaReentrarOrbita = 0f;
    private Planet[] planetasCache;
    private Satellite satelliteAtivo;
    private bool statusAberto = false;

    void Start()
    {
        // Planetas não mudam em runtime — busca uma vez só, em vez de escanear
        // a cena toda em Update() a cada frame.
        planetasCache = Object.FindObjectsByType<Planet>(FindObjectsSortMode.None);

        if (DadosGlobais.jaEntrouNoEspaco)
        {
            GameObject naveObj = GameObject.FindWithTag("Player");
            if (naveObj != null)
            {
                naveObj.transform.position = DadosGlobais.posicaoSalvaDaNave;

                Rigidbody2D rbNave = naveObj.GetComponent<Rigidbody2D>();
                if (rbNave != null)
                {
                    rbNave.linearVelocity = Vector2.zero;
                    rbNave.angularVelocity = 0f;
                }
            }
        }

        if (painelTextoOrbita) painelTextoOrbita.SetActive(false);
        if (painelStatus) painelStatus.SetActive(false);
        if (painelProgressoScan) painelProgressoScan.SetActive(false);
        Time.timeScale = 1;
    }

    void Update()
    {
        if (nave == null) return;

        // Enquanto o Debug Console está aberto, ignora todos os atalhos desta tela
        // (O, F, I, Tab) pra não conflitar com o que está sendo digitado nele.
        if (DebugConsole.Instancia != null && DebugConsole.Instancia.ConsoleEstaAberto) return;

        if (estadoAtual == Estado.Mapa)
        {
            VerificarProximidadeOrbita();
        }
        else if (estadoAtual == Estado.EmOrbita)
        {
            GerenciarOrbita();
        }

        AtualizarAvisoProximidade();
        AtualizarPainelStatus();

        GerenciarAtalhosGlobais();
    }

    // Aviso simples de "Pressione O" — só aparece perto do planeta e FORA da
    // órbita. Não some sozinho baseado em outro estado, é só essa condição.
    void AtualizarAvisoProximidade()
    {
        if (painelTextoOrbita == null || textoOrbita == null) return;

        bool pertoOSuficiente = estadoAtual == Estado.Mapa
            && tempoParaReentrarOrbita <= 0f
            && planetaAlvo != null
            && Vector2.Distance(nave.transform.position, planetaAlvo.transform.position) < (planetaAlvo.raio + raioOrbita);

        painelTextoOrbita.SetActive(pertoOSuficiente);

        if (pertoOSuficiente)
            textoOrbita.text = "Pressione 'O' para entrar em órbita";
    }

    // Painel único com tudo (satélite + atalhos da órbita) — só aparece
    // quando o jogador aperta a tecla, nunca sozinho.
    void AtualizarPainelStatus()
    {
        if (painelStatus == null) return;

        painelStatus.SetActive(statusAberto);
        if (painelProgressoScan != null)
            painelProgressoScan.SetActive(statusAberto && satelliteAtivo != null);

        if (!statusAberto) return;

        if (satelliteAtivo != null && barraProgressoScan != null)
            barraProgressoScan.value = satelliteAtivo.Progresso;

        if (textoStatus == null) return;

        string texto = "";

        if (estadoAtual == Estado.EmOrbita)
        {
            if (planetaAlvo != null && !planetaAlvo.sateliteLancado)
                texto += "Pressione 'F' para lançar satélite\n";
            texto += "Pressione 'O' para sair da órbita\n\n";
        }

        if (satelliteAtivo != null)
        {
            string nomePlaneta = satelliteAtivo.planetaAlvo != null ? satelliteAtivo.planetaAlvo.nomePlaneta : "?";
            int porcentagem = Mathf.RoundToInt(satelliteAtivo.Progresso * 100f);
            texto += $"{satelliteAtivo.nomeSatelite} ({satelliteAtivo.tipoSatelite})\n";
            texto += $"Planeta: {nomePlaneta}\n";
            texto += $"Coletando dados... {porcentagem}%";
        }
        else
        {
            texto += "Nenhum satélite ativo.";
        }

        textoStatus.text = texto;
    }

    void GerenciarOrbita()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            SairDaOrbita();
            return;
        }

        if (planetaAlvo != null && !planetaAlvo.sateliteLancado &&
            Input.GetKeyDown(KeyCode.F) && satellitePrefab != null)
        {
            GameObject sat = Instantiate(satellitePrefab, Vector3.zero, Quaternion.identity);
            Satellite satScript = sat.GetComponent<Satellite>();
            if (satScript != null)
            {
                satScript.planetaAlvo = planetaAlvo;
                if (inputNomeSatelite != null && !string.IsNullOrEmpty(inputNomeSatelite.text))
                    satScript.nomeSatelite = inputNomeSatelite.text;
                satelliteAtivo = satScript;
            }
        }
    }

    void GerenciarAtalhosGlobais()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && painelTabela != null)
        {
            bool ativo = painelTabela.activeSelf;
            painelTabela.SetActive(!ativo);
            if (!ativo) AtualizarTabela();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            statusAberto = !statusAberto;
        }
    }

    void VerificarProximidadeOrbita()
    {
        if (tempoParaReentrarOrbita > 0)
        {
            tempoParaReentrarOrbita -= Time.deltaTime;
            return;
        }

        foreach (Planet p in planetasCache)
        {
            float dist = Vector2.Distance(nave.transform.position, p.transform.position);
            if (dist < (p.raio + raioOrbita))
            {
                planetaAlvo = p;
                if (Input.GetKeyDown(KeyCode.O)) EntrarEmOrbita();
                return;
            }
        }
    }

    void EntrarEmOrbita()
    {
        estadoAtual = Estado.EmOrbita;
        nave.IniciarOrbita(planetaAlvo);
    }

    public void SairDaOrbita()
    {
        estadoAtual = Estado.Mapa;
        nave.SairDaOrbita();
        tempoParaReentrarOrbita = 1.0f;
    }

    void AtualizarTabela()
    {
        if (textoInfoPlanetas == null) return;

        string tabela = "Planetas Explorados:\n\n";
        bool algumExplorado = false;

        foreach (Planet p in planetasCache)
        {
            if (p.explorado)
            {
                algumExplorado = true;
                tabela += $"Nome: {p.nomePlaneta}\n";
                tabela += $"Massa: {p.massa}\n";
                tabela += $"Força Gravitacional: {p.forcaGravitacional}\n";
                tabela += $"Tem Vida: {(p.temVida ? "Sim" : "Não")}\n";
                tabela += $"Tipo de Vida: {p.tipoVida}\n";
                tabela += $"Seres Inteligentes: {(p.temSeresInteligentes ? "Sim" : "Não")}\n";
                tabela += "-------------------------\n";
            }
        }

        if (!algumExplorado) tabela = "Nenhum planeta explorado ainda.";
        textoInfoPlanetas.text = tabela;
    }
}