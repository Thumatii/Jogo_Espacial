using UnityEngine;

public class Planet : MonoBehaviour
{
    public string nomePlaneta;
    public float raio = 2f;
    public float massa = 5f; 

    [Header("Informações para o Satélite/Tabela")]
    public float forcaGravitacional;
    public bool temVida;
    public string tipoVida; // Ex: "Nenhuma", "Microbiana", "Inteligente"
    public bool temSeresInteligentes;

    [Header("Exploração e Satélite")]
    public bool explorado = false; 
    public bool sateliteLancado = false; // <--- NOVA FLAG: impede nova mensagem
}