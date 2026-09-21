using UnityEngine;
using TMPro;

// Cole esse script no MESMO objeto do texto que vai mostrar o relógio,
// no topo do Canvas de cada cena (um por cena — o dado em si mora no
// GerenciadorTempo, que é o que persiste entre cenas).
[RequireComponent(typeof(TextMeshProUGUI))]
public class RelogioUI : MonoBehaviour
{
    public TextMeshProUGUI texto;

    void Awake()
    {
        if (texto == null) texto = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (texto == null || GerenciadorTempo.Instancia == null) return;
        texto.text = GerenciadorTempo.Instancia.TextoFormatado();
    }
}
