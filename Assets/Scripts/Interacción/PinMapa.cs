using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class PinMapa : MonoBehaviour
{
    [Header("Objetos a mostrar/ocultar al tocar el pin")]
    [SerializeField] private GameObject[] objetosAR;

    public bool FueActivado { get; private set; } = false;
    public string idPin; // ej: "2000_2002"

    [Header("Audio del pin (introducción)")]
    [SerializeField] private AudioClip audioClip;
    [TextArea]
    [SerializeField] private string textoDelPin;

    [Header("Audios de las respuestas")]
    [SerializeField] private AudioClip audioRespuesta1;
    [SerializeField] private AudioClip audioRespuesta2;

    // Propiedades públicas para que PanelPreguntasZylo pueda acceder a ellas
    public AudioClip AudioRespuesta1 => audioRespuesta1;
    public AudioClip AudioRespuesta2 => audioRespuesta2;

    private AudioSource audioSource;

    void Awake()
    {
        if (objetosAR == null) return;
        foreach (var obj in objetosAR)
            if (obj) obj.SetActive(false);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    // Llamado cuando Zylo llega al pin
    public void OnZyloLlego(CatController gato)
    {
        if (!FueActivado)
        {
            FueActivado = true;
            MostrarObjetos();
            StartCoroutine(ReproducirDialogo(gato));
        }
        else
        {
            // Si ya fue activado, alterna mostrar/ocultar objetos
            bool activos = objetosAR.Length > 0 && objetosAR[0].activeSelf;
            if (activos) OcultarObjetos(); else MostrarObjetos();
        }

        // Notificar al manager
        var manager = FindObjectOfType<PlaneManagerPines>();
        manager?.NotificarPinCompletado(this);
    }

    private IEnumerator ReproducirDialogo(CatController gato)
    {
        if (audioClip == null)
        {
            Debug.LogWarning($"[PinMapa] No se asignó audio para el pin {idPin}");
            yield break;
        }

        Debug.Log($"[PinMapa] ▶ Reproduciendo audio del pin: {idPin}");
        gato?.SetTalking(true);

        audioSource.clip = audioClip;
        audioSource.Play();

        if (SubtitulosZylo.Instance != null)
            SubtitulosZylo.Instance.MostrarTexto(textoDelPin);

        yield return new WaitForSeconds(audioClip.length);

        gato?.SetTalking(false);
        Debug.Log($"[PinMapa] Audio finalizado del pin: {idPin}");

        // Mostrar panel de preguntas si existe en la escena
        PanelPreguntasZylo panelPreguntas = FindObjectOfType<PanelPreguntasZylo>(true);
        if (panelPreguntas != null)
        {
            Debug.Log($"[PinMapa] Abriendo PanelPreguntasZylo para el pin {idPin}");
            panelPreguntas.MostrarPanelPreguntas(this);
        }
        else
        {
            Debug.LogWarning("[PinMapa] No se encontró PanelPreguntasZylo en la escena.");
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