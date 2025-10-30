
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Objeto interactivo que permite avanzar al siguiente mapa
/// Funciona en computadores (mouse) y celulares (toques táctiles AR)
/// </summary>
public class ObjetoInteractivoCambioMapa : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlaneManager planeManager;
    [SerializeField] private Camera camaraAR;

    [Header("Efectos Visuales")]
    [SerializeField] private bool rotarConstantemente = true;
    [SerializeField] private float velocidadRotacion = 50f;
    [SerializeField] private bool animarEscala = true;
    [SerializeField] private float velocidadPulso = 2f;
    [SerializeField] private float escalaPulsoMin = 0.9f;
    [SerializeField] private float escalaPulsoMax = 1.1f;

    [Header("Audio")]
    [SerializeField] private AudioClip sonidoClick;
    private AudioSource audioSource;

    [Header("Detección de Toques")]
    [SerializeField] private LayerMask capasDetectables;
    [SerializeField] private float distanciaMaximaRaycast = 50f;

    private Vector3 escalaOriginal;
    private Collider miCollider;

    void Start()
    {
        // Buscar PlaneManager si no está asignado
        if (planeManager == null)
        {
            planeManager = Object.FindFirstObjectByType<PlaneManager>();
            if (planeManager == null)
            {
                Debug.LogError("[ObjetoInteractivoCambioMapa] No se encontró PlaneManagerPines en la escena.");
            }
        }

        // Buscar cámara AR
        if (camaraAR == null)
        {
            camaraAR = Camera.main;
            if (camaraAR == null)
            {
                Debug.LogError("[ObjetoInteractivoCambioMapa] No se encontró la cámara principal.");
            }
        }

        // Configurar audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && sonidoClick != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        // Asegurar que tenga un Collider
        miCollider = GetComponent<Collider>();
        if (miCollider == null)
        {
            // Buscar en hijos
            miCollider = GetComponentInChildren<Collider>();

            if (miCollider == null)
            {
                Debug.LogWarning("[ObjetoInteractivoCambioMapa] No hay Collider. Añadiendo SphereCollider automáticamente.");
                miCollider = gameObject.AddComponent<SphereCollider>();
                SphereCollider sphere = miCollider as SphereCollider;
                if (sphere != null)
                {
                    sphere.radius = 0.5f; // Ajusta según tu objeto
                }
            }
        }

        escalaOriginal = transform.localScale;

        Debug.Log("[ObjetoInteractivoCambioMapa] Inicializado y listo para detectar interacciones.");
    }

    void Update()
    {
        // Efectos visuales
        if (rotarConstantemente)
        {
            transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime);
        }

        if (animarEscala)
        {
            float pulso = Mathf.Lerp(escalaPulsoMin, escalaPulsoMax,
                (Mathf.Sin(Time.time * velocidadPulso) + 1f) / 2f);
            transform.localScale = escalaOriginal * pulso;
        }

        // Detección de interacciones - SOLO UNA VEZ POR FRAME
        DetectarInteracciones();
    }

    public void OnUsuarioTocaObjeto()
    {
        var manager = Object.FindFirstObjectByType<PlaneManager>();
        var panel = Object.FindFirstObjectByType<PanelPreguntasZylo>();

        if (manager != null && panel != null)
        {
            manager.NotificarPinCompletado(panel.GetPinActual()); // o panel.pinActual si es público
            panel.CerrarTodo();
        }
    }

    private void DetectarInteracciones()
    {
        bool toqueDetectado = false;
        Vector2 posicionToque = Vector2.zero;

        // Para CELULAR: Detectar toques táctiles
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                toqueDetectado = true;
                posicionToque = touch.position;
            }
        }
        // Para COMPUTADOR: Detectar click del mouse (solo si no hay toques)
        else if (Input.GetMouseButtonDown(0))
        {
            toqueDetectado = true;
            posicionToque = Input.mousePosition;
        }

        // Procesar el toque/click si se detectó
        if (toqueDetectado)
        {
            DetectarClickEnPosicion(posicionToque);
        }
    }

    private void DetectarClickEnPosicion(Vector2 posicionPantalla)
    {
        if (camaraAR == null)
        {
            Debug.LogWarning("[ObjetoInteractivoCambioMapa] No hay cámara asignada.");
            return;
        }

        // Crear raycast desde la cámara hacia la posición tocada/clickeada
        Ray ray = camaraAR.ScreenPointToRay(posicionPantalla);
        RaycastHit hit;

        // Usar capa específica si está configurada, sino usar todas
        bool impacto = false;
        if (capasDetectables != 0)
        {
            impacto = Physics.Raycast(ray, out hit, distanciaMaximaRaycast, capasDetectables);
        }
        else
        {
            impacto = Physics.Raycast(ray, out hit, distanciaMaximaRaycast);
        }

        if (impacto)
        {
            // Verificar si el objeto tocado es este objeto
            if (hit.collider == miCollider || hit.collider.transform == transform)
            {
                Debug.Log($"[ObjetoInteractivoCambioMapa] ¡Objeto tocado en posición {posicionPantalla}!");
                ProcesarClick();
            }
        }
    }

    // Método alternativo usando OnMouseDown (solo funciona en computador)
    void OnMouseDown()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        Debug.Log("[ObjetoInteractivoCambioMapa] Click detectado vía OnMouseDown (modo Editor/PC)");
        ProcesarClick();
