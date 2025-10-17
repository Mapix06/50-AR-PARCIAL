using Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class LecturaPMultiples : MonoBehaviour
{
    List<PreguntasMultiples> listaPreguntasM;
    public TextMeshProUGUI textoPregunta;
    public TextMeshProUGUI textoOp1;
    public TextMeshProUGUI textoOp2;
    public TextMeshProUGUI textoOp3;
    public TextMeshProUGUI textoOp4;
    public int indicadorPreguntaM;
    string respuesta;
    public GameObject panelRespuestaMultipleCorrecta, panelRespuestaMultipleIncorrecta, panelTerminalM;

    void Start()
    {
        listaPreguntasM = new List<PreguntasMultiples>();
        lecturaPreguntasM();
    }

    public void lecturaPreguntasM()
    {
        listaPreguntasM.Clear();
        try
        {
            using (StreamReader sr1 = new StreamReader("Assets/Recursos/TXT_CanvasFinal/SELECCION_MULTIPLE.txt"))
            {
                string liniaLeida;
                while ((liniaLeida = sr1.ReadLine()) != null)
                {
                    string[] lineapartida = liniaLeida.Split('-');

                    PreguntasMultiples pregunta = new PreguntasMultiples(
                        lineapartida[0],
                        lineapartida[1],
                        lineapartida[2],
                        lineapartida[3],
                        lineapartida[4],
                        lineapartida[5]
                    );
                    listaPreguntasM.Add(pregunta);
                }
                Debug.Log("Tamaño lista preguntas Multiples " + listaPreguntasM.Count);
                indicadorPreguntaM = listaPreguntasM.Count;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Exception: " + e.Message);
        }
    }

    public void asignarPregunta()
    {
        if (listaPreguntasM.Count > 0)
        {
            System.Random random = new System.Random();
            int numeroPregunta = random.Next(0, listaPreguntasM.Count);
            PreguntasMultiples preguntaMul = listaPreguntasM[numeroPregunta];
            textoPregunta.text = preguntaMul.PreguntaMultiple;

            textoOp1.text = preguntaMul.Opcion1;
            textoOp2.text = preguntaMul.Opcion2;
            textoOp3.text = preguntaMul.Opcion3;
            textoOp4.text = preguntaMul.Opcion4;

            respuesta = preguntaMul.Respeusta;
            listaPreguntasM.RemoveAt(numeroPregunta);
            indicadorPreguntaM -= 1;
        }
    }

    public void validarRespuesta1()
    {
        if (textoOp1.text.Equals(respuesta))
        {
            panelRespuestaMultipleCorrecta.SetActive(true);
            // 🔹 NUEVO: Incrementar contador de respuestas correctas
            if (NewBehaviourScript.instance != null)
                NewBehaviourScript.instance.IncrementarRespuestaCorrecta();
        }
        else
            panelRespuestaMultipleIncorrecta.SetActive(true);

        StartCoroutine(CambiarAPreguntaSiguiente());
    }

    public void validarRespuesta2()
    {
        if (textoOp2.text.Equals(respuesta))
        {
            panelRespuestaMultipleCorrecta.SetActive(true);
            // 🔹 NUEVO: Incrementar contador de respuestas correctas
            if (NewBehaviourScript.instance != null)
                NewBehaviourScript.instance.IncrementarRespuestaCorrecta();
        }
        else
            panelRespuestaMultipleIncorrecta.SetActive(true);

        StartCoroutine(CambiarAPreguntaSiguiente());
    }

    public void validarRespuesta3()
    {
        if (textoOp3.text.Equals(respuesta))
        {
            panelRespuestaMultipleCorrecta.SetActive(true);
            // 🔹 NUEVO: Incrementar contador de respuestas correctas
            if (NewBehaviourScript.instance != null)
                NewBehaviourScript.instance.IncrementarRespuestaCorrecta();
        }
        else
            panelRespuestaMultipleIncorrecta.SetActive(true);

        StartCoroutine(CambiarAPreguntaSiguiente());
    }

    public void validarRespuesta4()
    {
        if (textoOp4.text.Equals(respuesta))
        {
            panelRespuestaMultipleCorrecta.SetActive(true);
            // 🔹 NUEVO: Incrementar contador de respuestas correctas
            if (NewBehaviourScript.instance != null)
                NewBehaviourScript.instance.IncrementarRespuestaCorrecta();
        }
        else
            panelRespuestaMultipleIncorrecta.SetActive(true);

        StartCoroutine(CambiarAPreguntaSiguiente());
    }

    private IEnumerator CambiarAPreguntaSiguiente()
    {
        // Esperar 1.5 segundos para mostrar el resultado
        yield return new WaitForSeconds(1f);

        // Ocultar paneles de retroalimentación
        panelRespuestaMultipleCorrecta.SetActive(false);
        panelRespuestaMultipleIncorrecta.SetActive(false);

        // Pasar a la siguiente pregunta aleatoria
        FindObjectOfType<ControllerAllS>().SiguientePregunta();
    }
}
