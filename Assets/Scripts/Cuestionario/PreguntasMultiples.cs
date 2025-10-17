using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Models
{

    [Serializable]
    public class PreguntasMultiples
    {
        private string preguntaMultiple;
        private string opcion1;
        private string opcion2;
        private string opcion3;
        private string opcion4;
        private string respeusta;

        public PreguntasMultiples()
        {
        }

        public PreguntasMultiples(string preguntaMultiple, string opcion1, string opcion2, string opcion3, string opcion4, string respeusta)
        {
            this.preguntaMultiple = preguntaMultiple;
            this.opcion1 = opcion1;
            this.opcion2 = opcion2;
            this.opcion3 = opcion3;
            this.opcion4 = opcion4;
            this.respeusta = respeusta;
        }

        public string PreguntaMultiple { get => preguntaMultiple; set => preguntaMultiple = value; }
        public string Opcion1 { get => opcion1; set => opcion1 = value; }
        public string Opcion2 { get => opcion2; set => opcion2 = value; }
        public string Opcion3 { get => opcion3; set => opcion3 = value; }
        public string Opcion4 { get => opcion4; set => opcion4 = value; }
        public string Respeusta { get => respeusta; set => respeusta = value; }
    }
}
