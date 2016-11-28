using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Comun;
using cfd.FacturaElectronica;
using System.Xml.Linq;
using EMailManejador;
using CLConfiguracion;
using MaquinaDeEstados;
using System.IO;
using System.Xml.XPath;

namespace FEChile
{
    public class CFDServicioDespachoSolicitudes
    {
        private IParametros _param;
        private ConexionAFuenteDatos _conex;
        private String _rutSII;
        //private LogFacturaXMLService _bitacora;

        public delegate void reportaProgreso(int iAvance, string sMsj);
        public event reportaProgreso Progreso;
        public void MuestraAvance(int iAvance, string sMsj)
        {
            if (Progreso != null)
                Progreso(iAvance, sMsj);
        }
        //*******************************************************
        #region Propiedades
        private int _iErr = 0;

        public int iErr
        {
            get { return _iErr; }
            set { _iErr = value; }
        }
        private string _sMsj = string.Empty;

        public string sMsj
        {
            get { return _sMsj; }
            set { _sMsj = value; }
        }
        
        #endregion
        //*******************************************************
        public CFDServicioDespachoSolicitudes(ConexionAFuenteDatos conex, Parametros param)
        {
            try
            {
                _iErr = 0;
                _sMsj = string.Empty;
                _rutSII = "60803000-K";
                _param = param;
                _conex = conex;
            }
            catch (Exception ini)
            {
                _sMsj = "Excepción al inicializar el servicio de despacho. " + ini.Message + " [CFDServicioDespachoSolicitudes.constructor]";
                _iErr++;
            }
        }

        public void EnviaAlSII(CFDComprobanteFiscalDigitalFabrica cfdsFabricados)
        {
            _iErr = 0;
            try
            {
                int iMaxErr = 0;
                int numContenedores = cfdsFabricados.lContenedores.Count();

                CFDServicioDespachoSII.Encriptador = cfdsFabricados.encriptador;
                CFDServicioDespachoSII.uri = _param.URLwebServPAC;                      //"https://maullin.sii.cl/cgi_dte/UPL/DTEUpload";

                String valorToken = String.Empty;

                //#if DEBUG
                //        MuestraAvance(1, "Iniciando envío de documentos al SII...");
                //#else
                    if (numContenedores > 0)
                    {
                        MuestraAvance(1, "Iniciando envío de documentos al SII...");
                        valorToken = CFDServicioDespachoSII.GetToken();
                        _iErr = CFDServicioDespachoSII.IErr;
                        _sMsj = CFDServicioDespachoSII.SMsj;
                    }
                //#endif

                if (_iErr != 0)
                {
                    MuestraAvance(100 / numContenedores, _sMsj);
                    return;
                }

                int i = 1;  //temporal para test
                foreach (EnvioDteModel envio in cfdsFabricados.lContenedores)
                {
                    _iErr = 0;

                    //guarda el xml
                    string rutaYNomArchivo = envio.GuardaArchivo();
                    _iErr = envio.iErr;
                    _sMsj = envio.sMsj;

                    //#if DEBUG
                    //    MuestraAvance(1, "DEBUG enviado al SII...");
                    //#else
                        //enviar al SII
                        if (_iErr == 0)
                        {

                            //CFDServicioDespachoSII.xdocument = XDocument.Parse(envio.xDocXml.InnerXml, LoadOptions.PreserveWhitespace);
                            CFDServicioDespachoSII.nombreArchivo = envio.setId + ".xml"; //envio.RutaXml.Trim() + //
                            CFDServicioDespachoSII.rutEmisor = envio.RutPersonaEnvia;          //persona
                            CFDServicioDespachoSII.rutEmpresa = envio.RutEmpresaEmisora;       //compañía

                            //trackId es obtenido por UploadFile
                            CFDServicioDespachoSII.UploadFile(valorToken, envio.xDocXml);       //  habilitar en producción
                            //CFDServicioDespachoSII.trackId = i.ToString();                    //. habilitar para test
                            i++;

                            _iErr = CFDServicioDespachoSII.IErr;
                            _sMsj = CFDServicioDespachoSII.SMsj;
                        }

                        //si el envío fue satisfactorio registrar en la bitácora
                        if (_iErr == 0)
                        {
                            envio.GuardaActualiza(CFDServicioDespachoSII.trackId);
                            _iErr = envio.iErr;
                            _sMsj = envio.sMsj;
                        }
                    //#endif
                    MuestraAvance(100 / numContenedores, "Set: " + envio.setId + " " + _sMsj);

                    if (_iErr > 0) iMaxErr++;
                    if (iMaxErr > 10) break;
                }
                if (numContenedores > 0)
                    MuestraAvance(100, "Envío finalizado. ");

                MuestraAvance(100, "");

            }
            catch (Exception es)
            {
                _iErr++;
                _sMsj = "Excepción al enviar sets al SII. [CFDServicioDespachoSolicitudes.EnviaAlSII()]" + es.Message;
            }
        }

