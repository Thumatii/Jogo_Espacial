using UnityEngine;

public class Inspetor : MonoBehaviour
{
    [Header("Identificador Único (usado pelo save)")]
    [Tooltip("Se deixar vazio, usa o nome do GameObject. Preencha manualmente em objetos duplicados pra evitar ids repetidos (ex: dois planetas com o mesmo nome).")]
    public string idUnico;

    [Header("Objeto a Ser Seguido (opcional)")]
    public Transform alvoParaSeguir;

    [Header("Informações Secretas (Reveladas no clique)")]
    public string nomeReal = "Planeta Revelado";
    [TextArea(2, 4)] public string descricaoReal = "Descrição secreta do objeto.";

    [Header("Informações do Objeto (Estado Padrão)")]
    public string nomeObjeto = "Desconhecido";
    [TextArea(2, 4)] public string descricaoObjeto = "Nenhuma informação detalhada.";

    [HideInInspector] public bool jaFoiInspecionado = false;

    public Collider2D Colisor { get; private set; }
    public Transform Alvo => alvoParaSeguir != null ? alvoParaSeguir : transform;

    void Awake()
    {
        Colisor = GetComponent<Collider2D>();

        if (string.IsNullOrEmpty(idUnico))
            idUnico = gameObject.name;
    }

    void Start()
    {
        // Carrega do save: se esse objeto já foi descoberto antes, já nasce revelado
        jaFoiInspecionado = DadosGlobais.ObjetoJaFoiInspecionado(idUnico);
    }

    public void MarcarInspecionado()
    {
        jaFoiInspecionado = true;
        DadosGlobais.MarcarObjetoInspecionado(idUnico);
    }

    // Usado pelo comando "wipe_inspector" do Debug Console
    public void ResetInspecao()
    {
        jaFoiInspecionado = false;
        DadosGlobais.DesmarcarObjetoInspecionado(idUnico);

        if (GerenciadorInspetor.Instancia != null)
            GerenciadorInspetor.Instancia.AoResetarInspetor(this);
    }
}