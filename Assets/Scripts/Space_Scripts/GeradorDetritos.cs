using UnityEngine;

// Coloque num objeto vazio na cena de Space. Cria um campo de detritos
// espalhados, cada um com tamanho/dano/cor diferente.
// Não precisa de prefab nenhum — cria as formas sozinho, mas você pode
// arrastar um prefab talvez
public class GeradorDetritos : MonoBehaviour
{
    [Header("Configuração")]
    public GameObject prefabDetrito; // opcional — se vazio, cria formas simples sozinho
    public int quantidade = 30;
    public float tamanhoMinimo = 0.3f;
    public float tamanhoMaximo = 1.3f;
    public float danoMinimo = 3f;
    public float danoMaximo = 12f;
    public float velocidadeDerivaMinima = 0.2f;
    public float velocidadeDerivaMaxima = 0.8f;

    [Header("Área de Espalhamento")]
    public Vector2 centro = Vector2.zero;
    public float raioAreaX = 60f;
    public float raioAreaY = 60f;

    void Start()
    {
        for (int i = 0; i < quantidade; i++)
        {
            CriarDetrito();
        }
    }

    void CriarDetrito()
    {
        GameObject detrito;

        if (prefabDetrito != null)
        {
            detrito = Instantiate(prefabDetrito);
        }
        else
        {
            detrito = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(detrito.GetComponent<Collider>());
            detrito.AddComponent<CircleCollider2D>();

            Rigidbody2D rb = detrito.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearVelocity = Random.insideUnitCircle.normalized * Random.Range(velocidadeDerivaMinima, velocidadeDerivaMaxima);

            Renderer renderer = detrito.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));

            // Tons de cinza/marrom variados — cada um com sua própria cor via
            // MaterialPropertyBlock, sem quebrar o compartilhamento do material.
            MaterialPropertyBlock bloco = new MaterialPropertyBlock();
            float tom = Random.Range(0.25f, 0.55f);
            bloco.SetColor("_Color", new Color(tom, tom * 0.9f, tom * 0.8f));
            renderer.SetPropertyBlock(bloco);
            renderer.sortingOrder = -5;
        }

        float tamanho = Random.Range(tamanhoMinimo, tamanhoMaximo);
        detrito.transform.localScale = Vector3.one * tamanho;

        Vector2 pos = centro + new Vector2(Random.Range(-raioAreaX, raioAreaX), Random.Range(-raioAreaY, raioAreaY));
        detrito.transform.position = pos;

        DetritoEspacial script = detrito.GetComponent<DetritoEspacial>();
        if (script == null) script = detrito.AddComponent<DetritoEspacial>();

        // Detrito maior = mais dano
        float proporcaoTamanho = Mathf.InverseLerp(tamanhoMinimo, tamanhoMaximo, tamanho);
        script.dano = Mathf.Lerp(danoMinimo, danoMaximo, proporcaoTamanho);
        script.velocidadeRotacao = Random.Range(-30f, 30f);
    }
}
