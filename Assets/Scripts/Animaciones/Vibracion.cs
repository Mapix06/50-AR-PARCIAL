using UnityEngine;

public class Vibracion : MonoBehaviour
{
    public float intensidad = 0.02f;
    public float frecuencia = 20f;
    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.localPosition;
    }

    void Update()
    {
        float offsetX = Mathf.Sin(Time.time * frecuencia) * intensidad;
        float offsetY = Mathf.Cos(Time.time * frecuencia * 0.7f) * intensidad;
        transform.localPosition = posicionInicial + new Vector3(offsetX, offsetY, 0);
    }
}