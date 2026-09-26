using System.Collections;
using UnityEngine;
using TMPro;

// Um por cena. Arraste um texto TMP nele — qualquer script chama
// NotificacaoUI.Instancia?.Mostrar("mensagem") pra exibir um aviso
// temporário, sem precisar saber nada sobre UI.
public class NotificacaoUI : MonoBehaviour
{
    public static NotificacaoUI Instancia;

    public TextMeshProUGUI texto;
    public float duracaoPadrao = 3f;

    private Coroutine coroutineAtual;

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        Instancia = this;

        if (texto != null) texto.gameObject.SetActive(false);
    }

    public void Mostrar(string mensagem, float duracao = -1f)
    {
        if (texto == null) return;
        if (duracao < 0f) duracao = duracaoPadrao;

        if (coroutineAtual != null) StopCoroutine(coroutineAtual);
        coroutineAtual = StartCoroutine(MostrarCoroutine(mensagem, duracao));
    }

    IEnumerator MostrarCoroutine(string mensagem, float duracao)
    {
        texto.text = mensagem;
        texto.gameObject.SetActive(true);

        yield return new WaitForSeconds(duracao);

        texto.gameObject.SetActive(false);
        coroutineAtual = null;
    }
}