#endif
    }


    void OnDrawGizmos()
    {
        // Dibujar un indicador visual en el editor para ver el rango de detección
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }

    private void NotificarSiEsUltimoPin()
    {
        var manager = Object.FindFirstObjectByType<PlaneManager>();
        var panel = Object.FindFirstObjectByType<PanelPreguntasZylo>();

        if (manager == null || panel == null || panel.GetPinActual() == null)
        {
            Debug.LogWarning("[ObjetoInteractivoCambioMapa] No se pudo obtener referencias para notificar pin.");
            return;
        }

        PinMapa pinActual = panel.GetPinActual();
        GameObject mapaActual = manager.GetMapas()[manager.GetCurrentMapIndex()];
        PinMapa[] pins = mapaActual.GetComponentsInChildren<PinMapa>(true);
        System.Array.Sort(pins, (a, b) => a.OrdenPin.CompareTo(b.OrdenPin));

        bool esUltimoPin = pinActual == pins[pins.Length - 1];

        if (esUltimoPin)
        {
            Debug.Log("[ObjetoInteractivoCambioMapa] ✅ Es el último pin. Notificando al manager...");
            manager.NotificarPinCompletado(pinActual);
            panel.CerrarTodo();
        }
        else
        {
            Debug.Log("[ObjetoInteractivoCambioMapa] Este no es el último pin. No se notifica.");
        }
    }

    private void ProcesarClick()
    {
        if (planeManager == null)
        {
            Debug.LogError("[ObjetoInteractivoCambioMapa] PlaneManager no encontrado.");
            return;
        }

        // ✅ Notificar si es el último pin ANTES de verificar si se puede avanzar
        NotificarSiEsUltimoPin();

        if (!planeManager.PuedeAvanzar())
        {
            Debug.LogWarning("[ObjetoInteractivoCambioMapa] Aún no se puede avanzar. Esperando completar pines...");
            return;
        }

        Debug.Log("[ObjetoInteractivoCambioMapa] ✅ ¡Click confirmado! Avanzando al siguiente mapa...");

        // Reproducir sonido
        if (audioSource != null && sonidoClick != null)
        {
            audioSource.PlayOneShot(sonidoClick);
        }

        // Desactivar inmediatamente para evitar múltiples clicks
        gameObject.SetActive(false);

        // Llamar al PlaneManager para avanzar
        planeManager.OnObjetoAvanzarClickeado();
    }



}