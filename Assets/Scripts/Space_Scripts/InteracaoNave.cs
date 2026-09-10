using UnityEngine;
using TMPro;

public class InteracaoNave : MonoBehaviour
{
    public GameObject naveParada;     // A nave parada no chão do planeta
    public TextMeshProUGUI txtInteragir; // O texto "Interagir E"

    void Update()
    {
        if (naveParada == null || txtInteragir == null) return;

        float dist = Vector2.Distance(transform.position, naveParada.transform.position);

        if (dist < 3f) // Se estiver perto
        {
            // Faz o texto aparecer acima da nave (na tela)
            Vector3 posTela = Camera.main.WorldToScreenPoint(naveParada.transform.position + new Vector3(0, 2f, 0));
            txtInteragir.transform.position = posTela;
            txtInteragir.gameObject.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                // Acha o GameController e manda sair do planeta
                FindObjectOfType<GameController>().SairDoPlaneta();
            }
        }
        else
        {
            txtInteragir.gameObject.SetActive(false);
        }
    }
}