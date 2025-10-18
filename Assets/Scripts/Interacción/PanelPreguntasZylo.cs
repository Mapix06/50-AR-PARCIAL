using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PanelPreguntasZylo : MonoBehaviour
{
    [Header("Referencias UI - Paneles")]
    [SerializeField] private GameObject panelPreguntas;
    [SerializeField] private GameObject panelRespuesta;

    [Header("Referencias UI - Botones")]
    [SerializeField] private Button botonPregunta1;
    [SerializeField] private Button botonPregunta2;

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
    private bool esperandoRespuesta = false;

    void Start()
    {
        if (panelPreguntas != null)
            panelPreguntas.SetActive(true);

        if (panelRespuesta != null)
            panelRespuesta.SetActive(true);

        if (botonPregunta1 != null)
            botonPregunta1.onClick.AddListener(Pregunta1DesdeInspector);

        if (botonPregunta2 != null)
            botonPregunta2.onClick.AddListener(Pregunta2DesdeInspector);

    }

    public void MostrarPanelPreguntas(PinMapa pin)
    {
        if (pin == null || LectorPreguntas.instance == null || LectorRespuestas.instance == null)
        {
            Debug.LogError("❌ [PanelPreguntasZylo] Error de referencias o pin nulo");
            return;
        }

        idPinActual = pin.idPin;
        pinActual = pin;

        audioRespuesta1Actual = pin.AudioRespuesta1;
        audioRespuesta2Actual = pin.AudioRespuesta2;

        LectorPreguntas.instance.MostrarPreguntasPorID(pin.idPin);

        Debug.Log($"✅ [PanelPreguntasZylo] Panel abierto para pin: {pin.idPin}");

        if (panelPreguntas != null)
            panelPreguntas.SetActive(true);
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

        if (panelPreguntas != null)
            panelPreguntas.SetActive(true);

        StartCoroutine(SecuenciaRespuestaCompleta(numeroPregunta, audioResp));
    }

    private IEnumerator SecuenciaRespuestaCompleta(int numeroPregunta, AudioClip audio)
    {
        esperandoRespuesta = true;

        if (zyloAnimator != null && zyloAnimator.runtimeAnimatorController != null)
            zyloAnimator.SetBool(animacionPensar, true);

        yield return new WaitForSeconds(tiempoPensando);

        if (zyloAnimator != null && zyloAnimator.runtimeAnimatorController != null)
            zyloAnimator.SetBool(animacionPensar, false);

        DatosRespuesta datosResp = LectorRespuestas.instance.ObtenerDatosPorID(idPinActual);
        string textoRespuesta = numeroPregunta == 1 ? datosResp?.respuesta1 : datosResp?.respuesta2;

        if (zyloAnimator != null && zyloAnimator.runtimeAnimatorController != null)
            zyloAnimator.SetBool(animacionHablar, true);

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

        if (zyloAnimator != null && zyloAnimator.runtimeAnimatorController != null)
            zyloAnimator.SetBool(animacionHablar, false);

        LectorRespuestas.instance.MostrarRespuesta(idPinActual, numeroPregunta);

        if (panelRespuesta != null)
            panelRespuesta.SetActive(true);

        esperandoRespuesta = false;
    }

    private void CerrarRespuesta()
    {
        if (panelRespuesta != null)
            panelRespuesta.SetActive(false);

        if (panelPreguntas != null)
            panelPreguntas.SetActive(true);
    }

    public void CerrarTodo()
    {
        if (panelPreguntas != null)
            panelPreguntas.SetActive(false);

        if (panelRespuesta != null)
            panelRespuesta.SetActive(false);

        StopAllCoroutines();
        esperandoRespuesta = false;
        idPinActual = "";
        pinActual = null;

        if (zyloAnimator != null)
        {
            zyloAnimator.SetBool(animacionPensar, false);
            zyloAnimator.SetBool(animacionHablar, false);
        }

        Debug.Log("🔚 [PanelPreguntasZylo] Paneles cerrados y estado reseteado");
    }
}