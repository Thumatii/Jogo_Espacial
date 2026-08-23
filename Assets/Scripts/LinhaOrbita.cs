using UnityEngine;

public class OrbitaVisual : MonoBehaviour
{
    [Header("Configurações")]
    public int quantidadePontos = 40;          // Quantidade de pontinhos
    public float tamanhoPonto = 0.2f;           // Tamanho de cada pontinho
    public Color corPontos = Color.cyan;        // Cor dos pontos

    private GameObject[] pontos;
    private Planet planeta;
    private bool foiCriado = false;

    void Start()
    {
        planeta = GetComponentInParent<Planet>();
        CriarPontos(); // Cria os pontos no início
        Desativar();   // Começa invisível
    }

    void CriarPontos()
    {
        if (foiCriado) return;
        foiCriado = true;

        pontos = new GameObject[quantidadePontos];

        for (int i = 0; i < quantidadePontos; i++)
        {
            // Cria uma esfera primitiva (funciona em 2D também)
            GameObject ponto = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ponto.name = "PontoOrbita_" + i;
            
            // Remove o Colisor (para não atrapalhar a física)
            Destroy(ponto.GetComponent<Collider>());

            // =========================================================
            // CORREÇÃO: Força a esfera a respeitar a ordem 2D (ficar atrás da nave)
            // =========================================================
            // 1. Troca o material para o Sprites/Default (essencial para 2D)
            ponto.GetComponent<Renderer>().material = new Material(Shader.Find("Sprites/Default"));
            
            // 2. Define a cor do ponto
            ponto.GetComponent<Renderer>().material.color = corPontos;
            
            // 3. Coloca o ponto em uma camada bem negativa para ficar ATRÁS da nave
            ponto.GetComponent<Renderer>().sortingOrder = -10;
            // =========================================================

            // Define o tamanho
            ponto.transform.localScale = Vector3.one * tamanhoPonto;

            // Coloca como filho do planeta para organizar
            ponto.transform.SetParent(this.transform, false);

            // Deixa invisível por padrão
            ponto.SetActive(false);

            // Salva na lista
            pontos[i] = ponto;
        }
    }

    // Função chamada pela Nave quando entra em órbita
    public void Ativar(float distanciaOrbita)
    {
        if (!foiCriado) CriarPontos(); // Garante que existe
        if (planeta == null) planeta = GetComponentInParent<Planet>();

        float raioTotal = planeta.raio + distanciaOrbita;
        Vector3 posPlaneta = planeta.transform.position;

        // Posiciona cada ponto
        for (int i = 0; i < quantidadePontos; i++)
        {
            float angulo = (i / (float)quantidadePontos) * 360f * Mathf.Deg2Rad;
            
            // Calcula a posição no mundo (ignora escala do pai)
            float x = posPlaneta.x + Mathf.Cos(angulo) * raioTotal;
            float y = posPlaneta.y + Mathf.Sin(angulo) * raioTotal;

            pontos[i].transform.position = new Vector3(x, y, 0);
            pontos[i].SetActive(true);
        }
    }

    public void Desativar()
    {
        if (pontos == null) return;

        foreach (GameObject p in pontos)
        {
            if (p != null) p.SetActive(false);
        }
    }
}