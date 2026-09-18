using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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

    [Header("Troca de Aparência (setas)")]
    public Button botaoSetaEsquerda;
    public Button botaoSetaDireita;
    public float espacoAbaixoDaMoldura = 30f;
    public float espacoEntreSetas = 40f;

    private GameObject molduraInstanciada;
    private RectTransform rectMoldura;
    private Canvas canvasPrincipal;
    private Camera cam;

    private Inspetor alvoAtual;
    private TrocaAparencia trocaAparenciaAtual;
    private Coroutine coroutineTypewriter;

    void Awake()
    {
        Instancia = this;
        cam = Camera.main;

        if (canvasRect != null)
            canvasPrincipal = canvasRect.GetComponentInParent<Canvas>();

        if (prefabMolduraUI != null && canvasRect != null)
        {
            molduraInstanciada = Instantiate(prefabMolduraUI, canvasRect);
            rectMoldura = molduraInstanciada.GetComponent<RectTransform>();
            molduraInstanciada.SetActive(false);
        }

        if (textoTooltip != null)
            textoTooltip.gameObject.SetActive(false);

        if (botaoSetaEsquerda != null) botaoSetaEsquerda.gameObject.SetActive(false);
        if (botaoSetaDireita != null) botaoSetaDireita.gameObject.SetActive(false);
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        Inspetor alvoDetectado = DetectarAlvoSobMouse();

        if (alvoDetectado != alvoAtual)
            TrocarAlvo(alvoDetectado);

        if (alvoAtual != null)
        {
            AtualizarPosicaoEVisual();

            bool consoleAberto = DebugConsole.Instancia != null && DebugConsole.Instancia.ConsoleEstaAberto;

            if (!consoleAberto && Input.GetMouseButtonDown(0) && !alvoAtual.jaFoiInspecionado)
            {
                alvoAtual.MarcarInspecionado();
                IniciarTypewriter();
            }
        }
    }

    // Pega TODOS os colisores sob o mouse (não só um), e escolhe o Inspetor
    // com maior sortingOrder — ou seja, o que está "desenhado por cima".
    Inspetor DetectarAlvoSobMouse()
    {
        Vector2 mousePosWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] colisores = Physics2D.OverlapPointAll(mousePosWorld);

        Inspetor melhor = null;
        int melhorOrdem = int.MinValue;

        foreach (Collider2D c in colisores)
        {
            Inspetor insp = c.GetComponent<Inspetor>();
            if (insp == null) continue;

            SpriteRenderer sr = insp.GetComponent<SpriteRenderer>();
            int ordem = sr != null ? sr.sortingOrder : 0;

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
        alvoAtual = novoAlvo;
        trocaAparenciaAtual = alvoAtual != null ? alvoAtual.GetComponent<TrocaAparencia>() : null;

        if (alvoAtual == null)
        {
            if (molduraInstanciada != null) molduraInstanciada.SetActive(false);
            if (textoTooltip != null) textoTooltip.gameObject.SetActive(false);
            EsconderSetas();
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

        if (trocaAparenciaAtual != null)
        {
            botaoSetaEsquerda.onClick.AddListener(() => trocaAparenciaAtual.AparenciaAnterior());
            botaoSetaDireita.onClick.AddListener(() => trocaAparenciaAtual.ProximaAparencia());
        }
    }

    void EsconderSetas()
    {
        if (botaoSetaEsquerda != null) botaoSetaEsquerda.gameObject.SetActive(false);
        if (botaoSetaDireita != null) botaoSetaDireita.gameObject.SetActive(false);
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
            if (!alvoAtual.jaFoiInspecionado)
                AtualizarTexto();

            float larguraMolduraPixels = larguraTela + 40f;
            Vector3 posicaoDireita = screenPoint + new Vector3((larguraMolduraPixels / 2f) + offsetTexto.x, offsetTexto.y, 0);

            Vector2 localPointText;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, posicaoDireita, uiCam, out localPointText);
            textoTooltip.rectTransform.localPosition = localPointText;
        }

        AtualizarSetas(localPointMoldura, alturaTela, scaleFactor);
    }

    void AtualizarSetas(Vector2 localPointMoldura, float alturaTela, float scaleFactor)
    {
        if (botaoSetaEsquerda == null || botaoSetaDireita == null) return;

        bool mostrar = trocaAparenciaAtual != null && alvoAtual.jaFoiInspecionado;
        botaoSetaEsquerda.gameObject.SetActive(mostrar);
        botaoSetaDireita.gameObject.SetActive(mostrar);

        if (!mostrar) return;

        float alturaMolduraPixels = (alturaTela + 40f) / scaleFactor;
        Vector2 centroAbaixo = localPointMoldura + new Vector2(0f, -(alturaMolduraPixels / 2f) - espacoAbaixoDaMoldura);

        botaoSetaEsquerda.GetComponent<RectTransform>().localPosition = centroAbaixo + new Vector2(-espacoEntreSetas, 0f);
        botaoSetaDireita.GetComponent<RectTransform>().localPosition = centroAbaixo + new Vector2(espacoEntreSetas, 0f);
    }

    void AtualizarTexto()
    {
        if (textoTooltip == null || alvoAtual == null) return;

        if (!alvoAtual.jaFoiInspecionado)
        {
            textoTooltip.text = $"[ ? ] {alvoAtual.nomeObjeto}\n<size=80%>{alvoAtual.descricaoObjeto}</size>\n<i>Clique para Inspecionar</i>";
            textoTooltip.maxVisibleCharacters = int.MaxValue;
        }
        else
        {
            textoTooltip.text = $"<b>{alvoAtual.nomeReal}</b>\n<size=80%>{alvoAtual.descricaoReal}</size>";
        }
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
        AtualizarTexto();
    }
}