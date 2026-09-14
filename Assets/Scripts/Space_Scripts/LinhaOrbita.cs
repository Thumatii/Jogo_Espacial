using UnityEngine;

public class OrbitaVisual : MonoBehaviour
{
    [Header("Configurações")]
    public int quantidadePontos = 40;
    public float tamanhoPonto = 0.2f;
    public Color corPontos = Color.cyan;

    private GameObject[] pontos;
    private Planet planeta;
    private bool foiCriado = false;

    void Awake()
    {
        planeta = GetComponentInParent<Planet>();
        CriarPontos();
        Desativar();
    }

    void CriarPontos()
    {
        if (foiCriado) return;
        foiCriado = true;

        pontos = new GameObject[quantidadePontos];

        for (int i = 0; i < quantidadePontos; i++)
        {
            GameObject ponto = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ponto.name = "PontoOrbita_" + i;

            Destroy(ponto.GetComponent<Collider>());

            ponto.GetComponent<Renderer>().material = new Material(Shader.Find("Sprites/Default"));
            ponto.GetComponent<Renderer>().material.color = corPontos;
            ponto.GetComponent<Renderer>().sortingOrder = -10;

            ponto.transform.localScale = Vector3.one * tamanhoPonto;
            ponto.transform.SetParent(this.transform, false);
            ponto.SetActive(false);

            pontos[i] = ponto;
        }
    }

    public void Ativar(float distanciaOrbita)
    {
        if (!foiCriado) CriarPontos();
        if (planeta == null) planeta = GetComponentInParent<Planet>();

        float raioTotal = planeta.raio + distanciaOrbita;
        Vector3 posPlaneta = planeta.transform.position;

        for (int i = 0; i < quantidadePontos; i++)
        {
            float angulo = (i / (float)quantidadePontos) * 360f * Mathf.Deg2Rad;

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