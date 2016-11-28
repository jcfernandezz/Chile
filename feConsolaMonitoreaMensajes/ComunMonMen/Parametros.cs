﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using CLConfiguracion;

namespace Comun
{
    public class Parametros : IParametros
    {
        private int _iErr;
        private string _sMsj = "";
        private string _encoding = String.Empty;
        private string _URLArchivoXSD = "";
        private string _URLArchivoLibroCVXSD = String.Empty;
        private string _URLArchivoEnvioXSD = String.Empty;
        private string _URLArchivoXSLT = "";
        private string _rutaArchivosTemp = String.Empty;

        private string _URLwebServPAC = "";
        private string _reporteador = "";
        private string _rutaReporteCrystal = "";
        private string _bottomMargin = "";
        private string _topMargin = "";
        private string _leftMargin = "";
        private string _rightMargin = "";
        private string _rutaReporteSSRS = "";
        private string _SSRSServer = "";
        private List<PrmtrsReporte> _ListaParametrosReporte = new List<PrmtrsReporte>();
        private List<PrmtrsReporte> _ListaParametrosRepSSRS = new List<PrmtrsReporte>();
        private string _servidor = "";
        private string _seguridadIntegrada = "0";
        private string _usuarioSql = "";
        private string _passwordSql = "";
        private string _emite = "0";
        private string _anula = "0";
        private string _imprime = "0";
        private string _publica = "0";
        private string _envia = "0";
        private string _zip = "0";              //default no comprime
        private string _emailSmtp = "";
        private string _emailPop3 = "";
        private string _emailPort = "";
        private string _emailPortIn = "";
        private string _emailAccount = "";
        private string _emailUser = "";
        private string _emailPwd = "";
        private string _emailSsl = "";
        private string _replyto = "";
        private string _emailCarta = "";
        private string _emailAdjEmite = "na";   //default no aplica
        private string _emailAdjImprm = "na";   //default no aplica
        private string _appConfigName = "ParametrosMoMe.xml";

        public Parametros()
        {
            try
            {
                _iErr = 0;
                _sMsj = string.Empty;
                XmlDocument listaParametros = new XmlDocument();
                listaParametros.Load(new XmlTextReader(_appConfigName));
                XmlNodeList listaElementos = listaParametros.DocumentElement.ChildNodes;
                
                foreach (XmlNode n in listaElementos)
                {
                    if (n.Name.Equals("servidor"))
                        this._servidor = n.InnerXml;
                    if (n.Name.Equals("seguridadIntegrada"))
                        this._seguridadIntegrada = n.InnerXml;
                    if (n.Name.Equals("usuariosql"))
                        this._usuarioSql = n.InnerXml;
                    if (n.Name.Equals("passwordsql"))
                        this._passwordSql = n.InnerXml;
                    if (n.Name.Equals("ingresoPredeterminado"))
                        this._ingresoPredeterminado = n.InnerXml;
                }
            }
            catch (Exception eprm)
            {
                _iErr++;
                _sMsj = "Contacte al administrador. No se pudo obtener la configuración general. [Parametros()]" + eprm.Message;
            }
        }

