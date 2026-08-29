using UnityEngine;
using UnityEngine.UI;

public class FullScreenManager : MonoBehaviour
{
    public Toggle toggleFullscreen;

    [Header("Configuração de Sprites")]
    public Image imagemDoToggle;      // Arraste a Image (pode ser o Background)
    public Sprite spriteFullscreen;   // O Sprite X (quando estiver em tela cheia)
    public Sprite spriteJanela;       // O Sprite Y (quando estiver em modo janela)

    void Start()
    {
        bool estaFullscreen = PlayerPrefs.GetInt("FullscreenState", 1) == 1;

        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        Screen.fullScreen = estaFullscreen;

        if (toggleFullscreen != null)
        {
            toggleFullscreen.isOn = estaFullscreen;
            AtualizarVisual(estaFullscreen); // Aplica o sprite correto ao iniciar

            toggleFullscreen.onValueChanged.RemoveAllListeners();
            toggleFullscreen.onValueChanged.AddListener(AlternarFullscreen);
        }
    }

    public void AlternarFullscreen(bool isFullscreen)
    {
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        Screen.fullScreen = isFullscreen;

        PlayerPrefs.SetInt("FullscreenState", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        AtualizarVisual(isFullscreen); // Troca o sprite na hora do clique
    }

    void AtualizarVisual(bool isFullscreen)
    {
        if (imagemDoToggle != null)
        {
            
            imagemDoToggle.sprite = isFullscreen ? spriteFullscreen : spriteJanela;
        }
    }
    //rafa se estiver lendo isso são 00:34 to morrendo de sono eloucuras acabei de desocbrir commo salvara aqrquivos internos dentro do jogo yiippie aaaaaaaaaaaaaaaa
}