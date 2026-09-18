using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GerenciadorInspetor : MonoBehaviour
{
    public static GerenciadorInspetor Instancia;

    [Header("Referências Compartilhadas (uma vez só por cena)")]
    public GameObject prefabMolduraUI;
    public RectTransform canvasRect;
    public TextMeshProUGUI textoTooltip;
    public Vector2 offsetTexto = new Vector2(30f, 0f);

    [Header("Efeito de Vida na Moldura")]
    public float amplitudePulso = 0.04f;
    public float velocidadePulso = 2f;
    public float amplitudeRotacao = 1.5f;

    [Header("Typewriter")]
    public float velocidadeTypewriter = 35f;

    [Header("Painel de Aparência — Setas e Label")]
    public Button botaoSetaEsquerda;
    public Button botaoSetaDireita;
    public TextMeshProUGUI textoAparencia;
    public float espacoAbaixoDaMoldura = 30f;
    public float espacoEntreTextoESetas = 20f;
    public float distanciaSetasDoCentro = 140f;

    [Header("Painel de Aparência — Preview (grande no meio, pequeno nas laterais)")]
    public Image imagemAtual;
    public Image imagemAnterior;
    public Image imagemProxima;
    public float distanciaImagensDoCentro = 70f;
    public float escalaAtual = 1.3f;
    public float escalaAdjacente = 0.75f;

    [Header("Efeito de Clique no Preview")]
    public float duracaoEfeitoClique = 0.15f;
    public float escalaPicoEfeitoClique = 1.2f; // multiplica escalaAtual no instante do clique

    private GameObject molduraInstanciada;
    private RectTransform rectMoldura;
    private Canvas canvasPrincipal;
    private Camera cam;

    private Inspetor alvoAtual;
    private TrocaAparencia trocaAparenciaAtual;
    private Coroutine coroutineTypewriter;
    private Coroutine coroutineEfeitoPreview;
    private bool modoAparenciaAberto = false;

    public bool ModoAparenciaAberto => modoAparenciaAberto;

    private readonly List<Collider2D> bufferColisores = new List<Collider2D>();
    private ContactFilter2D filtroSemRestricao;

    void Awake()
    {
        Instancia = this;
        cam = Camera.main;

        filtroSemRestricao = new ContactFilter2D();
        filtroSemRestricao.NoFilter();

        if (canvasRect != null)
            canvasPrincipal = canvasRect.GetComponentInParent<Canvas>();

        if (prefabMolduraUI != null && canvasRect != null)
        {
            molduraInstanciada = Instantiate(prefabMolduraUI, canvasRect);
            rectMoldura = molduraInstanciada.GetComponent<RectTransform>();
            molduraInstanciada.SetActive(false);
        }

        if (textoTooltip != null) textoTooltip.gameObject.SetActive(false);
        EsconderPainelAparencia();
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        if (modoAparenciaAberto)
        {
            AtualizarQuandoAparenciaAberta();
            return;
        }

        Inspetor alvoDetectado = DetectarAlvoSobMouse();

        if (alvoDetectado != alvoAtual)
            TrocarAlvo(alvoDetectado);

        if (alvoAtual == null) return;

        AtualizarPosicaoEVisual();

        // Checa o clique primeiro (barato); só faz o raycast de UI (mais caro)
        // se realmente houve clique nesse frame — antes rodava todo frame à toa.
        if (!Input.GetMouseButtonDown(0)) return;

        bool consoleAberto = DebugConsole.Instancia != null && DebugConsole.Instancia.ConsoleEstaAberto;
        if (consoleAberto) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        if (!alvoAtual.jaFoiInspecionado)
        {
            alvoAtual.MarcarInspecionado();
            IniciarTypewriter();
        }
        else if (trocaAparenciaAtual != null)
        {
            modoAparenciaAberto = true;
        }
    }

    // Enquanto o painel de aparência está aberto, o alvo fica FIXO — não depende
    // mais do hover. Assim mover o mouse até as setas não fecha nada no meio do caminho.
    void AtualizarQuandoAparenciaAberta()
    {
        if (alvoAtual == null)
        {
            modoAparenciaAberto = false;
            return;
        }

        AtualizarPosicaoEVisual();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            modoAparenciaAberto = false;
            return;
        }

        if (!Input.GetMouseButtonDown(0)) return;

        bool cliqueSobreUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (!cliqueSobreUI)
            modoAparenciaAberto = false;
    }

    // Pega TODOS os colisores sob o mouse, escolhe o de maior sortingOrder ("de cima").
    Inspetor DetectarAlvoSobMouse()
    {
        Vector2 mousePosWorld = cam.ScreenToWorldPoint(Input.mousePosition);

        bufferColisores.Clear();
        Physics2D.OverlapPoint(mousePosWorld, filtroSemRestricao, bufferColisores);

        Inspetor melhor = null;
        int melhorOrdem = int.MinValue;

        foreach (Collider2D c in bufferColisores)
        {
            Inspetor insp = c.GetComponent<Inspetor>();
            if (insp == null) continue;

            int ordem = insp.Renderizador != null ? insp.Renderizador.sortingOrder : 0;

            if (melhor == null || ordem > melhorOrdem)
            {
                melhor = insp;
                melhorOrdem = ordem;
            }
        }

        return melhor;
    }

    void TrocarAlvo(Inspetor novoAlvo)
    {
        PararTypewriter();
        modoAparenciaAberto = false;

        alvoAtual = novoAlvo;
        trocaAparenciaAtual = alvoAtual != null ? alvoAtual.GetComponent<TrocaAparencia>() : null;

        if (alvoAtual == null)
        {
            if (molduraInstanciada != null) molduraInstanciada.SetActive(false);
            if (textoTooltip != null) textoTooltip.gameObject.SetActive(false);
            EsconderPainelAparencia();
            return;
        }

        if (molduraInstanciada != null) molduraInstanciada.SetActive(true);

        if (textoTooltip != null)
        {
            textoTooltip.gameObject.SetActive(true);
            AtualizarTexto();
        }

        ConfigurarSetas();
    }

    void ConfigurarSetas()
    {
        if (botaoSetaEsquerda == null || botaoSetaDireita == null) return;

        botaoSetaEsquerda.onClick.RemoveAllListeners();
        botaoSetaDireita.onClick.RemoveAllListeners();

        if (trocaAparenciaAtual == null) return;

        botaoSetaEsquerda.onClick.AddListener(() =>
        {
            trocaAparenciaAtual.AparenciaAnterior();
            EfeitoCliquePreview();
        });

        botaoSetaDireita.onClick.AddListener(() =>
        {
            trocaAparenciaAtual.ProximaAparencia();
            EfeitoCliquePreview();
        });
    }

    void EsconderPainelAparencia()
    {
        if (botaoSetaEsquerda != null) botaoSetaEsquerda.gameObject.SetActive(false);
        if (botaoSetaDireita != null) botaoSetaDireita.gameObject.SetActive(false);
        if (textoAparencia != null) textoAparencia.gameObject.SetActive(false);
        if (imagemAtual != null) imagemAtual.gameObject.SetActive(false);
        if (imagemAnterior != null) imagemAnterior.gameObject.SetActive(false);
        if (imagemProxima != null) imagemProxima.gameObject.SetActive(false);
    }

    void AtualizarPosicaoEVisual()
    {
        Collider2D colisor = alvoAtual.Colisor;
        if (colisor == null) return;

        Camera uiCam = (canvasPrincipal != null && canvasPrincipal.renderMode == RenderMode.ScreenSpaceCamera) ? canvasPrincipal.worldCamera : null;

        Vector3 screenPoint = cam.WorldToScreenPoint(alvoAtual.Alvo.position);
        Vector3 minScreen = cam.WorldToScreenPoint(colisor.bounds.min);
        Vector3 maxScreen = cam.WorldToScreenPoint(colisor.bounds.max);
        float larguraTela = Mathf.Abs(maxScreen.x - minScreen.x);
        float alturaTela = Mathf.Abs(maxScreen.y - minScreen.y);
        float scaleFactor = canvasPrincipal != null ? canvasPrincipal.scaleFactor : 1f;

        Vector2 localPointMoldura = Vector2.zero;

        if (rectMoldura != null)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, uiCam, out localPointMoldura);
            rectMoldura.localPosition = localPointMoldura;
            rectMoldura.sizeDelta = new Vector2((larguraTela + 40f) / scaleFactor, (alturaTela + 40f) / scaleFactor);

            float pulso = 1f + Mathf.Sin(Time.time * velocidadePulso) * amplitudePulso;
            rectMoldura.localScale = new Vector3(pulso, pulso, 1f);
            rectMoldura.localEulerAngles = new Vector3(0f, 0f, Mathf.Sin(Time.time * velocidadePulso * 0.6f) * amplitudeRotacao);
        }

        if (textoTooltip != null)
        {
            // Texto já foi definido no TrocarAlvo (estado não-inspecionado) ou
            // pelo typewriter (inspecionado) — reescrever aqui todo frame só
            // gerava trabalho de mesh do TMP à toa sem o texto nunca mudar.
            float larguraMolduraPixels = larguraTela + 40f;
            Vector3 posicaoDireita = screenPoint + new Vector3((larguraMolduraPixels / 2f) + offsetTexto.x, offsetTexto.y, 0);

            Vector2 localPointText;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, posicaoDireita, uiCam, out localPointText);
            textoTooltip.rectTransform.localPosition = localPointText;
        }

        AtualizarPainelAparencia(localPointMoldura);
    }

    void AtualizarPainelAparencia(Vector2 localPointMoldura)
    {
        bool mostrar = modoAparenciaAberto && trocaAparenciaAtual != null;

        if (botaoSetaEsquerda != null) botaoSetaEsquerda.gameObject.SetActive(mostrar);
        if (botaoSetaDireita != null) botaoSetaDireita.gameObject.SetActive(mostrar);
        if (textoAparencia != null) textoAparencia.gameObject.SetActive(mostrar);
        if (imagemAtual != null) imagemAtual.gameObject.SetActive(mostrar);
        if (imagemAnterior != null) imagemAnterior.gameObject.SetActive(mostrar);
        if (imagemProxima != null) imagemProxima.gameObject.SetActive(mostrar);

        if (!mostrar) return;

        Vector2 centroAbaixo = localPointMoldura + new Vector2(0f, -espacoAbaixoDaMoldura);

        if (textoAparencia != null)
        {
            textoAparencia.text = "Aparência:";
            textoAparencia.rectTransform.localPosition = centroAbaixo;
        }

        Vector2 centroPainel = centroAbaixo + new Vector2(0f, -espacoEntreTextoESetas);

        if (imagemAtual != null)
        {
            imagemAtual.sprite = trocaAparenciaAtual.SpriteAtual;
            imagemAtual.rectTransform.localPosition = centroPainel;

            // Não mexe na escala se o "pop" do clique estiver rodando — senão o
            // efeito é cancelado no mesmo frame em que começa.
            if (coroutineEfeitoPreview == null)
                imagemAtual.rectTransform.localScale = new Vector3(escalaAtual, escalaAtual, 1f);
        }

        if (imagemAnterior != null)
        {
            imagemAnterior.sprite = trocaAparenciaAtual.SpriteAnterior;
            imagemAnterior.rectTransform.localPosition = centroPainel + new Vector2(-distanciaImagensDoCentro, 0f);
            imagemAnterior.rectTransform.localScale = new Vector3(escalaAdjacente, escalaAdjacente, 1f);
        }

        if (imagemProxima != null)
        {
            imagemProxima.sprite = trocaAparenciaAtual.SpriteProximo;
            imagemProxima.rectTransform.localPosition = centroPainel + new Vector2(distanciaImagensDoCentro, 0f);
            imagemProxima.rectTransform.localScale = new Vector3(escalaAdjacente, escalaAdjacente, 1f);
        }

        if (botaoSetaEsquerda != null)
            botaoSetaEsquerda.GetComponent<RectTransform>().localPosition = centroPainel + new Vector2(-distanciaSetasDoCentro, 0f);

        if (botaoSetaDireita != null)
            botaoSetaDireita.GetComponent<RectTransform>().localPosition = centroPainel + new Vector2(distanciaSetasDoCentro, 0f);
    }

    void EfeitoCliquePreview()
    {
        if (imagemAtual == null) return;

        if (coroutineEfeitoPreview != null) StopCoroutine(coroutineEfeitoPreview);
        coroutineEfeitoPreview = StartCoroutine(EfeitoCliquePreviewCoroutine());
    }

    IEnumerator EfeitoCliquePreviewCoroutine()
    {
        RectTransform rt = imagemAtual.rectTransform;
        float escalaPico = escalaAtual * escalaPicoEfeitoClique;
        float t = 0f;

        while (t < duracaoEfeitoClique)
        {
            t += Time.deltaTime;
            float progresso = Mathf.Clamp01(t / duracaoEfeitoClique);
            float escala = Mathf.Lerp(escalaPico, escalaAtual, progresso);
            rt.localScale = new Vector3(escala, escala, 1f);
            yield return null;
        }

        rt.localScale = new Vector3(escalaAtual, escalaAtual, 1f);
        coroutineEfeitoPreview = null;
    }

    void AtualizarTexto()
    {
        if (textoTooltip == null || alvoAtual == null) return;

        if (!alvoAtual.jaFoiInspecionado)
        {
            textoTooltip.text = $"[ ? ] {alvoAtual.nomeObjeto}\n<size=80%>{alvoAtual.descricaoObjeto}</size>\n<i>Clique para Inspecionar</i>";
        }
        else
        {
            textoTooltip.text = $"<b>{alvoAtual.nomeReal}</b>\n<size=80%>{alvoAtual.descricaoReal}</size>";
        }

        // Sempre reseta pra texto completo — corrige o bug de texto travado
        // no meio quando o typewriter é interrompido (mouse sai antes de acabar).
        textoTooltip.maxVisibleCharacters = int.MaxValue;
    }

    void IniciarTypewriter()
    {
        PararTypewriter();
        coroutineTypewriter = StartCoroutine(TypewriterCoroutine());
    }

    void PararTypewriter()
    {
        if (coroutineTypewriter != null)
        {
            StopCoroutine(coroutineTypewriter);
            coroutineTypewriter = null;
        }
    }

    IEnumerator TypewriterCoroutine()
    {
        if (alvoAtual == null || textoTooltip == null) yield break;

        string textoCompleto = $"<b>{alvoAtual.nomeReal}</b>\n<size=80%>{alvoAtual.descricaoReal}</size>";
        textoTooltip.text = textoCompleto;
        textoTooltip.ForceMeshUpdate();

        int total = textoTooltip.textInfo.characterCount;
        textoTooltip.maxVisibleCharacters = 0;

        float intervalo = 1f / Mathf.Max(velocidadeTypewriter, 1f);

        for (int i = 0; i <= total; i++)
        {
            textoTooltip.maxVisibleCharacters = i;
            yield return new WaitForSeconds(intervalo);
        }

        coroutineTypewriter = null;
    }

    public void AoResetarInspetor(Inspetor alvo)
    {
        if (alvoAtual != alvo) return;

        PararTypewriter();
        modoAparenciaAberto = false;
        AtualizarTexto();
    }
}