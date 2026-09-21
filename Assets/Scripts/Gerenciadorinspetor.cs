using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[DefaultExecutionOrder(10000)] // roda depois de tudo, inclusive a Cinemachine — câmera já atualizada nesse ponto
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
    [Range(0f, 1f)] public float alphaAdjacente = 0.5f; // transparência das laterais (anterior/próxima)

    [Header("Painel de Aparência — Fundo atrás da imagem atual")]
    public Image fundoImagemAtual;
    public float paddingFundoAtual = 20f; // quanto o fundo passa da borda da imagem atual, por lado
    public Color corFundoAtual = new Color(1f, 1f, 1f, 0.2f);

    [Header("Efeito de Clique no Preview")]
    public float duracaoEfeitoClique = 0.15f;
    public float escalaPicoEfeitoClique = 1.2f; // multiplica escalaAtual no instante do clique

    [Header("Texto — Borda")]
    [Range(0f, 1f)] public float larguraBordaTexto = 0.2f;
    public Color corBordaTexto = Color.black;

    [Header("Efeito de Clique nas Setas")]
    public float escalaPicoSetas = 1.3f;

    private GameObject molduraInstanciada;
    private RectTransform rectMoldura;
    private Canvas canvasPrincipal;
    private Camera cam;

    private Inspetor alvoAtual;
    private TrocaAparencia trocaAparenciaAtual;
    private Coroutine coroutineTypewriter;
    private Coroutine coroutineEfeitoPreview;
    private Coroutine coroutineEfeitoSetaEsquerda;
    private Coroutine coroutineEfeitoSetaDireita;
    private Vector3 escalaOriginalSetaEsquerda = Vector3.one;
    private Vector3 escalaOriginalSetaDireita = Vector3.one;
    private bool modoAparenciaAberto = false;

    public bool ModoAparenciaAberto => modoAparenciaAberto;

    private readonly List<Collider2D> bufferColisores = new List<Collider2D>();
    private ContactFilter2D filtroSemRestricao;

    void Awake()
    {
        // Mata qualquer instância anterior (ex: uma que sobreviveu junto de um
        // objeto DontDestroyOnLoad por engano) antes de assumir como a atual.
        if (Instancia != null && Instancia != this)
        {
            Destroy(Instancia.gameObject);
        }

        Instancia = this;
        cam = Camera.main;

        filtroSemRestricao = new ContactFilter2D();
        filtroSemRestricao.NoFilter();
        filtroSemRestricao.useTriggers = true; // Herbert e Painel de Controle usam Is Trigger — sem isso a detecção falha/fica inconsistente

        if (canvasRect != null)
            canvasPrincipal = canvasRect.GetComponentInParent<Canvas>();

        if (prefabMolduraUI != null && canvasRect != null)
        {
            molduraInstanciada = Instantiate(prefabMolduraUI, canvasRect);
            rectMoldura = molduraInstanciada.GetComponent<RectTransform>();
            molduraInstanciada.SetActive(false);
        }

        // Se não arrastou nada nesses campos, cria sozinho — não precisa
        // montar Image na mão no Canvas pra isso funcionar.
        if (canvasRect != null)
        {
            if (imagemAtual == null) imagemAtual = CriarImagemPreview("ImagemAtual_Auto");
            if (imagemAnterior == null) imagemAnterior = CriarImagemPreview("ImagemAnterior_Auto");
            if (imagemProxima == null) imagemProxima = CriarImagemPreview("ImagemProxima_Auto");

            if (fundoImagemAtual == null)
            {
                fundoImagemAtual = CriarImagemPreview("FundoImagemAtual_Auto");
                fundoImagemAtual.preserveAspect = false;
                fundoImagemAtual.color = corFundoAtual;
            }
        }

        // Garante que o fundo renderiza ATRÁS da imagem atual, não em cima.
        if (fundoImagemAtual != null && imagemAtual != null)
            fundoImagemAtual.transform.SetSiblingIndex(imagemAtual.transform.GetSiblingIndex());

        // Guarda a escala ORIGINAL de cada seta (a que você configurou no Editor),
        // em vez de assumir 1,1,1 — era isso que causava o esticamento gigante.
        if (botaoSetaEsquerda != null)
            escalaOriginalSetaEsquerda = botaoSetaEsquerda.GetComponent<RectTransform>().localScale;
        if (botaoSetaDireita != null)
            escalaOriginalSetaDireita = botaoSetaDireita.GetComponent<RectTransform>().localScale;

        if (textoTooltip != null) textoTooltip.gameObject.SetActive(false);
        AplicarBordaTexto(textoTooltip);
        AplicarBordaTexto(textoAparencia);
        EsconderPainelAparencia();

        // Nada disso deve BLOQUEAR clique do mundo — só os botões de seta
        // precisam receber clique. Image/TMP têm Raycast Target ligado por
        // padrão, e isso tava impedindo o clique de "passar" pro Herbert
        // quando essas peças ficavam em cima dele.
        DesligarRaycast(textoTooltip);
        DesligarRaycast(textoAparencia);
        DesligarRaycast(imagemAtual);
        DesligarRaycast(imagemAnterior);
        DesligarRaycast(imagemProxima);
        DesligarRaycast(fundoImagemAtual);
        if (molduraInstanciada != null)
        {
            foreach (Graphic g in molduraInstanciada.GetComponentsInChildren<Graphic>(true))
                g.raycastTarget = false;
        }
    }

    void DesligarRaycast(Graphic g)
    {
        if (g != null) g.raycastTarget = false;
    }

    void AplicarBordaTexto(TextMeshProUGUI texto)
    {
        if (texto == null) return;
        texto.outlineWidth = larguraBordaTexto;
        texto.outlineColor = corBordaTexto;
    }

    Image CriarImagemPreview(string nome)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(canvasRect, false);

        Image img = go.GetComponent<Image>();
        img.preserveAspect = true;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100f, 100f); // tamanho base — escalaAtual/escalaAdjacente ajustam o resto

        go.SetActive(false);
        return img;
    }

    void LateUpdate()
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

        bool sobreUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        if (sobreUI) return;

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
            EfeitoCliqueSetaEsquerda();
        });

        botaoSetaDireita.onClick.AddListener(() =>
        {
            trocaAparenciaAtual.ProximaAparencia();
            EfeitoCliquePreview();
            EfeitoCliqueSetaDireita();
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
        if (fundoImagemAtual != null) fundoImagemAtual.gameObject.SetActive(false);
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
        if (fundoImagemAtual != null) fundoImagemAtual.gameObject.SetActive(mostrar);

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

        if (fundoImagemAtual != null)
        {
            fundoImagemAtual.rectTransform.localPosition = centroPainel;
            fundoImagemAtual.color = corFundoAtual;

            if (imagemAtual != null)
            {
                Vector2 tamanhoBase = imagemAtual.rectTransform.sizeDelta;
                fundoImagemAtual.rectTransform.sizeDelta = tamanhoBase + new Vector2(paddingFundoAtual, paddingFundoAtual) * 2f;
            }
        }

        if (imagemAnterior != null)
        {
            imagemAnterior.sprite = trocaAparenciaAtual.SpriteAnterior;
            imagemAnterior.rectTransform.localPosition = centroPainel + new Vector2(-distanciaImagensDoCentro, 0f);
            imagemAnterior.rectTransform.localScale = new Vector3(escalaAdjacente, escalaAdjacente, 1f);

            Color corAnterior = imagemAnterior.color;
            corAnterior.a = alphaAdjacente;
            imagemAnterior.color = corAnterior;
        }

        if (imagemProxima != null)
        {
            imagemProxima.sprite = trocaAparenciaAtual.SpriteProximo;
            imagemProxima.rectTransform.localPosition = centroPainel + new Vector2(distanciaImagensDoCentro, 0f);
            imagemProxima.rectTransform.localScale = new Vector3(escalaAdjacente, escalaAdjacente, 1f);

            Color corProxima = imagemProxima.color;
            corProxima.a = alphaAdjacente;
            imagemProxima.color = corProxima;
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

    void EfeitoCliqueSetaEsquerda()
    {
        if (botaoSetaEsquerda == null) return;

        if (coroutineEfeitoSetaEsquerda != null) StopCoroutine(coroutineEfeitoSetaEsquerda);
        coroutineEfeitoSetaEsquerda = StartCoroutine(EfeitoPopSetaCoroutine(botaoSetaEsquerda.GetComponent<RectTransform>(), escalaOriginalSetaEsquerda, r => coroutineEfeitoSetaEsquerda = r));
    }

    void EfeitoCliqueSetaDireita()
    {
        if (botaoSetaDireita == null) return;

        if (coroutineEfeitoSetaDireita != null) StopCoroutine(coroutineEfeitoSetaDireita);
        coroutineEfeitoSetaDireita = StartCoroutine(EfeitoPopSetaCoroutine(botaoSetaDireita.GetComponent<RectTransform>(), escalaOriginalSetaDireita, r => coroutineEfeitoSetaDireita = r));
    }

    IEnumerator EfeitoPopSetaCoroutine(RectTransform alvo, Vector3 escalaBase, System.Action<Coroutine> aoTerminar)
    {
        Vector3 escalaPico = escalaBase * escalaPicoSetas;
        float t = 0f;

        while (t < duracaoEfeitoClique)
        {
            t += Time.deltaTime;
            float progresso = Mathf.Clamp01(t / duracaoEfeitoClique);
            alvo.localScale = Vector3.Lerp(escalaPico, escalaBase, progresso);
            yield return null;
        }

        alvo.localScale = escalaBase;
        aoTerminar(null);
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