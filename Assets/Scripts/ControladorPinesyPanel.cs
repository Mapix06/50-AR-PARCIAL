using UnityEngine;
using UnityEngine.UI;

public class ControladorPinesYPanel : MonoBehaviour
{
    [Header("Botón para marcar todos como visitados")]
    [SerializeField] private Button botonMarcarTodos;

    [Header("Panel a activar")]
    [SerializeField] private GameObject panelPrincipal;

    [Header("Paneles a desactivar")]
    [SerializeField] private GameObject panelRespuestaZylo;
    [SerializeField] private GameObject panelDudaZylo;

    void Start()
    {
        if (botonMarcarTodos != null)
        {
            botonMarcarTodos.onClick.AddListener(MarcarTodosYActivarPanel);
        }
        else
        {
            Debug.LogWarning("[ControladorPinesYPanel] No se asignó el botón en el inspector.");
        }
    }

    public void MarcarTodosYActivarPanel()
    {
        // ⿡ Buscar todos los pines en la escena
        PinMapa[] todosLosPines = FindObjectsOfType<PinMapa>();

        if (todosLosPines.Length == 0)
        {
            Debug.LogWarning("[ControladorPinesYPanel] No se encontraron pines en la escena.");
            return;
        }

        int pinesMarcados = 0;

        // ⿢ Marcar cada pin como visitado
        foreach (PinMapa pin in todosLosPines)
        {
            // Marcar como activado usando reflexión
            typeof(PinMapa)
                .GetProperty("FueActivado")
                .SetValue(pin, true);

            // Mostrar los objetos AR del pin
            pin.MostrarObjetos();

            pinesMarcados++;
        }

        Debug.Log($"[ControladorPinesYPanel] ✓ Se marcaron {pinesMarcados} pines como visitados.");

        // ⿣ Notificar al manager si existe
        var manager = FindObjectOfType<PlaneManager>();
        if (manager != null)
        {
            foreach (PinMapa pin in todosLosPines)
            {
                manager.NotificarPinCompletado(pin);
            }
            Debug.Log("[ControladorPinesYPanel] ✓ PlaneManagerPines notificado.");
        }

        // ⿤ Activar el panel principal
        if (panelPrincipal != null)
        {
            panelPrincipal.SetActive(true);
            Debug.Log("[ControladorPinesYPanel] ✓ Panel Principal activado.");
        }
        else
        {
            Debug.LogWarning("[ControladorPinesYPanel] No se asignó el Panel Principal en el Inspector.");
        }
    }

    void OnDestroy()
    {
        if (botonMarcarTodos != null)
        {
            botonMarcarTodos.onClick.RemoveListener(MarcarTodosYActivarPanel);
        }
    }
}