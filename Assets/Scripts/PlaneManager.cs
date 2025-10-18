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
    private bool checkingPins = false;
    private bool esperandoAudio = false;
    private bool contentPlaced = false;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        if (!arPlaneManager)
            arPlaneManager = GetComponent<ARPlaneManager>();

        if (!arRaycastManager)
            arRaycastManager = FindObjectOfType<ARRaycastManager>();
    }

    void Start()
    {
        // ✅ HABILITAMOS la detección de planos
        arPlaneManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;

        if (verbose)
            Debug.Log("[PlaneManagerPines] Buscando plano horizontal frente al usuario...");
    }

    void Update()
    {
        // Si ya colocamos el contenido, solo verificamos pines
        if (contentPlaced)
        {
            if (checkingPins && !esperandoAudio)
                CheckPinsCompletion();
            return;
        }

        // Buscamos plano automáticamente frente al usuario
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

        // Hacer raycast desde el centro de la pantalla hacia adelante
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            ARRaycastHit hit = hits[0];

            // Calcular posición frente al usuario a la distancia especificada
            Vector3 forward = cam.forward;
            forward.y = 0; // mantener en horizontal
            forward.Normalize();

            Vector3 targetPosition = cam.position + forward * distanceFromCamera;

            // Usar la altura Y del plano detectado
            targetPosition.y = hit.pose.position.y;

            PlaceContentAtPosition(targetPosition);

            // Deshabilitamos la detección de planos después de colocar
            arPlaneManager.requestedDetectionMode = PlaneDetectionMode.None;

            // Opcional: ocultar planos visuales
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

        // 🐱 Gato frente al usuario, sobre el plano
        Vector3 directionToCat = cam.position - position;
        directionToCat.y = 0;
        Quaternion catRotation = Quaternion.LookRotation(directionToCat);

        catInstance = Instantiate(catPrefab, position, catRotation);

        if (verbose)
            Debug.Log($"[PlaneManagerPines] Gato colocado en: {position}");

        // 📍 Instancia mapas (ocultos) relativos al gato
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

        // Muestra el primero
        if (mapInstances.Count > 0)
        {
            currentMapIndex = 0;
            mapInstances[0].SetActive(true);
            checkingPins = true;
            if (verbose) Debug.Log("[PlaneManagerPines] Mostrando mapa inicial (1).");
        }
    }

    private void CheckPinsCompletion()
    {
        if (currentMapIndex >= mapInstances.Count) return;

        GameObject currentMap = mapInstances[currentMapIndex];
        PinMapa[] pins = currentMap.GetComponentsInChildren<PinMapa>(true);
        if (pins.Length == 0) return;

        bool todosActivados = true;
        foreach (var p in pins)
        {
            if (!p.FueActivado) { todosActivados = false; break; }
        }

        if (todosActivados)
        {
            esperandoAudio = true;
            if (verbose)
                Debug.Log($"[PlaneManagerPines] Todos los pines del mapa {currentMapIndex + 1} activados. Esperando fin de audio…");
        }
    }

    public void SolicitarCambioDeMapa()
    {
        if (!esperandoAudio) return;
        esperandoAudio = false;

        OcultarObjetosDelMapaActual();
        AvanzarAlSiguienteMapa();
    }

    private void OcultarObjetosDelMapaActual()
    {
        if (currentMapIndex >= mapInstances.Count) return;

        GameObject mapaActual = mapInstances[currentMapIndex];
        PinMapa[] pins = mapaActual.GetComponentsInChildren<PinMapa>(true);
        foreach (var pin in pins)
            pin.OcultarObjetos();
    }

    private void AvanzarAlSiguienteMapa()
    {
        currentMapIndex++;
        if (currentMapIndex < mapInstances.Count)
        {
            mapInstances[currentMapIndex].SetActive(true);
            checkingPins = true;
            if (verbose) Debug.Log($"[PlaneManagerPines] Mostrando mapa {currentMapIndex + 1}");
        }
        else
        {
            checkingPins = false;
            Debug.Log("[PlaneManagerPines] Todos los mapas completados.");
        }
    }

    public void NotificarPinCompletado(PinMapa pin)
    {
        if (pin == null) return;

        if (verbose)
            Debug.Log($"[PlaneManagerPines] Pin completado: {pin.name}");

        if (currentMapIndex < mapInstances.Count)
        {
            GameObject currentMap = mapInstances[currentMapIndex];
            PinMapa[] pins = currentMap.GetComponentsInChildren<PinMapa>(true);

            bool todosActivados = true;
            foreach (var p in pins)
            {
                if (!p.FueActivado)
                {
                    todosActivados = false;
                    break;
                }
            }

            if (todosActivados)
            {
                if (verbose)
                    Debug.Log($"[PlaneManagerPines] Todos los pines del mapa {currentMapIndex + 1} completados, pasando al siguiente...");

                SolicitarCambioDeMapa();
            }
        }
    }
}