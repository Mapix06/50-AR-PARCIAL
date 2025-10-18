using UnityEngine;

public class Flotar3D : MonoBehaviour
{
    public float amplitud = 0.2f;   // altura del movimiento
    public float frecuencia = 1f;   // velocidad
    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        transform.position = posicionInicial + Vector3.up * Mathf.Sin(Time.time * frecuencia) * amplitud;
    }
}