using UnityEngine;

/// <summary>
/// Componente individual para cada objeto coleccionable
/// Similar a PlanetTouch - adjuntar a cada objeto que se pueda recolectar
/// </summary>
public class ObjetoInteractivoCambioMapa : MonoBehaviour
{
    [Header("Datos del Coleccionable")]
    public int indiceEpoca;
    public string nombreEpoca;
    public GameObject prefabParaMostrar;

    [Header("Efectos")]
    public AudioClip sonidoRecolectar;

    private void OnMouseDown()
    {
        if (!gameObject.activeSelf) return;  // 🔧 NEW: Ignore if not active
        Recolectar();
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            {
                Debug.Log($"[DEBUG] Raycast hit {gameObject.name}");
                Recolectar();
            }
        }
    }
    public void Recolectar()
    {
        Debug.Log($"🔍 [{nombreEpoca}] Recolectar() llamado");

        // Guardar la instancia en una variable local para evitar race conditions
        ColeccionablesViewer viewer = ColeccionablesViewer.Instance;

        Debug.Log($"🔍 Viewer es null? {viewer == null}");

        if (viewer != null)
        {
            Debug.Log($"✅ Viewer encontrado en GameObject: {viewer.gameObject.name}");
            Debug.Log($"✅ Viewer está activo? {viewer.gameObject.activeInHierarchy}");
        }

        // Verificar que el manager exista
        if (viewer == null)
        {
            Debug.LogError($"❌ [{nombreEpoca}] No se encontró ColeccionablesViewer en la escena.");
            return;
        }

        // Verificar que tenga prefab asignado
        if (prefabParaMostrar == null)
        {
            Debug.LogError($"❌ [{nombreEpoca}] No se asignó el prefab para mostrar.");
            return;
        }

        Debug.Log($"✅ [{nombreEpoca}] Prefab asignado: {prefabParaMostrar.name}");
        Debug.Log($"🎯 [{nombreEpoca}] Llamando a MostrarColeccionable con índice: {indiceEpoca}");

        // Enviar al visor para que lo muestre en la cámara
        viewer.MostrarColeccionable(
            indiceEpoca,
            nombreEpoca,
            prefabParaMostrar,
            sonidoRecolectar
        );

        Debug.Log($"✅ [{nombreEpoca}] MostrarColeccionable llamado exitosamente");

        // 🔧 NUEVO: Notificar al PlaneManager para avanzar al siguiente mapa
        PlaneManager planeManager = FindFirstObjectByType<PlaneManager>();
        if (planeManager != null)
        {
            Debug.Log($"[{nombreEpoca}] 🔄 Notificando cambio de mapa...");
            planeManager.OnObjetoAvanzarClickeado();
        }
        else
        {
            Debug.LogWarning($"⚠️ [{nombreEpoca}] No se encontró PlaneManager para avanzar.");
        }

        // Desactivar o destruir este objeto
        gameObject.SetActive(false);
    }
}
