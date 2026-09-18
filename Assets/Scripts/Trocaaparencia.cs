using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TrocaAparencia : MonoBehaviour
{
    [Header("Opções de Aparência (5 a 10 sprites)")]
    public Sprite[] opcoes;

    private SpriteRenderer sr;
    private Inspetor inspetor;
    private int indiceAtual = 0;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        inspetor = GetComponent<Inspetor>();
    }

    void Start()
    {
        if (opcoes == null || opcoes.Length == 0) return;

        indiceAtual = DadosGlobais.CarregarAparenciaEscolhida(ObterId(), 0);
        indiceAtual = Mathf.Clamp(indiceAtual, 0, opcoes.Length - 1);
        AplicarAparencia();
    }

    public void ProximaAparencia()
    {
        if (opcoes == null || opcoes.Length == 0) return;

        indiceAtual = (indiceAtual + 1) % opcoes.Length;
        AplicarAparencia();
        DadosGlobais.SalvarAparenciaEscolhida(ObterId(), indiceAtual);
    }

    public void AparenciaAnterior()
    {
        if (opcoes == null || opcoes.Length == 0) return;

        indiceAtual = (indiceAtual - 1 + opcoes.Length) % opcoes.Length;
        AplicarAparencia();
        DadosGlobais.SalvarAparenciaEscolhida(ObterId(), indiceAtual);
    }

    void AplicarAparencia()
    {
        if (sr != null) sr.sprite = opcoes[indiceAtual];
    }

    string ObterId()
    {
        return inspetor != null ? inspetor.idUnico : gameObject.name;
    }

    public Sprite SpriteAtual => (opcoes != null && opcoes.Length > 0) ? opcoes[indiceAtual] : null;
    public Sprite SpriteProximo => (opcoes != null && opcoes.Length > 0) ? opcoes[(indiceAtual + 1) % opcoes.Length] : null;
    public Sprite SpriteAnterior => (opcoes != null && opcoes.Length > 0) ? opcoes[(indiceAtual - 1 + opcoes.Length) % opcoes.Length] : null;
}