        public Parametros(string IdCompannia)
        {
            try
            {
                _iErr = 0;
                _sMsj = string.Empty;
                XmlDocument listaParametros = new XmlDocument();
                listaParametros.Load(new XmlTextReader(_appConfigName));
                XmlNode elemento = listaParametros.DocumentElement;
                _encoding = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/encoding/text()").Value;
                _URLArchivoXSD = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/URLArchivoXSD/text()").Value;
                _URLArchivoXSLT = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/URLArchivoXSLT/text()").Value;
                _rutaArchivosTemp = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/RutaArchivosTemp/text()").Value;
                _URLwebServPAC = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/PAC/urlWebService/text()").Value;
                _emite = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/emite/text()").Value;
                _anula = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/anula/text()").Value;
                _imprime = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/imprime/text()").Value;
                _publica = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/publica/text()").Value;
                _envia = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/envia/text()").Value;
                _zip = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/zip/text()").Value;
                _reporteador = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/reporteador/text()").Value;

                _rutaReporteCrystal = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/rutaReporteCrystal[@tipo='default']/Ruta/text()").Value;
                _bottomMargin = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/rutaReporteCrystal[@tipo='default']/Margenes/bottomMargin/text()").Value;
                _topMargin = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/rutaReporteCrystal[@tipo='default']/Margenes/topMargin/text()").Value;
                _leftMargin = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/rutaReporteCrystal[@tipo='default']/Margenes/leftMargin/text()").Value;
                _rightMargin = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/rutaReporteCrystal[@tipo='default']/Margenes/rightMargin/text()").Value;

                _rutaReporteSSRS = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/ReporteSSRS[@tipo='default']/Ruta/text()").Value;
                _SSRSServer = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/ReporteSSRS[@tipo='default']/SSRSServer/text()").Value;

                _emailSmtp = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/emailSetup/smtp/text()").Value;
                _emailPop3 = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/emailSetup/pop3/text()").Value;
                _emailPort = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/emailSetup/puerto/text()").Value;
                _emailPortIn = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/emailSetup/puertoIn/text()").Value;
                _emailAccount = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/emailSetup/cuenta/text()").Value;
                _emailUser = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/emailSetup/usuario/text()").Value;
                _emailPwd = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/emailSetup/clave/text()").Value;
                _emailSsl = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/emailSetup/ssl/text()").Value;
                _replyto = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/emailSetup/replyto/text()").Value;
                _emailCarta = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/emailSetup/carta/text()").Value;
                _emailAdjEmite = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/emailSetup/adjuntoEmite/text()").Value;
                _emailAdjImprm = elemento.SelectSingleNode("//compannia[@bd='" + IdCompannia + "']/emailSetup/adjuntoImprime/text()").Value;
                
            }
            catch (Exception eprm)
            {
                _iErr++;
                _sMsj = "Contacte al administrador. No se pudo obtener la configuración de la compañía " + IdCompannia + ". [Parametros(Compañía)] " + eprm.Message;
            }
        }

        public int iErr
        {
            get { return _iErr; }
        }

        public string sMsj
        {
            get { return _sMsj; }
        }

        public string appConfigName
        {
            get { return _appConfigName; }
            set { _appConfigName = value; }
        }

        public string servidor
        {
            get { return _servidor; }
            set { _servidor = value; }
        }

        public bool seguridadIntegrada
        {
            get 
            { 
                return _seguridadIntegrada.Equals("1"); 
            }
            set 
            { 
                if (value)
                    _seguridadIntegrada = "1"; 
                else
                    _seguridadIntegrada = "0"; 
            }
        }

        public string usuarioSql
        {
            get { return _usuarioSql; }
            set{ _usuarioSql = value;}
        }

        public string passwordSql
        {
            get { return _passwordSql; }
            set {_passwordSql=value;}
        }

        private String _ingresoPredeterminado = String.Empty;
        public String IngresoPredeterminado
        {
            get { return _ingresoPredeterminado; }
            set { _ingresoPredeterminado = value; }
        }

        public string encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        public string URLArchivoXSD
        {
            get { return _URLArchivoXSD; }
            set { _URLArchivoXSD = value; }
        }

        public string URLArchivoLibroCVXSD
        {
            get { return _URLArchivoLibroCVXSD; }
            set { _URLArchivoLibroCVXSD = value; }
        }

        public string URLArchivoEnvioXSD
        {
            get { return _URLArchivoEnvioXSD; }
            set { _URLArchivoEnvioXSD = value; }
        }

        public string URLArchivoXSLT
        {
            get { return _URLArchivoXSLT; }
            set { _URLArchivoXSLT = value; }
        }

        public string RutaArchivosTemp
        {
            get { return _rutaArchivosTemp; }
            set { _rutaArchivosTemp = value; }
        }


