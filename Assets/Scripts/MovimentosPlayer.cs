
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
        movimento.x = Input.GetAxisRaw("Horizontal");
        movimento.y = Input.GetAxisRaw("Vertical");
        movimento = movimento.normalized;

        // teste, vtnc
        if (movimento != Vector2.zero)
        {
            Debug.Log("Movimento detectado: " + movimento);
        }
    }

    void FixedUpdate()
    {
        
        rb.linearVelocity = movimento * velocidade;
    }
}