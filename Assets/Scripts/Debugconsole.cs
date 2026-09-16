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
    // Use isso de qualquer outro script pra adicionar novos comandos:
    // DebugConsole.Instancia.RegistrarComando("nome", args => { ... });

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
    }

    void MostrarAjuda()
    {
        Log("Comandos disponíveis: " + string.Join(", ", comandos.Keys.OrderBy(k => k)));
    }

    void ExecutarComando(string linha)
    {
        linha = linha.Trim();
        if (string.IsNullOrEmpty(linha)) return;

        if (linha.StartsWith("/"))
            linha = linha.Substring(1);

        string[] partes = linha.Split(' ');
        string nomeComando = partes[0].ToLower();
        string[] args = partes.Skip(1).ToArray();

        historico.Add("> " + linha);

        if (comandos.TryGetValue(nomeComando, out Action<string[]> acao))
        {
            try
            {
                acao.Invoke(args);
            }
            catch (Exception e)
            {
                Log($"Erro ao executar '{nomeComando}': {e.Message}");
            }
        }
        else
        {
            Log($"Comando desconhecido: '{nomeComando}'. Digite 'help' para ver a lista.");
        }
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

        // Se for a nave (cena Space), usa o reset dela (zera velocidade, órbita, etc.)
        Nave nave = jogador.GetComponent<Nave>();
        if (nave != null)
        {
            nave.ResetarNave(Vector2.zero);
            Log("Nave reposicionada para (0,0).");
            return;
        }

        // Caso contrário (interior da nave / planeta), reposiciona direto
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

        GUILayout.BeginHorizontal();

        GUI.SetNextControlName("DebugInput");
        inputAtual = GUILayout.TextField(inputAtual, GUILayout.ExpandWidth(true));

        bool enterPressionado = Event.current.type == EventType.KeyDown &&
                                 (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter) &&
                                 GUI.GetNameOfFocusedControl() == "DebugInput";

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