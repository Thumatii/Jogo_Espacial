using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float velocidade = 4f;

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        transform.Translate(new Vector3(h, v, 0).normalized * velocidade * Time.deltaTime);
    }
}