        public void generaSETPruebaParaElSII(CFDComprobanteFiscalDigitalFabrica cfdsFabricados)
        {
            _iErr = 0;
            try
            {
                int iMaxErr = 0;
                int numContenedores = cfdsFabricados.lContenedores.Count();

                CFDServicioDespachoSII.Encriptador = cfdsFabricados.encriptador;
                CFDServicioDespachoSII.uri = _param.URLwebServPAC;                      //"https://maullin.sii.cl/cgi_dte/UPL/DTEUpload";

                String valorToken = String.Empty;

                if (numContenedores > 0)
                {
                    MuestraAvance(1, "SET de prueba para el SII...");
                    //valorToken = CFDServicioDespachoSII.GetToken();
                    //_iErr = CFDServicioDespachoSII.IErr;
                    //_sMsj = CFDServicioDespachoSII.SMsj;
                }

                if (_iErr != 0)
                {
                    MuestraAvance(100 / numContenedores, _sMsj);
                    return;
                }

                foreach (EnvioDteModel envio in cfdsFabricados.lContenedores)
                {
                    _iErr = 0;

                    //guarda el xml
                    string rutaYNomArchivo = envio.GuardaArchivo();
                    _iErr = envio.iErr;
                    _sMsj = envio.sMsj;

                    MuestraAvance(100 / numContenedores, "Set: " + envio.setId + " " + _sMsj);

                    if (_iErr > 0) iMaxErr++;
                    if (iMaxErr > 10) break;
                }
                if (numContenedores > 0)
                    MuestraAvance(100, "Envío finalizado. ");

                MuestraAvance(100, "");

            }
            catch (Exception es)
            {
                _iErr++;
                _sMsj = "Excepción al enviar sets al SII. [CFDServicioDespachoSolicitudes.generaSETPruebaParaElSII()]" + es.Message;
            }
        }

        /// <summary>
        /// Envía correos
        /// Requisito. Los cfds deben haber transicionado
        /// </summary>
        /// <param name="cfdsFabricados">contiene la lista cfds en la lista lDocumentos y la lista de envíos lContenedores</param>
        public void EnviaAlCliente(CFDComprobanteFiscalDigitalFabrica cfdsFabricados)
        {
            _iErr = 0;
            _sMsj = string.Empty;
            int numDocs = cfdsFabricados.lDocumentos.Count();
            if (numDocs == 0)
            {
                MuestraAvance(0, "No hay comprobantes válidos para enviar a los clientes. Verifique el estado de los documentos que requiere enviar.");
                return;
            }
            else
                MuestraAvance(1, "Iniciando envío de documentos a clientes...");
            
            cfdReglasEmailSolicitud envio = new cfdReglasEmailSolicitud(_conex, _param);
            if (envio.iErr != 0 || !envio.ObtieneSeccionesEmail(_param.emailCarta))
            {
                MuestraAvance(2, "No puede enviar e-mails. ");
                MuestraAvance(0, envio.sMsj);
                return;
            }

            foreach(CFDComprobanteFiscalDigital cfd in cfdsFabricados.lDocumentos)
            {
                ConstruyeEnvioAlCliente(cfd, cfdsFabricados);

                if (_iErr == 0)
                {
                    if (envio.ProcesaMensaje(cfd.custnmbr, cfd.sopnumbe, cfd.mensaje))
                    {
                        _sMsj = cfd.cicloDeVida.targetSingleStatus;

                        cfd.GuardaActualizaMensaje("E-mail enviado el " + DateTime.Today.ToString());

                    }
                    else
                    {
                        _iErr = envio.iErr;
                        _sMsj = envio.sMsj;
                    }
                }
                MuestraAvance(100 / numDocs, "Doc: " + cfd.sopnumbe + " " + _sMsj);
            }

            if (numDocs > 0)
                MuestraAvance(100, "Envío de correos finalizado. ");
            MuestraAvance(100, "");
        }

