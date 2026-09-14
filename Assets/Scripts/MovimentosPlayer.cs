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
        movimento = new Vector2(moveX, moveY).normalized;
    }

    void FixedUpdate()
    {
        
        rb.linearVelocity = movimento * velocidade;
    }
}