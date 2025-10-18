using UnityEngine;

public class Palpitar : MonoBehaviour
{
    public float velocidad = 2f;    // qué tan rápido late
    public float intensidad = 0.1f; // cuánto se expande o contrae
    private Vector3 escalaInicial;

    void Start()
    {
        escalaInicial = transform.localScale;
    }

    void Update()
    {
        float pulso = 1 + Mathf.Sin(Time.time * velocidad) * intensidad;
        transform.localScale = escalaInicial * pulso;
    }
}