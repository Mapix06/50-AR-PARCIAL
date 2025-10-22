using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
    [Header("Referencias de tiempo")]
    public TextMeshProUGUI min;
    public TextMeshProUGUI seg;
    public TextMeshProUGUI milSeg;

    [Header("Panel de resultado")]
    public GameObject panelFinal;
    public TextMeshProUGUI mensajeFinal;
    public Button botonReintentar;

    [Header("Panel del reloj")]
    public GameObject panelReloj;

    [Header("Panel principal")]
    public GameObject panelPrincipal;

    [Header("Panel principal de preguntas")]
    public GameObject panelPreguntas;

    [Header("Configuración")]
    public float tiempoLimite = 15f;
    public int respuestasCorrectas = 0;
    public int totalPreguntas = 8;


    private float startTime;
    private float timerTime;
    private bool isRunning = false;
    private bool quizTerminado = false;

    // 🔹 Singleton para acceder fácilmente desde otros scripts
    public static NewBehaviourScript instance;

    void Awake()
    {
        if (instance == null)
            instance = this;

    }

    void Start()
    {

        if (panelFinal != null)
            panelFinal.SetActive(false);

        if (panelPreguntas != null)
            panelPreguntas.SetActive(true);

        startTime = Time.time;
        isRunning = true;
        quizTerminado = false;

        if (botonReintentar != null)
            botonReintentar.onClick.AddListener(ReiniciarJuego);
    }

    void Update()
    {
        if (!isRunning || quizTerminado) return;

        timerTime = Time.time - startTime;

        int minutesInt = (int)timerTime / 60;
        int secondsInt = (int)timerTime % 60;
        int milSecondsInt = (int)((timerTime - (secondsInt + minutesInt * 60)) * 100);

        if (min != null) min.text = minutesInt.ToString("00");
        if (seg != null) seg.text = secondsInt.ToString("00");
        if (milSeg != null) milSeg.text = milSecondsInt.ToString("00");

        if (timerTime >= tiempoLimite)
        {
            isRunning = false;
            quizTerminado = true;
            MostrarResultado("¡Se acabó el tiempo!\nVuelve a intentarlo.");
        }
    }

    public void VerificarFinalizacion()
    {
        if (quizTerminado) return;

        if (respuestasCorrectas + ObtenerIncorrectas() >= totalPreguntas)
        {
            quizTerminado = true;
            isRunning = false;
            EvaluarDesempeño();
        }
    }

    private void EvaluarDesempeño()
    {
        float tiempoFinal = timerTime;

        if (respuestasCorrectas == totalPreguntas && tiempoFinal <= tiempoLimite / 2)
        {
            MostrarResultado("🏆 ¡Excelente! Respondió todo correctamente y muy rápido.");
        }
        else if (respuestasCorrectas == totalPreguntas)
        {
            MostrarResultado("🎉 ¡Buen trabajo! Todas las respuestas fueron correctas.");
        }
        else if (respuestasCorrectas > 0)
        {
            MostrarResultado("😅 Algunas respuestas fueron incorrectas.\nSigue practicando.");
        }
        else
        {
            MostrarResultado("😢 No hubo respuestas correctas.\nVuelve a intentarlo.");
        }
    }

    private void MostrarResultado(string mensaje)
    {
        if (panelFinal != null)
        {
            panelFinal.SetActive(true);
            if (mensajeFinal != null)
                mensajeFinal.text = mensaje;
        }

        if (panelPreguntas != null)
            panelPreguntas.SetActive(false);
    }

    public void ReiniciarJuego()
    {
        Debug.Log("=== INICIANDO REINICIO DEL JUEGO ===");

        // Reiniciar reloj
        startTime = Time.time;
        timerTime = 0;
        isRunning = true;
        quizTerminado = false;

        // Reiniciar contadores
        respuestasCorrectas = 0;

        if (min != null) min.text = "00";
        if (seg != null) seg.text = "00";
        if (milSeg != null) milSeg.text = "00";

        // 🔹 PASO 1: Ocultar panel final
        if (panelFinal != null)
            panelFinal.SetActive(false);

        // 🔹 PASO 2: Activar panel de preguntas ANTES de reiniciar
        if (panelPreguntas != null)
        {
            panelPreguntas.SetActive(true);
            Debug.Log(" Panel de preguntas activado");
        }

        // 🔹 PASO 3: Reiniciar el sistema de preguntas (esto mostrará la primera pregunta)
        ControllerAllS controller = FindObjectOfType<ControllerAllS>();
        if (controller != null)
        {
            controller.ReiniciarPreguntas();
        }
    }

    private int ObtenerIncorrectas()
    {
        return Mathf.Max(0, totalPreguntas - respuestasCorrectas);
    }
    public void IncrementarRespuestaCorrecta()
    {
        respuestasCorrectas++;
        Debug.Log($" Respuesta correcta! Total: {respuestasCorrectas}/{totalPreguntas}");
    }
    public void DetenerTiempo()
    {
        if (!isRunning) return;

        isRunning = false;
        quizTerminado = true;

        Debug.Log(" Tiempo detenido: el usuario terminó todas las preguntas.");

        if (panelPreguntas != null)
            panelPreguntas.SetActive(false);
    }

    public void DetenerYEvaluar()
    {
        DetenerTiempo();

        if (panelFinal != null)
            panelFinal.SetActive(true);

        EvaluarDesempeño();
    }

    public void SalirDelQuiz()
    {
        quizTerminado = true;
        isRunning = false;

        if (panelPreguntas != null)
            panelPreguntas.SetActive(false);

        if (panelFinal != null)
            panelFinal.SetActive(false);

        if (panelReloj != null)
            panelReloj.SetActive(false);

        if (panelPrincipal != null)
            panelPrincipal.SetActive(false);

        Debug.Log(" El usuario salió del quiz manualmente.");
    }


}