using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class PinMapa : MonoBehaviour
{
    [Header("Objetos a mostrar/ocultar al tocar el pin")]
    [SerializeField] private GameObject[] objetosAR;

    [Header("Letrero del pin (se oculta al completar)")]
    [SerializeField] private GameObject letrero;

    public bool FueActivado { get; private set; } = false;

    public string idPin; // ej: "2000_2002"

    [Header("Orden de aparición")]
    [SerializeField] private int ordenPin = 0;
    public int OrdenPin => ordenPin;

    [Header("Audio del pin (introducción)")]
    [SerializeField] private AudioClip audioClip;

    [TextArea]
    [SerializeField] private string textoDelPin;

    [Header("Audios de las respuestas")]
    [SerializeField] private AudioClip audioRespuesta1;
    [SerializeField] private AudioClip audioRespuesta2;

    [Header("Referencias directas")]
    [SerializeField] private PanelPreguntasZylo panelPreguntasZylo;

    // Propiedades públicas
    public AudioClip AudioRespuesta1 => audioRespuesta1;
    public AudioClip AudioRespuesta2 => audioRespuesta2;
    public AudioClip AudioDelPin => audioClip;
    public string TextoDelPin => textoDelPin;

    private AudioSource audioSource;

    void Awake()
    {
        // Ocultar objetos AR al inicio
        if (objetosAR != null)
        {
            foreach (var obj in objetosAR)
                if (obj) obj.SetActive(false);
        }

        // Ocultar letrero al inicio
        if (letrero != null)
        {
            letrero.SetActive(false);
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    // Mostrar letrero cuando el pin se activa en la escena
    void OnEnable()
    {
        if (letrero != null)
        {
            letrero.SetActive(true);
        }
    }

    void OnDisable()
    {
        // Cuando el pin se desactiva, ocultar el letrero
        if (letrero != null)
        {
            letrero.SetActive(false);
        }
    }

    // Llamado cuando Zylo llega al pin
    public void OnZyloLlego(CatController gato)
    {
        if (!FueActivado)
        {
            FueActivado = true;
            MostrarObjetos();
            IniciarDialogoConPanel(gato);

            // 🆕 Notificar inmediatamente al hacer click
            var manager = Object.FindFirstObjectByType<PlaneManagerPines>();
            if (manager != null)
            {
                Debug.Log($"[PinMapa] Pin {idPin} clickeado, notificando al manager");
                manager.NotificarPinCompletado(this);
            }
        }
        else
        {
            // Si ya fue activado, alterna mostrar/ocultar objetos
            bool activos = objetosAR != null && objetosAR.Length > 0 && objetosAR[0].activeSelf;
            if (activos)
                OcultarObjetos();
            else
                MostrarObjetos();
        }
    }

    private void IniciarDialogoConPanel(CatController gato)
    {
        Debug.Log($"[PinMapa] Pin {idPin} activado, buscando panel de preguntas...");

        PanelPreguntasZylo panelPreguntas = panelPreguntasZylo;

        if (panelPreguntas == null)
        {
            panelPreguntas = Object.FindFirstObjectByType<PanelPreguntasZylo>(FindObjectsInactive.Include);
        }

        if (panelPreguntas != null)
        {
            Debug.Log($"[PinMapa] ✅ Panel encontrado, iniciando diálogo para pin {idPin}");
            panelPreguntas.MostrarPanelPreguntas(this, gato);
        }
        else
        {
            Debug.LogError("[PinMapa] ❌ No se encontró PanelPreguntasZylo en la escena.");
        }
    }

    public void MostrarObjetos()
    {
        if (objetosAR == null) return;

        foreach (var obj in objetosAR)
            if (obj) obj.SetActive(true);
    }

    public void OcultarObjetos()
    {
        if (objetosAR == null) return;

        foreach (var obj in objetosAR)
            if (obj) obj.SetActive(false);
    }
}