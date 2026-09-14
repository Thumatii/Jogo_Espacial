using UnityEngine;
using UnityEngine.SceneManagement;

public class JogadorInterior : MonoBehaviour
{
    private bool pertoDoPainel = false; // Controla se o jogador pode interagir

    void Start()
    {
        if (DadosGlobais.temPosicaoInteriorSalva)
        {
            transform.position = DadosGlobais.posicaoJogadorInterior;
        }
    }

    void Update()
    {
        DadosGlobais.posicaoJogadorInterior = transform.position;
        DadosGlobais.temPosicaoInteriorSalva = true;


        if (Input.GetKeyDown(KeyCode.E) && pertoDoPainel)
        {
            SalvarEstado();
            SceneManager.LoadScene("Space");
        }
    }

    private void SalvarEstado()
    {
        DadosGlobais.posicaoJogadorInterior = transform.position;
        DadosGlobais.temPosicaoInteriorSalva = true;
        DadosGlobais.SalvarNoDisco();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PainelControle>() != null)
        {
            pertoDoPainel = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PainelControle>() != null)
        {
            pertoDoPainel = false;
        }
    }
}