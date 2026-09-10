using UnityEngine;

public class SpaceshipController : MonoBehaviour
{
    [Header("Movimento")]
    public float aceleracao = 5f;
    public float velocidadeMaxima = 10f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector2 direcao = Vector2.zero;

        // Cima
        if (Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.UpArrow))
        {
            direcao.y += 1;
        }

        // Baixo
        if (Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.DownArrow))
        {
            direcao.y -= 1;
        }

        // Esquerda
        if (Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.LeftArrow))
        {
            direcao.x -= 1;
        }

        // Direita
        if (Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.RightArrow))
        {
            direcao.x += 1;
        }

        // Normaliza para a nave não andar mais rápido
        // quando apertamos duas teclas ao mesmo tempo
        if (direcao != Vector2.zero)
        {
            direcao.Normalize();

            rb.AddForce(
                direcao * aceleracao,
                ForceMode2D.Force
            );
        }

        // Limita a velocidade
        if (rb.linearVelocity.magnitude > velocidadeMaxima)
        {
            rb.linearVelocity =
                rb.linearVelocity.normalized *
                velocidadeMaxima;
        }
    }
}