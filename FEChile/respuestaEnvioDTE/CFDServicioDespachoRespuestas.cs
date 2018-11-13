using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using cfd.FacturaElectronica;
using System.Xml.Linq;
using EMailManejador;
using EstructuraMensajeEMail;
using Encriptador;
using CLConfiguracion;
using MaquinaDeEstados;
using ReciboMercaderiaServicios;
using MyGeneration.dOOdads;

namespace respuestaEnvioDTE
{
    public class CFDServicioDespachoRespuestas
    {
        private String _xNameSpace = "{http://www.sii.cl/SiiDte}";
        private vwCfdCertificadosService _certificados;
        private TecnicaDeEncriptacion _encriptador;
        private Encoding _encoding;
        private IConexionAFuenteDatos _conex;
        private String _mensajeSii;
        private String _sTrackId;
        private String _tipoDoc;
        private String _continuarBusqueda = "NO";
        private IParametros _param;
        private short _sopType;
        private String _sopnumbe;
        private String _idImpuestoCliente = String.Empty;
        private String _compoundedBinStatus = String.Empty;
        private String _idxSingleStatus=String.Empty;
        private String rutPropio ;
        //*********************************************************************
        #region Propiedades
        private int _iErr = 0;
        public int IErr
        {
            get { return _iErr; }
            set { _iErr = value; }
        }
        private string _sMsj = string.Empty;
        public string SMsj
        {
            get { return _sMsj; }
            set { _sMsj = value; }
        }
        private List<CFDReciboMercaServicio> _lProdRecibido;

        public List<CFDReciboMercaServicio> LProdRecibido
        {
            get { return _lProdRecibido; }
            set { _lProdRecibido = value; }
        }

        private List<RespuestaEnvio> _lDocsRecibidos;
        public List<RespuestaEnvio> LDocsRecibidos
        {
            get { return _lDocsRecibidos; }
            set { _lDocsRecibidos = value; }
        }
        private List<IMensajeEMail> _newXmlMessages;

        public List<IMensajeEMail> NewXmlMessages
        {
            get { return _newXmlMessages; }
            set { _newXmlMessages = value; }
        }

        #endregion
        //*********************************************************************

        public delegate void reportaProgreso(int iAvance, string sMsj);
        public event reportaProgreso Progreso;
        public void MuestraAvance(int iAvance, string sMsj)
        {
            if (Progreso != null)
                Progreso(iAvance, sMsj);
        }

        public CFDServicioDespachoRespuestas(IConexionAFuenteDatos conex, IParametros param)  
        {
            try
            {
                _iErr = 0;
                _sMsj = string.Empty;
                _conex = conex;
                _encoding = Encoding.GetEncoding(param.encoding);
                _lDocsRecibidos = new List<RespuestaEnvio>();
                _param = param;
                preparaCertificado();                                       //carga certificados y _encriptador
                rutPropio = "76731982-7";        //gila chile "76055568-1";
            }
            catch (Exception ini)
            {
                _sMsj = "Excepción al inicializar el servicio de despacho. " + ini.Message + " [CFDServicioDespacho constructor]";
                _iErr++;
            }
        }

