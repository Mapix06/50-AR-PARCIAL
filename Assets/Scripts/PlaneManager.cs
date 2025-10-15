using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
public class PlaneManager : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] private ARPlaneManager arPlaneManager;

    [Header("Prefab del gato")]
    [SerializeField] private GameObject model3DPrefab;

    [Header("Opciones")]
    [Tooltip("Oculta planos tras colocar el modelo")]
    [SerializeField] private bool stopDetectionAfterPlace = true;

    [Tooltip("Separación mínima del plano para evitar z-fighting (m)")]
    [SerializeField, Range(0f, 0.02f)] private float lift = 0.003f;

    [Header("Debug")]
    [SerializeField] private bool verbose = true;

    private GameObject placed;
    private CatController catController;

    void Awake()
    {
        if (!arPlaneManager) arPlaneManager = GetComponent<ARPlaneManager>();
    }

    void OnEnable() { arPlaneManager.planesChanged += OnPlanesChanged; }
    void OnDisable() { arPlaneManager.planesChanged -= OnPlanesChanged; }

    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (placed != null) return;

        foreach (var p in args.added) { if (TryPlaceOnPlane(p)) return; }
        foreach (var p in args.updated) { if (TryPlaceOnPlane(p)) return; }
    }

    bool TryPlaceOnPlane(ARPlane plane)
    {
        if (plane == null || plane.trackingState != TrackingState.Tracking) return false;
        if (model3DPrefab == null) { Debug.LogWarning("[PlaneManager] Asigna model3DPrefab."); return false; }

        // 1) Centro del plano en mundo y normal del plano
        Vector3 centerWorld = plane.transform.TransformPoint(plane.center);
        Vector3 planeNormal = plane.transform.up;

        // 2) Instancia el modelo, orientado hacia la cámara proyectado al plano
        placed = Instantiate(model3DPrefab);
        Vector3 camF = Camera.main ? Camera.main.transform.forward : Vector3.forward;
        Vector3 forwardOnPlane = Vector3.ProjectOnPlane(camF, planeNormal).normalized;
        if (forwardOnPlane.sqrMagnitude < 1e-4f)
            forwardOnPlane = Vector3.ProjectOnPlane(Vector3.forward, planeNormal).normalized;
        placed.transform.rotation = Quaternion.LookRotation(forwardOnPlane, planeNormal);

        // Posición inicial aproximada (un poco sobre el centro del plano)
        placed.transform.position = centerWorld + planeNormal * lift;

        // 3) SNAP de "los pies" usando bounds.min y raycast al MeshCollider del ARPlane
        SnapFeetToPlane(plane, planeNormal);

        // 4) Cache para UI/botones
        catController = placed.GetComponent<CatController>();

        if (stopDetectionAfterPlace) StopPlaneDetection();

        if (verbose)
        {
            Vector3 planePos = plane.transform.position;
            Bounds wb = GetWorldBounds(placed);
            Debug.Log($"[PlaneManager] PlaneY:{planePos.y:F2} | center:{centerWorld} | " +
                      $"bounds.minY:{wb.min.y:F3} | placed:{placed.transform.position} | normal:{planeNormal}");
        }
        return true;
    }

    void SnapFeetToPlane(ARPlane plane, Vector3 planeNormal)
    {
        // Debe existir un collider en el plano (MeshCollider en el prefab 'AR Default Plane')
        var col = plane.GetComponent<Collider>();
        if (col == null) return;

        // Lanza un rayo desde arriba del modelo hacia el plano
        Vector3 rayStart = placed.transform.position + planeNormal * 1.0f;
        Ray ray = new Ray(rayStart, -planeNormal);

        if (col.Raycast(ray, out RaycastHit hit, 5f))
        {
            // Calcula el offset desde el pivote del modelo hasta la base real (bounds.min)
            Bounds wb = GetWorldBounds(placed);
            float baseOffsetFromPivot = placed.transform.position.y - wb.min.y; // cuánto hay desde pivote hasta “pies” en Y mundial

            // Nueva posición = punto del plano + offset hasta la base + leve lift
            Vector3 newPos = placed.transform.position;
            newPos = hit.point + planeNormal * (baseOffsetFromPivot + lift);
            placed.transform.position = newPos;
        }
    }

    static Bounds GetWorldBounds(GameObject go)
    {
        // Combina bounds de todos los SkinnedMeshRenderer/Renderer en espacio de mundo
        var smrs = go.GetComponentsInChildren<SkinnedMeshRenderer>();
        var rends = go.GetComponentsInChildren<Renderer>();

        bool hasAny = false;
        Bounds b = new Bounds(go.transform.position, Vector3.zero);

        foreach (var s in smrs)
        {
            if (!hasAny) { b = s.bounds; hasAny = true; }
            else b.Encapsulate(s.bounds);
        }
        foreach (var r in rends)
        {
            // evita contar colliders visuales del plano si accidentalmente están parentados
            if (r is SkinnedMeshRenderer) continue;
            if (!hasAny) { b = r.bounds; hasAny = true; }
            else b.Encapsulate(r.bounds);
        }

        if (!hasAny) b = new Bounds(go.transform.position, Vector3.one * 0.1f);
        return b;
    }

    public void StopPlaneDetection()
    {
        if (!arPlaneManager) return;
        arPlaneManager.requestedDetectionMode = PlaneDetectionMode.None;
        foreach (var p in arPlaneManager.trackables) p.gameObject.SetActive(false);
    }

}