using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PanelPreguntasZylo : MonoBehaviour
{
    [Header("Referencias UI - Paneles")]
    [SerializeField] private GameObject panelPreguntas;
    [SerializeField] private GameObject panelRespuesta;

    [Header("Referencias UI - Botones de preguntas")]
    [SerializeField] private Button botonPregunta1;
    [SerializeField] private Button botonPregunta2;

    [Header("Botones específicos para ocultar")]
    [SerializeField] private GameObject buttonDuda1;
    [SerializeField] private GameObject buttonDuda2;

    [Header("Referencia a Zylo")]
    [SerializeField] private Animator zyloAnimator;

    [Header("Configuración de Animaciones")]
    [SerializeField] private string animacionPensar = "isThinking";
    [SerializeField] private string animacionHablar = "isTalking";
    [SerializeField] private float tiempoPensando = 2f;

    private string idPinActual;
    private AudioClip audioRespuesta1Actual;
    private AudioClip audioRespuesta2Actual;
    private PinMapa pinActual;
    private CatController gatoActual;
    private bool esperandoRespuesta = false;

    void Start()
    {
        panelPreguntas?.SetActive(false);
        // Panel de respuesta debe estar visible pero los botones ocultos
        OcultarBotonesDuda();

        if (botonPregunta1 != null)
            botonPregunta1.onClick.AddListener(Pregunta1DesdeInspector);

        if (botonPregunta2 != null)
            botonPregunta2.onClick.AddListener(Pregunta2DesdeInspector);
    }

    public void MostrarPanelPreguntas(PinMapa pin, CatController gato)
    {
        if (pin == null || LectorPreguntas.instance == null || LectorRespuestas.instance == null)
        {
            Debug.LogError("❌ [PanelPreguntasZylo] Error de referencias o pin nulo");
            return;
        }

        // Si hay un diálogo en curso, cancelarlo
        if (esperandoRespuesta)
        {
            StopAllCoroutines();
            esperandoRespuesta = false;
        }

        // Cerrar panel anterior si existe
        panelPreguntas?.SetActive(false);
        OcultarBotonesDuda();

        idPinActual = pin.idPin;
        pinActual = pin;
        gatoActual = gato;

        audioRespuesta1Actual = pin.AudioRespuesta1;
        audioRespuesta2Actual = pin.AudioRespuesta2;

        LectorPreguntas.instance.MostrarPreguntasPorID(pin.idPin);

        StartCoroutine(SecuenciaDialogoInicial());
    }

    private IEnumerator SecuenciaDialogoInicial()
    {
        esperandoRespuesta = true;
        DesactivarBotones();
        OcultarBotonesDuda();

        if (zyloAnimator != null)
            zyloAnimator.SetBool(animacionHablar, true);

        gatoActual?.SetTalking(true);

        AudioSource fuenteAudio = pinActual.GetComponent<AudioSource>();
        AudioClip audioDialogo = pinActual.AudioDelPin;
        string textoDialogo = pinActual.TextoDelPin;

        if (fuenteAudio != null && audioDialogo != null)
        {
            fuenteAudio.PlayOneShot(audioDialogo);

            if (SubtitulosZylo.Instance != null && !string.IsNullOrEmpty(textoDialogo))
                SubtitulosZylo.Instance.MostrarSubtitulosConAudio(textoDialogo, audioDialogo);

            yield return new WaitForSeconds(audioDialogo.length);
        }
        else
        {
            if (SubtitulosZylo.Instance != null && !string.IsNullOrEmpty(textoDialogo))
                SubtitulosZylo.Instance.MostrarTexto(textoDialogo);

            yield return new WaitForSeconds(3f);
        }

        if (zyloAnimator != null)
            zyloAnimator.SetBool(animacionHablar, false);

        gatoActual?.SetTalking(false);

        panelPreguntas?.SetActive(true);
        ActivarBotones();
        MostrarBotonesDuda();
        esperandoRespuesta = false;
    }

    public void Pregunta1DesdeInspector() => EjecutarPregunta(1);
    public void Pregunta2DesdeInspector() => EjecutarPregunta(2);

    private void EjecutarPregunta(int numeroPregunta)
    {
        if (esperandoRespuesta)
        {
            Debug.Log("⏳ [PanelPreguntasZylo] Ya hay una respuesta en proceso...");
            return;
        }

        if (string.IsNullOrEmpty(idPinActual) || pinActual == null)
        {
            Debug.LogError("[PanelPreguntasZylo] No hay pin actual asignado");
            return;
        }

        Debug.Log($"🎯 [PanelPreguntasZylo] Usuario seleccionó pregunta {numeroPregunta}");

        AudioClip audioResp = numeroPregunta == 1 ? audioRespuesta1Actual : audioRespuesta2Actual;

        StartCoroutine(SecuenciaRespuestaCompleta(numeroPregunta, audioResp));
    }

    private IEnumerator SecuenciaRespuestaCompleta(int numeroPregunta, AudioClip audio)
    {
        esperandoRespuesta = true;
        DesactivarBotones();
        OcultarBotonesDuda();

        if (zyloAnimator != null)
            zyloAnimator.SetBool(animacionPensar, true);

        yield return new WaitForSeconds(tiempoPensando);

        if (zyloAnimator != null)
            zyloAnimator.SetBool(animacionPensar, false);

        DatosRespuesta datosResp = LectorRespuestas.instance.ObtenerDatosPorID(idPinActual);
        string textoRespuesta = numeroPregunta == 1 ? datosResp?.respuesta1 : datosResp?.respuesta2;

        if (zyloAnimator != null)
            zyloAnimator.SetBool(animacionHablar, true);

        gatoActual?.SetTalking(true);

        AudioSource fuenteAudio = pinActual.GetComponent<AudioSource>();
        if (fuenteAudio != null && audio != null)
        {
            fuenteAudio.PlayOneShot(audio);

            if (SubtitulosZylo.Instance != null && !string.IsNullOrEmpty(textoRespuesta))
                SubtitulosZylo.Instance.MostrarSubtitulosConAudio(textoRespuesta, audio);

            yield return new WaitForSeconds(audio.length);
        }
        else
        {
            if (SubtitulosZylo.Instance != null && !string.IsNullOrEmpty(textoRespuesta))
                SubtitulosZylo.Instance.MostrarTexto(textoRespuesta);

            yield return new WaitForSeconds(3f);
        }

        if (zyloAnimator != null)
            zyloAnimator.SetBool(animacionHablar, false);

        gatoActual?.SetTalking(false);

        LectorRespuestas.instance.MostrarRespuesta(idPinActual, numeroPregunta);

        ActivarBotones();
        MostrarBotonesDuda();
        esperandoRespuesta = false;
    }

    public void CerrarTodo()
    {
        panelPreguntas?.SetActive(false);

        StopAllCoroutines();
        esperandoRespuesta = false;
        idPinActual = "";
        pinActual = null;
        gatoActual = null;

        if (zyloAnimator != null)
        {
            zyloAnimator.SetBool(animacionPensar, false);
            zyloAnimator.SetBool(animacionHablar, false);
        }

        OcultarBotonesDuda();

        Debug.Log("🔚 [PanelPreguntasZylo] Paneles cerrados y estado reseteado");
    }

    private void DesactivarBotones()
    {
        if (botonPregunta1 != null) botonPregunta1.interactable = false;
        if (botonPregunta2 != null) botonPregunta2.interactable = false;
    }

    private void ActivarBotones()
    {
        if (botonPregunta1 != null) botonPregunta1.interactable = true;
        if (botonPregunta2 != null) botonPregunta2.interactable = true;
    }

    private void OcultarBotonesDuda()
    {
        if (buttonDuda1 != null)
        {
            Debug.Log($"🔴 Ocultando: {buttonDuda1.name}");
            buttonDuda1.SetActive(false);
        }
        if (buttonDuda2 != null)
        {
            Debug.Log($"🔴 Ocultando: {buttonDuda2.name}");
            buttonDuda2.SetActive(false);
        }
    }

    private void MostrarBotonesDuda()
    {
        if (buttonDuda1 != null)
        {
            Debug.Log($"🟢 Mostrando: {buttonDuda1.name}");
            buttonDuda1.SetActive(true);
        }
        if (buttonDuda2 != null)
        {
            Debug.Log($"🟢 Mostrando: {buttonDuda2.name}");
            buttonDuda2.SetActive(true);
        }
    }
}