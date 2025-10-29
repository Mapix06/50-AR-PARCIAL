using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
public class PlaneManagerPines : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] private ARPlaneManager arPlaneManager;
    [SerializeField] private ARRaycastManager arRaycastManager;

    [Header("Prefabs principales")]
    [SerializeField] private GameObject catPrefab;
    [Tooltip("Prefabs de mapas (máx. 5)")]
    [SerializeField] private List<GameObject> mapPrefabs = new List<GameObject>();

    [Header("Posiciones fijas relativas al gato")]
    [SerializeField]
    private List<Vector3> mapOffsets = new List<Vector3>()
    {
        new Vector3(0f, 0f, 0.5f),
        new Vector3(0.5f, 0f, 0f),
        new Vector3(-0.5f, 0f, 0f),
        new Vector3(0f, 0f, -0.5f),
        new Vector3(0.5f, 0f, 0.5f)
    };

    [Header("Opciones")]
    [SerializeField, Range(0f, 5f)] private float distanceFromCamera = 2f;
    [SerializeField] private bool verbose = true;

    [Header("Configuración de Avance")]
    [SerializeField] private float tiempoEsperaAntesDeObjeto = 1.5f; // Tiempo después de que aparece el coleccionable

    private bool todosPinesCompletados = false;
    public bool TodosPinesCompletados => todosPinesCompletados;

    private GameObject catInstance;
    private CatController catController;
    private readonly List<GameObject> mapInstances = new List<GameObject>();
    private int currentMapIndex = 0;
    private int currentPinIndex = 0;
    private bool contentPlaced = false;
    private bool listoParaAvanzar = false;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    [Header("Botón FAB Épocas")]
    [SerializeField] private GameObject fabButton;

    void Awake()
    {
        if (!arPlaneManager)
            arPlaneManager = GetComponent<ARPlaneManager>();

        if (!arRaycastManager)
            arRaycastManager = Object.FindFirstObjectByType<ARRaycastManager>();
    }

    void Start()
    {
        arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;

        if (verbose)
            Debug.Log("[PlaneManagerPines] Buscando plano horizontal frente al usuario...");
    }

    void Update()
    {
        if (contentPlaced) return;
        TryPlaceContentInFrontOfUser();
    }

    private void TryPlaceContentInFrontOfUser()
    {
        if (arRaycastManager == null)
        {
            Debug.LogError("[PlaneManagerPines] ARRaycastManager no encontrado.");
            return;
        }

        Transform cam = Camera.main?.transform;
        if (cam == null)
        {
            Debug.LogError("[PlaneManagerPines] No se encontró la cámara principal.");
            return;
        }

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            ARRaycastHit hit = hits[0];
            Vector3 forward = cam.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 targetPosition = cam.position + forward * distanceFromCamera;
            targetPosition.y = hit.pose.position.y;

            PlaceContentAtPosition(targetPosition);

            arPlaneManager.requestedDetectionMode = PlaneDetectionMode.None;
            foreach (var plane in arPlaneManager.trackables)
                plane.gameObject.SetActive(false);

            contentPlaced = true;
        }
    }

    private void PlaceContentAtPosition(Vector3 position)
    {
        if (catPrefab == null)
        {
            Debug.LogWarning("[PlaneManagerPines] Asigna el prefab del gato.");
            return;
        }

        Transform cam = Camera.main?.transform;
        if (cam == null)
        {
            Debug.LogError("[PlaneManagerPines] No se encontró la cámara principal (MainCamera).");
            return;
        }

        // Gato frente al usuario
        Vector3 directionToCat = cam.position - position;
        directionToCat.y = 0;
        Quaternion catRotation = Quaternion.LookRotation(directionToCat);

        catInstance = Instantiate(catPrefab, position, catRotation);
        catController = catInstance.GetComponent<CatController>();

        if (verbose)
            Debug.Log($"[PlaneManagerPines] Gato colocado en: {position}");

        // Instancia mapas (ocultos) relativos al gato
        for (int i = 0; i < mapPrefabs.Count; i++)
        {
            if (mapPrefabs[i] == null) continue;

            Vector3 offset = (i < mapOffsets.Count) ? mapOffsets[i] : Vector3.zero;
            Vector3 mapPos = catInstance.transform.position + catInstance.transform.TransformDirection(offset);

            GameObject map = Instantiate(mapPrefabs[i], mapPos, Quaternion.identity);
            Vector3 lookDir = catInstance.transform.position - map.transform.position;
            lookDir.y = 0;
            map.transform.rotation = Quaternion.LookRotation(lookDir);
            map.SetActive(false);

            mapInstances.Add(map);
        }

        // Muestra el primer mapa y activa solo el primer pin
        if (mapInstances.Count > 0)
        {
            currentMapIndex = 0;
            currentPinIndex = 0;
            MostrarMapaYPrimerPin();
        }
    }

    public bool PuedeAvanzar()
    {
        return listoParaAvanzar;
    }

    public void NotificarPinCompletado(PinMapa pin)
    {
        if (pin == null) return;

        if (verbose)
            Debug.Log($"[PlaneManagerPines] Pin completado: {pin.name} (orden {pin.OrdenPin})");

        if (currentMapIndex >= mapInstances.Count) return;

        GameObject currentMap = mapInstances[currentMapIndex];
        PinMapa[] pins = currentMap.GetComponentsInChildren<PinMapa>(true);

        // ⭐ ORDENAR PINES POR OrdenPin
        System.Array.Sort(pins, (a, b) => a.OrdenPin.CompareTo(b.OrdenPin));

        // Avanzar al siguiente pin si no es el último
        if (currentPinIndex + 1 < pins.Length)
        {
            currentPinIndex++;
            pins[currentPinIndex].gameObject.SetActive(true);

            if (verbose)
                Debug.Log($"[PlaneManagerPines] Activando siguiente pin: {pins[currentPinIndex].name} (orden {pins[currentPinIndex].OrdenPin}), {currentPinIndex + 1}/{pins.Length}");
        }
        else
        {
            // ✅ Todos los pines de este mapa se completaron
            if (verbose)
                Debug.Log($"[PlaneManagerPines] 🎉 Todos los pines del mapa {currentMapIndex + 1} completados.");

            // SECUENCIA CORRECTA:
            StartCoroutine(SecuenciaCompletarMapa(pins));
        }
    }

    private IEnumerator SecuenciaCompletarMapa(PinMapa[] pins)
    {
        // 1. Marcar todos los pines como completados
        MarcarTodosPinesComoCompletados(pins);
        yield return new WaitForSeconds(0.3f);

        // 2. Cerrar panel de preguntas
        PanelPreguntasZylo panel = Object.FindFirstObjectByType<PanelPreguntasZylo>(FindObjectsInactive.Include);
        panel?.CerrarTodo();
        yield return new WaitForSeconds(0.2f);

        // 3. Instanciar el coleccionable (aparece en pantalla)
        if (ColeccionablesManager.Instance != null)
        {
            ColeccionablesManager.Instance.RecolectarPorEpoca(currentMapIndex);
            Debug.Log($"[PlaneManagerPines] 🎁 Coleccionable de época {currentMapIndex} instanciado.");
        }

        // 4. Esperar un momento para que el usuario vea el coleccionable
        yield return new WaitForSeconds(tiempoEsperaAntesDeObjeto);

        // 5. Buscar y activar el objeto para avanzar dentro del mapa actual
        GameObject mapaActual = mapInstances[currentMapIndex];
        ObjetoInteractivoCambioMapa objetoAvanzar = mapaActual.GetComponentInChildren<ObjetoInteractivoCambioMapa>(true);

        if (objetoAvanzar != null)
        {
            objetoAvanzar.gameObject.SetActive(true);
            Debug.Log("[PlaneManagerPines] 🔘 Objeto para avanzar activado desde el prefab del mapa.");
        }
        else
        {
            Debug.LogWarning("[PlaneManagerPines] ⚠️ No se encontró ObjetoInteractivoCambioMapa en el mapa actual.");
        }

        // 6. Marcar como listo para avanzar
        listoParaAvanzar = true;

        if (verbose)
            Debug.Log("[PlaneManagerPines] ✅ Secuencia completada. Usuario puede avanzar.");
    }

    private void MarcarTodosPinesComoCompletados(PinMapa[] pins)
    {
        foreach (var pin in pins)
        {
            // Ocultar letrero del pin
            var letrero = pin.GetComponentInChildren<Transform>().Find("Letrero");
            if (letrero != null)
            {
                letrero.gameObject.SetActive(false);
            }

            if (verbose)
                Debug.Log($"[PlaneManagerPines] ✓ Pin {pin.name} marcado como completado.");
        }
    }

    // Este método debe ser llamado desde el objeto interactivo
    public void OnObjetoAvanzarClickeado()
    {
        if (!listoParaAvanzar) return;

        if (verbose)
            Debug.Log("[PlaneManagerPines] 👆 Usuario hizo click en objeto para avanzar.");

        // Desactivar objeto de avance del mapa actual
        if (currentMapIndex < mapInstances.Count)
        {
            GameObject mapaActual = mapInstances[currentMapIndex];
            ObjetoInteractivoCambioMapa objetoAvanzar = mapaActual.GetComponentInChildren<ObjetoInteractivoCambioMapa>(true);

            if (objetoAvanzar != null)
            {
                objetoAvanzar.gameObject.SetActive(false);
            }
        }

        // Avanzar al siguiente mapa
        AvanzarAlSiguienteMapa();
    }

    private void OcultarMapaActual()
    {
        if (currentMapIndex >= mapInstances.Count) return;

        GameObject mapaActual = mapInstances[currentMapIndex];
        mapaActual.SetActive(false);

        if (verbose)
            Debug.Log($"[PlaneManagerPines] 🙈 Mapa {currentMapIndex + 1} ocultado.");
    }

    public void AvanzarAlSiguienteMapa()
    {
        if (verbose)
            Debug.Log($"[PlaneManagerPines] 🔄 Iniciando cambio de mapa {currentMapIndex + 1} → {currentMapIndex + 2}");

        // Ocultar el mapa actual antes de avanzar
        OcultarMapaActual();

        // Reset del flag
        listoParaAvanzar = false;

        currentMapIndex++;
        currentPinIndex = 0;

        if (currentMapIndex < mapInstances.Count)
        {
            // Detener animaciones del gato antes de cambiar
            if (catController != null)
            {
                catController.DetenerMovimiento();
            }

            // Pequeña pausa antes de mostrar el nuevo mapa
            StartCoroutine(CambiarANuevoMapa());
        }
        else
        {
            Debug.Log("[PlaneManagerPines] 🎊 ¡Todos los mapas completados!");
            todosPinesCompletados = true;
            ActivarFABSiUltimoMapa();
        }
    }

    private IEnumerator CambiarANuevoMapa()
    {
        // Esperar un frame para que todo se estabilice
        yield return new WaitForEndOfFrame();

        // Mostrar el nuevo mapa y primer pin
        MostrarMapaYPrimerPin();

        if (verbose)
            Debug.Log($"[PlaneManagerPines] ✅ Cambio a mapa {currentMapIndex + 1} completado");
    }

    // Permitir que el FAB lea los mapas
    public List<GameObject> GetMapas() => mapInstances;
    public int GetCurrentMapIndex() => currentMapIndex;

    // Pines recorridos por mapa
    public List<string> GetPinesRecorridos(int mapIndex)
    {
        List<string> lista = new();
        if (mapIndex >= mapInstances.Count) return lista;

        PinMapa[] pins = mapInstances[mapIndex].GetComponentsInChildren<PinMapa>(true);
        for (int i = 0; i <= currentPinIndex && i < pins.Length; i++)
        {
            lista.Add(pins[i].name);
        }
        return lista;
    }

    // Ir a un pin específico
    public void IrAlPin(int mapIndex, string pinName)
    {
        if (mapIndex >= mapInstances.Count) return;

        GameObject mapa = mapInstances[mapIndex];
        mapa.SetActive(true);

        PinMapa[] pins = mapa.GetComponentsInChildren<PinMapa>(true);
        foreach (var pin in pins)
        {
            pin.gameObject.SetActive(pin.name == pinName);
        }

        currentMapIndex = mapIndex;
        Debug.Log($"[PlaneManagerPines] Regresando a {pinName} en {ObtenerNombreMapa(mapIndex)}");
    }
    private void MostrarMapaYPrimerPin()
    {
        if (currentMapIndex >= mapInstances.Count) return;

        GameObject currentMap = mapInstances[currentMapIndex];
        currentMap.SetActive(true);

        PinMapa[] pins = currentMap.GetComponentsInChildren<PinMapa>(true);
        if (pins.Length == 0)
        {
            Debug.LogWarning($"[PlaneManagerPines] El mapa {currentMapIndex + 1} no tiene pines.");
            return;
        }

        // ⭐ ORDENAR PINES POR OrdenPin antes de activarlos
        System.Array.Sort(pins, (a, b) => a.OrdenPin.CompareTo(b.OrdenPin));

        // Desactivar todos los pines primero
        foreach (var pin in pins)
            pin.gameObject.SetActive(false);

        // Activar solo el primer pin según el orden
        if (currentPinIndex < pins.Length)
        {
            pins[currentPinIndex].gameObject.SetActive(true);

            if (verbose)
                Debug.Log($"[PlaneManagerPines] Mostrando mapa {currentMapIndex + 1}, pin {pins[currentPinIndex].name} (orden {pins[currentPinIndex].OrdenPin}), {currentPinIndex + 1}/{pins.Length}");
        }
    }
    private string ObtenerNombreMapa(int index)
    {
        string[] nombres = { "70s", "80s", "90s", "2000s", "2010s" };
        return (index >= 0 && index < nombres.Length) ? nombres[index] : $"Mapa {index + 1}";
    }

    private void ActivarFABSiUltimoMapa()
    {
        // Si estamos en el último mapa y todos sus pines se completaron
        if (currentMapIndex >= mapInstances.Count)
        {
            if (fabButton != null)
            {
                fabButton.SetActive(true);
                Debug.Log("[PlaneManagerPines] 🎯 FAB activado: todas las épocas completadas.");
            }
            else
            {
                Debug.LogWarning("[PlaneManagerPines] No se asignó el FABButton en el inspector.");
            }
        }
    }
}