using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comun
{
    public class LogFacturaCompra
    {
        private short _tipo;

        public short Tipo
        {
            get { return _tipo; }
            set { _tipo = value; }
        }
        private String _folio;

        public String Folio
        {
            get { return _folio; }
            set { _folio = value; }
        }
        private String _idImpuestoTercero;

        public String IdImpuestoTercero
        {
            get { return _idImpuestoTercero; }
            set { _idImpuestoTercero = value; }
        }
        private String _estado;

        public String Estado
        {
            get { return _estado; }
            set { _estado = value; }
        }

        public LogFacturaCompra(short tipo, String folio, String idProv, String estado)
        {
            _tipo = tipo;
            _folio = folio;
            _idImpuestoTercero = idProv;
            _estado = estado;
        }
    }
}
