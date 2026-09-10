using UnityEngine;

public class CameraRigController : MonoBehaviour
{
    public Transform jogador; // Arraste o seu personagem aqui no Inspector
    public float velocidadeRotacao = 10f;

    private float anguloAlvo = 0f;

    void Update()
    {
        // Gira 90 graus para a esquerda (Q) ou direita (E)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            anguloAlvo += 90f;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            anguloAlvo -= 90f;
        }

        // Suaviza a rotação do Rig
        Quaternion rotacaoDesejada = Quaternion.Euler(0, 0, anguloAlvo);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacaoDesejada, velocidadeRotacao * Time.deltaTime);
    }

    void LateUpdate()
    {
        if (jogador != null)
        {
            
            transform.position = jogador.position;
        }
    }
}