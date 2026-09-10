using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlanetIndicator : MonoBehaviour
{
    [Header("Referências")]
    public Planet planetaAlvo;         // Arraste o planeta aqui
    public RectTransform indicadorRect; // Arraste o objeto pai (IndicadorUI) aqui
    public Image setaImage;             // Arraste a Imagem (Seta) aqui
    public TextMeshProUGUI nomeText;    // Arraste o Texto (NomePlaneta) aqui
    public float margemBorda = 50f;     // Distância da seta até a borda da tela

    private Camera cameraEspaco;

    void Start()
    {
        cameraEspaco = Camera.main;
        if (indicadorRect != null) indicadorRect.gameObject.SetActive(false);
        
        if (planetaAlvo == null) planetaAlvo = GetComponent<Planet>();
    }

    void Update()
    {
        if (planetaAlvo == null || cameraEspaco == null || indicadorRect == null) return;

        // Pega a posição do planeta na tela da câmera
        Vector3 viewportPos = cameraEspaco.WorldToViewportPoint(planetaAlvo.transform.position);

        // Verifica se o planeta está DENTRO da câmera
        bool estaDentro = viewportPos.x >= 0 && viewportPos.x <= 1 &&
                          viewportPos.y >= 0 && viewportPos.y <= 1 &&
                          viewportPos.z > 0;

        if (estaDentro)
        {
            indicadorRect.gameObject.SetActive(false); // Some se estiver na tela
            return;
        }

        // Se estiver fora, a seta aparece
        indicadorRect.gameObject.SetActive(true);

        // Pega a posição do planeta em pixels
        Vector2 posicaoTela = cameraEspaco.WorldToScreenPoint(planetaAlvo.transform.position);
        Vector2 centroTela = new Vector2(Screen.width, Screen.height) / 2f;

        // Calcula a direção e rotaciona a seta
        Vector2 direcao = posicaoTela - centroTela;
        
        // ATENÇÃO: Se a sua seta for desenhada apontando para CIMA, use "- 90f".
        // Se a seta for desenhada apontando para DIREITA, remova o "- 90f".
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg - 90f; 
        
        // Aplica a rotação APENAS na Imagem da Seta
        setaImage.rectTransform.rotation = Quaternion.Euler(0, 0, angulo);

        // ==========================================
        // TRAVAS ABSOLUTAS PARA O TEXTO NÃO DISTORCER
        // ==========================================
        // Garante que o texto SEMPRE fique com rotação 0 (reto para cima)
        nomeText.rectTransform.localRotation = Quaternion.Euler(0, 0, 0);
        
        // Garante que a escala do texto nunca seja esticada ou comprimida
        nomeText.rectTransform.localScale = Vector3.one; 
        // ==========================================

        // Prende a seta e o texto (o objeto pai) nas bordas da tela
        float xLimitado = Mathf.Clamp(posicaoTela.x, margemBorda, Screen.width - margemBorda);
        float yLimitado = Mathf.Clamp(posicaoTela.y, margemBorda, Screen.height - margemBorda);
        indicadorRect.position = new Vector3(xLimitado, yLimitado, 0);

        // Atualiza o nome do planeta
        nomeText.text = planetaAlvo.nomePlaneta;
    }
}