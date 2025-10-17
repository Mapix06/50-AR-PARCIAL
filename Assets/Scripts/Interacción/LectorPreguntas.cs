using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LectorPreguntas : MonoBehaviour
{
    public static LectorPreguntas instance;
    public List<DatosPregunta> preguntas = new List<DatosPregunta>();

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        CargarPreguntasDesdeTXT("Preguntas");
    }

    void CargarPreguntasDesdeTXT(string nombreArchivo)
    {
        TextAsset txt = Resources.Load<TextAsset>("Recursos/TXT_Interaccion/preguntas.txt");
        if (txt == null) { Debug.LogError("No se encontró el TXT de preguntas"); return; }

        string[] lineas = txt.text.Split('\n');
        foreach (string linea in lineas)
        {
            if (string.IsNullOrWhiteSpace(linea)) continue;

            // Formato: idPin-pregunta1-pregunta2
            string[] partes = linea.Split('-');
            if (partes.Length < 3) continue;

            DatosPregunta dp = new DatosPregunta
            {
                idPin = partes[0],
                pregunta1 = partes[1],
                pregunta2 = partes[2],
                audioPregunta1 = null,
                audioPregunta2 = null
            };

            preguntas.Add(dp);
        }

        Debug.Log($"[LectorPreguntas] Cargadas {preguntas.Count} preguntas");
    }

    public DatosPregunta ObtenerDatosPorID(string id)
    {
        return preguntas.Find(p => p.idPin == id);
    }
}