        public string URLwebServPAC
        {
            get { return _URLwebServPAC; }
            set { _URLwebServPAC = value; }
        }

        public string reporteador
        {
            get { return _reporteador; }
            set { _reporteador = value; }
        }

        public string rutaReporteCrystal
        {
            get { return _rutaReporteCrystal; }
        }

        public int bottomMargin
        {
            get
            {
               return Convert.ToInt32(_bottomMargin); 
            }
        }

        public int topMargin
        {
            get
            {
               return Convert.ToInt32(_topMargin);
            }
        }

        public int leftMargin
        {
            get { 
                return Convert.ToInt32(_leftMargin); 
            }
        }

        public int rightMargin
        {
            get { 
                return Convert.ToInt32(_rightMargin); 
            }
        }

        public string rutaReporteSSRS
        {
            get { return _rutaReporteSSRS; }
            set { _rutaReporteSSRS = value; }
        }

        public string SSRSServer
        {
            get { return _SSRSServer; }
            set { _SSRSServer = value; }
        }

        public List<PrmtrsReporte> ListaParametrosReporte
        {
            get { return _ListaParametrosReporte; }
        }

        public List<PrmtrsReporte> ListaParametrosRepSSRS
        {
            get { return _ListaParametrosRepSSRS; }
        }


        public int intEstadosPermitidos
        {
            get
            {
                return
                        Convert.ToInt32(_emite) +
                    2 * Convert.ToInt32(_anula) +
                    4 * Convert.ToInt32(_imprime) +
                    8 * Convert.ToInt32(_publica) +
                    16 * Convert.ToInt32(_envia);
            }
        }

        public int intEstadoCompletado
        {
            get
            {
                return
                        Convert.ToInt32(_emite) +
                    2 * 0 +
                    4 * Convert.ToInt32(_imprime) +
                    8 * Convert.ToInt32(_publica) +
                    16 * Convert.ToInt32(_envia);
            }
        }

        public bool emite
        {
            get { return _emite.Equals("1"); }
            set
            {
                if (value)
                    _emite = "1";
                else
                    _emite = "0";
            }
        }

        public bool anula
        {
            get { return _anula.Equals("1"); }
            set
            {
                if (value)
                    _anula = "1";
                else
                    _anula = "0";
            }
        }

        public bool imprime
        {
            get { return _imprime.Equals("1"); }
            set
            {
                if (value)
                    _imprime = "1";
                else
                    _imprime = "0";
            }
        }

        public bool publica
        {
            get { return _publica.Equals("1"); }
            set
            {
                if (value)
                    _publica = "1";
                else
                    _publica = "0";
            }
        }

        public bool envia
        {
            get { return _envia.Equals("1"); }
            set
            {
                if (value)
                    _envia = "1";
                else
                    _envia = "0";
            }
        }

        public bool zip
        {
            get { return _zip.Equals("1"); }
            set
            {
                if (value)
                    _zip = "1";
                else
                    _zip = "0";
            }
        }

        public string tipoDoc
        {
            get { return "FACTURA"; }
        }

        public string emailSmtp
        {
            get { return _emailSmtp; }
        }

        public string emailPop3
        {
            get { return _emailPop3; }
        }

        public int emailPort
        {
            get { return Convert.ToInt32( _emailPort); }
        }

        public int emailPortIn
        {
            get { return Convert.ToInt32(_emailPortIn); }
        }

        public string emailUser
        {
            get { return _emailUser; }
        }

        public string emailPwd
        {
            get { return _emailPwd; }
        }

        public string emailCarta
        {
            get { return _emailCarta; }
        }

        public string emailAccount
        {
            get { return _emailAccount; }
        }

        public bool emailSsl
        {
            get { return _emailSsl.ToLower().Equals("true"); }
        }

        public string replyto
        {
            get { return _replyto; }
        }

        public string emailAdjEmite
        {
            get { return _emailAdjEmite; }
        }

        public string emailAdjImprm
        {
            get { return _emailAdjImprm; }
        }
    }

}
