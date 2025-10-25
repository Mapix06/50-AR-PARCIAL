using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FabNavegacionZylo : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlaneManagerPines planeManager;
    [SerializeField] private Button fabButton;
    [SerializeField] private GameObject panelOpciones;
    [SerializeField] private GameObject botonPrefab;

    private bool mostrandoEpocas = true;
    private int epocaSeleccionada = -1;

    private void Start()
    {
        fabButton.onClick.AddListener(ToggleFAB);
        panelOpciones.SetActive(false);
    }

    private void Update()
    {
        if (planeManager == null) return;

        // Si los mapas aún no se han creado, oculta el FAB
        if (planeManager.GetMapas().Count == 0)
        {
            fabButton.gameObject.SetActive(false);
            return;
        }

        // Mostrar FAB solo cuando haya al menos un mapa activo
        if (!fabButton.gameObject.activeSelf)
            fabButton.gameObject.SetActive(true);
    }


    private void ToggleFAB()
    {
        if (panelOpciones.activeSelf)
        {
            panelOpciones.SetActive(false);
        }
        else
        {
            ActualizarOpciones();
            panelOpciones.SetActive(true);
        }
    }

    private void ActualizarOpciones()
    {
        Debug.Log("📋 Actualizando opciones del FAB...");
        Debug.Log($"🔍 Mapas detectados: {(planeManager.GetMapas() != null ? planeManager.GetMapas().Count : 0)}");

        if (planeManager.GetMapas() != null && planeManager.GetMapas().Count > 0)
        {
            for (int i = 0; i < planeManager.GetMapas().Count; i++)
            {
                Debug.Log($"🗺️ Mapa {i} activo: {planeManager.GetMapas()[i].activeSelf}");
            }
        }
        else
        {
            Debug.Log("⚠️ No hay mapas cargados aún.");
        }

        // Limpiar botones previos
        foreach (Transform child in panelOpciones.transform)
            Destroy(child.gameObject);

        if (planeManager == null || planeManager.GetMapas() == null)
            return;

        if (mostrandoEpocas)
        {
            // Mostrar épocas completadas (mapas anteriores al actual)
            for (int i = 0; i < planeManager.GetMapas().Count; i++)
            {
                if (i < planeManager.GetCurrentMapIndex())
                {
                    CrearBoton($"Volver a {ObtenerNombreEpoca(i)}", () =>
                    {
                        epocaSeleccionada = i;
                        mostrandoEpocas = false;
                        ActualizarOpciones();
                    });
                }
            }
        }
        else
        {
            // Mostrar pines recorridos dentro de la época seleccionada
            List<string> pines = GetPinesRecorridosPorLectura(epocaSeleccionada);

            foreach (var pin in pines)
            {
                CrearBoton($"Pin {pin}", () =>
                {
                    planeManager.IrAlPin(epocaSeleccionada, pin);
                    panelOpciones.SetActive(false);
                });
                Debug.Log($"Se generó un botón para: {pin}");
            }
            



            CrearBoton("⬅ Volver a épocas", () =>
            {
                mostrandoEpocas = true;
                ActualizarOpciones();
            });
        }
    }

    private void CrearBoton(string texto, UnityEngine.Events.UnityAction accion)
    {
        GameObject nuevo = Instantiate(botonPrefab, panelOpciones.transform);
        nuevo.GetComponentInChildren<TextMeshProUGUI>().text = texto;
        nuevo.GetComponent<Button>().onClick.AddListener(accion);
    }

    private string ObtenerNombreEpoca(int index)
    {
        string[] nombres = { "70s", "80s", "90s", "2000s", "2010s" };
        return (index >= 0 && index < nombres.Length) ? nombres[index] : $"Mapa {index + 1}";
    }
    private List<string> GetPinesRecorridosPorLectura(int mapIndex)
    {
        List<string> lista = new();
        var mapas = planeManager.GetMapas();
        if (mapIndex < 0 || mapIndex >= mapas.Count) return lista;

        GameObject mapa = mapas[mapIndex];
        PinMapa[] pines = mapa.GetComponentsInChildren<PinMapa>(true);

        // leer los pines activos o ya mostrados
        foreach (var pin in pines)
        {
            if (pin.gameObject.activeSelf)
                lista.Add(pin.name);
        }

        return lista;
    }

}
