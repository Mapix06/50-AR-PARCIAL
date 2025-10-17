using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    [Serializable]
    public class PreguntasFV
    {
        private string preguntaFV;
        private bool respuesta;

        public PreguntasFV()
        {
        }

        public PreguntasFV(string preguntaFV, bool respuesta)
        {
            this.preguntaFV = preguntaFV;
            this.respuesta = respuesta;
        }

        public string PreguntaFV { get => preguntaFV; set => preguntaFV = value; }
        public bool Respuesta { get => respuesta; set => respuesta = value; }
    }
}
