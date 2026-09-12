using UnityEngine;

public class MovimentosPlayer : MonoBehaviour
{
    public float velocidade = 5f;
    private Rigidbody2D rb;
    private Vector2 movimento;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // Calcula a direção usando Vector3 para evitar o erro de eixo Z
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direcaoMouse = (mousePos - transform.position);

        if (direcaoMouse.sqrMagnitude > 0.001f)
        {
            direcaoMouse.Normalize();
        }
        else
        {
            direcaoMouse = Vector2.up;
        }

        Vector2 direcaoFrente = direcaoMouse;
        Vector2 direcaoDireita = new Vector2(direcaoFrente.y, -direcaoFrente.x);

        movimento = (direcaoFrente * moveY + direcaoDireita * moveX).normalized;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movimento * velocidade;
    }
}