using UnityEngine;
using TMPro;
using UnityEngine.UI;

// Cole no MESMO objeto de um texto TMP (TextMeshProUGUI) pra ele ganhar um
// fundo preto translúcido que acompanha o tamanho do texto sozinho — não
// precisa montar nada no Canvas, o fundo é criado em código.
// Cor/opacidade e espaçamento são ajustáveis aqui no Inspector.
[RequireComponent(typeof(TextMeshProUGUI))]
public class PainelComFundo : MonoBehaviour
{
    [Header("Aparência do Fundo")]
    public Color corFundo = new Color(0f, 0f, 0f, 0.7f); // preto, 70% opaco por padrão
    public float paddingHorizontal = 20f; // espaço extra nas laterais, além do texto
    public float paddingVertical = 15f;   // espaço extra em cima/baixo, além do texto

    [Header("Espaçamento do Texto")]
    public float espacamentoLinha = 15f; // TMP usa unidade própria, não pixels — ajuste conforme o visual

    private TextMeshProUGUI texto;      // o texto que esse script está "servindo"
    private RectTransform rectTexto;    // o retângulo (posição/tamanho) do texto
    private Image fundo;                // a imagem de fundo criada em código
    private RectTransform rectFundo;    // o retângulo do fundo

    void Awake()
    {
        texto = GetComponent<TextMeshProUGUI>();
        rectTexto = texto.rectTransform;
        CriarFundo();
    }

    // Cria uma Image simples do mesmo pai do texto, posicionada ANTES dele
    // na hierarquia — isso faz ela renderizar ATRÁS do texto, não por cima.
    void CriarFundo()
    {
        GameObject fundoObj = new GameObject("FundoAuto_" + gameObject.name, typeof(RectTransform), typeof(Image));
        fundoObj.transform.SetParent(transform.parent, false);
        fundoObj.transform.SetSiblingIndex(transform.GetSiblingIndex()); // antes do texto = atrás dele

        fundo = fundoObj.GetComponent<Image>();
        fundo.color = corFundo;
        fundo.raycastTarget = false; // fundo nunca bloqueia clique em nada

        rectFundo = fundoObj.GetComponent<RectTransform>();

        // Usa a MESMA ancoragem/pivot do texto, pra ficar sempre alinhado com ele
        rectFundo.anchorMin = rectTexto.anchorMin;
        rectFundo.anchorMax = rectTexto.anchorMax;
        rectFundo.pivot = rectTexto.pivot;
    }

    // Roda depois do texto já ter atualizado nesse frame — garante que o
    // preferredWidth/Height já reflete o conteúdo atual do texto.
    void LateUpdate()
    {
        if (texto == null || rectFundo == null) return;

        texto.lineSpacing = espacamentoLinha;

        // O desalinhamento do fundo vinha daqui: o RETÂNGULO do texto podia
        // ter um tamanho diferente do CONTEÚDO real (preferredWidth/Height).
        // Redimensionando o texto pro tamanho real também, os dois (texto e
        // fundo) passam a usar a mesma referência, sem ficar desalinhado.
        float largura = texto.preferredWidth;
        float altura = texto.preferredHeight;
        rectTexto.sizeDelta = new Vector2(largura, altura);

        // Acompanha a posição do texto, caso ele se mova (ex: segue a nave)
        rectFundo.anchoredPosition = rectTexto.anchoredPosition;
        rectFundo.sizeDelta = new Vector2(largura + paddingHorizontal * 2f, altura + paddingVertical * 2f);

        fundo.color = corFundo; // permite mudar cor/opacidade em tempo real pelo Inspector
    }

    // Esconde/mostra o fundo junto com o texto automaticamente — se o
    // GameController (ou qualquer script) desativar o texto, o fundo
    // desaparece junto sem precisar de código extra em nenhum outro lugar.
    void OnEnable()
    {
        if (fundo != null) fundo.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        if (fundo != null) fundo.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (fundo != null) Destroy(fundo.gameObject);
    }
}