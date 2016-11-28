using System;
using System.Collections.Generic;
using System.Text;

namespace Comun
{
    public class LibroCV
    {
        private int _periodo;
        private String _tipo;
        private String _estado;

        public LibroCV(int periodo, String tipo, String estado)
        {
            this._periodo = periodo;
            this._tipo = tipo;
            this._estado = estado;
        }

        public int periodo
        {
            get { return _periodo; }
            set { _periodo = value; }
        }
        public String tipo
        {
            get { return _tipo; }
            set { _tipo = value; }
        }
        public String estado
        {
            get { return _estado; }
            set { _estado = value; }
        }
    }
}
