using UnityEngine;
using TMPro;
using System.Collections;

public class SubtitulosZylo : MonoBehaviour
{
    public static SubtitulosZylo Instance;
    [SerializeField] private TextMeshProUGUI textoSubtitulos;
    [SerializeField] private float tiempoExtra = 1f;

    void Awake() { Instance = this; }

    public void MostrarTexto(string texto) => StartCoroutine(MostrarCoroutine(texto, 0f));

    public void MostrarSubtitulosConAudio(string texto, AudioClip clip)
    {
        float duracion = clip != null ? clip.length : 3f;
        StartCoroutine(MostrarCoroutine(texto, duracion));
    }

    private IEnumerator MostrarCoroutine(string texto, float duracion)
    {
        if (textoSubtitulos != null) textoSubtitulos.text = texto;
        yield return new WaitForSeconds(duracion + tiempoExtra);
        if (textoSubtitulos != null) textoSubtitulos.text = "";
    }
}
