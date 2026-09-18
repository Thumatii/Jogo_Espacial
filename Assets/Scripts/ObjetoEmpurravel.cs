using UnityEngine;

// Empurrar/deslizar/bater na parede é o Rigidbody2D + Collider2D padrão do Unity
// já fazendo o trabalho (o jogador empurra por contato, o Linear Damping freia o
// deslize, e as paredes com Collider2D barram sozinhas). Esse script só evita que
// o objeto saia voando rápido demais ou comece a girar de forma estranha.
[RequireComponent(typeof(Rigidbody2D))]
public class ObjetoEmpurravel : MonoBehaviour
{
    [Header("Limite de Física")]
    public float velocidadeMaxima = 3f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true; // não deixa girar ao ser empurrado fora do centro
    }

    void FixedUpdate()
    {
        if (rb.linearVelocity.magnitude > velocidadeMaxima)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * velocidadeMaxima;
        }
    }
}
