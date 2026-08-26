using UnityEngine;
using TMPro;

public class ContadorFPS : MonoBehaviour
{
    [Tooltip("Text")]
    public TextMeshProUGUI textoFPS;

    [Tooltip("Quanto tempo até atualizar o FPS")]
    public float tempoDeAtualizacao = 0.5f;

    private float timer;

    
    private int maxFPS = 0;
    private int minFPS = 9999;
    private int totalFrames = 0;
    private float tempoTotal = 0f;

    void Update()
    {
        // Calculo do FPS
        int fpsAtual = (int)(1f / Time.unscaledDeltaTime);

        //Soma na média
        totalFrames++;
        tempoTotal += Time.unscaledDeltaTime;

        
        if (Time.unscaledTime > 1f)
        {
            if (fpsAtual > maxFPS) maxFPS = fpsAtual;
            if (fpsAtual < minFPS) minFPS = fpsAtual;
        }

        
        if (Time.unscaledTime > timer)
        {
            int mediaFPS = (int)(totalFrames / tempoTotal);


            textoFPS.text = $"FPS: {fpsAtual} | Média: {mediaFPS} | Máx: {maxFPS} | Mín: {minFPS}";

            // Cores baseadas nas médias de FPS
            if (fpsAtual >= 60)
            {
                textoFPS.color = Color.green;
            }
            else if (fpsAtual >= 30)
            {
                textoFPS.color = Color.yellow;
            }
            else
            {
                textoFPS.color = Color.red;
            }
            // : D

            timer = Time.unscaledTime + tempoDeAtualizacao;
        }
    }
}