using UnityEngine;
using Unity.Cinemachine;

public class CametaZoom : MonoBehaviour
{
    [Header("Configurações do Zoom")]
    public CinemachineCamera vcam; // Usa a vcam em vez da Camera normal
    public float zoomSpeed = 4f;
    public float minZoom = 3f;
    public float maxZoom = 40f;

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f && vcam != null)
        {
            vcam.Lens.OrthographicSize -= scroll * zoomSpeed;
            vcam.Lens.OrthographicSize = Mathf.Clamp(
                vcam.Lens.OrthographicSize,
                minZoom,
                maxZoom
            );
        }
    }
}
