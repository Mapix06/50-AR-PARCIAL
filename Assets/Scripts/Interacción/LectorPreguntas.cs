using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine.Networking;

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

        StartCoroutine(CargarPreguntasDesdeTXT());
    }

    /// <summary>
    /// Carga las preguntas desde el archivo TXT - Compatible con Android
    /// </summary>
    IEnumerator CargarPreguntasDesdeTXT()
    {
        string contenido = "";

        // En Android, debemos usar UnityWebRequest
        if (Application.platform == RuntimePlatform.Android)
        {
            UnityWebRequest www = UnityWebRequest.Get(rutaArchivo);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[LectorPreguntas] Error en Android: {www.error}");
                yield break;
            }

            contenido = www.downloadHandler.text;
        }
        else
        {
            // En otras plataformas (Editor, PC, Mac), usamos StreamReader
            if (!File.Exists(rutaArchivo))
            {
                Debug.LogError($"[LectorPreguntas] No se encontró el archivo en: {rutaArchivo}");
                yield break;
            }

            try
            {
                contenido = File.ReadAllText(rutaArchivo);
            }
            catch (System.Exception e)
            {
                Debug.LogError($" [LectorPreguntas] Error al leer archivo: {e.Message}");
                yield break;
            }
        }

        // Procesar el contenido del archivo
        ProcesarContenido(contenido);
    }

    /// <summary>
    /// Procesa el contenido del archivo de texto
    /// </summary>
    void ProcesarContenido(string contenido)
    {
        preguntas.Clear();

        string[] lineas = contenido.Split('\n');

        foreach (string linea in lineas)
        {
            if (string.IsNullOrWhiteSpace(linea)) continue;

            // Formato esperado: idPin-pregunta1-pregunta2
            string[] partes = linea.Split('-');

            if (partes.Length < 3)
            {
                Debug.LogWarning($"[LectorPreguntas] Línea con formato incorrecto: {linea}");
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

        Debug.Log($"[LectorPreguntas] Cargadas {preguntas.Count} preguntas desde TXT.");
    }

    /// <summary>
    /// Obtiene los datos de preguntas por ID del pin
    /// </summary>
    public DatosPregunta ObtenerDatosPorID(string id)
    {
        DatosPregunta resultado = preguntas.Find(p => p.idPin == id);

        if (resultado == null)
            Debug.LogWarning($"[LectorPreguntas] No se encontró pregunta con ID: {id}");

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
            Debug.LogWarning($"[LectorPreguntas] No se pueden mostrar preguntas para ID: {id}");
            return;
        }

        // Actualizar textos de los botones
        if (textoPregunta1 != null)
            textoPregunta1.text = datos.pregunta1;
        else
            Debug.LogError("[LectorPreguntas] textoPregunta1 no está asignado en el Inspector");

        if (textoPregunta2 != null)
            textoPregunta2.text = datos.pregunta2;
        else
            Debug.LogError("[LectorPreguntas] textoPregunta2 no está asignado en el Inspector");

        Debug.Log($"[LectorPreguntas] Preguntas mostradas para ID '{id}':");
        Debug.Log($"   Pregunta 1: {datos.pregunta1}");
        Debug.Log($"   Pregunta 2: {datos.pregunta2}");
    }
}
