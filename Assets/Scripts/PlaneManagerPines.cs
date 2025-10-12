using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
public class PlaneManagerPines : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] private ARPlaneManager arPlaneManager;

    [Header("Mapas de Pines (prefabs)")]
    [Tooltip("Asigna aquí los 5 prefabs de mapas diferentes")]
    [SerializeField] private GameObject[] mapasDePines = new GameObject[5];

    [Tooltip("Índice del mapa a colocar (0 a 4)")]
    [Range(0, 4)]
    [SerializeField] private int mapaSeleccionado = 0;

    [Header("Opciones")]
    [SerializeField] private bool stopDetectionAfterPlace = true;
    [SerializeField, Range(0f, 0.02f)] private float lift = 0.003f;

    private GameObject placed;

    void Awake()
    {
        if (!arPlaneManager) arPlaneManager = GetComponent<ARPlaneManager>();
    }

    void OnEnable() => arPlaneManager.planesChanged += OnPlanesChanged;
    void OnDisable() => arPlaneManager.planesChanged -= OnPlanesChanged;

    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (placed != null) return;

        foreach (var plane in args.added)
        {
            if (TryPlaceOnPlane(plane)) return;
        }

        foreach (var plane in args.updated)
        {
            if (TryPlaceOnPlane(plane)) return;
        }
    }

    bool TryPlaceOnPlane(ARPlane plane)
    {
        if (plane == null || plane.trackingState != TrackingState.Tracking) return false;

        if (mapasDePines == null || mapasDePines.Length == 0 || mapasDePines[mapaSeleccionado] == null)
        {
            Debug.LogWarning("[PlaneManagerPines] Prefab de mapa no asignado.");
            return false;
        }

        GameObject mapaPrefab = mapasDePines[mapaSeleccionado];

        Vector3 centerWorld = plane.transform.TransformPoint(plane.center);
        Vector3 planeNormal = plane.transform.up;

        placed = Instantiate(mapaPrefab);
        placed.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(Camera.main.transform.forward, planeNormal), planeNormal);
        placed.transform.position = centerWorld + planeNormal * lift;

        SnapFeetToPlane(plane, planeNormal);

        if (stopDetectionAfterPlace) StopPlaneDetection();

        Debug.Log($"[PlaneManagerPines] Mapa {mapaSeleccionado + 1} colocado.");
        return true;
    }

    void SnapFeetToPlane(ARPlane plane, Vector3 planeNormal)
    {
        var col = plane.GetComponent<Collider>();
        if (col == null) return;

        Vector3 rayStart = placed.transform.position + planeNormal * 1.0f;
        Ray ray = new Ray(rayStart, -planeNormal);

        if (col.Raycast(ray, out RaycastHit hit, 5f))
        {
            Bounds wb = GetWorldBounds(placed);
            float offset = placed.transform.position.y - wb.min.y;
            placed.transform.position = hit.point + planeNormal * (offset + lift);
        }
    }

    static Bounds GetWorldBounds(GameObject go)
    {
        var rends = go.GetComponentsInChildren<Renderer>();
        Bounds b = new Bounds(go.transform.position, Vector3.zero);
        bool hasBounds = false;

        foreach (var r in rends)
        {
            if (!hasBounds)
            {
                b = r.bounds;
                hasBounds = true;
            }
            else
            {
                b.Encapsulate(r.bounds);
            }
        }

        if (!hasBounds) b = new Bounds(go.transform.position, Vector3.one * 0.1f);
        return b;
    }

    public void StopPlaneDetection()
    {
        arPlaneManager.requestedDetectionMode = PlaneDetectionMode.None;
        foreach (var plane in arPlaneManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }
    }
}