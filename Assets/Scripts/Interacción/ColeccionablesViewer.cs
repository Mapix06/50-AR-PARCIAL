using UnityEngine;
/// <summary>
/// Gestor que muestra los coleccionables recolectados en la cámara
/// Similar a PlanetUIManager - maneja la visualización
/// </summary>
public class ColeccionablesViewer : MonoBehaviour
{
    public static ColeccionablesViewer Instance;

    [Header("Configuración Visual")]
    [SerializeField] private float escalaObjetos = 10f;
    [SerializeField] private Vector3 posicionRelativa = new Vector3(-0.04f, 0.3f, 0.44f);
    [SerializeField] private Vector3 rotacionRelativa = new Vector3(0, 180, 0); // 👈 NUEVO
    [SerializeField] private float espaciadoEntreObjetos = 0.03f;
    [SerializeField] private float suavizadoMovimiento = 5f;

    private Transform camaraTransform;
    private AudioSource audioSource;

    // Lista de objetos actualmente mostrados
    private System.Collections.Generic.List<GameObject> objetosMostrados =
        new System.Collections.Generic.List<GameObject>();

    // Registro de índices ya recolectados (solo en memoria - se resetea al cerrar app)
    private System.Collections.Generic.HashSet<int> indicesRecolectados =
        new System.Collections.Generic.HashSet<int>();

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ [ColeccionablesViewer] Instancia creada");
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Start()
    {
        StartCoroutine(InicializarCuandoCamaraLista());
    }

    void Update()
    {
        if (camaraTransform != null)
        {
            ActualizarPosicionObjetos();
        }
    }

    /// <summary>
    /// Muestra un coleccionable en la cámara
    /// </summary>

    public void MostrarColeccionable(int indiceEpoca, string nombre, GameObject prefab, AudioClip sonido)
    {
        Debug.Log($"🔵 [ColeccionablesViewer] Intentando mostrar - Época: {indiceEpoca}, Nombre: {nombre}");

        if (indicesRecolectados.Contains(indiceEpoca))
        {
            Debug.LogWarning($"⚠️ [ColeccionablesViewer] '{nombre}' ya fue recolectado en esta sesión.");
            return;
        }

        if (camaraTransform == null)
        {
            Debug.LogWarning("[ColeccionablesViewer] Cámara no disponible.");
            return;
        }

        indicesRecolectados.Add(indiceEpoca);

        GameObject obj = Instantiate(prefab);
        obj.transform.SetParent(camaraTransform, false);

        // Posición
        Vector3 posicionLocal = posicionRelativa;
        posicionLocal.x += objetosMostrados.Count * espaciadoEntreObjetos;

        obj.transform.localPosition = posicionLocal;
        obj.transform.localScale = Vector3.one * escalaObjetos;
        obj.transform.localRotation = Quaternion.Euler(rotacionRelativa); // 👈 USA LA ROTACIÓN CONFIGURABLE

        objetosMostrados.Add(obj);

        if (sonido != null)
        {
            audioSource.PlayOneShot(sonido);
        }

        StartCoroutine(AnimarAparicion(obj));

        Debug.Log($"[ColeccionablesViewer] ✅ '{nombre}' mostrado en cámara.");
    }
    /// <summary>
    /// Muestra un coleccionable en la cámara con escala y offset personalizados
    /// </summary>
    public void MostrarColeccionable(int indiceEpoca, string nombre, GameObject prefab, AudioClip sonido,
                                     float escalaPersonalizada, Vector3 offsetPersonalizado)
    {
        Debug.Log($"🔵 [ColeccionablesViewer] Intentando mostrar - Época: {indiceEpoca}, Nombre: {nombre}");

        if (indicesRecolectados.Contains(indiceEpoca))
        {
            Debug.LogWarning($"⚠️ [ColeccionablesViewer] '{nombre}' ya fue recolectado en esta sesión.");
            return;
        }

        if (camaraTransform == null)
        {
            Debug.LogWarning("[ColeccionablesViewer] Cámara no disponible.");
            return;
        }

        indicesRecolectados.Add(indiceEpoca);

        GameObject obj = Instantiate(prefab);
        obj.transform.SetParent(camaraTransform, false);

        // Posición base + offset personalizado
        Vector3 posicionLocal = posicionRelativa + offsetPersonalizado;
        posicionLocal.x += objetosMostrados.Count * espaciadoEntreObjetos;

        obj.transform.localPosition = posicionLocal;
        obj.transform.localScale = Vector3.one * escalaPersonalizada; // 👈 ESCALA PERSONALIZADA
        obj.transform.localRotation = Quaternion.Euler(rotacionRelativa);

        objetosMostrados.Add(obj);

        if (sonido != null)
        {
            audioSource.PlayOneShot(sonido);
        }

        StartCoroutine(AnimarAparicion(obj));

        Debug.Log($"[ColeccionablesViewer] ✅ '{nombre}' mostrado en cámara (escala: {escalaPersonalizada}).");
    }
    private void ActualizarPosicionObjetos()
    {
        for (int i = 0; i < objetosMostrados.Count; i++)
        {
            if (objetosMostrados[i] == null) continue;

            Vector3 posicionLocalObjetivo = posicionRelativa;
            posicionLocalObjetivo.x += i * espaciadoEntreObjetos;

            objetosMostrados[i].transform.localPosition = Vector3.Lerp(
                objetosMostrados[i].transform.localPosition,
                posicionLocalObjetivo,
                Time.deltaTime * suavizadoMovimiento
            );

            // Mantener la rotación configurada
            objetosMostrados[i].transform.localRotation = Quaternion.Euler(rotacionRelativa); // 👈 AQUÍ TAMBIÉN
        }
    }

