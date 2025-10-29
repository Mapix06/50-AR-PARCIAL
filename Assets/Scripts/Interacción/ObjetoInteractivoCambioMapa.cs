using UnityEngine;

public class ObjetoInteractivoCambioMapa : MonoBehaviour
{
    private PlaneManagerPines manager;

    void Start()
    {
        manager = FindFirstObjectByType<PlaneManagerPines>();
        if (manager == null)
        {
            Debug.LogWarning("[ObjetoInteractivoCambioMapa] No se encontró PlaneManagerPines en la escena.");
        }
    }

    void Update()
    {
        // 📱 Entrada móvil
        if (Application.isMobilePlatform && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
                RevisarInteraccion(touch.position);
        }

        // 🖱️ Entrada PC
        if (Input.GetMouseButtonDown(0))
        {
            RevisarInteraccion(Input.mousePosition);
        }
    }

    private void RevisarInteraccion(Vector3 posicionPantalla)
    {
        Ray ray = Camera.main.ScreenPointToRay(posicionPantalla);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                IntentarAvanzar();
            }
        }
    }

    private void IntentarAvanzar()
    {
        if (manager == null)
        {
            Debug.LogWarning("[ObjetoInteractivoCambioMapa] No hay referencia a PlaneManagerPines.");
            return;
        }

        if (manager.PuedeAvanzar())
        {
            Debug.Log("[ObjetoInteractivoCambioMapa] ✅ Jugador tocó el objeto. Avanzando al siguiente mapa...");
            manager.AvanzarAlSiguienteMapa();
        }
        else
        {
            Debug.Log("[ObjetoInteractivoCambioMapa] 🚫 No puedes avanzar todavía. Faltan pines.");
        }
    }
}
