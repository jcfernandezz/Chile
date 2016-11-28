using System;
using System.Collections.Generic;
using System.Text;
using MyGeneration.dOOdads;

using Comun;
using MaquinaDeEstados;
using EMailManejador;
using CLConfiguracion;

namespace FEChile
{

    class cfdReglasEmailSolicitud
    {
        public string sMsj = "";
        public int iErr = 0;
        private IConexionAFuenteDatos _Conexion;
        private IParametros _Param;
        private EmailSmtp _motorDeCorreo;
        private string _asunto;
        private string _cuerpo;

        public cfdReglasEmailSolicitud (IConexionAFuenteDatos conex, IParametros Param)
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

        public bool ObtieneSeccionesEmail(String plantilla)
        {
            iErr = 0;
            vwCfdCartasReclamacionDeuda carta = new vwCfdCartasReclamacionDeuda(_Conexion.ConnStr);
            carta.Where.Letter_type.Value = 3;
            carta.Where.Letter_type.Operator = WhereParameter.Operand.Equal;
            carta.Where.Ltrrptnm.Conjuction = WhereParameter.Conj.And;
            carta.Where.Ltrrptnm.Value = plantilla;
            carta.Where.Ltrrptnm.Operator = WhereParameter.Operand.Equal;
            sMsj = "";
            try
            {
                if (!carta.Query.Load())
                {
                    sMsj = "No está configurada la plantilla de la carta " + _Param.emailCarta + ". Ingrese esta plantilla en GP Tarjetas > ventas > Collection Letters.";
                    iErr++;
                    return false;
                }
                else
                {
                    _asunto = carta.CN_Email_Subject;
                    _cuerpo = carta.CN_Letter_Text;
                }
                return true;
            }
            catch (Exception eSe)
            {
                sMsj = "Contacte al administrador. No se pudo consultar la base de datos. [SeccionesEmail] " + eSe.Message;
                iErr++;
                return false;
            }

        }

        public DireccionesEmail ObtieneDirecciones(string custnmbr)
        {
            iErr = 0;
            sMsj = string.Empty;
            vwCfdClienteDireccionesCorreo dirCorreo = new vwCfdClienteDireccionesCorreo(_Conexion.ConnStr);     //direcciones de correo de los clientes
            dirCorreo.Where.CUSTNMBR.Value = custnmbr;
            dirCorreo.Where.CUSTNMBR.Operator = WhereParameter.Operand.Equal;
            DireccionesEmail dir = new DireccionesEmail("", "", "");
            try
            {
                if (!dirCorreo.Query.Load())
                {
                    sMsj = "El cliente no tiene direcciones de correo registradas.";
                    iErr++;
                    return dir;
                }
                else
                {
                    dir.mailTo = dirCorreo.EmailTo;
                    dir.mailCC = dirCorreo.EmailCC;
                    dir.mailCCO = dirCorreo.EmailCCO;
                }
                return dir;
            }
            catch (Exception edc)
            {
                sMsj = "Contacte al administrador. No se pudo consultar la base de datos. [ObtieneDireccionesDeCorreo] " + edc.Message;
                iErr++;
                return dir;
            }
        }

        public bool ProcesaMensaje(string custnmbr, string idFactura, string rutaYNombreArchivo)
        {
            iErr = 0;
            sMsj = string.Empty;
            List<string> Adjunto = new List<string>();
            DireccionesEmail dir = ObtieneDirecciones(custnmbr);

            if (iErr==0)
            {
                if (_Param.emite)
                    Adjunto.Add(rutaYNombreArchivo.Replace(".xml", ".cliente." + _Param.emailAdjEmite));    //xml o zip

                if (_Param.imprime)
                    Adjunto.Add(rutaYNombreArchivo.Replace(".xml", "." + _Param.emailAdjImprm));    //pdf

                if (!_motorDeCorreo.SendMessage(Utiles.Derecha(dir.mailTo, dir.mailTo.Length - 1), _Param.emailAccount,
                                    _asunto.Trim() + " (" + idFactura + ")", _cuerpo,
                                    Utiles.Derecha(dir.mailCC, dir.mailCC.Length - 1),
                                    Utiles.Derecha(dir.mailCCO, dir.mailCCO.Length - 1),
                                    _Param.replyto, Adjunto))
                {
                    iErr++;
                    sMsj = _motorDeCorreo.ultimoMensaje;
                }
            }

            return iErr==0;
        }
    }
}
