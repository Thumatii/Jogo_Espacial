using System.Collections;
using UnityEngine;
using TMPro;

public class Inspetor : MonoBehaviour
{
    [Header("Objeto a Ser Seguido")]
    public Transform alvoParaSeguir;

    [Header("Informações Secretas (Reveladas no clique)")]
    public string nomeReal = "Planeta Revelado";
    [TextArea(2, 4)] public string descricaoReal = "Descrição secreta do objeto.";

    [Header("Informações do Objeto (Estado Padrão)")]
    public string nomeObjeto = "Desconhecido";
    [TextArea(2, 4)] public string descricaoObjeto = "Nenhuma informação detalhada.";

    [Header("Configuração Visual")]
    public GameObject prefabMolduraUI;
    public RectTransform canvasRect;

    [Header("Efeito de Vida na Moldura")]
    public float amplitudePulso = 0.04f;   // variação de escala (0.04 = 4%)
    public float velocidadePulso = 2f;     // velocidade da "respiração"
    public float amplitudeRotacao = 1.5f;  // graus de balanço leve

    [Header("UI de Texto (Tooltip à Direita)")]
    public TextMeshProUGUI textoTooltip;
    public Vector2 offsetTexto = new Vector2(30f, 0f); // X = Distância para a direita | Y = Ajuste de altura

    [Header("Typewriter (texto revelado no clique)")]
    public float velocidadeTypewriter = 35f; // caracteres por segundo

    private GameObject molduraInstanciada;
    private RectTransform rectMoldura;
    private Canvas canvasPrincipal;

    private bool jaFoiInspecionado = false;
    private Collider2D colisor;
    private Camera cam;

    private Coroutine coroutineTypewriter;

    void Start()
    {
        colisor = GetComponent<Collider2D>();
        cam = Camera.main;

        if (alvoParaSeguir == null)
            alvoParaSeguir = transform;

        if (canvasRect != null)
            canvasPrincipal = canvasRect.GetComponentInParent<Canvas>();

        if (textoTooltip != null)
            textoTooltip.gameObject.SetActive(false);
    }

    void Update()
    {
        if (colisor == null || canvasRect == null) return;

        Vector2 mousePosWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        bool mouseEmCima = colisor.OverlapPoint(mousePosWorld);

        if (mouseEmCima)
        {
            if (prefabMolduraUI != null && molduraInstanciada == null)
            {
                molduraInstanciada = Instantiate(prefabMolduraUI, canvasRect);
                rectMoldura = molduraInstanciada.GetComponent<RectTransform>();
            }

            Camera uiCam = (canvasPrincipal != null && canvasPrincipal.renderMode == RenderMode.ScreenSpaceCamera) ? canvasPrincipal.worldCamera : null;

            Vector3 screenPoint = cam.WorldToScreenPoint(alvoParaSeguir.position);
            Vector3 minScreen = cam.WorldToScreenPoint(colisor.bounds.min);
            Vector3 maxScreen = cam.WorldToScreenPoint(colisor.bounds.max);
            float larguraTela = Mathf.Abs(maxScreen.x - minScreen.x);
            float alturaTela = Mathf.Abs(maxScreen.y - minScreen.y);
            float scaleFactor = canvasPrincipal != null ? canvasPrincipal.scaleFactor : 1f;

            if (rectMoldura != null)
            {
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, uiCam, out localPoint);
                rectMoldura.localPosition = localPoint;

                rectMoldura.sizeDelta = new Vector2((larguraTela + 40f) / scaleFactor, (alturaTela + 40f) / scaleFactor);

                // efeito moldura
                float pulso = 1f + Mathf.Sin(Time.time * velocidadePulso) * amplitudePulso;
                rectMoldura.localScale = new Vector3(pulso, pulso, 1f);
                rectMoldura.localEulerAngles = new Vector3(0f, 0f, Mathf.Sin(Time.time * velocidadePulso * 0.6f) * amplitudeRotacao);
            }

            if (textoTooltip != null)
            {
                textoTooltip.gameObject.SetActive(true);

              
                if (!jaFoiInspecionado)
                    AtualizarTextoUI();

                
                float larguraMolduraPixels = larguraTela + 40f;
                Vector3 posicaoDireita = screenPoint + new Vector3((larguraMolduraPixels / 2f) + offsetTexto.x, offsetTexto.y, 0);

                Vector2 localPointText;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, posicaoDireita, uiCam, out localPointText);
                textoTooltip.rectTransform.localPosition = localPointText;
            }

            if (Input.GetMouseButtonDown(0) && !jaFoiInspecionado)
            {
                jaFoiInspecionado = true;
                IniciarTypewriter();
            }
        }
        else
        {
            if (molduraInstanciada != null)
            {
                Destroy(molduraInstanciada);
            }

            if (textoTooltip != null)
            {
                textoTooltip.gameObject.SetActive(false);
            }

            PararTypewriter();
        }
    }

    void AtualizarTextoUI()
    {
        if (textoTooltip != null)
        {
            if (!jaFoiInspecionado)
            {
                textoTooltip.text = $"[ ? ] {nomeObjeto}\n<size=80%>{descricaoObjeto}</size>\n<i>Clique para Inspecionar</i>";
                textoTooltip.maxVisibleCharacters = int.MaxValue; // garante texto padrão sempre visível por completo
            }
            else
            {
                textoTooltip.text = $"<b>{nomeReal}</b>\n<size=80%>{descricaoReal}</size>";
            }
        }
    }

    void IniciarTypewriter()
    {
        if (textoTooltip == null) return;

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
        string textoCompleto = $"<b>{nomeReal}</b>\n<size=80%>{descricaoReal}</size>";
        textoTooltip.text = textoCompleto;
        textoTooltip.ForceMeshUpdate();

        int totalCaracteresVisiveis = textoTooltip.textInfo.characterCount;
        textoTooltip.maxVisibleCharacters = 0;

        float intervalo = 1f / Mathf.Max(velocidadeTypewriter, 1f);

        for (int i = 0; i <= totalCaracteresVisiveis; i++)
        {
            textoTooltip.maxVisibleCharacters = i;
            yield return new WaitForSeconds(intervalo);
        }

        coroutineTypewriter = null;
    }

    void OnDisable()
    {
        PararTypewriter();
    }

    // Usado pelo comando wipe_inspector
    public void ResetInspecao()
    {
        jaFoiInspecionado = false;
        PararTypewriter();

        if (textoTooltip != null)
            AtualizarTextoUI();
    }
}