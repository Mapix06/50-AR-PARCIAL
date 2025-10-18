using UnityEngine;

public class Oscilacion : MonoBehaviour
{
    public float angulo = 15f;   // amplitud del giro
    public float velocidad = 2f; // frecuencia
    private Quaternion rotacionInicial;

    void Start()
    {
        rotacionInicial = transform.rotation;
    }

    void Update()
    {
        float oscilacion = Mathf.Sin(Time.time * velocidad) * angulo;
        transform.rotation = rotacionInicial * Quaternion.Euler(0, 0, oscilacion);
    }
}