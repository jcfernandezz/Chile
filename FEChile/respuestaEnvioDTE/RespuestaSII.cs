using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace respuestaEnvioDTE
{
    public class RespuestaSII
    {
        private String _respHdrEstado;

        public String RespHdrEstado
        {
            get { return _respHdrEstado; }
            set { _respHdrEstado = value; }
        }
        private String _respBodyRecibido;

        public String RespBodyRecibido
        {
            get { return _respBodyRecibido; }
            set { _respBodyRecibido = value; }
        }
        private String _respBodyEstado;

        public String RespBodyEstado
        {
            get { return _respBodyEstado; }
            set { _respBodyEstado = value; }
        }
        private String _respBodyGlosa;

        public String RespBodyGlosa
        {
            get { return _respBodyGlosa; }
            set { _respBodyGlosa = value; }
        }

        public RespuestaSII()
        {

        }

        public String VerificaEstadoDTE()
        {
            _respBodyRecibido = "SI";
            _respHdrEstado = "0";
            _respBodyEstado = "DOK";
            _respBodyGlosa = "Documento Recibido por el SII. Datos Coinciden con los Registrados";
            return "0";
        }

    }
}
