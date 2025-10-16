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
    [Tooltip("Coordenadas en metros desde el gato (x derecha, z adelante)")]
    [SerializeField]
    private List<Vector3> mapOffsets = new List<Vector3>()
    {
        new Vector3(0f, 0f, 0.5f),   // Mapa 1 (frente)
        new Vector3(0.5f, 0f, 0f),   // Mapa 2 (derecha)
        new Vector3(-0.5f, 0f, 0f),  // Mapa 3 (izquierda)
        new Vector3(0f, 0f, -0.5f),  // Mapa 4 (atrás)
        new Vector3(0.5f, 0f, 0.5f)  // Mapa 5 (diagonal)
    };

    [Header("Opciones")]
    [SerializeField, Range(0f, 5f)] private float distanceFromCamera = 2f;
    [SerializeField, Range(0f, 0.02f)] private float lift = 0.003f;
    [SerializeField] private bool verbose = true;

    private GameObject catInstance;

    void Awake()
    {
        if (!arPlaneManager) arPlaneManager = GetComponent<ARPlaneManager>();
    }

    void Start()
    {
        // Desactivar detección de planos (no se necesita)
        arPlaneManager.requestedDetectionMode = PlaneDetectionMode.None;

        // Instanciar objetos frente a la cámara
        PlaceContentInFrontOfCamera();
    }

    void PlaceContentInFrontOfCamera()
    {
        if (catPrefab == null)
        {
            Debug.LogWarning("[PlaneManagerPines] Asigna el prefab del gato.");
            return;
        }

        Transform cam = Camera.main.transform;
        Vector3 forward = cam.forward;
        Vector3 basePos = cam.position + forward * distanceFromCamera;
        basePos.y = cam.position.y - 0.1f; // un poco más abajo para el suelo

        // 🐱 Instancia el gato
        catInstance = Instantiate(catPrefab, basePos, Quaternion.LookRotation(-forward));
        if (verbose)
            Debug.Log("[PlaneManagerPines] Gato instanciado frente a la cámara.");

        // 📍 Instancia los mapas alrededor del gato
        for (int i = 0; i < mapPrefabs.Count; i++)
        {
            if (mapPrefabs[i] == null) continue;

            Vector3 offset = (i < mapOffsets.Count) ? mapOffsets[i] : Vector3.zero;
            Vector3 mapPos = catInstance.transform.position + catInstance.transform.TransformDirection(offset);

            GameObject map = Instantiate(mapPrefabs[i], mapPos, Quaternion.identity);

            // Que mire hacia el gato
            Vector3 lookDir = catInstance.transform.position - map.transform.position;
            lookDir.y = 0;
            map.transform.rotation = Quaternion.LookRotation(lookDir);

            if (verbose)
                Debug.Log($"[PlaneManagerPines] Mapa {i + 1} colocado en offset {offset}");
        }
    }
}