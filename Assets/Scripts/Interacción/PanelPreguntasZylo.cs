using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PanelPreguntasZylo : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject panelPreguntas;
    [SerializeField] private Button botonPregunta1;
    [SerializeField] private Button botonPregunta2;
    [SerializeField] private TextMeshProUGUI textoPregunta1;
    [SerializeField] private TextMeshProUGUI textoPregunta2;

    [Header("Panel de respuesta")]
    [SerializeField] private GameObject panelRespuesta;
    [SerializeField] private TextMeshProUGUI textoRespuesta;
    [SerializeField] private Button botonCerrarRespuesta;

    [Header("Referencia a Zylo")]
    [SerializeField] private Animator zyloAnimator;
    [SerializeField] private AudioSource zyloAudioSource;

    [Header("Animaciones")]
    [SerializeField] private string animacionPensar = "isThinking";
    [SerializeField] private string animacionHablar = "isTalking";
    [SerializeField] private float tiempoPensando = 2f;

    private DatosPregunta pregunta1Datos;
    private DatosPregunta pregunta2Datos;
    private DatosRespuesta respuestaDatos;

    private bool esperandoRespuesta = false;

    void Start()
    {
        panelPreguntas?.SetActive(false);
        panelRespuesta?.SetActive(false);

        botonPregunta1.onClick.AddListener(() => HacerPregunta(1));
        botonPregunta2.onClick.AddListener(() => HacerPregunta(2));
        botonCerrarRespuesta.onClick.AddListener(CerrarRespuesta);
    }

    /// <summary>
    /// Mostrar panel de preguntas de un pin
    /// </summary>
    public void MostrarPanelPreguntas(PinMapa pin)
    {
        if (pin == null) return;

        // Obtener datos de preguntas y respuestas por ID
        pregunta1Datos = LectorPreguntas.instance.ObtenerDatosPorID(pin.idPin);
        respuestaDatos = LectorRespuestas.instance.ObtenerDatosPorID(pin.idPin);

        if (pregunta1Datos == null || respuestaDatos == null)
        {
            Debug.LogWarning($"[PanelPreguntasZylo] No hay datos para el pin {pin.idPin}");
            return;
        }

        // Actualizar textos de botones
        textoPregunta1.text = pregunta1Datos.pregunta1;
        textoPregunta2.text = pregunta1Datos.pregunta2;

        panelPreguntas.SetActive(true);
    }

    private void HacerPregunta(int numero)
    {
        if (esperandoRespuesta) return;

        string textoResp = numero == 1 ? respuestaDatos.respuesta1 : respuestaDatos.respuesta2;
        AudioClip audioResp = numero == 1 ? pregunta1Datos.audioPregunta1 : pregunta1Datos.audioPregunta2;

        panelPreguntas.SetActive(false);
        StartCoroutine(SecuenciaRespuesta(textoResp, audioResp));
    }

    private IEnumerator SecuenciaRespuesta(string texto, AudioClip audio)
    {
        esperandoRespuesta = true;

        // 1️⃣ Pensando
        zyloAnimator?.SetBool(animacionPensar, true);
        yield return new WaitForSeconds(tiempoPensando);
        zyloAnimator?.SetBool(animacionPensar, false);

        // 2️⃣ Hablando
        zyloAnimator?.SetBool(animacionHablar, true);

        if (zyloAudioSource != null && audio != null)
        {
            zyloAudioSource.clip = audio;
            zyloAudioSource.Play();

            if (SubtitulosZylo.Instance != null)
                SubtitulosZylo.Instance.MostrarSubtitulosConAudio(texto, audio);

            yield return new WaitForSeconds(audio.length);
        }
        else
        {
            if (SubtitulosZylo.Instance != null)
                SubtitulosZylo.Instance.MostrarTexto(texto);

            yield return new WaitForSeconds(3f);
        }

        zyloAnimator?.SetBool(animacionHablar, false);

        // 3️⃣ Mostrar panel de respuesta con texto
        textoRespuesta.text = texto;
        panelRespuesta.SetActive(true);

        esperandoRespuesta = false;
    }

    private void CerrarRespuesta()
    {
        panelRespuesta.SetActive(false);
        panelPreguntas.SetActive(true);
    }

    public void CerrarTodo()
    {
        panelPreguntas.SetActive(false);
        panelRespuesta.SetActive(false);
        StopAllCoroutines();
        esperandoRespuesta = false;
        if (zyloAnimator != null)
        {
            zyloAnimator.SetBool(animacionPensar, false);
            zyloAnimator.SetBool(animacionHablar, false);
        }
    }
}
