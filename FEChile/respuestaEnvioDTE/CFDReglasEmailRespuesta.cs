using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EMailManejador;
using CLConfiguracion;

namespace respuestaEnvioDTE
{
    public struct DireccionesEmail
    {
        private string _mailTo;
        private string _mailCC;
        private string _mailCCO;

        public DireccionesEmail(string mailTo, string mailCC, string mailCCO)
        {
            this._mailTo = mailTo;
            this._mailCC = mailCC;
            this._mailCCO = mailCCO;
        }

        public string mailTo { get { return _mailTo; } set { _mailTo = value; } }
        public string mailCC { get { return _mailCC; } set { _mailCC = value; } }
        public string mailCCO { get { return _mailCCO; } set { _mailCCO = value; } }
    }

    class CFDReglasEmailRespuesta
    {
        public string sMsj = "";
        public int iErr = 0;
        private IConexionAFuenteDatos _Conexion;
        private IParametros _Param;
        private EmailSmtp _motorDeCorreo;
        private string _asunto;
        //**********************************************
        #region Propiedades
        public string Asunto
        {
            get { return _asunto; }
            set { _asunto = value; }
        }
        private string _cuerpo;

        public string Cuerpo
        {
            get { return _cuerpo; }
            set { _cuerpo = value; }
        }
        private String _emailTo = String.Empty;

        public String EmailTo
        {
            get { return _emailTo; }
            set { _emailTo = value; }
        }
        private String _emailCC = String.Empty;

        public String EmailCC
        {
            get { return _emailCC; }
            set { _emailCC = value; }
        }
        private String _emailCCO = String.Empty;

        public String EmailCCO
        {
            get { return _emailCCO; }
            set { _emailCCO = value; }
        }
        
        #endregion
        //**********************************************

        public CFDReglasEmailRespuesta(IConexionAFuenteDatos conex, IParametros Param)
        {
            try
            {
                iErr = 0;
                _Conexion = conex;
                _Param = Param;
                _motorDeCorreo = new EmailSmtp(_Param.emailSmtp, _Param.emailPort, _Param.emailUser, _Param.emailPwd, _Param.emailSsl);
            }
            catch (Exception re)
            {
                iErr++;
                sMsj = "Excepción al iniciar el motor de correos. " + re.Message + "[cfdReglasEmail()]";
            }
        }

        //public bool ObtieneSeccionesEmail()
        //{
        //    iErr = 0;
        //    vwCfdCartasReclamacionDeuda carta = new vwCfdCartasReclamacionDeuda(_Conexion.ConnStr);
        //    carta.Where.Letter_type.Value = 3;
        //    carta.Where.Letter_type.Operator = WhereParameter.Operand.Equal;
        //    carta.Where.Ltrrptnm.Conjuction = WhereParameter.Conj.And;
        //    carta.Where.Ltrrptnm.Value = _Param.emailCarta;
        //    carta.Where.Ltrrptnm.Operator = WhereParameter.Operand.Equal;
        //    sMsj = "";
        //    try
        //    {
        //        if (!carta.Query.Load())
        //        {
        //            sMsj = "No está configurada la plantilla de la carta " + _Param.emailCarta + ". Ingrese esta plantilla en GP Tarjetas > ventas > Collection Letters.";
        //            iErr++;
        //            return false;
        //        }
        //        else
        //        {
        //            _asunto = carta.CN_Email_Subject;
        //            _cuerpo = carta.CN_Letter_Text;
        //        }
        //        return true;
        //    }
        //    catch (Exception eSe)
        //    {
        //        sMsj = "Contacte al administrador. No se pudo consultar la base de datos. [SeccionesEmail] " + eSe.Message;
        //        iErr++;
        //        return false;
        //    }

        //}

        //public DireccionesEmail ObtieneDirecciones(string custnmbr)
        //{
        //    iErr = 0;
        //    sMsj = string.Empty;
        //    vwCfdClienteDireccionesCorreo dirCorreo = new vwCfdClienteDireccionesCorreo(_Conexion.ConnStr);     //direcciones de correo de los clientes
        //    dirCorreo.Where.CUSTNMBR.Value = custnmbr;
        //    dirCorreo.Where.CUSTNMBR.Operator = WhereParameter.Operand.Equal;
        //    DireccionesEmail dir = new DireccionesEmail("", "", "");
        //    try
        //    {
        //        if (!dirCorreo.Query.Load())
        //        {
        //            sMsj = "El cliente no tiene direcciones de correo registradas.";
        //            iErr++;
        //            return dir;
        //        }
        //        else
        //        {
        //            dir.mailTo = dirCorreo.EmailTo;
        //            dir.mailCC = dirCorreo.EmailCC;
        //            dir.mailCCO = dirCorreo.EmailCCO;
        //        }
        //        return dir;
        //    }
        //    catch (Exception edc)
        //    {
        //        sMsj = "Contacte al administrador. No se pudo consultar la base de datos. [ObtieneDireccionesDeCorreo] " + edc.Message;
        //        iErr++;
        //        return dir;
        //    }

        //}

        public bool ProcesaMensaje(string idFactura, string rutaYNombreArchivo)
        {
            iErr = 0;
            sMsj = string.Empty;
            List<string> Adjunto = new List<string>();
            //dir = new DireccionesEmail(_emailTo, _emailCC, _emailCCO);

            if (iErr==0)
            {
                if (_Param.emite)
                    Adjunto.Add(rutaYNombreArchivo.Replace(".xml", "." + _Param.emailAdjEmite));    //xml o zip

                if (_Param.imprime)
                    Adjunto.Add(rutaYNombreArchivo.Replace(".xml", "." + _Param.emailAdjImprm));    //pdf

                if (!_motorDeCorreo.SendMessage(_emailTo, _Param.emailAccount,
                                    _asunto.Trim() + " (" + idFactura + ")", _cuerpo,
                                    _emailCC, _emailCCO, _Param.replyto, Adjunto))
                {
                    iErr++;
                    sMsj = _motorDeCorreo.ultimoMensaje;
                }
            }

            return iErr==0;
        }

        private string Derecha(string Texto, int Cuantos)
        {
            if (Texto.Length > Cuantos && Cuantos > 0)
                return Texto.Remove(0, Texto.Length - Cuantos);
            else
                return Texto;
        }

    }
}
