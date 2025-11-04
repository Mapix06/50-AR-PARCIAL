using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
public class PlaneManager : MonoBehaviour
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
    [SerializeField] private float tiempoEsperaAntesDeObjeto = 1.5f;

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
            arRaycastManager = FindFirstObjectByType<ARRaycastManager>();
    }

    void Start()
    {
        arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;

        if (verbose)
            Debug.Log("[PlaneManager] Buscando plano horizontal frente al usuario...");
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
            Debug.LogError("[PlaneManager] ARRaycastManager no encontrado.");
            return;
        }

        Transform cam = Camera.main?.transform;
        if (cam == null)
        {
            Debug.LogError("[PlaneManager] No se encontró la cámara principal.");
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
            Debug.LogWarning("[PlaneManager] Asigna el prefab del gato.");
            return;
        }

        Transform cam = Camera.main?.transform;
        if (cam == null)
        {
            Debug.LogError("[PlaneManager] No se encontró la cámara principal.");
            return;
        }

        Vector3 directionToCat = cam.position - position;
        directionToCat.y = 0;
        Quaternion catRotation = Quaternion.LookRotation(directionToCat);

        catInstance = Instantiate(catPrefab, position, catRotation);
        catController = catInstance.GetComponent<CatController>();

        if (verbose)
            Debug.Log($"[PlaneManager] Gato colocado en: {position}");

        // Instanciar mapas relativos al gato
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

        // Mostrar primer mapa y pin
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
            Debug.Log($"[PlaneManager] Pin completado: {pin.name} (orden {pin.OrdenPin})");

        if (currentMapIndex >= mapInstances.Count) return;

        GameObject currentMap = mapInstances[currentMapIndex];
        PinMapa[] pins = currentMap.GetComponentsInChildren<PinMapa>(true);

        // Ordenar pines por OrdenPin
        System.Array.Sort(pins, (a, b) => a.OrdenPin.CompareTo(b.OrdenPin));

        // Avanzar al siguiente pin si no es el último
        if (currentPinIndex + 1 < pins.Length)
        {
            currentPinIndex++;
            pins[currentPinIndex].gameObject.SetActive(true);

            if (verbose)
                Debug.Log($"[PlaneManager] Activando siguiente pin: {pins[currentPinIndex].name}, {currentPinIndex + 1}/{pins.Length}");
        }
        else
        {
            // Todos los pines completados
            if (verbose)
                Debug.Log($"[PlaneManager] 🎉 Todos los pines del mapa {currentMapIndex + 1} completados.");

            StartCoroutine(SecuenciaCompletarMapa(pins));
        }
    }

    private IEnumerator SecuenciaCompletarMapa(PinMapa[] pins)
    {
        listoParaAvanzar = true;
        Debug.Log("[DEBUG] SecuenciaCompletarMapa started. listoParaAvanzar set to true.");

        // 1. Marcar todos los pines como completados
        MarcarTodosPinesComoCompletados(pins);
        Debug.Log("[DEBUG] Pins marked complete.");
        yield return new WaitForSeconds(0.3f);

        // 2. Cerrar panel de preguntas
        PanelPreguntasZylo panel = FindFirstObjectByType<PanelPreguntasZylo>(FindObjectsInactive.Include);
        panel?.CerrarTodo();
        Debug.Log("[DEBUG] Panel closed.");
        yield return new WaitForSeconds(0.2f);

        // 3. Instanciar el coleccionable
        if (ColeccionablesViewer.Instance != null)
        {
            ColeccionablesViewer.Instance.RecolectarPorEpoca(currentMapIndex);
            Debug.Log($"[DEBUG] Collectible shown for epoch {currentMapIndex}.");
        }
        else
        {
            Debug.LogWarning("[DEBUG] ColeccionablesViewer not found.");
        }

        // 4. Esperar
        Debug.Log("[DEBUG] Waiting 1.5s before activating object...");
        yield return new WaitForSeconds(tiempoEsperaAntesDeObjeto);

        // 5. Buscar y activar el objeto
        GameObject mapaActual = mapInstances[currentMapIndex];
        Debug.Log($"[DEBUG] Current map: {mapaActual?.name}, active: {mapaActual?.activeSelf}");

        ObjetoInteractivoCambioMapa objetoAvanzar = mapaActual.GetComponentInChildren<ObjetoInteractivoCambioMapa>(true);
        Debug.Log($"[DEBUG] ObjetoInteractivoCambioMapa found: {objetoAvanzar != null}, name: {objetoAvanzar?.name}, current active: {objetoAvanzar?.gameObject.activeSelf}");

        if (objetoAvanzar != null)
        {
            objetoAvanzar.gameObject.SetActive(true);
            Debug.Log("[DEBUG] ObjetoInteractivoCambioMapa ACTIVATED!");
        }
        else
        {
            Debug.LogWarning("[DEBUG] ObjetoInteractivoCambioMapa NOT FOUND in map.");
        }

        Debug.Log("[DEBUG] SecuenciaCompletarMapa completed.");
    }


    private void MarcarTodosPinesComoCompletados(PinMapa[] pins)
    {
        foreach (var pin in pins)
        {
            // 🔧 PROBLEMA 2 RESUELTO: Búsqueda segura del letrero
            Transform letrero = pin.transform.Find("Letrero");
            if (letrero != null)
            {
                letrero.gameObject.SetActive(false);
            }

            if (verbose)
                Debug.Log($"[PlaneManager] ✓ Pin {pin.name} marcado como completado.");
        }
    }

    public void OnObjetoAvanzarClickeado()
    {
        Debug.Log($"[PlaneManager] 🎯 OnObjetoAvanzarClickeado() llamado. listoParaAvanzar = {listoParaAvanzar}");

        if (!listoParaAvanzar)
        {
            Debug.LogWarning("[PlaneManager] ⚠️ No está listo para avanzar. Abortando.");
            return;
        }

        Debug.Log("[PlaneManager] 👆 Click en objeto para avanzar CONFIRMADO.");

        // Desactivar objeto de avance
        if (currentMapIndex < mapInstances.Count)
        {
            GameObject mapaActual = mapInstances[currentMapIndex];
            Debug.Log($"[PlaneManager] 🗺️ Buscando ObjetoInteractivoCambioMapa en mapa {currentMapIndex}");

            ObjetoInteractivoCambioMapa objetoAvanzar = mapaActual.GetComponentInChildren<ObjetoInteractivoCambioMapa>(true);

            if (objetoAvanzar != null)
            {
                Debug.Log($"[PlaneManager] 🔘 Desactivando objeto: {objetoAvanzar.name}");
                objetoAvanzar.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[PlaneManager] ⚠️ No se encontró ObjetoInteractivoCambioMapa para desactivar.");
            }
        }
        else
        {
            Debug.LogWarning($"[PlaneManager] ⚠️ currentMapIndex ({currentMapIndex}) fuera de rango.");
        }

        Debug.Log("[PlaneManager] 🚀 Llamando a AvanzarAlSiguienteMapa()...");
        AvanzarAlSiguienteMapa();
    }

    private void OcultarMapaActual()
    {
        if (currentMapIndex >= mapInstances.Count) return;

        GameObject mapaActual = mapInstances[currentMapIndex];
        mapaActual.SetActive(false);

        if (verbose)
            Debug.Log($"[PlaneManager] 🙈 Mapa {currentMapIndex + 1} ocultado.");
    }

    public void AvanzarAlSiguienteMapa()
    {
        if (verbose)
            Debug.Log($"[PlaneManager] 🔄 Cambio de mapa {currentMapIndex + 1} → {currentMapIndex + 2}");

        OcultarMapaActual();
        listoParaAvanzar = false;

        currentMapIndex++;
        currentPinIndex = 0;

        if (currentMapIndex < mapInstances.Count)
        {
            if (catController != null)
            {
                catController.DetenerMovimiento();
            }

            StartCoroutine(CambiarANuevoMapa());
        }
        else
        {
            Debug.Log("[PlaneManager] 🎊 ¡Todos los mapas completados!");
            todosPinesCompletados = true;
            ActivarFABSiUltimoMapa();
        }
    }

    private IEnumerator CambiarANuevoMapa()
    {
        yield return new WaitForEndOfFrame();

        MostrarMapaYPrimerPin();

        if (verbose)
            Debug.Log($"[PlaneManager] ✅ Cambio a mapa {currentMapIndex + 1} completado");
    }

    // 🔧 PROBLEMA 3 RESUELTO: Métodos públicos con nombres consistentes
    public List<GameObject> GetMapas() => mapInstances;
    public int GetCurrentMapIndex() => currentMapIndex;

    public List<string> GetPinesRecorridos(int mapIndex)
    {
        List<string> lista = new List<string>();
        if (mapIndex >= mapInstances.Count) return lista;

        PinMapa[] pins = mapInstances[mapIndex].GetComponentsInChildren<PinMapa>(true);
        System.Array.Sort(pins, (a, b) => a.OrdenPin.CompareTo(b.OrdenPin));

        for (int i = 0; i <= currentPinIndex && i < pins.Length; i++)
        {
            lista.Add(pins[i].name);
        }
        return lista;
    }

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
        Debug.Log($"[PlaneManager] Regresando a {pinName} en {ObtenerNombreMapa(mapIndex)}");
    }

    private void MostrarMapaYPrimerPin()
    {
        if (currentMapIndex >= mapInstances.Count) return;

        GameObject currentMap = mapInstances[currentMapIndex];
        currentMap.SetActive(true);

        PinMapa[] pins = currentMap.GetComponentsInChildren<PinMapa>(true);
        if (pins.Length == 0)
        {
            Debug.LogWarning($"[PlaneManager] El mapa {currentMapIndex + 1} no tiene pines.");
            return;
        }

        // Ordenar pines por OrdenPin
        System.Array.Sort(pins, (a, b) => a.OrdenPin.CompareTo(b.OrdenPin));

        // Desactivar todos los pines
        foreach (var pin in pins)
            pin.gameObject.SetActive(false);

        // Activar solo el primer pin
        if (currentPinIndex < pins.Length)
        {
            pins[currentPinIndex].gameObject.SetActive(true);

            if (verbose)
                Debug.Log($"[PlaneManager] Mostrando mapa {currentMapIndex + 1}, pin {pins[currentPinIndex].name}, {currentPinIndex + 1}/{pins.Length}");
        }
    }

    private string ObtenerNombreMapa(int index)
    {
        string[] nombres = { "70s", "80s", "90s", "2000s", "2010s" };
        return (index >= 0 && index < nombres.Length) ? nombres[index] : $"Mapa {index + 1}";
    }

    private void ActivarFABSiUltimoMapa()
    {
        // 🔧 PROBLEMA 4 RESUELTO: Verificar correctamente si es el último mapa
        if (currentMapIndex >= mapInstances.Count && todosPinesCompletados)
        {
            if (fabButton != null)
            {
                fabButton.SetActive(true);
                Debug.Log("[PlaneManager] 🎯 FAB activado: todas las épocas completadas.");
            }
            else
            {
                Debug.LogWarning("[PlaneManager] No se asignó el FABButton en el inspector.");
            }
        }
    }
}