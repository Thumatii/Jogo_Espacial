using UnityEngine;

public class GerenciadorSave : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnApplicationQuit()
    {
        DadosGlobais.SalvarNoDisco();
    }
}