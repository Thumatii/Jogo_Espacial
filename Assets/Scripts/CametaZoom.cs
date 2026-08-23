using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [Header("Configurações do Zoom")]
    public float zoomSpeed = 2f;      // Velocidade do zoom
    public float minZoom = 3f;        // Zoom máximo (mais aproximado)
    public float maxZoom = 15f;       // Zoom mínimo (mais afastado)

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        // Detecta o scroll do mouse (roda do mouse)
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            // Ajusta o tamanho da câmera (orthographicSize) - ideal para jogos 2D
            cam.orthographicSize -= scroll * zoomSpeed;
            
            // Limita o zoom entre o mínimo e o máximo
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }
}