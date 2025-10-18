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
    [SerializeField] private Button botonCerrarRespuesta;

    [Header("Referencia a Zylo")]
    [SerializeField] private Animator zyloAnimator;
    [SerializeField] private AudioSource zyloAudioSource;

    [Header("Configuración de Animaciones")]
    [SerializeField] private string animacionPensar = "isThinking";
    [SerializeField] private string animacionHablar = "isTalking";
    [SerializeField] private float tiempoPensando = 2f;

    [Header("Configuración de Audios")]
    [SerializeField] private string carpetaAudios = "Audios/Pines/"; // Ruta en Resources

    // ID del pin actual y audios cargados dinámicamente
    private string idPinActual;
    private AudioClip audioPreguntaActual;
    private AudioClip audioRespuesta1Actual;
    private AudioClip audioRespuesta2Actual;
    private bool esperandoRespuesta = false;

    void Start()
    {
        // Ocultar paneles al inicio
        if (panelPreguntas != null)
            panelPreguntas.SetActive(true);

        if (panelRespuesta != null)
            panelRespuesta.SetActive(true);

        // Configurar listeners de botones
        if (botonPregunta1 != null)
            botonPregunta1.onClick.AddListener(() => HacerPregunta(1));

        if (botonPregunta2 != null)
            botonPregunta2.onClick.AddListener(() => HacerPregunta(2));

        if (botonCerrarRespuesta != null)
            botonCerrarRespuesta.onClick.AddListener(CerrarRespuesta);
    }

    /// <summary>
    /// Método principal que se llama cuando Zylo llega a un pin.
    /// Los lectores se encargan de actualizar sus propios textos.
    /// </summary>
    public void MostrarPanelPreguntas(PinMapa pin)
    {
        if (pin == null)
        {
            Debug.LogError("❌ [PanelPreguntasZylo] Pin es null");
            return;
        }

        // Verificar que los lectores existen
        if (LectorPreguntas.instance == null)
        {
            Debug.LogError("❌ [PanelPreguntasZylo] LectorPreguntas.instance es null. ¿Está en la escena?");
            return;
        }

        if (LectorRespuestas.instance == null)
        {
            Debug.LogError("❌ [PanelPreguntasZylo] LectorRespuestas.instance es null. ¿Está en la escena?");
            return;
        }

        // Guardar ID del pin actual
        idPinActual = pin.idPin;

        // El LECTOR DE PREGUNTAS actualiza sus propios textos (botones) en la UI
        LectorPreguntas.instance.MostrarPreguntasPorID(pin.idPin);

        Debug.Log($"✅ [PanelPreguntasZylo] Panel abierto para pin: {pin.idPin}");

        // Mostrar el panel de preguntas
        if (panelPreguntas != null)
            panelPreguntas.SetActive(true);

        // Reproducir audio de bienvenida/pregunta si existe
        if (zyloAudioSource != null && audioPreguntaActual != null)
        {
            zyloAudioSource.clip = audioPreguntaActual;
            zyloAudioSource.Play();

            if (zyloAnimator != null)
                zyloAnimator.SetBool(animacionHablar, true);

            Invoke(nameof(DetenerHabla), audioPreguntaActual.length);
        }

    }

    /// <summary>
    /// Se ejecuta cuando el usuario hace clic en una pregunta
    /// </summary>
    private void HacerPregunta(int numeroPregunta)
    {
        if (esperandoRespuesta)
        {
            Debug.Log("⏳ [PanelPreguntasZylo] Ya hay una respuesta en proceso...");
            return;
        }

        if (string.IsNullOrEmpty(idPinActual))
        {
            Debug.LogError("[PanelPreguntasZylo] No hay ID de pin cargado");
            return;
        }

        Debug.Log($"🎯 [PanelPreguntasZylo] Usuario seleccionó pregunta {numeroPregunta}");

        AudioClip audioResp = numeroPregunta == 1 ? audioRespuesta1Actual : audioRespuesta2Actual;

        if (panelPreguntas != null)
            panelPreguntas.SetActive(false);

        // ✅ Iniciar secuencia completa
        StartCoroutine(SecuenciaRespuestaCompleta(numeroPregunta, audioResp));
    }

    /// <summary>
    /// Corrutina que maneja la animación de pensar, hablar y mostrar respuesta
    /// </summary>
    private IEnumerator SecuenciaRespuestaCompleta(int numeroPregunta, AudioClip audio)
    {
        esperandoRespuesta = true;

        // 1. Animación de pensar
        if (zyloAnimator != null)
            zyloAnimator.SetBool(animacionPensar, true);

        yield return new WaitForSeconds(tiempoPensando);

        if (zyloAnimator != null)
            zyloAnimator.SetBool(animacionPensar, false);

        // 2. Obtener texto de respuesta
        DatosRespuesta datosResp = LectorRespuestas.instance.ObtenerDatosPorID(idPinActual);
        string textoRespuesta = numeroPregunta == 1 ? datosResp?.respuesta1 : datosResp?.respuesta2;

        // 3. Animación de hablar
        if (zyloAnimator != null)
            zyloAnimator.SetBool(animacionHablar, true);

        // 4. Reproducir audio y mostrar subtítulos
        if (zyloAudioSource != null && audio != null)
        {
            zyloAudioSource.clip = audio;
            zyloAudioSource.Play();

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

        // 5. Detener animación de hablar
        if (zyloAnimator != null)
            zyloAnimator.SetBool(animacionHablar, false);

        // 6. Mostrar respuesta en panel
        LectorRespuestas.instance.MostrarRespuesta(idPinActual, numeroPregunta);

        if (panelRespuesta != null)
            panelRespuesta.SetActive(true);

        esperandoRespuesta = false;
    }
    /// <summary>
    /// Detiene la animación de hablar (usado con Invoke)
    /// </summary>
    void DetenerHabla()
    {
        if (zyloAnimator != null)
            zyloAnimator.SetBool(animacionHablar, false);
    }

    /// <summary>
    /// Cierra el panel de respuesta y vuelve a mostrar las preguntas
    /// </summary>
    private void CerrarRespuesta()
    {
        if (panelRespuesta != null)
            panelRespuesta.SetActive(false);

        if (panelPreguntas != null)
            panelPreguntas.SetActive(true);
    }

    /// <summary>
    /// Cierra todos los paneles y resetea el estado (útil cuando Zylo se aleja del pin)
    /// </summary>
    public void CerrarTodo()
    {
        if (panelPreguntas != null)
            panelPreguntas.SetActive(false);

        if (panelRespuesta != null)
            panelRespuesta.SetActive(false);

        StopAllCoroutines();
        esperandoRespuesta = false;
        idPinActual = "";

        // Resetear animaciones
        if (zyloAnimator != null)
        {
            zyloAnimator.SetBool(animacionPensar, false);
            zyloAnimator.SetBool(animacionHablar, false);
        }

        // Detener audio si está sonando
        if (zyloAudioSource != null && zyloAudioSource.isPlaying)
            zyloAudioSource.Stop();

        Debug.Log("🔚 [PanelPreguntasZylo] Paneles cerrados y estado reseteado");
    }
}