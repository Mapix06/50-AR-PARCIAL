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

    private GameObject catInstance;
    private readonly List<GameObject> mapInstances = new List<GameObject>();
    private int currentMapIndex = 0;
    private int currentPinIndex = 0;
    private bool contentPlaced = false;

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
        if (contentPlaced)
            return;

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
            {
                plane.gameObject.SetActive(false);
            }

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

        // Desactivar todos los pines primero
        foreach (var pin in pins)
        {
            pin.gameObject.SetActive(false);
        }

        // Activar solo el primer pin
        if (currentPinIndex < pins.Length)
        {
            pins[currentPinIndex].gameObject.SetActive(true);
            if (verbose)
                Debug.Log($"[PlaneManagerPines] Mostrando mapa {currentMapIndex + 1}, pin {currentPinIndex + 1}/{pins.Length}");
        }
    }

    public void NotificarPinCompletado(PinMapa pin)
    {
        if (pin == null) return;

        if (verbose)
            Debug.Log($"[PlaneManagerPines] Pin completado: {pin.name}");

        if (currentMapIndex >= mapInstances.Count) return;

        GameObject currentMap = mapInstances[currentMapIndex];
        PinMapa[] pins = currentMap.GetComponentsInChildren<PinMapa>(true);

        // Avanzar al siguiente pin
        currentPinIndex++;

        if (currentPinIndex < pins.Length)
        {
            // Hay más pines en este mapa, activar el siguiente
            pins[currentPinIndex].gameObject.SetActive(true);

            if (verbose)
                Debug.Log($"[PlaneManagerPines] Activando siguiente pin: {currentPinIndex + 1}/{pins.Length}");
        }
        else
        {
            // Se completaron todos los pines de este mapa
            if (verbose)
                Debug.Log($"[PlaneManagerPines] Todos los pines del mapa {currentMapIndex + 1} completados.");
            ColeccionablesManager.Instance?.RecolectarPorEpoca(currentMapIndex);
            // Ocultar objetos AR del mapa actual
            OcultarObjetosDelMapaActual();

            // Cerrar panel de preguntas
            PanelPreguntasZylo panel = Object.FindFirstObjectByType<PanelPreguntasZylo>(FindObjectsInactive.Include);
            panel?.CerrarTodo();

            // 🆕 OCULTAR el mapa actual antes de avanzar
            OcultarMapaActual();

            // Avanzar al siguiente mapa
            AvanzarAlSiguienteMapa();
        }
    }

    private void OcultarObjetosDelMapaActual()
    {
        if (currentMapIndex >= mapInstances.Count) return;

        GameObject mapaActual = mapInstances[currentMapIndex];
        PinMapa[] pins = mapaActual.GetComponentsInChildren<PinMapa>(true);

        foreach (var pin in pins)
            pin.OcultarObjetos();
    }

    // 🆕 Método para ocultar completamente el mapa actual
    private void OcultarMapaActual()
    {
        if (currentMapIndex >= mapInstances.Count) return;

        GameObject mapaActual = mapInstances[currentMapIndex];
        mapaActual.SetActive(false);

        if (verbose)
            Debug.Log($"[PlaneManagerPines] Mapa {currentMapIndex + 1} ocultado.");
    }

    private void AvanzarAlSiguienteMapa()
    {
        currentMapIndex++;
        currentPinIndex = 0; // Resetear índice de pines para el nuevo mapa

        if (currentMapIndex < mapInstances.Count)
        {
            MostrarMapaYPrimerPin();
            if (verbose)
                Debug.Log($"[PlaneManagerPines] Pasando al mapa {currentMapIndex + 1}");
        }
        else
        {
            Debug.Log("[PlaneManagerPines] ¡Todos los mapas completados!");
            //  Activa el FABButton cuando se llegue al último mapa
            if (fabButton != null)
            {
                var fab = fabButton.GetComponent<FABDropdownDown>();
                if (fab != null)
                {
                    fab.DesbloquearFAB();
                }
                else
                {
                    fabButton.SetActive(true);
                    Debug.Log("[PlaneManagerPines] FAB activado directamente (sin script).");
                }

            }
            else
            {
                Debug.LogWarning("[PlaneManagerPines] No se asignó el FABButton en el inspector.");
            }
        }
    }

    // Permitir que el FAB lea los mapas
    public List<GameObject> GetMapas() => mapInstances;
    public int GetCurrentMapIndex() => currentMapIndex;

    // Pines recorridos por mapa (puedes mejorarlo si llevas registro exacto)
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
                Debug.Log("[PlaneManagerPines]  FAB activado: todas las épocas completadas.");
            }
            else
            {
                Debug.LogWarning("[PlaneManagerPines]  No se asignó el FABButton en el inspector.");
            }
        }
    }

}