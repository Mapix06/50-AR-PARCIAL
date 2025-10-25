using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SubtitulosZylo : MonoBehaviour
{
    public static SubtitulosZylo Instance;

    [SerializeField] private TextMeshProUGUI textoSubtitulos;
    [SerializeField] private float tiempoExtra = 0.5f;

    [Header("Control de Overflow y segmentación")]
    [SerializeField] private bool activarSegmentacionAutomatica = true;
    [SerializeField] private int maxLineasVisibles = 4; // NUEVO límite
    //[SerializeField] private float tiempoPorSegmento = 9f;

    [Header("Duración dinámica por texto")]
    [SerializeField] private float segundosPorCaracter = 0.07f;

    void Awake()
    {
        Instance = this;
    }

    public void MostrarTexto(string texto) => StartCoroutine(MostrarCoroutine(texto, CalcularDuracionTexto(texto)));

    public void MostrarSubtitulosConAudio(string texto, AudioClip clip)
    {
        float duracion = clip != null ? clip.length : CalcularDuracionTexto(texto);
        StartCoroutine(MostrarCoroutine(texto, duracion));
    }

    private IEnumerator MostrarCoroutine(string texto, float duracion)
    {
        if (textoSubtitulos == null) yield break;

        if (activarSegmentacionAutomatica)
        {
            List<string> segmentos = DividirTextoPorLineas(texto);

            // 🧮 Calcular la longitud total (en caracteres)
            int totalCaracteres = texto.Length;

            foreach (string segmento in segmentos)
            {
                // Tiempo proporcional a la cantidad de caracteres
                float proporcion = (float)segmento.Length / totalCaracteres;
                float duracionSegmento = (duracion * proporcion) + 0.3f; // 0.3s extra de seguridad

                textoSubtitulos.text = segmento;
                yield return new WaitForSeconds(duracionSegmento);
            }
        }
        else
        {
            textoSubtitulos.text = texto;
            yield return new WaitForSeconds(duracion + tiempoExtra);
        }

        textoSubtitulos.text = "";
    }


    // 🔹 Calcula duración estimada si no hay audio
    private float CalcularDuracionTexto(string texto)
    {
        return Mathf.Max(1f, texto.Length * segundosPorCaracter);
    }

    // 🔹 Divide en bloques de máximo 4 líneas
    private List<string> DividirTextoPorLineas(string texto)
    {
        List<string> segmentos = new List<string>();
        string[] palabras = texto.Split(' ');
        string bloqueActual = "";

        textoSubtitulos.text = "";
        textoSubtitulos.ForceMeshUpdate();

        foreach (string palabra in palabras)
        {
            string prueba = string.IsNullOrEmpty(bloqueActual)
                ? palabra
                : bloqueActual + " " + palabra;

            textoSubtitulos.text = prueba;
            textoSubtitulos.ForceMeshUpdate();

            // 🧠 Si se pasa de 4 líneas o hace overflow → guardar bloque
            if ((textoSubtitulos.textInfo.lineCount > maxLineasVisibles ||
                 textoSubtitulos.isTextOverflowing) &&
                 !string.IsNullOrEmpty(bloqueActual))
            {
                segmentos.Add(bloqueActual);
                bloqueActual = palabra;
            }
            else
            {
                bloqueActual = prueba;
            }
        }

        if (!string.IsNullOrEmpty(bloqueActual))
            segmentos.Add(bloqueActual);

        textoSubtitulos.text = "";
        return segmentos;
    }
}
