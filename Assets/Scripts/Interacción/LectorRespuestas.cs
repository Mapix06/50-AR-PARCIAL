using UnityEngine;
using System.Collections.Generic;

public class LectorRespuestas : MonoBehaviour
{
    public static LectorRespuestas instance;
    public List<DatosRespuesta> respuestas = new List<DatosRespuesta>();

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        CargarRespuestasDesdeTXT("Respuestas");
    }

    void CargarRespuestasDesdeTXT(string nombreArchivo)
    {
        TextAsset txt = Resources.Load<TextAsset>("Recursos/TXT_Interaccion/respuestas.txt");
        if (txt == null) { Debug.LogError("No se encontró el TXT de respuestas"); return; }

        string[] lineas = txt.text.Split('\n');
        foreach (string linea in lineas)
        {
            if (string.IsNullOrWhiteSpace(linea)) continue;

            // Formato: idPin-respuesta1-respuesta2
            string[] partes = linea.Split('-');
            if (partes.Length < 3) continue;

            DatosRespuesta dr = new DatosRespuesta
            {
                idPin = partes[0],
                respuesta1 = partes[1],
                respuesta2 = partes[2]
            };

            respuestas.Add(dr);
        }

        Debug.Log($"[LectorRespuestas] Cargadas {respuestas.Count} respuestas");
    }

    public DatosRespuesta ObtenerDatosPorID(string id)
    {
        return respuestas.Find(r => r.idPin == id);
    }
}
