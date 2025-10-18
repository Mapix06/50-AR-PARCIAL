using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TMPro;

public class LectorPreguntas : MonoBehaviour
{
    public static LectorPreguntas instance;

    [Header("Referencias UI - Textos de los botones")]
    [SerializeField] private TextMeshProUGUI textoPregunta1;
    [SerializeField] private TextMeshProUGUI textoPregunta2;

    private List<DatosPregunta> preguntas = new List<DatosPregunta>();
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
        rutaArchivo = Path.Combine(Application.streamingAssetsPath, "Recursos", "TXT_Interaccion", "preguntas.txt");

        CargarPreguntasDesdeTXT();
    }

    /// <summary>
    /// Carga las preguntas desde el archivo TXT usando StreamReader
    /// </summary>
    void CargarPreguntasDesdeTXT()
    {
        if (!File.Exists(rutaArchivo))
        {
            Debug.LogError($" [LectorPreguntas] No se encontró el archivo en: {rutaArchivo}");
            return;
        }

        preguntas.Clear();

        try
        {
            using (StreamReader sr = new StreamReader(rutaArchivo))
            {
                string linea;
                while ((linea = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;

                    // Formato esperado: idPin-pregunta1-pregunta2
                    string[] partes = linea.Split('-');

                    if (partes.Length < 3)
                    {
                        Debug.LogWarning($" [LectorPreguntas] Línea con formato incorrecto: {linea}");
                        continue;
                    }

                    DatosPregunta dp = new DatosPregunta
                    {
                        idPin = partes[0].Trim(),
                        pregunta1 = partes[1].Trim(),
                        pregunta2 = partes[2].Trim()
                    };

                    preguntas.Add(dp);
                }
            }

            Debug.Log($" [LectorPreguntas] Cargadas {preguntas.Count} preguntas desde TXT.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($" [LectorPreguntas] Error al leer archivo: {e.Message}");
        }
    }

    /// <summary>
    /// Obtiene los datos de preguntas por ID del pin
    /// </summary>
    public DatosPregunta ObtenerDatosPorID(string id)
    {
        DatosPregunta resultado = preguntas.Find(p => p.idPin == id);

        if (resultado == null)
            Debug.LogWarning($" [LectorPreguntas] No se encontró pregunta con ID: {id}");

        return resultado;
    }

    /// <summary>
    /// Muestra las preguntas en los textos de los botones según el ID del pin.
    /// Este método actualiza directamente la UI.
    /// </summary>
    public void MostrarPreguntasPorID(string id)
    {
        DatosPregunta datos = ObtenerDatosPorID(id);

        if (datos == null)
        {
            Debug.LogWarning($" [LectorPreguntas] No se pueden mostrar preguntas para ID: {id}");
            return;
        }

        // Actualizar textos de los botones
        if (textoPregunta1 != null)
            textoPregunta1.text = datos.pregunta1;
        else
            Debug.LogError(" [LectorPreguntas] textoPregunta1 no está asignado en el Inspector");

        if (textoPregunta2 != null)
            textoPregunta2.text = datos.pregunta2;
        else
            Debug.LogError(" [LectorPreguntas] textoPregunta2 no está asignado en el Inspector");

        Debug.Log($" [LectorPreguntas] Preguntas mostradas para ID '{id}':");
        Debug.Log($"   Pregunta 1: {datos.pregunta1}");
        Debug.Log($"   Pregunta 2: {datos.pregunta2}");
    }
}
