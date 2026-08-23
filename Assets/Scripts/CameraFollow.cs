using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform alvo;
    public float suavidade = 0.1f;
    private Vector3 offset;

    void Start()
    {
        if (alvo == null)
            alvo = FindObjectOfType<Nave>().transform;
        offset = transform.position - alvo.position;
    }

    void LateUpdate()
    {
        Vector3 posDesejada = alvo.position + offset;
        transform.position = Vector3.Lerp(transform.position, posDesejada, suavidade);
    }
}