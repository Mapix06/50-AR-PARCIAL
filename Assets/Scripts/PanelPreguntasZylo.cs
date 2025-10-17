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

    [Header("Preguntas del pin actual")]
    [SerializeField] private PreguntaRespuesta pregunta1;
    [SerializeField] private PreguntaRespuesta pregunta2;

    private bool esperandoRespuesta = false;

    void Start()
    {
        if (panelPreguntas != null)
            panelPreguntas.SetActive(false);

        if (panelRespuesta != null)
            panelRespuesta.SetActive(false);

        if (botonPregunta1 != null)
            botonPregunta1.onClick.AddListener(() => HacerPregunta(pregunta1));

        if (botonPregunta2 != null)
            botonPregunta2.onClick.AddListener(() => HacerPregunta(pregunta2));

        if (botonCerrarRespuesta != null)
            botonCerrarRespuesta.onClick.AddListener(CerrarRespuesta);
    }

    public void MostrarPanelPreguntas(PreguntaRespuesta p1, PreguntaRespuesta p2)
    {
        if (panelPreguntas == null) return;

        pregunta1 = p1;
        pregunta2 = p2;

        if (textoPregunta1 != null)
            textoPregunta1.text = p1.textoPregunta;

        if (textoPregunta2 != null)
            textoPregunta2.text = p2.textoPregunta;

        panelPreguntas.SetActive(true);
        Debug.Log("[PanelPreguntasZylo] Panel de preguntas mostrado");
    }

    public void OcultarPanelPreguntas()
    {
        if (panelPreguntas != null)
            panelPreguntas.SetActive(false);
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
        if (zyloAnimator != null)
            zyloAnimator.SetBool(animacionPensar, true);

        yield return new WaitForSeconds(tiempoPensando);

        if (zyloAnimator != null)
            zyloAnimator.SetBool(animacionPensar, false);

        // Zylo habla
        if (zyloAnimator != null)
            zyloAnimator.SetBool(animacionHablar, true);

        if (zyloAudioSource != null && pregunta.audioRespuesta != null)
        {
            zyloAudioSource.clip = pregunta.audioRespuesta;
            zyloAudioSource.Play();
            yield return new WaitForSeconds(pregunta.audioRespuesta.length);
        }
        else
        {
            yield return new WaitForSeconds(3f);
        }

        if (zyloAnimator != null)
            zyloAnimator.SetBool(animacionHablar, false);

        MostrarRespuesta(pregunta.textoRespuesta);
        esperandoRespuesta = false;
    }

    private void MostrarRespuesta(string respuesta)
    {
        if (panelRespuesta == null) return;

        if (textoRespuesta != null)
            textoRespuesta.text = respuesta;

        panelRespuesta.SetActive(true);
        Debug.Log("[PanelPreguntasZylo] Respuesta mostrada en pantalla");
    }

    private void CerrarRespuesta()
    {
        if (panelRespuesta != null)
            panelRespuesta.SetActive(false);

        if (pregunta1 != null && pregunta2 != null)
            MostrarPanelPreguntas(pregunta1, pregunta2);
    }

    public void CerrarTodo()
    {
        OcultarPanelPreguntas();

        if (panelRespuesta != null)
            panelRespuesta.SetActive(false);

        StopAllCoroutines();
        esperandoRespuesta = false;

        if (zyloAnimator != null)
        {
            zyloAnimator.SetBool(animacionPensar, false);
            zyloAnimator.SetBool(animacionHablar, false);
        }

        Debug.Log("[PanelPreguntasZylo] Sistema cerrado");
    }
}
