using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
public class PlaneManagerPines : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] private ARPlaneManager arPlaneManager;

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

    void Awake()
    {
        if (!arPlaneManager)
            arPlaneManager = GetComponent<ARPlaneManager>();
    }

    void Start()
    {
        arPlaneManager.requestedDetectionMode = PlaneDetectionMode.None;
        PlaceContentInFrontOfCamera();
    }

    void Update()
    {
        if (checkingPins && !esperandoAudio)
            CheckPinsCompletion();
    }

    private void PlaceContentInFrontOfCamera()
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

        Vector3 forward = cam.forward;
        Vector3 basePos = cam.position + forward * distanceFromCamera;
        basePos.y = cam.position.y - 0.1f;

        // 🐱 Gato frente a cámara
        catInstance = Instantiate(catPrefab, basePos, Quaternion.LookRotation(-forward));

        // 📍 Instancia mapas (ocultos)
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
            esperandoAudio = true; // ahora esperamos a que el gato nos avise
            if (verbose)
                Debug.Log($"[PlaneManagerPines] Todos los pines del mapa {currentMapIndex + 1} activados. Esperando fin de audio…");
        }
    }

    /// <summary>
    /// Llamado por CatController cuando terminó el audio del último pin tocado.
    /// </summary>
    public void SolicitarCambioDeMapa()
    {
        if (!esperandoAudio) return;
        esperandoAudio = false;

        OcultarObjetosDelMapaActual(); // solo ocultar objetos de pines
        AvanzarAlSiguienteMapa();      // activar próximo mapa
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

    // ✅ Llamado por CatController cuando un pin termina su audio
    public void NotificarPinCompletado(PinMapa pin)
    {
        if (pin == null) return;

        if (verbose)
            Debug.Log($"[PlaneManagerPines] Pin completado: {pin.name}");

        // Verificar si todos los pines del mapa actual ya se activaron y terminaron
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

            // Si ya todos fueron activados → avanzar al siguiente mapa
            if (todosActivados)
            {
                if (verbose)
                    Debug.Log($"[PlaneManagerPines] Todos los pines del mapa {currentMapIndex + 1} completados, pasando al siguiente...");

                SolicitarCambioDeMapa();
            }
        }
    }

}
