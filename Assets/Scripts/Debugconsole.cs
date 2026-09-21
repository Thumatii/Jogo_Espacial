using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebugConsole : MonoBehaviour
{
    public static DebugConsole Instancia;

    [Header("Configuração")]
    public KeyCode teclaToggle = KeyCode.BackQuote; // tecla ~

    public bool ConsoleEstaAberto => visivel;

    private bool visivel = false;
    private bool deveFocar = false;
    private string inputAtual = "";
    private List<string> historico = new List<string>();
    private Vector2 scrollHistorico;
    private Rect janelaRect = new Rect(20, 20, 640, 340);

    private float timeScaleAnterior = 1f;

    // Histórico de comandos digitados (setas cima/baixo pra navegar, tipo terminal/chat do Minecraft)
    private List<string> historicoDeComandos = new List<string>();
    private int indiceHistorico = -1; // -1 = não navegando
    private string rascunhoAtual = "";

    private Dictionary<string, Action<string[]>> comandos = new Dictionary<string, Action<string[]>>();

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;
        DontDestroyOnLoad(gameObject);

        RegistrarComandosPadrao();
        Log("Debug Console pronto. Digite 'help' para ver os comandos.");
    }

    void Update()
    {
        if (Input.GetKeyDown(teclaToggle))
        {
            visivel = !visivel;

            if (visivel)
            {
                timeScaleAnterior = Time.timeScale;
                Time.timeScale = 0f;
                deveFocar = true;
            }
            else
            {
                Time.timeScale = timeScaleAnterior;
            }
        }
    }

    // ===== REGISTRO DE COMANDOS =====

    public void RegistrarComando(string nome, Action<string[]> acao)
    {
        comandos[nome.ToLower()] = acao;
    }

    void RegistrarComandosPadrao()
    {
        RegistrarComando("help", args => MostrarAjuda());
        RegistrarComando("ajuda", args => MostrarAjuda());

        RegistrarComando("clear", args => historico.Clear());
        RegistrarComando("limpar", args => historico.Clear());

        RegistrarComando("spawn_center_of_world", args => ComandoSpawnCenterOfWorld());

        RegistrarComando("wipe_inspector", args => ComandoWipeInspector());

        RegistrarComando("reset_time", args => ComandoResetTime());
    }

    void MostrarAjuda()
    {
        Log("Comandos disponíveis: " + string.Join(", ", comandos.Keys.OrderBy(k => k)));
    }

    // ===== EXECUÇÃO (com histórico e autocompletar por prefixo) =====

    void ExecutarComando(string linhaOriginal)
    {
        string linha = linhaOriginal.Trim();
        if (string.IsNullOrEmpty(linha)) return;

        // Guarda no histórico de navegação (setas cima/baixo)
        historicoDeComandos.Add(linha);
        indiceHistorico = -1;
        rascunhoAtual = "";

        if (linha.StartsWith("/"))
            linha = linha.Substring(1);

        string[] partes = linha.Split(' ');
        string nomeComando = partes[0].ToLower();
        string[] args = partes.Skip(1).ToArray();

        historico.Add("> " + linha);

        if (comandos.TryGetValue(nomeComando, out Action<string[]> acao))
        {
            ExecutarComSeguranca(nomeComando, acao, args);
            return;
        }

        // Não achou nome exato — tenta por prefixo único (ex: "wipe_insp" -> "wipe_inspector")
        List<string> candidatos = comandos.Keys.Where(k => k.StartsWith(nomeComando)).ToList();

        if (candidatos.Count == 1)
        {
            string nomeReal = candidatos[0];
            ExecutarComSeguranca(nomeReal, comandos[nomeReal], args);
        }
        else if (candidatos.Count > 1)
        {
            Log($"'{nomeComando}' é ambíguo, pode ser: {string.Join(", ", candidatos)}");
        }
        else
        {
            Log($"Comando desconhecido: '{nomeComando}'. Digite 'help' para ver a lista.");
        }
    }

    void ExecutarComSeguranca(string nome, Action<string[]> acao, string[] args)
    {
        try
        {
            acao.Invoke(args);
        }
        catch (Exception e)
        {
            Log($"Erro ao executar '{nome}': {e.Message}");
        }
    }

    // Navega o histórico: direcao -1 = mais antigo (seta cima), +1 = mais novo (seta baixo)
    void NavegarHistorico(int direcao)
    {
        if (historicoDeComandos.Count == 0) return;

        if (indiceHistorico == -1)
        {
            if (direcao > 0) return; // já tá no mais novo, seta baixo não faz nada
            rascunhoAtual = inputAtual;
            indiceHistorico = historicoDeComandos.Count - 1;
            inputAtual = historicoDeComandos[indiceHistorico];
            return;
        }

        int novoIndice = indiceHistorico + direcao;

        if (novoIndice < 0) novoIndice = 0;

        if (novoIndice >= historicoDeComandos.Count)
        {
            indiceHistorico = -1;
            inputAtual = rascunhoAtual; // volta pro que você tava digitando antes de navegar
            return;
        }

        indiceHistorico = novoIndice;
        inputAtual = historicoDeComandos[indiceHistorico];
    }

    void Log(string mensagem)
    {
        historico.Add(mensagem);
        Debug.Log("[DebugConsole] " + mensagem);
    }

    // ===== COMANDOS =====

    void ComandoSpawnCenterOfWorld()
    {
        GameObject jogador = GameObject.FindWithTag("Player");
        if (jogador == null)
        {
            Log("Nenhum objeto com tag 'Player' encontrado na cena atual.");
            return;
        }

        Nave nave = jogador.GetComponent<Nave>();
        if (nave != null)
        {
            nave.ResetarNave(Vector2.zero);
            Log("Nave reposicionada para (0,0).");
            return;
        }

        Rigidbody2D rb = jogador.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        jogador.transform.position = Vector3.zero;
        Log("Jogador reposicionado para (0,0).");
    }

    void ComandoWipeInspector()
    {
        Inspetor[] inspetores = FindObjectsByType<Inspetor>(FindObjectsSortMode.None);
        foreach (Inspetor i in inspetores)
        {
            i.ResetInspecao();
        }
        Log($"{inspetores.Length} objeto(s) inspecionável(eis) resetado(s).");
    }

    void ComandoResetTime()
    {
        if (GerenciadorTempo.Instancia != null)
        {
            GerenciadorTempo.Instancia.ResetarTempo();
            Log("Tempo resetado: Ano 1, Dia 1, 0h.");
        }
        else
        {
            Log("GerenciadorTempo não encontrado (confere se o objeto existe na cena de boot).");
        }
    }

    // ===== UI (IMGUI — funciona em qualquer cena, sem precisar de Canvas) =====

    void OnGUI()
    {
        if (!visivel) return;
        janelaRect = GUI.Window(958123, janelaRect, DesenharJanela, "Debug Console (~ para fechar)");
    }

    void DesenharJanela(int windowID)
    {
        GUILayout.BeginVertical();

        scrollHistorico = GUILayout.BeginScrollView(scrollHistorico, GUILayout.Height(250));
        foreach (string linha in historico)
            GUILayout.Label(linha);
        GUILayout.EndScrollView();

        // Intercepta seta cima/baixo ANTES do TextField desenhar, senão ele consome o evento primeiro.
        bool campoFocado = GUI.GetNameOfFocusedControl() == "DebugInput";
        if (campoFocado && Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.UpArrow)
            {
                NavegarHistorico(-1);
                Event.current.Use();
            }
            else if (Event.current.keyCode == KeyCode.DownArrow)
            {
                NavegarHistorico(1);
                Event.current.Use();
            }
        }

        GUILayout.BeginHorizontal();

        GUI.SetNextControlName("DebugInput");
        inputAtual = GUILayout.TextField(inputAtual, GUILayout.ExpandWidth(true));

        bool enterPressionado = Event.current.type == EventType.KeyDown &&
                                 (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter) &&
                                 campoFocado;

        if (GUILayout.Button("Enviar", GUILayout.Width(70)) || enterPressionado)
        {
            ExecutarComando(inputAtual);
            inputAtual = "";
            deveFocar = true;
            scrollHistorico.y = float.MaxValue;

            if (enterPressionado) Event.current.Use();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        if (deveFocar)
        {
            GUI.FocusControl("DebugInput");
            deveFocar = false;
        }

        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }
}