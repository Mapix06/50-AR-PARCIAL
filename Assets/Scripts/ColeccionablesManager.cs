using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ColeccionablesManager : MonoBehaviour
{
    public static ColeccionablesManager Instance;

    [System.Serializable]
    public class Coleccionable
    {
        public int indiceEpoca;
        public string nombreEpoca;
        public GameObject prefabObjeto;
        public bool recolectado;
        [HideInInspector] public GameObject instanciaActual;
    }

    [Header("Configuración")]
    [SerializeField] private List<Coleccionable> coleccionables = new List<Coleccionable>();
    [SerializeField] private float escalaObjetos = 0.05f;
    [SerializeField] private Vector3 posicionLateral = new Vector3(0.3f, -0.2f, 0.5f);
    [SerializeField] private float espaciado = 0.15f;

    [Header("Efectos")]
    [SerializeField] private AudioClip sonidoRecolectar;

    [Header("Seguimiento de Cámara")]
    [SerializeField] private bool seguirCamara = true;
    [SerializeField] private float suavizadoSeguimiento = 5f;

    private AudioSource audioSource;
    private Transform camaraTransform;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[ColeccionablesManager] Ya existe una instancia, destruyendo duplicado.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        Debug.Log("[ColeccionablesManager] Inicializado y persistente entre escenas.");
    }

    void Start()
    {
        // En Editor, resetear progreso automáticamente
#if UNITY_EDITOR
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[ColeccionablesManager] 🧹 Progreso reseteado automáticamente en Editor.");
#endif

        StartCoroutine(CargarProgresoCuandoCamaraLista());
    }

    void Update()
    {
        // Mantener coleccionables pegados a la cámara
        if (seguirCamara && camaraTransform != null)
        {
            ActualizarPosicionesColeccionables();
        }
    }

    private void ActualizarPosicionesColeccionables()
    {
        int indiceVisible = 0;

        foreach (var colec in coleccionables)
        {
            if (colec.recolectado && colec.instanciaActual != null)
            {
                // Calcular posición objetivo relativa a la cámara
                Vector3 offset = posicionLateral;
                offset.x += indiceVisible * espaciado;

                Vector3 posicionObjetivo = camaraTransform.position + camaraTransform.TransformDirection(offset);

                // Mover suavemente hacia la posición objetivo
                colec.instanciaActual.transform.position = Vector3.Lerp(
                    colec.instanciaActual.transform.position,
                    posicionObjetivo,
                    Time.deltaTime * suavizadoSeguimiento
                );

                // Mantener rotación hacia la cámara
                colec.instanciaActual.transform.LookAt(camaraTransform);
                colec.instanciaActual.transform.Rotate(0, 180, 0);

                indiceVisible++;
            }
        }
    }

    public void RecolectarPorEpoca(int indiceEpoca)
    {
        Debug.Log($"[ColeccionablesManager] Intentando recolectar coleccionable de época {indiceEpoca}...");

        Coleccionable colec = coleccionables.Find(c => c.indiceEpoca == indiceEpoca);

        if (colec == null)
        {
            Debug.LogWarning($"[ColeccionablesManager] No se encontró coleccionable con índice {indiceEpoca}.");
            return;
        }

        if (colec.recolectado)
        {
            Debug.Log($"[ColeccionablesManager] El coleccionable '{colec.nombreEpoca}' ya fue recolectado anteriormente.");
            return;
        }

        colec.recolectado = true;
        Debug.Log($"[ColeccionablesManager] Marcando '{colec.nombreEpoca}' como recolectado.");

        InstanciarObjeto(colec);
        GuardarProgreso();

        Debug.Log($"[ColeccionablesManager] ✅ Coleccionable '{colec.nombreEpoca}' desbloqueado e instanciado.");
    }

    private void InstanciarObjeto(Coleccionable colec)
    {
        if (colec.prefabObjeto == null)
        {
            Debug.LogWarning($"[ColeccionablesManager] El prefab del coleccionable '{colec.nombreEpoca}' no está asignado.");
            return;
        }

        if (camaraTransform == null)
        {
            camaraTransform = Camera.main?.transform;
            if (camaraTransform == null)
            {
                Debug.LogWarning("[ColeccionablesManager] No se encontró la cámara principal. No se puede instanciar el objeto.");
                return;
            }
        }

        int cantidadPrevios = coleccionables.FindAll(c => c.indiceEpoca < colec.indiceEpoca && c.recolectado).Count;
        Vector3 offset = posicionLateral;
        offset.x += cantidadPrevios * espaciado;

        Vector3 posicion = camaraTransform.position + camaraTransform.TransformDirection(offset);

        Debug.Log($"[ColeccionablesManager] Instanciando '{colec.nombreEpoca}' en posición {posicion} (offset {offset}).");

        GameObject obj = Instantiate(colec.prefabObjeto, posicion, Quaternion.identity);

        // NO hacerlo hijo de la cámara para evitar problemas con la jerarquía
        obj.transform.localScale = Vector3.one * escalaObjetos;
        obj.transform.LookAt(camaraTransform);
        obj.transform.Rotate(0, 180, 0);

        colec.instanciaActual = obj;

        Debug.DrawLine(camaraTransform.position, posicion, Color.green, 5f);
        Debug.Log($"[ColeccionablesManager] Línea verde dibujada desde cámara hasta posición del coleccionable {colec.nombreEpoca}.");

        if (audioSource != null && sonidoRecolectar != null)
        {
            audioSource.PlayOneShot(sonidoRecolectar);
            Debug.Log("[ColeccionablesManager] 🎵 Sonido de recolección reproducido.");
        }

        StartCoroutine(AnimarAparicion(obj));
    }

    private IEnumerator AnimarAparicion(GameObject obj)
    {
        Vector3 escalaFinal = obj.transform.localScale;
        obj.transform.localScale = Vector3.zero;

        Debug.Log("[ColeccionablesManager] Iniciando animación de aparición...");

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2f;
            obj.transform.localScale = Vector3.Lerp(Vector3.zero, escalaFinal, t);
            yield return null;
        }

        Debug.Log("[ColeccionablesManager] Animación de aparición completada.");
    }

    private void GuardarProgreso()
    {
        // Solo guardar en build, no en Editor
#if !UNITY_EDITOR
        foreach (var c in coleccionables)
        {
            PlayerPrefs.SetInt($"colec_{c.indiceEpoca}", c.recolectado ? 1 : 0);
            Debug.Log($"[ColeccionablesManager] Guardando progreso: {c.nombreEpoca} = {(c.recolectado ? "recolectado" : "no recolectado")}");
        }
        PlayerPrefs.Save();
        Debug.Log("[ColeccionablesManager] 💾 Progreso guardado correctamente.");
#else
        Debug.Log("[ColeccionablesManager] Modo Editor: progreso NO guardado (se resetea al dar Play).");
#endif
    }

    private void CargarProgreso()
    {
        Debug.Log("[ColeccionablesManager] Cargando progreso guardado...");

        foreach (var c in coleccionables)
        {
            // Solo cargar progreso en build, no en Editor
#if !UNITY_EDITOR
            c.recolectado = PlayerPrefs.GetInt($"colec_{c.indiceEpoca}", 0) == 1;
#else
            c.recolectado = false; // En Editor siempre empezar limpio
#endif

            if (c.recolectado)
                Debug.Log($"[ColeccionablesManager] Coleccionable '{c.nombreEpoca}' ya fue recolectado, recreando instancia...");

            if (c.recolectado && c.instanciaActual == null && c.prefabObjeto != null && camaraTransform != null)
            {
                int previos = coleccionables.FindAll(x => x.indiceEpoca < c.indiceEpoca && x.recolectado).Count;
                Vector3 offset = posicionLateral;
                offset.x += previos * espaciado;

                Vector3 pos = camaraTransform.position + camaraTransform.TransformDirection(offset);

                c.instanciaActual = Instantiate(c.prefabObjeto, pos, Quaternion.identity);
                c.instanciaActual.transform.localScale = Vector3.one * escalaObjetos;
                c.instanciaActual.transform.LookAt(camaraTransform);
                c.instanciaActual.transform.Rotate(0, 180, 0);

                Debug.Log($"[ColeccionablesManager] 🔁 Coleccionable '{c.nombreEpoca}' restaurado en posición {pos}.");
            }
        }

        Debug.Log("[ColeccionablesManager] Progreso cargado completamente.");
    }

    private IEnumerator CargarProgresoCuandoCamaraLista()
    {
        Debug.Log("[ColeccionablesManager] Esperando a que la cámara principal esté disponible...");

        // Esperar hasta que la cámara principal exista y esté habilitada
        while (Camera.main == null || !Camera.main.isActiveAndEnabled)
        {
            yield return null;
        }

        camaraTransform = Camera.main.transform;

        // Esperar un par de frames más por seguridad
        yield return new WaitForSeconds(0.2f);

        Debug.Log("[ColeccionablesManager] Cámara detectada correctamente. Cargando progreso...");
        CargarProgreso();
    }

    [ContextMenu("Resetear Todo")]
    public void ResetearTodo()
    {
        Debug.LogWarning("[ColeccionablesManager] 🔄 Reseteando todos los coleccionables...");

        foreach (var c in coleccionables)
        {
            if (c.instanciaActual != null)
            {
                Destroy(c.instanciaActual);
                Debug.Log($"[ColeccionablesManager] Destruyendo instancia de '{c.nombreEpoca}'.");
            }

            c.recolectado = false;
            PlayerPrefs.DeleteKey($"colec_{c.indiceEpoca}");
        }

        PlayerPrefs.Save();
        Debug.Log("[ColeccionablesManager] ✅ Todos los progresos reseteados y guardados.");
    }
}