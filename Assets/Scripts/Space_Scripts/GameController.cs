using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public enum Estado { Mapa, Decisao, MinigamePouso, ExplorandoPlaneta, EmOrbita }

    [Header("Referências de Objetos")]
    public Nave nave;
    public Camera cameraPrincipal;

    [Header("UI do Aviso de Órbita")]
    public TextMeshProUGUI textoOrbita;
    public GameObject painelTextoOrbita;
    public float raioOrbita = 5f;

    [Header("Satélite e Tabela")]
    public GameObject satellitePrefab;
    public TextMeshProUGUI mensagemOrbita;
    public GameObject painelTabela;
    public TextMeshProUGUI textoInfoPlanetas;

    [Header("UI (Painéis)")]
    public GameObject painelDecisaoPouso;
    public GameObject painelPouso;
    public GameObject painelExploracao;

    [Header("Cenário (Planeta)")]
    public GameObject espacoCenario;
    public GameObject planetaInterior;
    public Camera cameraPlaneta;

    private Estado estadoAtual = Estado.Mapa;
    private Planet planetaAlvo;
    private float tempoParaReentrarOrbita = 0f;

    void Start()
    {
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

        AlternarPaineisIniciais();
    }

    void Update()
    {
        if (nave == null) return;

        if (estadoAtual == Estado.Mapa)
        {
            VerificarProximidadePlanetas();
            VerificarProximidadeOrbita();

            if (Input.GetKeyDown(KeyCode.E))
            {
                SalvarEspacoEEntrarNaNave();
            }
        }
        else if (estadoAtual == Estado.EmOrbita)
        {
            GerenciarOrbita();
        }

        GerenciarAtalhosGlobais();
    }

    void VerificarProximidadePlanetas()
    {
        Planet[] planetas = Object.FindObjectsByType<Planet>(FindObjectsSortMode.None);
        foreach (Planet p in planetas)
        {
            float dist = Vector2.Distance(nave.transform.position, p.transform.position);
            if (dist < p.raio + 0.5f)
            {
                planetaAlvo = p;
                AbrirPainelDecisao();
                break;
            }
        }
    }

    void SalvarEspacoEEntrarNaNave()
    {
        GameObject naveObj = GameObject.FindWithTag("Player");
        if (naveObj != null)
        {
            Rigidbody2D rbNave = naveObj.GetComponent<Rigidbody2D>();
            if (rbNave != null)
            {
                rbNave.linearVelocity = Vector2.zero;
                rbNave.angularVelocity = 0f;
            }

            DadosGlobais.posicaoSalvaDaNave = naveObj.transform.position;
            DadosGlobais.jaEntrouNoEspaco = true;
            DadosGlobais.SalvarNoDisco();

            SceneManager.LoadScene("InteriorNave");
        }
    }

    void GerenciarOrbita()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            SairDaOrbita();
        }

        if (planetaAlvo != null && !planetaAlvo.sateliteLancado)
        {
            if (mensagemOrbita != null)
            {
                mensagemOrbita.gameObject.SetActive(true);
                mensagemOrbita.text = "Pressione F para lançar satélite";
            }

            if (Input.GetKeyDown(KeyCode.F) && satellitePrefab != null)
            {
                GameObject sat = Instantiate(satellitePrefab, Vector3.zero, Quaternion.identity);
                Satellite satScript = sat.GetComponent<Satellite>();
                if (satScript != null) satScript.planetaAlvo = planetaAlvo;

                if (mensagemOrbita != null) mensagemOrbita.gameObject.SetActive(false);
            }
        }
        else if (mensagemOrbita != null)
        {
            mensagemOrbita.gameObject.SetActive(false);
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
    }

    void AlternarPaineisIniciais()
    {
        if (painelDecisaoPouso) painelDecisaoPouso.SetActive(false);
        if (painelPouso) painelPouso.SetActive(false);
        if (painelExploracao) painelExploracao.SetActive(false);
        if (planetaInterior) planetaInterior.SetActive(false);
        if (cameraPlaneta) cameraPlaneta.gameObject.SetActive(false);
        if (painelTextoOrbita) painelTextoOrbita.SetActive(false);
        Time.timeScale = 1;
    }

    void AbrirPainelDecisao()
    {
        estadoAtual = Estado.Decisao;
        if (painelDecisaoPouso) painelDecisaoPouso.SetActive(true);
        Time.timeScale = 0;
    }

    public void VoltarParaEspaco()
    {
        Time.timeScale = 1;
        if (painelDecisaoPouso) painelDecisaoPouso.SetActive(false);
        estadoAtual = Estado.Mapa;

        if (planetaAlvo != null && nave != null)
        {
            Vector2 direcaoParaLonge = ((Vector2)nave.transform.position - (Vector2)planetaAlvo.transform.position).normalized;
            Vector2 novaPos = (Vector2)nave.transform.position + (direcaoParaLonge * 2.5f);
            nave.ResetarNave(novaPos);
        }
    }

    public void IniciarMinigamePouso()
    {
        Time.timeScale = 1;
        if (painelDecisaoPouso) painelDecisaoPouso.SetActive(false);
        estadoAtual = Estado.MinigamePouso;
        if (painelPouso) painelPouso.SetActive(true);

        // Como o SistemaPouso foi removido, adapte aqui caso tenha outro método de pouso, 
        // ou deixe apenas ativando o painel de pouso visual.
    }

    public void PousoFalhou()
    {
        Time.timeScale = 1;
        if (painelPouso) painelPouso.SetActive(false);

        Vector2 pos = Random.insideUnitCircle * 12f;
        nave.ResetarNave(pos);
        Rigidbody2D rb = nave.GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Random.insideUnitCircle * 20f;
        estadoAtual = Estado.Mapa;
    }

    public void PousoBemSucedido()
    {
        Time.timeScale = 1;
        if (painelPouso) painelPouso.SetActive(false);
        EntrarNoPlaneta();
    }

    void EntrarNoPlaneta()
    {
        estadoAtual = Estado.ExplorandoPlaneta;
        if (espacoCenario) espacoCenario.SetActive(false);
        if (planetaInterior) planetaInterior.SetActive(true);
        if (cameraPrincipal) cameraPrincipal.gameObject.SetActive(false);
        if (cameraPlaneta) cameraPlaneta.gameObject.SetActive(true);
        if (painelExploracao) painelExploracao.SetActive(true);
    }

    public void SairDoPlaneta()
    {
        if (espacoCenario) espacoCenario.SetActive(true);
        if (planetaInterior) planetaInterior.SetActive(false);
        if (cameraPrincipal) cameraPrincipal.gameObject.SetActive(true);
        if (cameraPlaneta) cameraPlaneta.gameObject.SetActive(false);
        if (painelExploracao) painelExploracao.SetActive(false);

        Vector2 pos = (Vector2)planetaAlvo.transform.position + new Vector2(6f, 6f);
        nave.ResetarNave(pos);
        estadoAtual = Estado.Mapa;
    }

    void VerificarProximidadeOrbita()
    {
        if (tempoParaReentrarOrbita > 0)
        {
            tempoParaReentrarOrbita -= Time.deltaTime;
            if (painelTextoOrbita) painelTextoOrbita.SetActive(false);
            return;
        }

        Planet[] planetas = Object.FindObjectsByType<Planet>(FindObjectsSortMode.None);
        foreach (Planet p in planetas)
        {
            float dist = Vector2.Distance(nave.transform.position, p.transform.position);
            if (dist < (p.raio + raioOrbita))
            {
                planetaAlvo = p;
                if (painelTextoOrbita) painelTextoOrbita.SetActive(true);
                if (textoOrbita) textoOrbita.text = "Pressione 'O' para entrar em órbita";

                if (Input.GetKeyDown(KeyCode.O)) EntrarEmOrbita();
                return;
            }
        }

        if (painelTextoOrbita && estadoAtual != Estado.EmOrbita) painelTextoOrbita.SetActive(false);
    }

    void EntrarEmOrbita()
    {
        estadoAtual = Estado.EmOrbita;
        if (painelDecisaoPouso) painelDecisaoPouso.SetActive(false);
        if (textoOrbita) textoOrbita.text = "Pressione 'O' para sair da órbita";
        nave.IniciarOrbita(planetaAlvo);
    }

    public void SairDaOrbita()
    {
        estadoAtual = Estado.Mapa;
        if (textoOrbita) textoOrbita.text = "Pressione 'O' para entrar em órbita";
        nave.SairDaOrbita();
        tempoParaReentrarOrbita = 1.0f;
        if (painelTextoOrbita) painelTextoOrbita.SetActive(false);
    }

    void AtualizarTabela()
    {
        if (textoInfoPlanetas == null) return;

        string tabela = "Planetas Explorados:\n\n";
        Planet[] planetas = Object.FindObjectsByType<Planet>(FindObjectsSortMode.None);
        bool algumExplorado = false;

        foreach (Planet p in planetas)
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