        /// <summary>
        /// Forma el envío de la factura usando como receptor el rut del cliente. El nuevo archivo es guardado con la extensión .cliente.xml
        /// Debe existir un archivo xml enviado al SII y aceptado por el SII
        /// </summary>
        /// <param name="cfd"></param>
        /// <param name="cfdsFabricados"></param>
        private void ConstruyeEnvioAlCliente(CFDComprobanteFiscalDigital cfd, CFDComprobanteFiscalDigitalFabrica cfdsFabricados)
        {
            try
            {
                XmlDocument xDteParaCliente = new XmlDocument();
                xDteParaCliente.PreserveWhitespace = true;

                String rutaYNomArchivo = cfd.mensaje;
                String dteEnviadoAlSii = "";
                using (StreamReader sr = new StreamReader(rutaYNomArchivo, cfdsFabricados.encodig))
                {
                    dteEnviadoAlSii = sr.ReadToEnd();
                }
                xDteParaCliente.LoadXml(dteEnviadoAlSii);

                XPathNavigator navigator = xDteParaCliente.CreateNavigator();

                XmlNamespaceManager nsManager = new XmlNamespaceManager(navigator.NameTable);
                nsManager.AddNamespace("env", "http://www.sii.cl/SiiDte");
                nsManager.AddNamespace("sig", "http://www.w3.org/2000/09/xmldsig#");

                //Reemplaza el rut del SII por el rut del cliente
                foreach (XPathNavigator nav in navigator.Select("//env:Caratula/env:RutReceptor", nsManager))
                {
                    if (nav.Value.Equals(_rutSII))
                    {
                        nav.SetValue(cfd.idCliente);
                    }
                }

                XmlNodeList nodes = xDteParaCliente.SelectNodes("//env:EnvioDTE/sig:Signature", nsManager);
                //Quita el nodo Signature del envío
                foreach (XmlNode node in nodes)
                {
                    node.RemoveAll();
                    break;
                }
                String sDteParaCliente = xDteParaCliente.InnerXml.Replace("<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"></Signature>", String.Empty);
                xDteParaCliente.LoadXml(sDteParaCliente);

                //Firma el nuevo envío
                XmlAttribute idAFirmar = (XmlAttribute)xDteParaCliente.SelectSingleNode("//env:EnvioDTE/env:SetDTE/@ID", nsManager);
                cfdsFabricados.ModeloEnvio = new EnvioDteModel();
                cfdsFabricados.ModeloEnvio.setId = idAFirmar.Value;
                cfdsFabricados.ModeloEnvio.xDocXml = xDteParaCliente;
                cfdsFabricados.ModeloEnvio.criptografo = cfdsFabricados.encriptador;
                cfdsFabricados.firmaEnvio();

                String rutaYNomArchivoCliente = rutaYNomArchivo.Replace(".xml", ".cliente.xml");
                CustomXmlTextWriter tw = new CustomXmlTextWriter(rutaYNomArchivoCliente, cfdsFabricados.encodig.WebName.ToUpper());
                cfdsFabricados.ModeloEnvio.xDocXml.Save(tw);
                tw.Close();

            }
            catch (Exception ce)
            {
                _sMsj =  "Excepción al formar el envío al cliente. " + ce.Message + " [CFDServicioDespachoSolicitudes.ConstruyeEnvioAlCliente]";
                _iErr++;
            } 
        }

        public void EnviaAlProveedor()
        {
 
        }

    }
}