    private System.Collections.IEnumerator AnimarAparicion(GameObject obj)
    {
        Vector3 escalaFinal = obj.transform.localScale;
        obj.transform.localScale = Vector3.zero;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2f;
            obj.transform.localScale = Vector3.Lerp(Vector3.zero, escalaFinal, t);
            yield return null;
        }
    }

    private System.Collections.IEnumerator InicializarCuandoCamaraLista()
    {
        // Esperar a la cámara
        while (Camera.main == null || !Camera.main.isActiveAndEnabled)
        {
            yield return null;
        }

        camaraTransform = Camera.main.transform;
        yield return new WaitForSeconds(0.2f);

        Debug.Log("[ColeccionablesViewer] ✅ Inicializado correctamente.");
    }

    /// <summary>
    /// Recolecta el coleccionable de una época específica
    /// Busca el ObjetoInteractivoCambioMapa con el índice correspondiente
    /// NOTA: Solo activa el objeto, NO lo recolecta automáticamente
    /// </summary>
    public void RecolectarPorEpoca(int indiceEpoca)
    {
        // Buscar todos los objetos coleccionables en la escena
        ObjetoInteractivoCambioMapa[] todosLosColeccionables =
            FindObjectsByType<ObjetoInteractivoCambioMapa>(FindObjectsSortMode.None);

        // Buscar el que corresponde a esta época
        ObjetoInteractivoCambioMapa coleccionable = null;
        foreach (var obj in todosLosColeccionables)
        {
            if (obj.indiceEpoca == indiceEpoca)
            {
                coleccionable = obj;
                break;
            }
        }

        if (coleccionable == null)
        {
            Debug.LogWarning($"[ColeccionablesViewer] ⚠️ No se encontró coleccionable para época {indiceEpoca}");
            return;
        }

        // Verificar que tenga prefab asignado
        if (coleccionable.prefabParaMostrar == null)
        {
            Debug.LogError($"[ColeccionablesViewer] ❌ El coleccionable de época {indiceEpoca} no tiene prefab asignado.");
            return;
        }

        Debug.Log($"[ColeccionablesViewer] 🎁 Preparando coleccionable de época {indiceEpoca}: {coleccionable.nombreEpoca}");
    }

    [ContextMenu("Resetear Progreso")]
    public void ResetearTodo()
    {
        // Destruir objetos mostrados
        foreach (var obj in objetosMostrados)
        {
            if (obj != null) Destroy(obj);
        }

        objetosMostrados.Clear();
        indicesRecolectados.Clear();

        Debug.Log("[ColeccionablesViewer] 🔄 Progreso reseteado.");
    }
}