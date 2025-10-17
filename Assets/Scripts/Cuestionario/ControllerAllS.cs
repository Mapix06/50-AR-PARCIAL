using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerAllS : MonoBehaviour
{
    [SerializeField] private List<GameObject> listaControllers;
    private List<GameObject> listaControllersOriginal;
    private GameObject controlSelected;
    public GameObject panelMultiples;
    public GameObject panelFV;
    public GameObject panelFinal;
    private bool panelMostrado = false;
    private bool preguntasCargadas = false;

    [Header("Control de preguntas")]
    public int totalPreguntasAMostrar = 8; // 🔹 Total de preguntas del quiz
    private int preguntasMostradas = 0; // 🔹 Contador de preguntas mostradas

    [Header("Panel de activación")]
    [SerializeField] private GameObject panelPrincipal;

    void Start()
    {
        listaControllersOriginal = new List<GameObject>(listaControllers);
        preguntasMostradas = 0;

        InicializarPaneles();
        StartCoroutine(EsperarActivacionPanelPrincipal());
    }

    private void InicializarPaneles()
    {
        panelMultiples.SetActive(false);
        panelFV.SetActive(false);
        panelFinal.SetActive(false);
    }

    // Esperar a que las preguntas se carguen antes de iniciar
    private IEnumerator EsperarCargaDePreguntas()
    {
        // Esperar un frame adicional para asegurar que Start() se ejecutó en todos los scripts
        yield return new WaitForSeconds(0.5f);

        // Verificar que las listas de preguntas están cargadas
        bool todasCargadas = true;
        foreach (GameObject controller in listaControllers)
        {
            var controlMulti = controller.GetComponent<LecturaPMultiples>();
            var controlFV = controller.GetComponent<LecturaPFV>();

            if (controlMulti != null && controlMulti.indicadorPreguntaM == 0)
            {
                todasCargadas = false;
                break;
            }
            if (controlFV != null && controlFV.indicadorPreguntaFV == 0)
            {
                todasCargadas = false;
                break;
            }
        }

        if (todasCargadas)
        {
            preguntasCargadas = true;
            SelectQuestionFaciles();
        }
        else
        {
            Debug.LogError("Error: Las preguntas no se cargaron correctamente");
        }
    }

    public void SelectQuestionFaciles()
    {
        if (!preguntasCargadas) return;

        // 🔹 Verificar si ya se mostraron las 8 preguntas
        if (preguntasMostradas >= totalPreguntasAMostrar)
        {
            TerminarQuiz();
            return;
        }

        // Verificar si hay preguntas disponibles en algún controlador
        bool hayPreguntasDisponibles = false;
        foreach (GameObject controller in listaControllers)
        {
            var controlMulti = controller.GetComponent<LecturaPMultiples>();
            var controlFV = controller.GetComponent<LecturaPFV>();

            if ((controlMulti != null && controlMulti.indicadorPreguntaM > 0) ||
                (controlFV != null && controlFV.indicadorPreguntaFV > 0))
            {
                hayPreguntasDisponibles = true;
                break;
            }
        }

        if (!hayPreguntasDisponibles)
        {
            TerminarQuiz();
            return;
        }

        if (listaControllers.Count > 0)
        {
            System.Random random = new System.Random();
            int numero = random.Next(0, listaControllers.Count);
            controlSelected = listaControllers[numero];

            if (controlSelected.GetComponent<LecturaPMultiples>() != null)
            {
                var controlMulti = controlSelected.GetComponent<LecturaPMultiples>();
                if (controlMulti.indicadorPreguntaM > 0)
                {
                    Debug.Log($"📝 Mostrando pregunta múltiple #{preguntasMostradas + 1}");
                    panelFV.SetActive(false);
                    panelMultiples.SetActive(true);
                    Debug.Log($"   Panel Múltiples activado: {panelMultiples.activeSelf}");
                    controlMulti.asignarPregunta();
                    preguntasMostradas++; // 🔹 Incrementar contador
                    Debug.Log($"✅ Pregunta {preguntasMostradas}/{totalPreguntasAMostrar} mostrada (Múltiple)");
                }
                else
                {
                    listaControllers.Remove(controlSelected);
                    SelectQuestionFaciles();
                }
            }
            else if (controlSelected.GetComponent<LecturaPFV>() != null)
            {
                var controlFV = controlSelected.GetComponent<LecturaPFV>();
                if (controlFV.indicadorPreguntaFV > 0)
                {
                    Debug.Log($"📝 Mostrando pregunta F/V #{preguntasMostradas + 1}");
                    panelMultiples.SetActive(false);
                    panelFV.SetActive(true);
                    Debug.Log($"   Panel F/V activado: {panelFV.activeSelf}");
                    controlFV.asignarPregunta();
                    preguntasMostradas++; // 🔹 Incrementar contador
                    Debug.Log($"✅ Pregunta {preguntasMostradas}/{totalPreguntasAMostrar} mostrada (Falso/Verdadero)");
                }
                else
                {
                    listaControllers.Remove(controlSelected);
                    SelectQuestionFaciles();
                }
            }
        }
    }

    private void TerminarQuiz()
    {
        panelMultiples.SetActive(false);
        panelFV.SetActive(false);

        if (!panelMostrado)
        {
            NewBehaviourScript tiempo = FindObjectOfType<NewBehaviourScript>();
            if (tiempo != null)
            {
                tiempo.DetenerYEvaluar();
            }
            panelMostrado = true;
        }
    }

    public void SiguientePregunta()
    {
        SelectQuestionFaciles();
    }

    public void ReiniciarPreguntas()
    {
        // 🔹 PRIMERO: Detener TODAS las coroutines en este objeto también
        StopAllCoroutines();

        // 🔹 Detener el proceso de preguntas mientras se reinicia
        preguntasCargadas = false;

        // Reiniciar paneles
        InicializarPaneles();
        panelMostrado = false;
        preguntasMostradas = 0; // 🔹 Reiniciar contador

        // 🔹 IMPORTANTE: Ocultar paneles de retroalimentación
        OcultarPanelesRetroalimentacion();

        // 🔹 Restaurar lista de controladores
        listaControllers = new List<GameObject>(listaControllersOriginal);

        Debug.Log("=== INICIANDO REINICIO ===");

        // Recargar las listas de preguntas
        foreach (GameObject controller in listaControllers)
        {
            var controlMulti = controller.GetComponent<LecturaPMultiples>();
            var controlFV = controller.GetComponent<LecturaPFV>();

            if (controlMulti != null)
            {
                controlMulti.lecturaPreguntasM();
                Debug.Log($"Preguntas múltiples cargadas: {controlMulti.indicadorPreguntaM}");
            }
            if (controlFV != null)
            {
                controlFV.lecturaPreguntasFV();
                Debug.Log($"Preguntas F/V cargadas: {controlFV.indicadorPreguntaFV}");
            }
        }

        // Reiniciar el juego después de recargar
        StartCoroutine(ReiniciarDespuesDeCarga());
    }

    private void OcultarPanelesRetroalimentacion()
    {
        // Buscar y ocultar todos los paneles de retroalimentación
        foreach (GameObject controller in listaControllersOriginal)
        {
            var controlMulti = controller.GetComponent<LecturaPMultiples>();
            var controlFV = controller.GetComponent<LecturaPFV>();

            if (controlMulti != null)
            {
                controlMulti.StopAllCoroutines(); // 🔹 Detener coroutines activas
                if (controlMulti.panelRespuestaMultipleCorrecta != null)
                    controlMulti.panelRespuestaMultipleCorrecta.SetActive(false);
                if (controlMulti.panelRespuestaMultipleIncorrecta != null)
                    controlMulti.panelRespuestaMultipleIncorrecta.SetActive(false);
            }

            if (controlFV != null)
            {
                controlFV.StopAllCoroutines(); // 🔹 Detener coroutines activas
                if (controlFV.panelRespuestaFVCorrecta != null)
                    controlFV.panelRespuestaFVCorrecta.SetActive(false);
                if (controlFV.panelRespuestaFVIncorrecta != null)
                    controlFV.panelRespuestaFVIncorrecta.SetActive(false);
            }
        }
    }

    private IEnumerator ReiniciarDespuesDeCarga()
    {
        // 🔹 NO activar preguntasCargadas aún
        yield return new WaitForSeconds(0.3f);

        // 🔹 Asegurarse de que los paneles principales estén en el estado correcto
        panelMultiples.SetActive(false);
        panelFV.SetActive(false);
        panelFinal.SetActive(false);

        preguntasCargadas = true;

        Debug.Log("=== REINICIO COMPLETO ===");
        Debug.Log($"Preguntas mostradas antes de iniciar: {preguntasMostradas}/{totalPreguntasAMostrar}");

        // 🔹 Esperar un frame adicional para que Unity actualice la UI
        yield return null;

        SelectQuestionFaciles();
    }

    private IEnumerator EsperarActivacionPanelPrincipal()
    {
        // Espera hasta que el panelPrincipal esté activo
        while (panelPrincipal != null && !panelPrincipal.activeInHierarchy)
        {
            yield return null; // espera un frame
        }

        // Cuando el panelPrincipal esté activo, continúa con la carga
        StartCoroutine(EsperarCargaDePreguntas());
    }
}