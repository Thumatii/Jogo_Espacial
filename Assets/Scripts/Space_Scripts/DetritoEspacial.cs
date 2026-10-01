using UnityEngine;

public class DetritoEspacial : MonoBehaviour
{
    public float dano = 5f;
    public float velocidadeRotacao = 10f;

    void Update()
    {
        transform.Rotate(0f, 0f, velocidadeRotacao * Time.deltaTime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TratarColisao(collision.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TratarColisao(other.gameObject);
    }

    void TratarColisao(GameObject alvo)
    {
        Nave nave = alvo.GetComponent<Nave>();
        if (nave == null) return;

        nave.ReceberDano(dano);
        Destroy(gameObject);
    }
}
