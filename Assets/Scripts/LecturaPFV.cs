using Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class LecturaPFV : MonoBehaviour
{
    List<PreguntasFV> listaPreguntasFV;
    public TextMeshProUGUI textoPregunta;
    bool respuestaFV;
    bool respuesta;
    public int indicadorPreguntaFV;
    public GameObject panelRespuestaFVCorrecta, panelRespuestaFVIncorrecta, panelTerminarFV;

    void Start()
    {
        listaPreguntasFV = new List<PreguntasFV>();
        lecturaPreguntasFV();
    }

    public void lecturaPreguntasFV()
    {
        listaPreguntasFV.Clear();
        try
        {
            using (StreamReader sr1 = new StreamReader("Assets/Recursos/TXT_CanvasFinal/FALSO_VERDADERO.txt"))
            {
                string liniaLeida;
                while ((liniaLeida = sr1.ReadLine()) != null)
                {
                    string[] lineapartida = liniaLeida.Split('-');
                    respuesta = lineapartida[1].ToLower() == "true";

                    PreguntasFV preguntaFV = new PreguntasFV(lineapartida[0], respuesta);
                    listaPreguntasFV.Add(preguntaFV);
                }
                Debug.Log("Tamaño lista preguntas Falso/Verdadero " + listaPreguntasFV.Count);
                indicadorPreguntaFV = listaPreguntasFV.Count;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Exception: " + e.Message);
        }
    }

    public void asignarPregunta()
    {
        if (listaPreguntasFV.Count > 0)
        {
            System.Random random = new System.Random();
            int numeroPregunta = random.Next(0, listaPreguntasFV.Count);
            PreguntasFV preguntaFV = listaPreguntasFV[numeroPregunta];
            textoPregunta.text = preguntaFV.PreguntaFV;
            respuestaFV = preguntaFV.Respuesta;
            listaPreguntasFV.RemoveAt(numeroPregunta);
            indicadorPreguntaFV -= 1;
        }
    }

    public void validarRespuestaF()
    {
        if (respuestaFV == false)
        {
            panelRespuestaFVCorrecta.SetActive(true);
            // 🔹 NUEVO: Incrementar contador de respuestas correctas
            if (NewBehaviourScript.instance != null)
                NewBehaviourScript.instance.IncrementarRespuestaCorrecta();
        }
        else
        {
            panelRespuestaFVIncorrecta.SetActive(true);
        }
        StartCoroutine(CambiarAPreguntaSiguiente());
    }

    public void validarRespuestaV()
    {
        if (respuestaFV == true)
        {
            panelRespuestaFVCorrecta.SetActive(true);
            // 🔹 NUEVO: Incrementar contador de respuestas correctas
            if (NewBehaviourScript.instance != null)
                NewBehaviourScript.instance.IncrementarRespuestaCorrecta();
        }
        else
        {
            panelRespuestaFVIncorrecta.SetActive(true);
        }
        StartCoroutine(CambiarAPreguntaSiguiente());
    }

    private IEnumerator CambiarAPreguntaSiguiente()
    {
        // Esperar 1.5 segundos para que el jugador vea si fue correcto o incorrecto
        yield return new WaitForSeconds(1f);

        // Ocultar los paneles de resultado
        panelRespuestaFVCorrecta.SetActive(false);
        panelRespuestaFVIncorrecta.SetActive(false);

        // Pasar a la siguiente pregunta aleatoria
        FindObjectOfType<ControllerAllS>().SiguientePregunta();
    }
}
