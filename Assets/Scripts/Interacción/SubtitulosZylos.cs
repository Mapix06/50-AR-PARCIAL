using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SubtitulosZylo : MonoBehaviour
{
    public static SubtitulosZylo Instance;

    [SerializeField] private TextMeshProUGUI textoSubtitulos;
    [SerializeField] private float tiempoExtra = 1f;

    [Header("Control de Overflow")]
    [SerializeField] private bool activarSegmentacionAutomatica = true;
    [SerializeField] private float tiempoPorSegmento = 9f;

    void Awake()
    {
        Instance = this;
    }

    public void MostrarTexto(string texto) => StartCoroutine(MostrarCoroutine(texto, 7f));

    public void MostrarSubtitulosConAudio(string texto, AudioClip clip)
    {
        float duracion = clip != null ? clip.length : 3f;
        StartCoroutine(MostrarCoroutine(texto, duracion));
    }

    private IEnumerator MostrarCoroutine(string texto, float duracion)
    {
        if (textoSubtitulos == null) yield break;

        // Verificar si el texto genera overflow visual
        if (activarSegmentacionAutomatica && VerificarOverflowVisual(texto))
        {
            List<string> segmentos = DividirTextoPorOverflow(texto);

            foreach (string segmento in segmentos)
            {
                textoSubtitulos.text = segmento;
                yield return new WaitForSeconds(tiempoPorSegmento);
            }
        }
        else
        {
            // Lógica original
            textoSubtitulos.text = texto;
            yield return new WaitForSeconds(duracion + tiempoExtra);
        }

        textoSubtitulos.text = "";
    }

    private bool VerificarOverflowVisual(string texto)
    {
        if (textoSubtitulos == null) return false;

        string textoOriginal = textoSubtitulos.text;
        textoSubtitulos.text = texto;
        textoSubtitulos.ForceMeshUpdate();

        bool hayOverflow = textoSubtitulos.isTextOverflowing;

        textoSubtitulos.text = textoOriginal;

        return hayOverflow;
    }

    private List<string> DividirTextoPorOverflow(string texto)
    {
        List<string> segmentos = new List<string>();

        // Dividir por palabras para mayor precisión
        string[] palabras = texto.Split(' ');
        string segmentoActual = "";

        foreach (string palabra in palabras)
        {
            string pruebaTexto = string.IsNullOrEmpty(segmentoActual)
                ? palabra
                : segmentoActual + " " + palabra;

            // Probar si el texto con esta palabra genera overflow
            textoSubtitulos.text = pruebaTexto;
            textoSubtitulos.ForceMeshUpdate();

            if (textoSubtitulos.isTextOverflowing && !string.IsNullOrEmpty(segmentoActual))
            {
                // La palabra actual causa overflow, guardar el segmento anterior
                segmentos.Add(segmentoActual);
                segmentoActual = palabra;
            }
            else
            {
                segmentoActual = pruebaTexto;
            }
        }

        // Agregar el último segmento
        if (!string.IsNullOrEmpty(segmentoActual))
        {
            segmentos.Add(segmentoActual);
        }

        // Limpiar el texto temporal
        textoSubtitulos.text = "";

        return segmentos;
    }
}