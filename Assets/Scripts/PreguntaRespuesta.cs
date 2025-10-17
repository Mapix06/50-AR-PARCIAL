using UnityEngine;

[System.Serializable]
public class PreguntaRespuesta
{
    [TextArea(2, 4)]
    public string textoPregunta;

    [TextArea(3, 6)]
    public string textoRespuesta;

    public AudioClip audioRespuesta;
}