        /// <summary>
        /// Prepara la firma electrónica del usuario
        /// </summary>
        private void preparaCertificado()
        {
            try
            {
                _certificados = new vwCfdCertificadosService(_conex.ConnStr);
                _encriptador = new TecnicaDeEncriptacion();

                bool existeCert = _certificados.get(_conex.Usuario);
                if (existeCert)
                {
                    _encriptador.PreparaEncriptacion("", _certificados.clave.Trim(), _certificados.Ruta_certificado.Trim(), "");
                }
                else
                {
                    _iErr = 50;
                    _sMsj = "El usuario no tiene asignado un certificado de firma. Ingrese al Mantenimiento de certificados y agregue la ruta del certificado para " + _conex.Usuario + " " + _certificados.sMsj + " [CFDServicioDespachoRespuestas.preparaCertificado()]";
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        //public void EnviaAlSII(CFDComprobanteFiscalDigitalFabrica cfdsFabricados)
        //{
        //    iErr = 0;
        //    int iMaxErr = 0;
        //    int numContenedores = cfdsFabricados.lContenedores.Count();
        //    if (numContenedores > 0)
        //        MuestraAvance(1, "Iniciando envío de documentos al SII");

        //    foreach (EnvioDteModel envio in cfdsFabricados.lContenedores)
        //    { 
        //        string rutaYNomArchivo = envio.Guarda();                        //guarda el xml

        //        //enviar al SII
        //        //.....
        //        //iErr = 

        //        //si el envío fue satisfactorio registrar en la bitácora
        //        if (iErr == 0)
        //        {
        //            foreach (var rec in cfdsFabricados.lDocumentos.Where(d => d.setId.Equals(envio.setId)).Select(d => d))
        //            {
        //                sMsj = rec.cicloDeVida.targetSingleStatus;
        //                _bitacora.Save(rec.soptype, rec.sopnumbe, rutaYNomArchivo + ".xml",
        //                            rec.cicloDeVida.idxTargetSingleStatus.ToString(), _conex.Usuario, envio.xDocXml.InnerXml,
        //                            rec.cicloDeVida.targetSingleStatus, rec.cicloDeVida.targetBinStatus, rec.cicloDeVida.EstadoEnPalabras(rec.cicloDeVida.targetBinStatus));

        //                _bitacora.Update(rec.soptype, rec.sopnumbe, _conex.Usuario, "emitido", "emitido", rec.cicloDeVida.targetBinStatus,
        //                            rec.cicloDeVida.EstadoEnPalabras(rec.cicloDeVida.targetBinStatus), rec.cicloDeVida.idxTargetSingleStatus.ToString());
        //            }
        //        }
        //        else
        //            sMsj = "error al enviar al SII";

        //        MuestraAvance(100 / numContenedores, "Conjunto: " + envio.setId + " " + sMsj);

        //        if (iErr > 0) iMaxErr++;
        //        if (iMaxErr > 10) break;
        //    }

        //    if (numContenedores > 0)
        //        MuestraAvance(100, "Envío finalizado. ");
        //    MuestraAvance(100, "");

        //}

        ///// <summary>
        ///// Envía correos
        ///// </summary>
        ///// <param name="cfdsFabricados">contiene la lista de comprobantes</param>
        //public void EnviaAlCliente(CFDComprobanteFiscalDigitalFabrica cfdsFabricados)
        //{
        //    iErr = 0;
        //    sMsj = string.Empty;
        //    int numDocs = cfdsFabricados.lDocumentos.Count();
        //    if (numDocs == 0)
        //    {
        //        MuestraAvance(0, "No hay comprobantes válidos para enviar a clientes.");
        //        return;
        //    }
        //    else
        //        MuestraAvance(1, "Iniciando envío de documentos a clientes...");
            
        //    cfdReglasEmail envio = new cfdReglasEmail(_conex, _param);
        //    if (envio.iErr != 0 || !envio.ObtieneSeccionesEmail())
        //    {
        //        MuestraAvance(2, "No puede enviar e-mails");
        //        MuestraAvance(0, envio.sMsj);
        //        return;
        //    }

        //    foreach(CFDComprobanteFiscalDigital cfd in cfdsFabricados.lDocumentos)
        //    {
        //        if (envio.ProcesaMensaje(cfd.custnmbr, cfd.sopnumbe, cfd.mensaje))
        //        {
        //            sMsj = cfd.cicloDeVida.targetSingleStatus;
        //            _bitacora.Save(cfd.soptype, cfd.sopnumbe, "ok",
        //                        cfd.cicloDeVida.idxTargetSingleStatus.ToString(), _conex.Usuario, "E-mail enviado el " + DateTime.Today.ToString(),
        //                        cfd.cicloDeVida.targetSingleStatus, cfd.cicloDeVida.targetBinStatus, cfd.cicloDeVida.EstadoEnPalabras(cfd.cicloDeVida.targetBinStatus));

        //            _bitacora.Update(cfd.soptype, cfd.sopnumbe, _conex.Usuario, "emitido", "emitido", cfd.cicloDeVida.targetBinStatus,
        //                        cfd.cicloDeVida.EstadoEnPalabras(cfd.cicloDeVida.targetBinStatus), cfd.cicloDeVida.idxTargetSingleStatus.ToString());
        //        }
        //        else
        //        {
        //            iErr = envio.iErr;
        //            sMsj = envio.sMsj;
        //        }
        //        MuestraAvance(100 / numDocs, "Doc: " + cfd.sopnumbe + " " + sMsj);

        //    }

        //    if (numDocs > 0)
        //        MuestraAvance(100, "Envío de emails finalizado. ");
        //    MuestraAvance(100, "");
        //}

        public void Recepciona()
        {
            _iErr = 0;
            _sMsj = string.Empty;
            MuestraAvance(100, "Revisando el contenido...");

            try
            {
                string sTipoDte = string.Empty;
                string sFolio = string.Empty;
                string sIdImpuesto = string.Empty;
                string sNomTercero = string.Empty;
                DateTime fechaRecep = DateTime.Now;
                XDocument xDoc;
                int numMensajes = _newXmlMessages.Count();
                foreach (IMensajeEMail xmlAdjunto in _newXmlMessages)
                {
                    int eventoSII = 0;
                    try
                    {
                        xDoc = XDocument.Parse(xmlAdjunto.xmlString, LoadOptions.PreserveWhitespace);

                        //Revisar mensajes del SII
                        eventoSII = RevisaRespuestaDelSII(xDoc);
                        if (_continuarBusqueda.Equals("NO") && _iErr == 0 && eventoSII != 0)
                        {
                            GuardaRespuestaDelSII(xDoc, eventoSII, xmlAdjunto.Uid);
                        }

                        //Revisar si es Factura de proveedor
                        if (_continuarBusqueda.Equals("SI") && _iErr == 0)
                        {
                            RevisaYFormaRespuestaPorDTEDeProveedor(xDoc, xmlAdjunto);
                        }

                        //Revisar envío de recepción factura por parte del cliente
                        if (_continuarBusqueda.Equals("SI") && _iErr == 0)
                            RevisaRespuestaDelClienteFacturaRecibida(xDoc, xmlAdjunto);                 //recibido con error o conforme

                        //Revisar envío de producto recibido por parte del cliente
                        if (_continuarBusqueda.Equals("SI") && _iErr == 0)
                            RevisaRespuestaDelClienteProductoRecibido(xDoc, xmlAdjunto);               //recibo de productos

                        //Revisar envío de resultado de factura por parte del cliente
                        if (_continuarBusqueda.Equals("SI") && _iErr == 0)
                            RevisaRespuestaDelClienteResultado(xDoc, xmlAdjunto);                      //aceptado o rechazado

                    }
                    catch (Exception px)
                    {
                        _iErr++;
                        _sMsj = "El archivo está corrupto. Elimine este correo y notifique al remitente." + px.Message + " [CFDServicioDespachoRespuestas.Recepciona()]";
                    }
                    MuestraAvance(100 / numMensajes, "Archivo " + xmlAdjunto.nombreArchivoXml + " recibido de " + xmlAdjunto.Mensaje.Headers.From.Address + ". " + _sMsj);
                }

            }
            catch (Exception mr)
            {
                _sMsj = "Excepción al recibir los mensajes del correo. " + mr.Message + " [CFDServicioDespachoRespuestas.Recepciona]";
                _iErr++;
                MuestraAvance(100, _sMsj);
            }
        }

        /// <summary>
        /// Si el evento es mensaje recibido, sólo guarda el log.
        /// Si el evento es acuse o recibido con error, adicionalmente envía la respuesta al proveedor
        /// </summary>
        /// <param name="evento"></param>
        /// <param name="rcb"></param>
        /// <param name="envio"></param>
        private void ProcesaMensajeRecibidoDelProveedor(int evento, RespuestaEnvio rcb, CFDReglasEmailRespuesta envio)
        {
            _iErr = 0;
            LogFacturaCompraService logReceptor = new LogFacturaCompraService(_conex.ConnStr, rcb.Folio, Convert.ToInt16(rcb.tipoDTE), rcb.rutEmisor, Maquina.estadoBaseReceptor);

            //Obtiene el status del documento, verifica la transición y guarda en el log del receptor.
            if (logReceptor.CicloDeVida.Transiciona(evento, _certificados.envia))
            {
                if (evento == Maquina.eventoAcuseDocumento)
                {
                    rcb.GuardaArchivoDelProveedor();            //archivo xml enviado por el proveedor
                    _sMsj = rcb.archivoRecibido + " " + rcb.SMsj;
                    if (rcb.IErr > 0)
                        throw new Exception(_sMsj);
                }

                if (evento != Maquina.eventoRecibidoConforme)
                {
                    rcb.SaveFile();                             //archivo xml que indica la recepción de la factura
                    _sMsj = rcb.RutaYNomArchivo + " " + rcb.SMsj;
                    if (rcb.IErr > 0)
                       throw new Exception(_sMsj);
                }

                if (evento != Maquina.eventoRecibidoConforme)
                {
                    envio.Asunto = "Getty Chile - " + rcb.TipoRespuestaResultado + " Dte: ";
                    envio.Cuerpo = "Esta es una respuesta automática. \n\nAtte. \nGetty Images Chile.";
                    envio.EmailTo = rcb.EmailProveedor;

                    envio.ProcesaMensaje(rcb.tipoDTE + "-" + rcb.Folio, rcb.RutaYNomArchivo + ".xml");
                    if (envio.iErr > 0)
                        throw new Exception(envio.sMsj);
                    _sMsj = " Respuesta enviada. ";
                }

                logReceptor.Tipo = Convert.ToInt16(rcb.tipoDTE);
                logReceptor.Folio = rcb.Folio;
                logReceptor.IdImpuestoTercero = rcb.rutEmisor;
                logReceptor.NombreTercero = rcb.NomEmisor;
                logReceptor.FechaRecepcion = DateTime.Now;
                logReceptor.Mensaje = rcb.EmailProveedor;
                logReceptor.SDocXml = rcb.xDocXml.InnerXml;
                logReceptor.Pdf = rcb.archivoRecibido;
                logReceptor.IdExterno = rcb.Uid;
                logReceptor.IdUsuario = rcb.Usuario;

                logReceptor.GuardaYActualiza();

                if (logReceptor.IErr > 0)
                    throw new Exception(logReceptor.SMsj);
                _sMsj += "Log registrado. ";

            }
            else
            {
                if (logReceptor.ExisteDoc && rcb.Evento == Maquina.eventoAcuseDocumento)
                    logReceptor.Save(0, rcb.Folio, rcb.rutEmisor, rcb.NomEmisor, rcb.fechaRecepcion, rcb.Uid, "correo repetido", 0, "0", "procesado", "-", "-", rcb.Uid, rcb.Usuario);

                _sMsj = "Tipo: " + logReceptor.Tipo.ToString() + " " + logReceptor.CicloDeVida.sMsj + " " + logReceptor.SMsj;
                _iErr++;
            }

        }

        /// <summary>
        /// Envía las respuestas guardadas en _ldocsRecibidos y registra el log
        /// </summary>
        public void ProcesaRespuestasAlProveedor()
        {
            _iErr = 0;
            _sMsj = String.Empty;

            int numDocs = _lDocsRecibidos.Count();
            if (numDocs == 0)
            {
                MuestraAvance(0, "Mensajes para enviar a los proveedores: 0");
                return;
            }
            else
                MuestraAvance(1, "Iniciando el envío de mensajes a proveedores...");

            _param.imprime = false;
            CFDReglasEmailRespuesta envio = new CFDReglasEmailRespuesta(_conex, _param);

            if (envio.iErr != 0)
            {
                MuestraAvance(100, "Envío de mensajes a proveedores finalizado. " + envio.sMsj);
                return;
            }

            foreach (RespuestaEnvio rcb in _lDocsRecibidos)
            {
                try
                {
                    ProcesaMensajeRecibidoDelProveedor(rcb.Evento, rcb, envio);
                    MuestraAvance(100 / numDocs, "Doc: " + rcb.tipoDTE + "-" + rcb.Folio + _sMsj);

                }
                catch (Exception re)
                {
                    MuestraAvance(100 / numDocs, "Doc: " + rcb.tipoDTE + "-" + rcb.Folio + " Excepción desconocida. No se pudo procesar la respuesta al proveedor. " + re.Message);
                }
            }

            if (numDocs > 0)
                MuestraAvance(100, "Envío de mensajes finalizado. ");
            MuestraAvance(100, "");
        }

        /// <summary>
        /// deprecated
        /// </summary>
        private void EnviaRespuestaAlProveedor()
        {
            _iErr = 0;
            _sMsj = String.Empty;

            int numDocs = _lDocsRecibidos.Count();
            if (numDocs == 0)
            {
                MuestraAvance(0, "Mensajes para enviar a los proveedores: 0");
                return;
            }
            else
                MuestraAvance(1, "Iniciando el envío de mensajes a proveedores...");

            _param.imprime = false;
            CFDReglasEmailRespuesta envio = new CFDReglasEmailRespuesta(_conex, _param);

            if (envio.iErr != 0)
            {
                MuestraAvance(100, "Envío de mensajes a proveedores finalizado. " + envio.sMsj);
                return;
            }

            foreach (RespuestaEnvio rcb in _lDocsRecibidos)
            {
                try
                {
                    LogFacturaCompraService logReceptor = new LogFacturaCompraService(_conex.ConnStr, rcb.Folio, Convert.ToInt16(rcb.tipoDTE), rcb.rutEmisor, Maquina.estadoBaseReceptor);
                    //Obtiene el status del documento, verifica la transición y guarda en el log del receptor.
                    if (logReceptor.CicloDeVida.Transiciona(rcb.Evento, _certificados.envia))
                    {
                        if (rcb.Evento == Maquina.eventoAcuseDocumento)
                        {
                            rcb.GuardaArchivoDelProveedor();            //archivo xml enviado por el proveedor
                            _iErr = rcb.IErr;
                            _sMsj = rcb.archivoRecibido + " " + rcb.SMsj;
                        }

                        if (_iErr == 0)
                        {
                            rcb.SaveFile();                             //archivo xml que indica la recepción de la factura
                            _iErr = rcb.IErr;
                            _sMsj = rcb.RutaYNomArchivo + " " + rcb.SMsj;
                        }

                        if (_iErr == 0)
                        {
                            envio.Asunto = "Getty Chile - " + rcb.TipoRespuestaResultado + " Dte: ";
                            envio.Cuerpo = "Esta es una respuesta automática. \n\nAtte. \nGetty Images Chile.";
                            envio.EmailTo = rcb.EmailProveedor;

                            envio.ProcesaMensaje(rcb.tipoDTE + "-" + rcb.Folio, rcb.RutaYNomArchivo + ".xml");
                            _iErr = envio.iErr;
                            _sMsj = envio.sMsj;
                        }

                        if (_iErr == 0)
                        {
                            logReceptor.Tipo = Convert.ToInt16(rcb.tipoDTE);
                            logReceptor.Folio = rcb.Folio;
                            logReceptor.IdImpuestoTercero = rcb.rutEmisor;
                            logReceptor.NombreTercero = rcb.NomEmisor;
                            logReceptor.FechaRecepcion = DateTime.Now;
                            logReceptor.Mensaje = rcb.EmailProveedor;
                            logReceptor.SDocXml = rcb.xDocXml.InnerXml;
                            logReceptor.Pdf = rcb.archivoRecibido;
                            logReceptor.IdExterno = rcb.Uid;
                            logReceptor.IdUsuario = rcb.Usuario;

                            logReceptor.GuardaYActualiza();

                            _iErr = logReceptor.IErr;
                            _sMsj = logReceptor.SMsj;
                        }
                        MuestraAvance(100 / numDocs, "Doc: " + rcb.tipoDTE + "-" + rcb.Folio + " Respuesta enviada. " + _sMsj);
                    }
                    else
                    {
                        if (logReceptor.ExisteDoc && rcb.Evento == Maquina.eventoAcuseDocumento)
                            logReceptor.Save(0, rcb.Folio, rcb.rutEmisor, rcb.NomEmisor, rcb.fechaRecepcion, rcb.Uid, "correo repetido", 0, "0", "procesado", "-", "-", rcb.Uid, rcb.Usuario);
                        _iErr = logReceptor.CicloDeVida.iErr;
                        _sMsj = logReceptor.CicloDeVida.sMsj + " " + logReceptor.SMsj;
                        MuestraAvance(100 / numDocs, "Doc: " + rcb.tipoDTE + "-" + rcb.Folio + " " + _sMsj);
                    }
                }
                catch (Exception re)
                {
                    MuestraAvance(100 / numDocs, "Doc: " + rcb.tipoDTE + "-" + rcb.Folio + " Excepción desconocida. No se pudo enviar respuesta al proveedor. " + re.Message);
                }
            }

            if (numDocs > 0)
                MuestraAvance(100, "Envío de mensajes finalizado. ");
            MuestraAvance(100, "");
        }

        public void EnviaAcuseAlProveedor()
        {
            _iErr = 0;
            _sMsj = string.Empty;

            int numDocs = _lProdRecibido.Count();
            if (numDocs == 0)
            {
                MuestraAvance(0, "Verifique el estado de los documentos que ha marcado. Sólo se puede realizar el acuse a los comprobantes que han sido aceptados.");
                return;
            }
            else
                MuestraAvance(1, "Iniciando envío de acuse de recepción...");
            
            _param.imprime = false;
            CFDReglasEmailRespuesta envio = new CFDReglasEmailRespuesta(_conex, _param);
            _iErr = envio.iErr;
            _sMsj = envio.sMsj;

            if (_iErr != 0)
                return;
            //if (envio.iErr != 0 || !envio.ObtieneSeccionesEmail(_param.emailCarta))
            //{
            //    MuestraAvance(2, "No puede enviar e-mails. ");
            //    MuestraAvance(0, envio.sMsj);
            //    return;
            //}

            foreach (CFDReciboMercaServicio acu in _lProdRecibido)
            {

                LogFacturaCompraService logReceptor = new LogFacturaCompraService(_conex.ConnStr, acu.Folio, Convert.ToInt16(acu.TipoDte), acu.IdImpuestoTercero, Maquina.estadoBaseReceptor);
                //Obtiene el status del documento, verifica la transición y guarda en el log del receptor.
                if (logReceptor.CicloDeVida.Transiciona(acu.Evento, _certificados.envia))
                {
                    acu.SaveFile();
                    _iErr = acu.iErr;
                    _sMsj = acu.RutaYNomArchivo + " " + acu.sMsj;

                    if (_iErr == 0)
                    {
                        envio.Asunto = "Getty Chile - Acuse Dte: ";
                        envio.Cuerpo = "Esta es una respuesta automática de Getty Images Chile.";
                        envio.EmailTo = acu.EmailProveedor;

                        envio.ProcesaMensaje(acu.TipoDte.ToString() + "-" + acu.Folio, acu.RutaYNomArchivo + ".xml");
                        _iErr = envio.iErr;
                        _sMsj = envio.sMsj;
                    }

                    if (_iErr == 0)
                    {
                        logReceptor.Tipo = Convert.ToInt16(acu.TipoDte);
                        logReceptor.Folio = acu.Folio;
                        logReceptor.IdImpuestoTercero = acu.IdImpuestoTercero;
                        logReceptor.NombreTercero = acu.NombreTercero;
                        logReceptor.FechaRecepcion = DateTime.Now;
                        logReceptor.Mensaje = acu.Mensaje;
                        logReceptor.SDocXml = acu.XDocXml.InnerXml;
                        logReceptor.IdExterno = acu.IdExterno;
                        logReceptor.IdUsuario = acu.Usuario;
                        logReceptor.Pdf = "-";
                        logReceptor.Mensaje = acu.EmailProveedor;
                        logReceptor.GuardaYActualiza();

                        _iErr = logReceptor.IErr;
                        _sMsj = logReceptor.SMsj;
                    }

                    MuestraAvance(100 / numDocs, "Doc: " + acu.TipoDte.ToString() + "-" + acu.Folio + " " + _sMsj);
                }
                else
                {
                    _iErr = logReceptor.CicloDeVida.iErr;
                    _sMsj = logReceptor.CicloDeVida.sMsj + " " + logReceptor.SMsj;
                    MuestraAvance(100 / numDocs, "Doc: " + acu.TipoDte.ToString() + "-" + acu.Folio + " " + _sMsj);
                }

            }

            if (numDocs > 0)
                MuestraAvance(100, "Envío de correos finalizado. ");
            MuestraAvance(100, "");
        }

        /// <summary>
        /// Identifica la respuesta del SII. 
        /// Requisito. La factura debe estar asociada al trackId en el log
        /// </summary>
        /// <param name="xPosibleRespuesta"></param>
        /// <returns>Evento que corresponde al SII</returns>
        public int RevisaRespuestaDelSII(XDocument xPosibleRespuesta)
        {
            _iErr = 0;
            _sMsj = String.Empty;
            _continuarBusqueda = "NO";
            int eventoSII = 0;

            try
            {
                _sTrackId = xPosibleRespuesta.Element("RESULTADO_ENVIO").Element("IDENTIFICACION").Element("TRACKID").Value;
            }
            catch (NullReferenceException nr)
            {
                _sMsj = "No se encuentra el trackId del SII. " + nr.Message + " [CFDServicioDespachoRespuestas.RevisaRespuestaDelSII]";
                _continuarBusqueda = "SI";
                return eventoSII;
            }

            try
            {
                _sTrackId = _sTrackId.PadLeft(10, '0');
                eventoSII = Maquina.eventoSIIRechazo;
                _mensajeSii = "RECHAZA";
                String resultadoSIIOtros = String.Empty;

                _tipoDoc = xPosibleRespuesta.Element("RESULTADO_ENVIO").Element("ESTADISTICA").Element("SUBTOTAL").Element("TIPODOC").Value;

                var xSet = xPosibleRespuesta.Elements("RESULTADO_ENVIO").Elements("ESTADISTICA").Elements("SUBTOTAL").Elements("ACEPTA");
                foreach (var reg in xSet)
                {
                    eventoSII = Maquina.eventoSIIAcepta;
                    _mensajeSii = "ACEPTA";
                    break;
                }

                xSet = xPosibleRespuesta.Elements("RESULTADO_ENVIO").Elements("ESTADISTICA").Elements("SUBTOTAL").Elements("RECHAZO");
                foreach (var reg in xSet)
                {                    
                    eventoSII = Maquina.eventoSIIRechazo;
                    _mensajeSii = "RECHAZA";
                    break;
                }

                xSet = xPosibleRespuesta.Elements("RESULTADO_ENVIO").Elements("ESTADISTICA").Elements("SUBTOTAL").Elements("REPARO");
                foreach (var reg in xSet)
                {                    
                    eventoSII = Maquina.eventoSIIReparo;
                    _mensajeSii = "REPARO";
                    break;
                }

                //Revisa error
                //try
                //{
                    xSet = xPosibleRespuesta.Elements("RESULTADO_ENVIO").Elements("ERRORENVIO").Elements("DETERRENVIO");
                    foreach (var reg in xSet)
                    {
                        resultadoSIIOtros = "ERROR";
                        _mensajeSii += reg.Value + "\n";
                    }

                //}
                //catch (Exception)
                //{
                //}

                //Revisa detalle envío
                //try
                //{
                    xSet = xPosibleRespuesta.Elements("RESULTADO_ENVIO").Elements("REVISIONENVIO").Elements("REVISIONDTE");
                    foreach (var reg in xSet)
                    {
                        resultadoSIIOtros = "EN REVISION";
                        _mensajeSii += reg.Name.ToString() + ": " + reg.Value + "\n";
                    }

                //}
                //catch (Exception)
                //{
                //}                
                _sMsj = _mensajeSii;
                return eventoSII;
            }
            catch (NullReferenceException nr)
            {
                _tipoDoc = "33";    //Sirve para usar la máquina de estados
                _sMsj = "El SII indica inconsistencia en el id " + _sTrackId +" "+ nr.Message + " [CFDServicioDespachoRespuestas.RevisaRespuestaDelSII]";
            }
            catch (Exception re)
            {
                _sMsj = "Excepción desconocida al revisar la respuesta del SII. " + re.Message + " [CFDServicioDespachoRespuestas.RevisaRespuestaDelSII]";
                _iErr++;
            }
            return eventoSII;
        }

        private string Derecha(string Texto, int Cuantos)
        {
            if (Texto.Length > Cuantos && Cuantos > 0)
            {
                return Texto.Remove(0, Texto.Length - Cuantos);
            }
            else
                return Texto;
        }

        public void GuardaRespuestaDelSII(XDocument xRespuesta, int eventoSII, String uid)
        {
            _iErr = 0;
            _sMsj = String.Empty;
            try
            {
                if (eventoSII == Maquina.eventoSIIAcepta || eventoSII == Maquina.eventoSIIRechazo || eventoSII == Maquina.eventoSIIReparo)
                {
                    LogFacturaCompraService logReceptor = new LogFacturaCompraService(_conex.ConnStr);
                    LogFacturaXMLService logEmisor = new LogFacturaXMLService(_conex, _tipoDoc);
                    bool existe = logEmisor.TraeLlaves(_sTrackId, Maquina.estadoBaseEmisor);

                    if (existe)
                    {
                        logEmisor.CicloDeVida = new Maquina(logEmisor.CompoundedBinStatus, logEmisor.IdxSingleStatus, 0, "emisor", _tipoDoc);
                        if (logEmisor.CicloDeVida.Transiciona(eventoSII, _certificados.envia))
                        {
                            logEmisor.Save("Resultado del SII. " + _mensajeSii, _conex.Usuario, xRespuesta.ToString(), _sTrackId);

                            logEmisor.Update(_conex.Usuario, Maquina.estadoBaseEmisor, Maquina.estadoBaseEmisor);

                            logReceptor.Save(0, logEmisor.Sopnumbe, _sTrackId, "-", DateTime.Now, Maquina.estadoBaseReceptor, "Resultado del SII", 0, "-", _mensajeSii, "", "", uid, _conex.Usuario);
                        }
                        else
                        {
                            _sMsj = "Repetido. Probablemente el SII envió su resultado varias veces. ";
                            logReceptor.Save(0, logEmisor.Sopnumbe, Derecha(_sTrackId + "-" + uid, 15), "-", DateTime.Now, "ELIMINAR EMAIL", _sMsj, 0, "-", _mensajeSii, "", "", uid, _conex.Usuario);
                        }
                        _iErr = logEmisor.CicloDeVida.iErr;
                        _sMsj = _sMsj + logEmisor.CicloDeVida.sMsj;
                    }
                    else
                    {
                        _iErr++;
                        _sMsj = "No existe el documento con track Id del SII: " + _sTrackId + "[CFDServicioDespachoRespuestas.GuardaRespuestaDelSII]";
                    }
                }

            }
            catch (Exception pr)
            {
                _sMsj = "Excepción desconocida al guardar la respuesta del SII. Track id del SII: " + _sTrackId +" "+ pr.Message + " [CFDServicioDespachoRespuestas.GuardaRespuestaDelSII]";
                _iErr++;
            }
        }

        /// <summary>
        /// Sólo para etapa de certificación: Intercambio 
        /// </summary>
        /// <param name="xPosibleRespuesta"></param>
        /// <param name="xmlAdjunto"></param>
        public void DaUnaRespuestaPorEnvioDeProveedor(XDocument xPosibleRespuesta, IMensajeEMail xmlAdjunto)
        {
            _iErr = 0;
            _sMsj = String.Empty;
            _continuarBusqueda = "NO";

            try
            {
                var xSet = xPosibleRespuesta.Elements(_xNameSpace + "EnvioDTE").Elements(_xNameSpace + "SetDTE").Elements();
                RespuestaEnvio respuesta = new RespuestaEnvio(_conex.ConnStr, _encoding, _conex.Usuario, _certificados.envia);
                respuesta.criptografo = _encriptador;
                respuesta.idResultado = "RECIBEDOC";
                respuesta.EnvioDTEID = xPosibleRespuesta.Element(_xNameSpace + "EnvioDTE").Element(_xNameSpace + "SetDTE").Attribute("ID").Value;
                respuesta.Uid = xmlAdjunto.Uid;
                int numDte = 0;
                foreach (var dte in xSet)     //Revisar cada DTE
                {
                    //Revisar esquema
                    //Revisar firma

                    if (dte.Name.ToString().Equals(_xNameSpace + "Caratula"))
                    {
                        foreach (XElement id in dte.Elements())
                        {
                            if (id.Name.ToString().Equals(_xNameSpace + "RutEnvia"))                         //persona que envía el doc.
                                respuesta.RutRecibe = id.Value;
                            if (id.Name.ToString().Equals(_xNameSpace + "RutEmisor"))                        //empresa que envía (según carátula)
                                respuesta.rutEmisor = id.Value;
                            if (id.Name.ToString().Equals(_xNameSpace + "RutReceptor"))                      //empresa que recibe el envío (según carátula)
                                respuesta.rutReceptor = id.Value;
                        }
                        respuesta.RutResponde = _certificados.idImpuesto;
                        respuesta.archivoRecibido = xmlAdjunto.nombreArchivoXml;
                        respuesta.fechaRecepcion = xmlAdjunto.dateSent;
                        respuesta.EstadoRecepEnv = 0;                                                        //envío recibido conforme

                        respuesta.EnsamblaRecepcionCab(2);
                    }

                    if (dte.Name.ToString().Equals(_xNameSpace + "DTE"))
                    {
                        //inicio etapa intercambio
                        if (numDte == 0) respuesta.EstadoRecepDTE = "0";
                        if (numDte == 1) respuesta.EstadoRecepDTE = "3";
                        //fin etapa intercambio

                        foreach (XElement id in dte.Elements(_xNameSpace + "Documento").Elements(_xNameSpace + "Encabezado").Elements(_xNameSpace + "IdDoc").Elements()) //obtener el id del DTE
                        {
                            //Obtener datos del documento
                            if (id.Name.ToString().Equals(_xNameSpace + "TipoDTE"))
                                respuesta.tipoDTE = id.Value;   // short.Parse(id.Value);
                            if (id.Name.ToString().Equals(_xNameSpace + "Folio"))
                                respuesta.Folio = id.Value;
                            if (id.Name.ToString().Equals(_xNameSpace + "FchEmis"))
                            {
                                respuesta.fchEmis = DateTime.ParseExact(id.Value, "yyyy-M-d", null);
                            }
                        }
                        foreach (XElement id in dte.Elements(_xNameSpace + "Documento").Elements(_xNameSpace + "Encabezado").Elements(_xNameSpace + "Emisor").Elements())
                        {
                            if (id.Name.ToString().Equals(_xNameSpace + "RUTEmisor"))        //empresa que envía el dte (según DTE)
                                respuesta.rutEmisor = id.Value;
                            if (id.Name.ToString().Equals(_xNameSpace + "RznSoc"))           //empresa que envía el dte (según DTE)
                                respuesta.NomEmisor = id.Value;
                        }
                        foreach (XElement id in dte.Elements(_xNameSpace + "Documento").Elements(_xNameSpace + "Encabezado").Elements(_xNameSpace + "Receptor").Elements())
                        {
                            if (id.Name.ToString().Equals(_xNameSpace + "RUTRecep"))         //empresa que recibe el dte (según DTE)
                                respuesta.rutReceptor = id.Value;
                        }
                        foreach (XElement id in dte.Elements(_xNameSpace + "Documento").Elements(_xNameSpace + "Encabezado").Elements(_xNameSpace + "Totales").Elements())
                        {
                            if (id.Name.ToString().Equals(_xNameSpace + "MntTotal"))
                                respuesta.sMntTotal = id.Value;
                        }
                        respuesta.EnsamblaRecepcionDet(numDte);
                        numDte++;

                    }

                    //Mover aquí la firma de la respuesta para enviar una por dte
                }
                //Averiguar en el SII si el dte es válido

                //Revisar si transiciona

                //if (!respuesta.Folio.Equals(string.Empty))
                if (numDte > 0)
                {
                    respuesta.EnsamblaRecepcionPie();
                    respuesta.Serializa(_encoding);
                    respuesta.Canonicaliza();
                    respuesta.reAjustaAtributos();
                    respuesta.firma(respuesta.idResultado);

                    //Registrar el acuse en la bitácora y obtener el identificador de respuesta (correlativo)
                    //respuesta.Guarda(10);
                    //respuesta.Save("recibido cl");

                    //respuesta.GetIdExterno();

                    //Enviar el acuse
                    //EnviaAlProveedor();
                }

            }
            catch (NullReferenceException nr)
            {
                _sMsj = "No se encuentra el id del envío. " + nr.Message + " [CFDServicioDespachoRespuestas.RevisarRespuestaDelProveedor]";
                _continuarBusqueda = "SI";
            }
            catch (Exception re)
            {
                _sMsj = "Excepción desconocida al revisar la respuesta del Proveedor. " + re.Message + " [CFDServicioDespachoRespuestas.RevisarRespuestaDelProveedor]";
                _iErr++;
            }
        }

        /// <summary>
        /// Identifica la factura de un proveedor y ensambla la respuesta de recepción.
        /// </summary>
        /// <param name="xPosibleRespuesta"></param>
        /// <param name="xmlAdjunto"></param>
        public void RevisaYFormaRespuestaPorDTEDeProveedor(XDocument xPosibleRespuesta, IMensajeEMail xmlAdjunto)
        {
            _iErr = 0;
            _sMsj = String.Empty;
            _continuarBusqueda = "SI";

            try
            {
                var xSet = xPosibleRespuesta.Elements(_xNameSpace + "EnvioDTE").Elements(_xNameSpace + "SetDTE").Elements();
                //Revisar esquema
                //Revisar firma

                //Revisar cada DTE
                foreach (var dte in xSet)                                                     
                {
                    if (dte.Name.ToString().Equals(_xNameSpace + "DTE"))
                    {
                        String rutEmisor = xPosibleRespuesta.Element(_xNameSpace + "EnvioDTE").Element(_xNameSpace + "SetDTE").Element(_xNameSpace + "Caratula").Element(_xNameSpace + "RutEmisor").Value;       //empresa que envía (según carátula)
                        _continuarBusqueda = "NO";

                        if (rutEmisor.Equals(rutPropio))   //en caso que la factura emitida rebote
                            break;

                        RespuestaEnvio respuesta = new RespuestaEnvio(_conex.ConnStr, _encoding, _conex.Usuario, _certificados.envia);
                        respuesta.RutaXml = _param.RutaArchivosTemp;
                        respuesta.criptografo = _encriptador;
                        respuesta.XDocProveedor = xPosibleRespuesta;
                        respuesta.idResultado = "RECIBEDOC";
                        respuesta.Prefijo = "RCB";
                        respuesta.EnvioDTEID = xPosibleRespuesta.Element(_xNameSpace + "EnvioDTE").Element(_xNameSpace + "SetDTE").Attribute("ID").Value;
                        respuesta.Uid = xmlAdjunto.Uid;
                        respuesta.EmailProveedor = xmlAdjunto.Mensaje.Headers.From.Address;

                        respuesta.RutRecibe = xPosibleRespuesta.Element(_xNameSpace + "EnvioDTE").Element(_xNameSpace + "SetDTE").Element(_xNameSpace + "Caratula").Element(_xNameSpace + "RutEnvia").Value;        //persona que envía el doc.
                        respuesta.rutEmisor = rutEmisor;
                        respuesta.rutReceptor = xPosibleRespuesta.Element(_xNameSpace + "EnvioDTE").Element(_xNameSpace + "SetDTE").Element(_xNameSpace + "Caratula").Element(_xNameSpace + "RutReceptor").Value;   //empresa que recibe el envío (según carátula)
                        respuesta.RutResponde = _certificados.idImpuesto;
                        respuesta.archivoRecibido = xmlAdjunto.nombreArchivoXml;
                        respuesta.fechaRecepcion = xmlAdjunto.dateSent;

                        //Verificar en el SII si el dte es válido

                        respuesta.EstadoRecepEnv = 99;                      //envío recibido con error
                        respuesta.Evento = Maquina.eventoRecibidoConError;
                        RespuestaSII respuestaSii = new RespuestaSII();
                        respuesta.EstadoRecepDTE = respuestaSii.VerificaEstadoDTE();

                        if (respuestaSii.RespHdrEstado.Equals("0"))
                        {
                            if (respuestaSii.RespBodyRecibido.Equals("SI"))
                            {
                                respuesta.EstadoRecepEnv = 0;               //envío recibido conforme
                                respuesta.Evento = Maquina.eventoAcuseDocumento;
                            }
                        }

                        respuesta.tipoDTE = dte.Element(_xNameSpace + "Documento").Element(_xNameSpace + "Encabezado").Element(_xNameSpace + "IdDoc").Element(_xNameSpace + "TipoDTE").Value;
                        respuesta.Folio = dte.Element(_xNameSpace + "Documento").Element(_xNameSpace + "Encabezado").Element(_xNameSpace + "IdDoc").Element(_xNameSpace + "Folio").Value;
                        respuesta.fchEmis = DateTime.ParseExact(
                                        dte.Element(_xNameSpace + "Documento").Element(_xNameSpace + "Encabezado").Element(_xNameSpace + "IdDoc").Element(_xNameSpace + "FchEmis").Value,
                                        "yyyy-M-d", null);

                        respuesta.rutEmisor = dte.Element(_xNameSpace + "Documento").Element(_xNameSpace + "Encabezado").Element(_xNameSpace + "Emisor").Element(_xNameSpace + "RUTEmisor").Value; //empresa que envía el dte (según DTE)
                        respuesta.NomEmisor = dte.Element(_xNameSpace + "Documento").Element(_xNameSpace + "Encabezado").Element(_xNameSpace + "Emisor").Element(_xNameSpace + "RznSoc").Value;

                        respuesta.rutReceptor = dte.Element(_xNameSpace + "Documento").Element(_xNameSpace + "Encabezado").Element(_xNameSpace + "Receptor").Element(_xNameSpace + "RUTRecep").Value; //empresa que recibe el dte (según DTE)
                        respuesta.sMntTotal = dte.Element(_xNameSpace + "Documento").Element(_xNameSpace + "Encabezado").Element(_xNameSpace + "Totales").Element(_xNameSpace + "MntTotal").Value;

                        respuesta.EnsamblaRecepcionCab(1);  //una respuesta por dte
                        respuesta.EnsamblaRecepcionDet(0);  //índice empieza en cero
                        respuesta.EnsamblaRecepcionPie();

                        respuesta.Serializa(_encoding);
                        respuesta.Canonicaliza();
                        respuesta.reAjustaAtributos();
                        respuesta.firma(respuesta.idResultado);

                        _iErr = respuesta.IErr;
                        _sMsj = respuesta.SMsj;

                        if (_iErr == 0)
                        {
                            _lDocsRecibidos.Add(respuesta);
                            ProcesaMensajeRecibidoDelProveedor(Maquina.eventoRecibidoConforme, respuesta, null);
                        }

                        _sMsj += " " + respuestaSii.RespBodyGlosa;
                    }
                }
            }
            catch (NullReferenceException nr)
            {
                _sMsj = "No se encuentra el id del envío. " + nr.Message + " [CFDServicioDespachoRespuestas.RevisaYFormaRespuestaPorDTEDeProveedor]";
                _continuarBusqueda = "SI";
            }
            catch (Exception re)
            {
                _sMsj = "Excepción al revisar el mensaje del Proveedor. " + re.Message + " [CFDServicioDespachoRespuestas.RevisaYFormaRespuestaPorDTEDeProveedor]";
                _iErr++;
            }
        }

        /// <summary>
        /// Identifica el resultado comercial del cliente. Hace dos transiciones: Mensaje recibido y factura aceptada o rechazada
        /// </summary>
        /// <param name="xPosibleRespuesta"></param>
        /// <param name="uid">Id del email a guardar en el log receptor</param>
        private void RevisaRespuestaDelClienteResultado(XDocument xPosibleRespuesta, IMensajeEMail xmlAdjunto)  
        {
            _iErr = 0;
            _sMsj = String.Empty;
            _continuarBusqueda = "SI";
            String estadoDte = String.Empty;
            int eventoCliente = 0;
            String folio = String.Empty;
            String tipoDte = String.Empty;
            String mensajeDte = String.Empty;
            try
            {
                _sTrackId = xPosibleRespuesta.Element(_xNameSpace + "RespuestaDTE").Element(_xNameSpace + "Resultado").Element(_xNameSpace + "Caratula").Element(_xNameSpace + "IdRespuesta").Value;
                String sIdCliente = xPosibleRespuesta.Element(_xNameSpace + "RespuestaDTE").Element(_xNameSpace + "Resultado").Element(_xNameSpace + "Caratula").Element(_xNameSpace + "RutResponde").Value;

                var xRecepcionEnvio = xPosibleRespuesta.Element(_xNameSpace + "RespuestaDTE").Elements(_xNameSpace + "Resultado").Elements(_xNameSpace + "ResultadoDTE");

                foreach (var rcp in xRecepcionEnvio)
                {
                    _continuarBusqueda = "NO";

                    estadoDte = rcp.Element(_xNameSpace + "EstadoDTE").Value;
                    mensajeDte = rcp.Element(_xNameSpace + "EstadoDTEGlosa").Value;
                    tipoDte = rcp.Element(_xNameSpace + "TipoDTE").Value;
                    folio = rcp.Element(_xNameSpace + "Folio").Value;

                    RegistrarRespuestaDelCliente(Maquina.eventoRecibidoConforme, xPosibleRespuesta, xmlAdjunto.Uid, folio, tipoDte,
                                        "Id:" + _sTrackId + " Dte:" + tipoDte + "-" + folio + " " + mensajeDte + "(" + estadoDte + ") del:" + xmlAdjunto.Mensaje.Headers.Date,
                                        xmlAdjunto.nombreArchivoXml, sIdCliente, xmlAdjunto.Mensaje.Headers.From.Address);
                    MuestraAvance(100, "Folio: " + tipoDte + "-" + folio + " " + _sMsj);

                    if (estadoDte.Equals("0") || estadoDte.Equals("1"))
                        eventoCliente = Maquina.eventoResultadoAceptado;
                    else
                        eventoCliente = Maquina.eventoResultadoRechazado;

                    RegistrarRespuestaDelCliente( eventoCliente, xPosibleRespuesta, xmlAdjunto.Uid, folio, tipoDte,
                                        "Id:" + _sTrackId + " Dte:" + tipoDte + "-" + folio + " " + mensajeDte + "(" + estadoDte + ") del:" + xmlAdjunto.Mensaje.Headers.Date,
                                        xmlAdjunto.nombreArchivoXml, sIdCliente, xmlAdjunto.Mensaje.Headers.From.Address);

                    MuestraAvance(100, "Folio: "+ tipoDte +"-"+ folio + " " + _sMsj);
                }
            }
            catch (NullReferenceException nr)
            {
                _sMsj = "No se encuentra el Id de la respuesta. " + nr.Message + " [CFDServicioDespachoRespuestas.RevisaRespuestaDelClienteResultado]";
                _continuarBusqueda = "SI";
            }
            catch (Exception re)
            {
                _sMsj = "Excepción desconocida al revisar la respuesta del cliente. " + re.Message + " [CFDServicioDespachoRespuestas.RevisaRespuestaDelClienteResultado]";
                _iErr++;
            }
        }

        /// <summary>
        /// Revisa si la respuesta es un acuse de recepción de producto. Hace dos transiciones: Mensaje recibido y acuse de recepción de producto.
        /// </summary>
        /// <param name="xPosibleRespuesta"></param>
        /// <param name="uid">Id del email a guardar en el log receptor</param>
        private void RevisaRespuestaDelClienteProductoRecibido(XDocument xPosibleRespuesta, IMensajeEMail xmlAdjunto) 
        {

            _iErr = 0;
            _sMsj = String.Empty;
            _continuarBusqueda = "SI";
            String folio = String.Empty;
            String tipoDte = String.Empty;
            String mensajeDte = String.Empty;
            String sPosibleRespuesta = xPosibleRespuesta.ToString();
            String xNameSpaceTmp = String.Empty;
            try
            {
                if (sPosibleRespuesta.IndexOf("xmlns=\"http://www.sii.cl/SiiDte\"") > 0)
                    xNameSpaceTmp = _xNameSpace;
                _sTrackId = xPosibleRespuesta.Element(xNameSpaceTmp + "EnvioRecibos").Element(xNameSpaceTmp + "SetRecibos").Element(xNameSpaceTmp + "Caratula").Element(xNameSpaceTmp + "RutRecibe").Value;
                String sIdCliente = xPosibleRespuesta.Element(xNameSpaceTmp + "EnvioRecibos").Element(xNameSpaceTmp + "SetRecibos").Element(xNameSpaceTmp + "Caratula").Element(xNameSpaceTmp + "RutResponde").Value;

                var xRecibo = xPosibleRespuesta.Element(xNameSpaceTmp + "EnvioRecibos").Elements(xNameSpaceTmp + "SetRecibos").Elements(xNameSpaceTmp + "Recibo"); 

                //acuse de recibo
                foreach (var rcb in xRecibo)
                {
                    _continuarBusqueda = "NO";

                    _sTrackId = rcb.Element(xNameSpaceTmp + "DocumentoRecibo").Attribute("ID").Value;
                    tipoDte = rcb.Element(xNameSpaceTmp + "DocumentoRecibo").Element(xNameSpaceTmp + "TipoDoc").Value;
                    folio = rcb.Element(xNameSpaceTmp + "DocumentoRecibo").Element(xNameSpaceTmp + "Folio").Value;
                    mensajeDte = tipoDte + "-" + folio;

                    RegistrarRespuestaDelCliente(Maquina.eventoRecibidoConforme, xPosibleRespuesta, xmlAdjunto.Uid, folio, tipoDte, "Acuse:" + _sTrackId + " Dte:" + mensajeDte + " del " + xmlAdjunto.Mensaje.Headers.Date,
                                                  xmlAdjunto.nombreArchivoXml, sIdCliente, xmlAdjunto.Mensaje.Headers.From.Address);
                    MuestraAvance(100, "Folio: " + tipoDte + "-" + folio + " " + _sMsj);

                    RegistrarRespuestaDelCliente(Maquina.eventoAcuseProducto, xPosibleRespuesta, xmlAdjunto.Uid, folio, tipoDte, "Acuse:" + _sTrackId + " Dte:" + mensajeDte + " del " + xmlAdjunto.Mensaje.Headers.Date,
                                                 xmlAdjunto.nombreArchivoXml, sIdCliente, xmlAdjunto.Mensaje.Headers.From.Address);

                    MuestraAvance(100, "Folio: " + tipoDte + "-" + folio + " " + _sMsj);

                }
            }
            catch (NullReferenceException nr)
            {
                _sMsj = "No se encuentra el Id de la respuesta. " + nr.Message + " [CFDServicioDespachoRespuestas.RevisaRespuestaDelClienteProductoRecibido]";
                _continuarBusqueda = "SI";
            }
            catch (Exception re)
            {
                _sMsj = "Excepción desconocida al revisar la respuesta del cliente. " + re.Message + " [CFDServicioDespachoRespuestas.RevisaRespuestaDelClienteProductoRecibido]";
                _iErr++;
            }
        }

        /// <summary>
        /// Identifica la respuesta del cliente y hace dos transiciones: Mensaje recibido y acuse de documento conforme o con error. 
        /// </summary>
        /// <param name="xPosibleRespuesta"></param>
        /// <param name="uid">Id del email a guardar en el log receptor</param>
        private void RevisaRespuestaDelClienteFacturaRecibida(XDocument xPosibleRespuesta, IMensajeEMail xmlAdjunto) 
        {
            _iErr = 0;
            _sMsj = String.Empty;
            _continuarBusqueda = "SI";
            String estadoEnvio = String.Empty;
            String estadoDte = String.Empty;
            int eventoCliente = 0;
            String folio = String.Empty;
            String tipoDte = String.Empty;
            String mensajeEnvio = String.Empty;
            String mensajeDte = String.Empty;
            try
            {
                _sTrackId = xPosibleRespuesta.Element(_xNameSpace + "RespuestaDTE").Element(_xNameSpace + "Resultado").Element(_xNameSpace + "Caratula").Element(_xNameSpace + "IdRespuesta").Value;
                String sIdCliente = xPosibleRespuesta.Element(_xNameSpace + "RespuestaDTE").Element(_xNameSpace + "Resultado").Element(_xNameSpace + "Caratula").Element(_xNameSpace + "RutResponde").Value;

                var xRecepcionEnvio = xPosibleRespuesta.Elements(_xNameSpace + "RespuestaDTE").Elements(_xNameSpace + "Resultado").Elements(_xNameSpace + "RecepcionEnvio");

                //envío conforme o con error
                foreach (var rcp in xRecepcionEnvio)
                {
                    estadoEnvio = rcp.Element(_xNameSpace + "EstadoRecepEnv").Value;
                    mensajeEnvio = rcp.Element(_xNameSpace + "RecepEnvGlosa").Value;
                    String nombreEnvio = rcp.Element(_xNameSpace + "NmbEnvio").Value;
                    foreach (var dte in rcp.Elements())
                    {
                        if (dte.Name.ToString().Equals(_xNameSpace + "RecepcionDTE"))
                        {
                            _continuarBusqueda = "NO";

                            tipoDte = dte.Element(_xNameSpace + "TipoDTE").Value;
                            folio = dte.Element(_xNameSpace + "Folio").Value;
                            estadoDte = dte.Element(_xNameSpace + "EstadoRecepDTE").Value;
                            mensajeDte = dte.Element(_xNameSpace + "RecepDTEGlosa").Value;

                            RegistrarRespuestaDelCliente(Maquina.eventoRecibidoConforme, xPosibleRespuesta, xmlAdjunto.Uid, folio, tipoDte,
                                                "Id:" + _sTrackId + " Dte:" + tipoDte + "-" + folio + " " + mensajeDte + "(" + estadoDte + ") del " + xmlAdjunto.Mensaje.Headers.Date + " " + mensajeEnvio,
                                                xmlAdjunto.nombreArchivoXml, sIdCliente, xmlAdjunto.Mensaje.Headers.From.Address);
                            MuestraAvance(100, "Folio: " + tipoDte + "-" + folio + " " + _sMsj);

                            if (estadoEnvio.Equals("0") && estadoDte.Equals("0"))
                                eventoCliente = Maquina.eventoAcuseDocumento;
                            else
                                eventoCliente = Maquina.eventoRecibidoConError;

                            RegistrarRespuestaDelCliente(eventoCliente, xPosibleRespuesta, xmlAdjunto.Uid, folio, tipoDte,
                                                "Id:" + _sTrackId + " Dte:" + tipoDte + "-" + folio + " " + mensajeDte + "(" + estadoDte + ") del " + xmlAdjunto.Mensaje.Headers.Date + " " + mensajeEnvio, 
                                                xmlAdjunto.nombreArchivoXml, sIdCliente, xmlAdjunto.Mensaje.Headers.From.Address);

                            MuestraAvance(100, "Folio: " + tipoDte + "-" + folio + " " + _sMsj);
                        }
                    }

                    //Si no existe el nodo RecepcionDTE y el envío tiene error, indicar recibido con error
                    if (_continuarBusqueda.Equals("SI") && !estadoEnvio.Equals("0"))
                    {
                        //Ej. 33-00000001_90193000-7_EMPRESASELMERCURI.xml
                        tipoDte = Izquierda(nombreEnvio, 2);
                        int finPosicionFolio = nombreEnvio.IndexOf("_");
                        folio = nombreEnvio.Substring(3, finPosicionFolio - 3);

                        _continuarBusqueda = "NO";

                        RegistrarRespuestaDelCliente(Maquina.eventoRecibidoConError, xPosibleRespuesta, xmlAdjunto.Uid, folio, tipoDte,
                                            "Id:" + _sTrackId + " Dte:" + tipoDte + "-" + folio + " " + mensajeEnvio + "(" + estadoEnvio + ") del " + xmlAdjunto.Mensaje.Headers.Date + " " + mensajeEnvio,
                                            xmlAdjunto.nombreArchivoXml, sIdCliente, xmlAdjunto.Mensaje.Headers.From.Address);

                        MuestraAvance(100, "Folio: " + tipoDte + "-" + folio + " " + _sMsj);
                    }
                }
            }
            catch (NullReferenceException nr)
            {
                _sMsj = "No se encuentra el Id de la respuesta. " + nr.Message + " [CFDServicioDespachoRespuestas.RevisarRespuestaDelClienteFacturaRecibida]";
                _continuarBusqueda = "SI";
            }
            catch (Exception re)
            {
                _sMsj = "Excepción desconocida al revisar la respuesta del cliente. " + re.Message + " [CFDServicioDespachoRespuestas.RevisarRespuestaDelClienteFacturaRecibida]";
                _iErr++;
            }
        }

        /// <summary>
        /// Si existe una transición para el evento, la factura cambia al siguiente estado y registra la respuesta del cliente en el log
        /// </summary>
        /// <param name="eventoCliente"></param>
        /// <param name="xRespuesta"></param>
        /// <param name="uid"></param>
        /// <param name="folio"></param>
        /// <param name="tipo"></param>
        /// <param name="mensajeResultado"></param>
        /// <param name="nomArchivoRecibido"></param>
        /// <param name="rutClienteXml"></param>
        /// <param name="direccionRemitente"></param>
        private void RegistrarRespuestaDelCliente(int eventoCliente, XDocument xRespuesta, String uid, String folio, String tipo, String mensajeResultado, String nomArchivoRecibido, String rutClienteXml, String direccionRemitente)
        {
            try
            {
                _sMsj = string.Empty;
                LogFacturaCompraService logReceptor = new LogFacturaCompraService(_conex.ConnStr);
                bool existeDoc = TraeDatosDocVentas(folio, tipo, Maquina.estadoBaseEmisor);
                if (!existeDoc)
                    _sopnumbe = uid;

                LogFacturaXMLService logEmisor = new LogFacturaXMLService(_conex, _compoundedBinStatus, _idxSingleStatus, _sopnumbe, _sopType, tipo);

                if (!_idImpuestoCliente.Equals(rutClienteXml))
                {
                    _sMsj = "El remitente envió este archivo por error. El documento de referencia con RUT: " + rutClienteXml + " no está en GP. Este correo ha sido procesado. [CFDServicioDespachoRespuestas.GuardaRespuestaDelCliente()]";
                    logReceptor.Save(0, logEmisor.Sopnumbe, rutClienteXml, "-", DateTime.Now, string.Concat(uid, "-", eventoCliente.ToString()), nomArchivoRecibido, 0, "-", mensajeResultado, "", "", uid, _conex.Usuario);
                    return;
                }

                if (logEmisor.CicloDeVida.Transiciona(eventoCliente, _certificados.envia))
                {
                    logEmisor.Save("Resultado del cliente. " + mensajeResultado, _conex.Usuario, xRespuesta.ToString(), uid);
                    logEmisor.Update(_conex.Usuario, Maquina.estadoBaseEmisor, Maquina.estadoBaseEmisor);
                    logReceptor.Save(0, logEmisor.Sopnumbe, rutClienteXml, "-", DateTime.Now, string.Concat(uid, "-", eventoCliente.ToString()), nomArchivoRecibido, 0, "-", mensajeResultado, "", "", uid, _conex.Usuario);
                }
                else
                    if (logEmisor.CicloDeVida.iErr < 0)
                    {
                        bool yaRecorrido = logEmisor.CicloDeVida.transicionRecorrida(eventoCliente, _compoundedBinStatus);
                        if (yaRecorrido)
                            _sMsj = "Este correo ya fue procesado anteriormente. [CFDServicioDespachoRespuestas.GuardaLogRespuestaDelCliente()]";
                        else
                            _sMsj += "\nVerifique el " + mensajeResultado + " No hay una transición para el evento " +eventoCliente.ToString() + " desde el estado " + _idxSingleStatus.ToString() + " Se guardará en el log sin procesar.";

                        logReceptor.Save(0, logEmisor.Sopnumbe, rutClienteXml, "-", DateTime.Now, string.Concat(uid, "-", eventoCliente.ToString()), nomArchivoRecibido, 0, "-", mensajeResultado, "", "", uid, _conex.Usuario);
                    }
            }
            catch (Exception pr)
            {
                _sMsj = "Folio: " + tipo + "-" + folio + " uid: " + uid + " evento: " + eventoCliente.ToString() + " rut cliente: " + rutClienteXml + " respuesta: " + mensajeResultado +
                    " Excepción al guardar el log de la respuesta del cliente. [CFDServicioDespachoRespuestas.GuardaLogRespuestaDelCliente] " + pr.Message;
                throw new Exception(_sMsj);
            }
        }

        private void GuardaRespuestaDelCliente(XDocument xRespuesta, int eventoCliente, String uid, String folio, String tipo, String mensajeResultado, String nomArchivoRecibido, String rutClienteXml, String direccionRemitente)
        {
            _iErr = 0;
            _sMsj = String.Empty;
            LogFacturaCompraService logReceptor = new LogFacturaCompraService(_conex.ConnStr);
            bool existeDoc = TraeDatosDocVentas(folio, tipo, Maquina.estadoBaseEmisor);
            if (!existeDoc)
                _sopnumbe = uid;

            LogFacturaXMLService logEmisor = new LogFacturaXMLService(_conex, _compoundedBinStatus, _idxSingleStatus, _sopnumbe, _sopType, tipo);
            _iErr = logEmisor.iErr;
            _sMsj = logEmisor.sMsj;

            if (_iErr != 0)
                return;
            try
            {
                if (!_idImpuestoCliente.Equals(rutClienteXml))
                {
                    _iErr++;
                    _sMsj = "El remitente envió este archivo por error. El documento de referencia con RUT: " + rutClienteXml + " no está en GP. Este correo ha sido procesado. [CFDServicioDespachoRespuestas.GuardaRespuestaDelCliente()]";
                    logReceptor.Save(0, logEmisor.Sopnumbe, rutClienteXml, "-", DateTime.Now, uid, nomArchivoRecibido, 0, "-", mensajeResultado, "", "", uid, _conex.Usuario);
                }

                if (_iErr == 0)
                {
                    if (logEmisor.CicloDeVida.Transiciona(eventoCliente, _certificados.envia))
                    {
                        logEmisor.Save("Resultado del cliente. " + mensajeResultado, _conex.Usuario, xRespuesta.ToString(), uid);

                        logEmisor.Update(_conex.Usuario, Maquina.estadoBaseEmisor, Maquina.estadoBaseEmisor);

                        logReceptor.Save(0, logEmisor.Sopnumbe, rutClienteXml, "-", DateTime.Now, uid, nomArchivoRecibido, 0, "-", mensajeResultado, "", "", uid, _conex.Usuario);
                    }
                    _iErr = logEmisor.CicloDeVida.iErr;
                    _sMsj = logEmisor.CicloDeVida.sMsj;
                }

                if (_iErr < 0)
                {
                    bool yaRecorrido = logEmisor.CicloDeVida.transicionRecorrida(eventoCliente, _compoundedBinStatus);
                    if (yaRecorrido)
                    {
                        _sMsj = "Este correo ya fue procesado anteriormente. [CFDServicioDespachoRespuestas.GuardaRespuestaDelCliente()]";
                        logReceptor.Save(0, logEmisor.Sopnumbe, rutClienteXml, "-", DateTime.Now, uid, nomArchivoRecibido, 0, "-", mensajeResultado, "", "", uid, _conex.Usuario);
                    }
                    else
                        _sMsj += "\nVerifique el " + mensajeResultado + " Es probable que deba esperar a procesar este correo hasta que el documento tenga el estado correcto en GP.";
                }
            }
            catch (Exception pr)
            {
                _sMsj = "Excepción desconocida al guardar la respuesta del cliente. " + pr.Message + " [CFDServicioDespachoRespuestas.GuardaRespuestaDelCliente]";
                _sMsj += " " + logReceptor.SMsj + logEmisor.sMsj;
                _iErr++;
            }
        }

        private bool TraeDatosDocVentas(String folio, String tipo, String estadoBase)
        {
            try
            {
                _compoundedBinStatus = String.Empty;
                bool trae = false;
                vwCfdTransaccionesDeVenta logView = new vwCfdTransaccionesDeVenta(_conex.ConnStr);
                logView.Where.Folio.Value = folio;
                logView.Where.Folio.Operator = WhereParameter.Operand.Equal;
                logView.Where.Doctype.Conjuction = WhereParameter.Conj.And;
                logView.Where.Doctype.Value = tipo;
                logView.Where.Doctype.Operator = WhereParameter.Operand.Equal;
                logView.Where.Estado.Conjuction = WhereParameter.Conj.And;
                logView.Where.Estado.Value = estadoBase;
                logView.Where.Estado.Operator = WhereParameter.Operand.Equal;

                if (logView.Query.Load())
                {
                    trae = true;
                    logView.Rewind();

                    do
                    {
                        _compoundedBinStatus = logView.EstadoActual;
                        _idxSingleStatus = logView.IdxSingleStatus;
                        _idImpuestoCliente = logView.IdImpuestoCliente;
                        _sopnumbe = logView.Sopnumbe;
                        _sopType = logView.Soptype;
                        _tipoDoc = logView.Doctype;
                    } while (logView.MoveNext());
                }


                return trae;
            }
            catch (Exception ePla)
            {
                _sMsj = "Excepción al leer el log de facturas de venta filtrado por llave: folio, tipo, estado. " + ePla.Message + "[CFDServicioDespachoRespuestas.TraeDatosDocVentas]";
                throw new Exception(_sMsj);
            }
        }

        static public string Izquierda(string Texto, int Cuantos)
        {
            if (Texto.Length > Cuantos && Cuantos > 0)
                return Texto.Substring(0, Cuantos);
            else
                return Texto;
        }
    }
}
