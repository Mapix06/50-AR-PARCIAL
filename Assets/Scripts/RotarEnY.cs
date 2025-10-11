using UnityEngine;

public class RotarEnY : MonoBehaviour
{
    public float velocidadRotacion = 50f;

    void Update()
    {
        // Rotar solo en el eje Y con el tiempo
        transform.Rotate(0, velocidadRotacion * Time.deltaTime, 0);
    }
}
