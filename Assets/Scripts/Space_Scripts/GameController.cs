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
    public float raioOrbita = 5f;

    [Header("Painel da Órbita (automático — só aparece enquanto EM órbita)")]
    public TextMeshProUGUI textoEmOrbita;

    [Header("Satélite")]
    public GameObject satellitePrefab;
    public GameObject painelTabela;
    public TextMeshProUGUI textoInfoPlanetas;
    public TMP_InputField inputNomeSatelite; // opcional — mesmo esquema do nome da nave (UIManager)

    [Header("Painel de Status do Satélite (abre/fecha só pelo botão — ícone no canto da tela)")]
    public TextMeshProUGUI textoStatusSatelite;
    public UnityEngine.UI.Slider barraProgressoScan;

    [Header("Contador de Satélites Ativos (opcional)")]
    public TextMeshProUGUI textoContadorSatelites;

    [Header("Seta que Inverte ao Abrir/Fechar (opcional)")]
    public RectTransform setaStatusSatelite; // a imagem da seta, não o botão
    public float rotacaoFechado = 0f;
    public float rotacaoAberto = 180f;
    public float velocidadeRotacaoSeta = 10f; // gira suave, não instantâneo

    private Estado estadoAtual = Estado.Mapa;
    private Planet planetaAlvo;
    private float tempoParaReentrarOrbita = 0f;
    private Planet[] planetasCache;
    private Satellite satelliteAtivo;
    private bool statusAberto = false;
    private bool sincronizouEstadoInicial = false; // ver comentário no Update()

    void Start()
    {
        // Planetas não mudam em runtime — busca uma vez só, em vez de escanear
        // a cena toda em Update() a cada frame.
        planetasCache = Object.FindObjectsByType<Planet>(FindObjectsSortMode.None);

        // A restauração de posição da nave já é feita pelo próprio Nave.cs
        // (com cálculo de inércia inclusive) — não duplica aqui.

        if (textoOrbita) textoOrbita.gameObject.SetActive(false);
        if (textoEmOrbita) textoEmOrbita.gameObject.SetActive(false);
        if (textoStatusSatelite) textoStatusSatelite.gameObject.SetActive(false);
        if (barraProgressoScan) barraProgressoScan.gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    void Update()
    {
        if (nave == null) return;

        // A Nave restaura o PRÓPRIO estado de órbita sozinha (no Start dela,
        // a partir do save). Sem isso aqui, esse estadoAtual ficava sempre
        // em Mapa mesmo com a nave já orbitando de verdade ao carregar o
        // jogo — causava "F não faz nada" e "Pressione O" aparecendo errado.
        // Fazemos isso no primeiro Update (não no Start) pra garantir que o
        // Start da Nave já rodou, não importa a ordem de execução dos scripts.
        if (!sincronizouEstadoInicial)
        {
            sincronizouEstadoInicial = true;
            if (nave.EstaEmOrbita)
            {
                estadoAtual = Estado.EmOrbita;
                planetaAlvo = nave.PlanetaOrbitando;
            }
        }

        // Enquanto o Debug Console está aberto, ignora os atalhos desta tela.
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
        AtualizarPainelOrbitaAtual();
        AtualizarPainelStatusSatelite();
        AtualizarContadorSatelites();
        AtualizarSetaStatus();

        if (Input.GetKeyDown(KeyCode.Tab) && painelTabela != null)
        {
            bool ativo = painelTabela.activeSelf;
            painelTabela.SetActive(!ativo);
            if (!ativo) AtualizarTabela();
        }
    }

    // "Pressione O para entrar em órbita" — só perto do planeta, só fora da órbita.
    void AtualizarAvisoProximidade()
    {
        if (textoOrbita == null) return;

        bool pertoOSuficiente = estadoAtual == Estado.Mapa
            && tempoParaReentrarOrbita <= 0f
            && planetaAlvo != null
            && Vector2.Distance(nave.transform.position, planetaAlvo.transform.position) < (planetaAlvo.raio + raioOrbita);

        textoOrbita.gameObject.SetActive(pertoOSuficiente);

        if (pertoOSuficiente)
            textoOrbita.text = "Pressione 'O' para entrar em órbita";
    }

    // "Pressione F / Pressione O para sair" — automático, só enquanto EM órbita.
    // Não depende de botão nem tecla, só do estado atual.
    void AtualizarPainelOrbitaAtual()
    {
        if (textoEmOrbita == null) return;

        bool emOrbitaAgora = estadoAtual == Estado.EmOrbita;
        textoEmOrbita.gameObject.SetActive(emOrbitaAgora);

        if (!emOrbitaAgora) return;

        string texto = "";
        if (planetaAlvo != null && !planetaAlvo.sateliteLancado)
            texto += "Pressione 'F' para lançar satélite\n";
        texto += "Pressione 'O' para sair da órbita";

        textoEmOrbita.text = texto;
    }

    // Painel do satélite: só abre/fecha pelo botão (AlternarPainelStatus),
    // funciona a qualquer momento — não depende de estar perto nem em órbita,
    // já que o satélite continua trabalhando sozinho mesmo longe da nave.
    void AtualizarPainelStatusSatelite()
    {
        if (textoStatusSatelite == null) return;

        textoStatusSatelite.gameObject.SetActive(statusAberto);
        if (barraProgressoScan != null)
            barraProgressoScan.gameObject.SetActive(statusAberto && satelliteAtivo != null);

        if (!statusAberto) return;

        if (satelliteAtivo != null)
        {
            if (barraProgressoScan != null)
                barraProgressoScan.value = satelliteAtivo.Progresso;

            string nomePlaneta = satelliteAtivo.planetaAlvo != null ? satelliteAtivo.planetaAlvo.nomePlaneta : "?";
            int porcentagem = Mathf.RoundToInt(satelliteAtivo.Progresso * 100f);

            textoStatusSatelite.text =
                $"{satelliteAtivo.nomeSatelite} ({satelliteAtivo.tipoSatelite})\n" +
                $"Planeta: {nomePlaneta}\n" +
                $"Coletando dados... {porcentagem}%";
        }
        else
        {
            textoStatusSatelite.text = "Nenhum satélite ativo.";
        }
    }

    // Conta TODOS os satélites ativos na cena, não só o último lançado
    // (satelliteAtivo guarda só o mais recente, mas pode ter vários
    // trabalhando em planetas diferentes ao mesmo tempo).
    void AtualizarContadorSatelites()
    {
        if (textoContadorSatelites == null) return;

        int quantidade = Object.FindObjectsByType<Satellite>(FindObjectsSortMode.None).Length;
        textoContadorSatelites.text = $"Satélites ativos: {quantidade}";
    }

    // Gira a seta suavemente (não de uma vez) conforme abre/fecha o painel.
    void AtualizarSetaStatus()
    {
        if (setaStatusSatelite == null) return;

        float alvo = statusAberto ? rotacaoAberto : rotacaoFechado;
        float atual = setaStatusSatelite.localEulerAngles.z;
        float novoAngulo = Mathf.LerpAngle(atual, alvo, Time.deltaTime * velocidadeRotacaoSeta);

        setaStatusSatelite.localEulerAngles = new Vector3(0f, 0f, novoAngulo);
    }

    // Chame isso no OnClick do botão de satélite (canto da tela).
    public void AlternarPainelStatus()
    {
        statusAberto = !statusAberto;
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