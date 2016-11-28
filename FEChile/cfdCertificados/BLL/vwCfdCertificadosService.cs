using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyGeneration.dOOdads;

namespace cfd.FacturaElectronica
{
    public class vwCfdCertificadosService
    {
        private string _connStr;
        private string _sMsj;
        private string _rutaCertificado;
        private string _clave;
        private string _idImpuesto;
        private byte _solicita;
        private byte _envia;
        private byte _firma;
        private byte _anula;
        private DateTime _Fecha_vig_hasta;
        private DateTime _Fecha_vig_desde;
        private string _fchResol;
        private string _nroResol;

        public vwCfdCertificadosService(string connStr)
        {
            _connStr = connStr;
        }
        //****************************************************
        #region Propiedades
        public string sMsj
        {
            get { return _sMsj; }
        }
        public string Ruta_certificado
        {
            get { return _rutaCertificado; }
        }
        public string clave
        {
            get { return _clave; }
        }
        public string idImpuesto
        {
            get { return _idImpuesto; }
        }
        public byte solicita
        {
            get { return _solicita; }
        }
        public byte envia
        {
            get { return _envia; }
        }
        public byte firma
        {
            get { return _firma; }
        }
        public byte anula
        {
            get { return _anula; }
        }
        public DateTime Fecha_vig_desde
        {
            get { return _Fecha_vig_desde; }
        }
        public DateTime Fecha_vig_hasta
        {
            get { return _Fecha_vig_hasta; }
        }
        public string fchResol
        {
            get { return _fchResol; }
        }
        public string nroResol
        {
            get { return _nroResol; }
        }
        #endregion
        //****************************************************
        #region Métodos
       
        public bool get(string idUsuario)
        {
            vwCfdCertificados certs = new vwCfdCertificados(_connStr);
            _sMsj = string.Empty;
            certs.Where.USERID.Value = idUsuario.ToLower().Trim();
            certs.Where.USERID.Operator = WhereParameter.Operand.Equal;

            try
            {
                if (certs.Query.Load())
                {
                    _rutaCertificado = certs.Ruta_certificado.Trim();
                    _clave = certs.Contrasenia_clave.Trim();
                    _idImpuesto = certs.ACA_RUT.Trim();
                    _solicita = certs.ACA_SolicitaFolio;
                    _envia = certs.ACA_EnviaDocumentos;
                    _firma = certs.ACA_FirmaDocumentos;
                    _anula = certs.ACA_AnulaDocumentos;
                    _Fecha_vig_desde = certs.Fecha_vig_desde;
                    _Fecha_vig_hasta = certs.Fecha_vig_hasta;
                    _fchResol = certs.FchResol;
                    _nroResol = certs.NroResol;
                }
                else
                    return false;
            }
            catch (Exception ePla)
            {
                _sMsj = "Excepción al leer la ruta del certificado del usuario. " + ePla.Message + " [vwCfdCertificadosService.get()]";
                return false;
            }
            return true;
        }
        #endregion

    }
}
