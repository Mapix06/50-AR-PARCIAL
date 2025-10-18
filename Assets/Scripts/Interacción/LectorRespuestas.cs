using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TMPro;

public class LectorRespuestas : MonoBehaviour
{
    public static LectorRespuestas instance;

    [Header("Referencias UI - Texto de respuesta")]
    [SerializeField] private TextMeshProUGUI textoRespuesta;

    private List<DatosRespuesta> respuestas = new List<DatosRespuesta>();
    private string rutaArchivo;

    void Awake()
    {
        // Singleton
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Definir ruta del archivo
        rutaArchivo = Path.Combine(Application.streamingAssetsPath, "Recursos", "TXT_Interaccion", "respuestas.txt");

        CargarRespuestasDesdeTXT();
    }

    /// <summary>
    /// Carga las respuestas desde el archivo TXT usando StreamReader
    /// </summary>
    public void CargarRespuestasDesdeTXT()
    {
        if (!File.Exists(rutaArchivo))
        {
            Debug.LogError($" [LectorRespuestas] No se encontró el archivo en: {rutaArchivo}");
            return;
        }

        respuestas.Clear();

        try
        {
            using (StreamReader sr = new StreamReader(rutaArchivo))
            {
                string linea;
                while ((linea = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;

                    // Formato esperado: idPin-respuesta1-respuesta2
                    string[] partes = linea.Split('-');

                    if (partes.Length < 3)
                    {
                        Debug.LogWarning($" [LectorRespuestas] Línea con formato incorrecto: {linea}");
                        continue;
                    }

                    DatosRespuesta dr = new DatosRespuesta
                    {
                        idPin = partes[0].Trim(),
                        respuesta1 = partes[1].Trim(),
                        respuesta2 = partes[2].Trim()
                    };

                    respuestas.Add(dr);
                }
            }

            Debug.Log($" [LectorRespuestas] Cargadas {respuestas.Count} respuestas desde TXT.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($" [LectorRespuestas] Error al leer archivo: {e.Message}");
        }
    }

    /// <summary>
    /// Obtiene los datos de respuestas por ID del pin
    /// </summary>
    public DatosRespuesta ObtenerDatosPorID(string id)
    {
        DatosRespuesta resultado = respuestas.Find(r => r.idPin == id);

        if (resultado == null)
            Debug.LogWarning($" [LectorRespuestas] No se encontró respuesta con ID: {id}");

        return resultado;
    }

    /// <summary>
    /// Muestra la respuesta correspondiente al ID y al botón elegido (1 o 2).
    /// Este método actualiza directamente el texto de respuesta en la UI.
    /// </summary>
    public void MostrarRespuesta(string idPin, int numeroPregunta)
    {
        DatosRespuesta datos = ObtenerDatosPorID(idPin);

        if (datos == null)
        {
            Debug.LogWarning($" [LectorRespuestas] No se pueden mostrar respuestas para ID: {idPin}");
            return;
        }

        string respuestaSeleccionada = numeroPregunta == 1 ? datos.respuesta1 : datos.respuesta2;

        if (textoRespuesta != null)
            textoRespuesta.text = respuestaSeleccionada;
        else
            Debug.LogError(" [LectorRespuestas] textoRespuesta no está asignado en el Inspector");

        Debug.Log($" [LectorRespuestas] Respuesta mostrada para ID '{idPin}' (pregunta {numeroPregunta}): {respuestaSeleccionada}");
    }
}
