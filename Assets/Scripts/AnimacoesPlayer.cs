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
        
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        bool correndo = (moveX != 0 || moveY != 0);

        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direcaoMouse = (mousePos - transform.position).normalized;

        float angulo = Mathf.Atan2(direcaoMouse.y, direcaoMouse.x) * Mathf.Rad2Deg;
        if (angulo < 0) angulo += 360f;

        int setor = Mathf.FloorToInt((angulo + 22.5f) / 45f) % 8;

        // Prefixo do movimento
        string nomeAnimacao = correndo ? "Walking_" : "Idle_";

        
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
            case 6: // frente
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