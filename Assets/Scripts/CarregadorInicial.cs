using UnityEngine;
using UnityEngine.SceneManagement;

public class CarregadorInicial : MonoBehaviour
{
    public static CarregadorInicial instancia;
    private static bool jaCarregouNoBoot = false;

    void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (!jaCarregouNoBoot)
        {
            jaCarregouNoBoot = true;

            // Carrega dados salvos e força o início sempre na cena de dentro da nave
            DadosGlobais.CarregarDoDisco();

            if (SceneManager.GetActiveScene().name != "InteriorNave")
            {
                SceneManager.LoadScene("InteriorNave");
            }
        }
    }
}