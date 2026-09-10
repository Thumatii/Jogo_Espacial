using UnityEngine;

public class RotacaoPlaneta : MonoBehaviour
{
    [Header("Configuração de Tempo")]
    [Tooltip("Tempo em segundos para o planeta dar uma volta completa (360 graus caso voce seja um orangutango selvagem:D).")]
    public float TempoDaVolta; 

    [Header("Eixo de Rotação")]
    public Vector3 eixoDeRotacao = Vector3.up; 

    void Update()
    {
        
        if (TempoDaVolta <= 0f) return;

        // Graus/Segndo
        float grausPorSegundo = 360f / TempoDaVolta;

       
        transform.Rotate(eixoDeRotacao * grausPorSegundo * Time.deltaTime);
    }
}