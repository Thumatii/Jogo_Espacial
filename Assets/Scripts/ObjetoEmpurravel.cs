using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ObjetoEmpurravel : MonoBehaviour
{
    [Header("Física do Empurrão")]
    public float velocidadeMaxima = 3f;
    [Range(0f, 1f)] public float quicada = 0.4f; // 0 = não quica, 1 = quica igual bola de borracha
    public float desaceleracao = 2f; // Linear Damping — quanto maior, mais rápido para

    [Header("Animação (opcional)")]
    [Tooltip("Estado de girar tocando em loop. Velocidade de reprodução acompanha a física.")]
    public Animator animador;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true; // rotação visual é a animação, não a física
        rb.linearDamping = desaceleracao;

        Collider2D colisor = GetComponent<Collider2D>();
        if (colisor != null)
        {
            PhysicsMaterial2D material = new PhysicsMaterial2D("MaterialEmpurravel");
            material.friction = 0.1f;
            material.bounciness = quicada;
            colisor.sharedMaterial = material;
        }

        if (animador == null)
            animador = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (rb.linearVelocity.magnitude > velocidadeMaxima)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * velocidadeMaxima;
        }

        if (animador != null)
        {
            float razao = rb.linearVelocity.magnitude / velocidadeMaxima;
            animador.speed = Mathf.Clamp01(razao);
        }
    }
}