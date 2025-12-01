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

    [Header("Configuración de Animaciones")]
    [SerializeField] private string animacionPensar = "isThinking";
    [SerializeField] private float tiempoPensando = 2f;

    private string idPinActual;
    private AudioClip audioRespuesta1Actual;
    private AudioClip audioRespuesta2Actual;
    public PinMapa pinActual;
    private CatController gatoActual;
    private Animator zyloAnimator;
    private bool esperandoRespuesta = false;

    // ✅ NUEVO: Rastrear qué preguntas han sido respondidas
    private bool pregunta1Respondida = false;
    private bool pregunta2Respondida = false;

    void Start()
    {
        panelPreguntas?.SetActive(false);
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

        if (esperandoRespuesta)
        {
            StopAllCoroutines();
            esperandoRespuesta = false;
        }

        panelPreguntas?.SetActive(false);
        OcultarBotonesDuda();

        idPinActual = pin.idPin;
        pinActual = pin;
        gatoActual = gato;

        // ✅ Resetear estado de preguntas respondidas
        pregunta1Respondida = false;
        pregunta2Respondida = false;

        if (gatoActual != null)
        {
            zyloAnimator = gatoActual.GetComponent<Animator>();
        }

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

        gatoActual?.SetTalking(false);

        panelPreguntas?.SetActive(true);
        ActivarBotones();
        MostrarBotonesDuda();
        esperandoRespuesta = false;

        var manager = Object.FindFirstObjectByType<PlaneManager>();
        if (manager != null && pinActual != null)
        {
            GameObject mapaActual = manager.GetMapas()[manager.GetCurrentMapIndex()];
            PinMapa[] pins = mapaActual.GetComponentsInChildren<PinMapa>(true);
            System.Array.Sort(pins, (a, b) => a.OrdenPin.CompareTo(b.OrdenPin));

            bool esUltimoPin = pinActual == pins[pins.Length - 1];

            if (!esUltimoPin)
            {
                manager.NotificarPinCompletado(pinActual);
            }
            else
            {
                Debug.Log("[PanelPreguntasZylo] Último pin detectado. Esperando interacción del usuario para avanzar.");
            }
        }
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

        if (zyloAnimator != null && zyloAnimator.runtimeAnimatorController != null)
            zyloAnimator.SetBool(animacionPensar, true);

        yield return new WaitForSeconds(tiempoPensando);

        if (zyloAnimator != null && zyloAnimator.runtimeAnimatorController != null)
            zyloAnimator.SetBool(animacionPensar, false);

        DatosRespuesta datosResp = LectorRespuestas.instance.ObtenerDatosPorID(idPinActual);
        string textoRespuesta = numeroPregunta == 1 ? datosResp?.respuesta1 : datosResp?.respuesta2;

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

        gatoActual?.SetTalking(false);

        LectorRespuestas.instance.MostrarRespuesta(idPinActual, numeroPregunta);

        // ✅ Marcar pregunta como respondida
        if (numeroPregunta == 1)
            pregunta1Respondida = true;
        else
            pregunta2Respondida = true;

        // ✅ SIEMPRE reactivar botones primero
        ActivarBotones();
        MostrarBotonesDuda();
        esperandoRespuesta = false;

        // ✅ Verificar si es el último pin y mostrar coleccionable sin cerrar panel
        var manager = Object.FindFirstObjectByType<PlaneManager>();

        if (manager != null && pinActual != null)
        {
            GameObject mapaActual = manager.GetMapas()[manager.GetCurrentMapIndex()];
            PinMapa[] pins = mapaActual.GetComponentsInChildren<PinMapa>(true);
            System.Array.Sort(pins, (a, b) => a.OrdenPin.CompareTo(b.OrdenPin));
            bool esUltimoPin = pinActual == pins[pins.Length - 1];

            if (esUltimoPin && (pregunta1Respondida || pregunta2Respondida))
            {
                Debug.Log("[PanelPreguntasZylo] 🎁 Pregunta respondida en último pin. Mostrando coleccionable SIN cerrar panel.");
                manager.MostrarColeccionableSinAvanzar();
            }
        }
    }

    public void CerrarTodo()
    {
        panelPreguntas?.SetActive(false);

        StopAllCoroutines();
        esperandoRespuesta = false;
        idPinActual = "";
        pinActual = null;
        gatoActual = null;
        zyloAnimator = null;

        // ✅ Resetear estado de preguntas
        pregunta1Respondida = false;
        pregunta2Respondida = false;

        OcultarBotonesDuda();

        Debug.Log("🔒 [PanelPreguntasZylo] Paneles cerrados y estado reseteado");
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

    public PinMapa GetPinActual()
    {
        return pinActual;
    }
}