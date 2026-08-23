using UnityEngine;

public class EstrelasParallax : MonoBehaviour
{
    [Header("Configurações")]
    public int quantidadeEstrelas = 400;      // Aumente se quiser mais densidade
    public float tamanhoMinimo = 0.1f;        
    public float tamanhoMaximo = 0.5f;        
    public float velocidadeBase = 1f;         
    public float fatorProfundidade = 2f;      

    [Header("Direção do Movimento")]
    public bool moverParaEsquerda = true;     
    public bool moverParaCima = false;        

    [Header("Área de Segurança Fixa")]
    [Tooltip("O maior valor de orthographicSize que a câmera pode atingir (zoom out máximo). Ajuste conforme o seu script de zoom.")]
    public float zoomMaximoOrtho = 20f; // Defina esse valor igual ao maxZoom da sua câmera

    private GameObject[] estrelas;
    private Camera cam;
    private float alturaCamera;
    private float larguraCamera;
    
    // Área de segurança FIXA (não muda com o zoom)
    private float alturaSegura;
    private float larguraSegura;
    private Vector2 centroCamera;

    void Start()
    {
        cam = Camera.main;
        
        // Define a área segura usando o ZOOM MÁXIMO (não o atual)
        alturaSegura = zoomMaximoOrtho * 2f * 2f; // Multiplicado por 2 para garantir que cubra bem
        larguraSegura = alturaSegura * cam.aspect;
        
        CriarEstrelas();
    }

    void CriarEstrelas()
    {
        estrelas = new GameObject[quantidadeEstrelas];

        // O centro da câmera no início é a origem (0,0) ou a posição da câmera
        centroCamera = cam.transform.position;

        for (int i = 0; i < quantidadeEstrelas; i++)
        {
            GameObject estrela = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            estrela.name = "Estrela_" + i;
            
            Destroy(estrela.GetComponent<Collider>());
            estrela.GetComponent<Renderer>().material.color = Color.white;

            float tamanho = Random.Range(tamanhoMinimo, tamanhoMaximo);
            estrela.transform.localScale = Vector3.one * tamanho;

            // Distribui as estrelas na ÁREA DE SEGURANÇA FIXA (grande)
            float x = Random.Range(centroCamera.x - larguraSegura / 2f, centroCamera.x + larguraSegura / 2f);
            float y = Random.Range(centroCamera.y - alturaSegura / 2f, centroCamera.y + alturaSegura / 2f);
            estrela.transform.position = new Vector3(x, y, 0);

            estrela.transform.SetParent(this.transform, true);
            estrelas[i] = estrela;
        }
    }

    void Update()
    {
        // Calcula a área visível ATUAL (para sabermos onde estão as bordas da tela)
        alturaCamera = cam.orthographicSize * 2f;
        larguraCamera = alturaCamera * cam.aspect;
        centroCamera = cam.transform.position;

        Vector2 direcaoMovimento = Vector2.zero;
        if (moverParaEsquerda) direcaoMovimento += Vector2.left;
        else if (!moverParaEsquerda) direcaoMovimento += Vector2.right;

        if (moverParaCima) direcaoMovimento += Vector2.up;
        else if (!moverParaCima) direcaoMovimento += Vector2.down;

        foreach (GameObject estrela in estrelas)
        {
            if (estrela == null) continue;

            float velocidade = velocidadeBase * estrela.transform.localScale.x * fatorProfundidade;
            estrela.transform.position += (Vector3)(direcaoMovimento * velocidade * Time.deltaTime);

            // Reposiciona usando os limites da ÁREA DE SEGURANÇA FIXA (sempre grande)
            float limiteEsquerda = centroCamera.x - larguraSegura / 2f;
            float limiteDireita = centroCamera.x + larguraSegura / 2f;
            float limiteBaixo = centroCamera.y - alturaSegura / 2f;
            float limiteCima = centroCamera.y + alturaSegura / 2f;

            if (estrela.transform.position.x < limiteEsquerda)
            {
                estrela.transform.position = new Vector3(limiteDireita, Random.Range(limiteBaixo, limiteCima), 0);
            }
            else if (estrela.transform.position.x > limiteDireita)
            {
                estrela.transform.position = new Vector3(limiteEsquerda, Random.Range(limiteBaixo, limiteCima), 0);
            }

            if (estrela.transform.position.y < limiteBaixo)
            {
                estrela.transform.position = new Vector3(Random.Range(limiteEsquerda, limiteDireita), limiteCima, 0);
            }
            else if (estrela.transform.position.y > limiteCima)
            {
                estrela.transform.position = new Vector3(Random.Range(limiteEsquerda, limiteDireita), limiteBaixo, 0);
            }
        }
    }
}