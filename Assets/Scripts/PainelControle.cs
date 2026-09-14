using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PainelControle : MonoBehaviour
{
    [Header("Configuração de Cena")]
    public string nomeCenaEspaco = "Space";

    [Header("Aviso De Tecla")]
    public GameObject objetoTextoAviso;

    private bool jogadorNaArea = false;

    void Start()
    {
        if (objetoTextoAviso != null)
            objetoTextoAviso.SetActive(false);
    }

    void Update()
    {
        if (jogadorNaArea && Input.GetKeyDown(KeyCode.E))
        {
            SceneManager.LoadScene(nomeCenaEspaco);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jogadorNaArea = true;
            if (objetoTextoAviso != null) objetoTextoAviso.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jogadorNaArea = false;
            if (objetoTextoAviso != null) objetoTextoAviso.SetActive(false);
        }
    }
}