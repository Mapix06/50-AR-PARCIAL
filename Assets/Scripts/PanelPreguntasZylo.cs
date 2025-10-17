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

    private PreguntaRespuesta pregunta1;
    private PreguntaRespuesta pregunta2;
    private bool esperandoRespuesta = false;

    void Start()
    {
        panelPreguntas?.SetActive(true);
        panelRespuesta?.SetActive(true);

        botonPregunta1?.onClick.AddListener(() => HacerPregunta(pregunta1));
        botonPregunta2?.onClick.AddListener(() => HacerPregunta(pregunta2));
        botonCerrarRespuesta?.onClick.AddListener(CerrarRespuesta);
    }

    /// <summary>
    /// Recibe las preguntas desde el pin y las muestra en el panel
    /// </summary>
    public void MostrarPanelPreguntas(PreguntaRespuesta p1, PreguntaRespuesta p2)
    {
        pregunta1 = p1;
        pregunta2 = p2;

        textoPregunta1.text = p1.textoPregunta;
        textoPregunta2.text = p2.textoPregunta;

        panelPreguntas.SetActive(true);

        Debug.Log("[PanelPreguntasZylo] Panel de preguntas mostrado");
    }

    public void OcultarPanelPreguntas()
    {
        panelPreguntas?.SetActive(false);
    }

    private void HacerPregunta(PreguntaRespuesta pregunta)
    {
        if (esperandoRespuesta || pregunta == null) return;

        Debug.Log($"[PanelPreguntasZylo] Pregunta seleccionada: {pregunta.textoPregunta}");
        OcultarPanelPreguntas();
        StartCoroutine(SecuenciaRespuesta(pregunta));
    }

    private IEnumerator SecuenciaRespuesta(PreguntaRespuesta pregunta)
    {
        esperandoRespuesta = true;

        // Zylo piensa
        zyloAnimator?.SetBool(animacionPensar, true);
        yield return new WaitForSeconds(tiempoPensando);
        zyloAnimator?.SetBool(animacionPensar, false);

        // Zylo habla
        zyloAnimator?.SetBool(animacionHablar, true);

        if (zyloAudioSource != null && pregunta.audioRespuesta != null)
        {
            zyloAudioSource.clip = pregunta.audioRespuesta;
            zyloAudioSource.Play();
            yield return new WaitForSeconds(pregunta.audioRespuesta.length);
        }
        else
        {
            yield return new WaitForSeconds(3f); // fallback si no hay audio
        }

        zyloAnimator?.SetBool(animacionHablar, false);

        MostrarRespuesta(pregunta.textoPregunta, pregunta.textoRespuesta);
        esperandoRespuesta = false;
    }

    /// <summary>
    /// Muestra la conversación completa en el panel inferior
    /// </summary>
    private void MostrarRespuesta(string preguntaUsuario, string respuestaZylo)
    {
        textoRespuesta.text = $"<b>Usuario:</b> {preguntaUsuario}\n\n<b>Zylo:</b> {respuestaZylo}";
        panelRespuesta.SetActive(true);
        Debug.Log("[PanelPreguntasZylo] Respuesta mostrada en pantalla");
    }

    private void CerrarRespuesta()
    {
        panelRespuesta?.SetActive(false);
        MostrarPanelPreguntas(pregunta1, pregunta2);
    }

    public void CerrarTodo()
    {
        StopAllCoroutines();
        esperandoRespuesta = false;

        panelPreguntas?.SetActive(false);
        panelRespuesta?.SetActive(false);

        zyloAnimator?.SetBool(animacionPensar, false);
        zyloAnimator?.SetBool(animacionHablar, false);

        Debug.Log("[PanelPreguntasZylo] Sistema cerrado");
    }
}
