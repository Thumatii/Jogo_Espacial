using UnityEngine;

public class OrbitaLine : MonoBehaviour
{
    [Header("Configurações do Círculo")]
    public int segmentos = 40;            // Quantidade de "tracinhos"
    public int pontosPorSegmento = 4;     // Pontos que formam cada tracinho
    public float anguloEspaco = 4f;       // Tamanho do espaço entre os traços

    private LineRenderer line;
    private Planet planeta;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        planeta = GetComponentInParent<Planet>(); // Pega o planeta pai
        
        // CORREÇÃO IMPORTANTE: Usamos o espaço do MUNDO para a escala não interferir
        line.useWorldSpace = true; 
        line.enabled = false;
        line.loop = false;
    }

    public void Ativar(float distanciaOrbita)
    {
        if (planeta == null) planeta = GetComponentInParent<Planet>();

        // O raio total é a soma do raio do planeta + a distância da órbita
        float raioTotal = planeta.raio + distanciaOrbita;

        // Calcula o total de pontos (tracinhos * pontos por tracinho)
        int totalPontos = segmentos * pontosPorSegmento;
        line.positionCount = totalPontos;

        float anguloPorSegmento = 360f / segmentos;
        float anguloAtual = 0f;

        // Guarda a posição do planeta no mundo (para somar ao círculo)
        Vector2 posPlaneta = planeta.transform.position;

        for (int i = 0; i < segmentos; i++)
        {
            // Desenha cada pedacinho do traço
            for (int j = 0; j < pontosPorSegmento; j++)
            {
                float angulo = (anguloAtual + (anguloEspaco / 2f) + (j * (pontosPorSegmento / 100f))) * Mathf.Deg2Rad;
                
                // Calcula a posição do ponto no espaço do mundo (posição do planeta + ponto local)
                float x = Mathf.Cos(angulo) * raioTotal;
                float y = Mathf.Sin(angulo) * raioTotal;
                
                line.SetPosition((i * pontosPorSegmento) + j, new Vector3(posPlaneta.x + x, posPlaneta.y + y, 0));
            }

            anguloAtual += anguloPorSegmento;
        }

        line.enabled = true;
    }

    public void Desativar()
    {
        line.enabled = false;
    }
}