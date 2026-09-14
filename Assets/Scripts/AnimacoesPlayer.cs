using UnityEngine;

public class AnimacaoPlayer : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer sr;

    void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Pega o input do teclado
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        Vector2 movimento = new Vector2(moveX, moveY).normalized;

        bool correndo = movimento != Vector2.zero;
        float angulo;

        if (correndo)
        {
            // Se andando, olha para a direção do WASD
            angulo = Mathf.Atan2(movimento.y, movimento.x) * Mathf.Rad2Deg;
        }
        else
        {
            // Se parado, olha para a direção do Mouse
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direcaoMouse = (mousePos - transform.position).normalized;
            angulo = Mathf.Atan2(direcaoMouse.y, direcaoMouse.x) * Mathf.Rad2Deg;
        }

        // Converte ângulo negativo para 0-360 e calcula o setor (0 a 7)
        if (angulo < 0) angulo += 360f;
        int setor = Mathf.FloorToInt((angulo + 22.5f) / 45f) % 8;

        string nomeAnimacao = correndo ? "Walking_" : "Idle_";

        // Toca a animação correspondente
        switch (setor)
        {
            case 0: // Direita
                anim.Play(nomeAnimacao + "Lado");
                sr.flipX = false;
                break;
            case 1: // Diagonal de costas direita
                anim.Play(nomeAnimacao + "DiagCostas");
                sr.flipX = false;
                break;
            case 2: // Costas
                anim.Play(nomeAnimacao + "Costas");
                sr.flipX = false;
                break;
            case 3: // Diagonal de costas esquerda
                anim.Play(nomeAnimacao + "DiagCostas");
                sr.flipX = true;
                break;
            case 4: // Esquerda
                anim.Play(nomeAnimacao + "Lado");
                sr.flipX = true;
                break;
            case 5: // Diagonal de frente esquerda
                anim.Play(nomeAnimacao + "DiagFrente");
                sr.flipX = true;
                break;
            case 6: // Frente
                anim.Play(nomeAnimacao + "Frente");
                sr.flipX = false;
                break;
            case 7: // Diagonal de frente direita
                anim.Play(nomeAnimacao + "DiagFrente");
                sr.flipX = false;
                break;
        }
    }
}