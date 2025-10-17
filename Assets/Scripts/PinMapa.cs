using UnityEngine;

public class PinMapa : MonoBehaviour
{
    [Header("Estado del pin")]
    [SerializeField] private bool fueActivado = false;

    [Header("Objetos a ocultar")]
    [SerializeField] private GameObject[] objetosParaOcultar;

    [Header("Preguntas interactivas")]
    [Tooltip("Primera pregunta que el usuario puede hacer a Zylo")]
    [SerializeField] private PreguntaRespuesta preguntaInteractiva1;

    [Tooltip("Segunda pregunta que el usuario puede hacer a Zylo")]
    [SerializeField] private PreguntaRespuesta preguntaInteractiva2;

    [Header("Referencias")]
    [SerializeField] private PanelPreguntasZylo panelPreguntasZylo;

    private AudioSource audioSource;

    public bool FueActivado => fueActivado;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Si no se asignó el panel desde el Inspector, buscarlo en la escena
        if (panelPreguntasZylo == null)
            panelPreguntasZylo = FindObjectOfType<PanelPreguntasZylo>();
    }

    /// <summary>
    /// Llamado cuando Zylo llega al pin
    /// </summary>
    public void OnPinClicked()
    {
        if (fueActivado)
        {
            Debug.Log($"[PinMapa] {name} ya fue activado previamente.");
            return;
        }

        fueActivado = true;
        Debug.Log($"[PinMapa] {name} activado por primera vez.");

        // El audio se reproduce desde el CatController
        // Aquí solo marcamos que fue activado
    }

    /// <summary>
    /// Llamado después de que termine el audio del pin
    /// </summary>
    public void MostrarPreguntasInteractivas()
    {
        if (panelPreguntasZylo == null)
        {
            Debug.LogWarning($"[PinMapa] {name}: No se encontró PanelPreguntasZylo en la escena");
            return;
        }

        if (preguntaInteractiva1 == null || preguntaInteractiva2 == null)
        {
            Debug.LogWarning($"[PinMapa] {name}: No se configuraron las preguntas interactivas");
            return;
        }

        Debug.Log($"[PinMapa] {name}: Mostrando preguntas interactivas");
        panelPreguntasZylo.MostrarPanelPreguntas(preguntaInteractiva1, preguntaInteractiva2);
    }

    /// <summary>
    /// Oculta los objetos asociados al pin cuando se cambia de mapa
    /// </summary>
    public void OcultarObjetos()
    {
        if (objetosParaOcultar == null || objetosParaOcultar.Length == 0)
            return;

        foreach (var obj in objetosParaOcultar)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        Debug.Log($"[PinMapa] {name}: Objetos ocultados");
    }

    /// <summary>
    /// Reinicia el estado del pin (opcional)
    /// </summary>
    public void Reiniciar()
    {
        fueActivado = false;

        // Reactivar objetos
        if (objetosParaOcultar != null)
        {
            foreach (var obj in objetosParaOcultar)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }

        Debug.Log($"[PinMapa] {name}: Reiniciado");
    }
}