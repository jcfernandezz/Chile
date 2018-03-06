using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.ComponentModel;
using System.Diagnostics;

using Comun;
using MaquinaDeEstados;
using Encriptador;
using System.Xml.Linq;
using cfd.FacturaElectronica;
using System.IO;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;

namespace FEChile
{
    public class CFDComprobanteFiscalDigitalFabrica
    {
        private Parametros _Param;
        private ConexionAFuenteDatos _Conex;
        private List<EnvioDteModel> _lContenedores;                     //Contiene los envíos
        private List<CFDComprobanteFiscalDigital> _lDocumentos;         //contiene los cfd
        private vwCfdCertificadosService _certificados;
        private TecnicaDeEncriptacion _encriptador;
        private EnvioDteModel _modeloEnvio;

        private Encoding _encoding;
        //private String sSource = "Factura Electrónica Chile";
        //private String sLog = "CFDComprobanteFiscalDigitalFabrica";

        public int iErr;
        public string sMsj;
        public delegate void LogHandler(int iAvance, string sMsj);
        //public delegate void bwProgreso(object sender, ProgressChangedEventArgs e);

        /// <summary>
        /// Dispara el evento para actualizar la barra de progreso
        /// </summary>
        /// <param name="iAvance"></param>
        /// <param name="sMsj"></param>
        public event LogHandler Progreso;
        public void OnProgreso(int iAvance, string sMsj)
        {
            if (Progreso != null)
                Progreso(iAvance, sMsj);

        }
        public void OnProgreso(int iAvance, string sMsj, String sEvent)
        {
            if (Progreso != null)
                Progreso(iAvance, sMsj);

            //usuario necesita permisos de administrador
            //if (!EventLog.SourceExists(sSource))
            //    EventLog.CreateEventSource(sSource, sLog);

            //EventLog.WriteEntry(sSource, sEvent);
        }

        public CFDComprobanteFiscalDigitalFabrica(ConexionAFuenteDatos Conex, Parametros Param)
        {
            try
            {
                sMsj = string.Empty;
                iErr = 0;
                _Param = Param;
                _Conex = Conex;

                _encoding = Encoding.GetEncoding(_Param.encoding);
                _lContenedores = new List<EnvioDteModel>();
                _lDocumentos = new List<CFDComprobanteFiscalDigital>();

                preparaCertificado();                                       //carga certificados y _encriptador

            }
            catch(Exception cf)
            {
                sMsj = "Excepción al inicializar fábrica de cfds. " + cf.Message + " [CFDComprobanteFiscalDigitalFabrica]";
                iErr++;
            }
        }
        //******************************************************
        #region Propiedades
        public vwCfdCertificadosService certificados
        {
            get { return _certificados; }
            set { _certificados = value; }
        }
        public List<EnvioDteModel> lContenedores
        {
            get { return _lContenedores; }
        }
        public Encoding encodig
        {
            get { return _encoding; }
        }
        public List<CFDComprobanteFiscalDigital> lDocumentos
        {
            get { return _lDocumentos; }
//            set { _lDocumentos = value; }
        }
        public TecnicaDeEncriptacion encriptador
        {
            get { return _encriptador; }
        }
        public EnvioDteModel ModeloEnvio
        {
            get { return _modeloEnvio; }
            set { _modeloEnvio = value; }
        }

        #endregion
        //*****************************************************
        private void preparaCertificado()
        {
            try
            {
                _certificados = new vwCfdCertificadosService(_Conex.ConnStr);
                _encriptador = new TecnicaDeEncriptacion();
                
                bool existeCert = _certificados.get(_Conex.Usuario);
                if (existeCert)
                {
                    _encriptador.PreparaEncriptacion("", _certificados.clave.Trim(), _certificados.Ruta_certificado.Trim(), "");
                    //_encriptador.PreparaEncriptacion("", _certificados.clave.Trim(), @"C:\GPUsuario\GPCfdi\feGettyChile\Certificados\pparra\Certificado_PriscillaParra.VenMay18.p12", "");
                    
                }
                else
                {
                    iErr = 50;
                    sMsj = "El usuario no tiene asignado un certificado de firma. Ingrese al Mantenimiento de certificados y agregue la ruta del certificado para " + _Conex.Usuario + " " + _certificados.sMsj;
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Elimina documentos que quedaron en estado emitido ya que no se puede reutilizar el xml de la bd. Se debe regenerar.
        /// </summary>
        private void Limpia()
        {
            try
            {
                sMsj = string.Empty;
                iErr = 0;
                var docs = _lDocumentos
                    .Where(y => y.cicloDeVida.binStatus.Equals(Maquina.binStatusBaseEmisor)); //revisar: debería iniciar una máquina y transicionar a emitido y comparar ese binStatus
                foreach (var cfd in docs)
                {
                    cfd.Limpia();
                }

            }
            catch (Exception errorGral)
            {
                sMsj = "Excepción encontrada al limpiar cfds. " + errorGral.Message + " [CFDComprobanteFiscalDigitalFabrica.Limpia] ";
                iErr++;
            }
        }

        /// <summary>
        /// Ensambla y firma una lista de envíos dte a partir de un lote de facturas. Un envío por factura.
        /// _lContenedores: Lista de envios Dte
        /// </summary>
        /// <param name="loteCfds">Lote de documentos a procesar</param>
        public void ensamblaFacturas(vwCfdTransaccionesDeVenta loteCfds)
        {
            try
            {
                OnProgreso(1, "Iniciando...");                              //Notifica al suscriptor

                loteCfds.Rewind();                                          //move to first record

                sMsj = string.Empty;
                iErr = 0;
                int iMaxErr = 0;
                CFDComprobanteFiscalDigital cfd;
                XmlDocument autorizacion = new XmlDocument();
                autorizacion.PreserveWhitespace = true;
                string rutaCertsAnterior = string.Empty;
                String docXmlLog = String.Empty;
                do
                {
                    try
                    {
                        //Obtiene la llave de timbrado por cada nuevo Id de documento de venta
                        if (iErr == 0 && _certificados.firma == 1 && !loteCfds.RutaCerts.Equals(rutaCertsAnterior))
                        {
                            autorizacion.Load(loteCfds.RutaCerts);
                            rutaCertsAnterior = loteCfds.RutaCerts;
                        }

                        cfd = preparaDte(loteCfds, autorizacion);
                        docXmlLog = cfd.modeloDte.xDocXml.ToString();
                        if (iErr == 0)
                        {
                            preparaContenedor(cfd);
                            docXmlLog = _modeloEnvio.xDocXml.ToString();
                        }

                    }
                    catch (Exception errFactura)
                    {
                        if (loteCfds.RutaCerts.Trim().Equals("_noexiste"))
                            sMsj = "El Id de la factura no está configurado en la tabla de folios o no tiene más folios para emitir. [CFDComprobanteFiscalDigitalFabrica.ensamblaFacturas]";
                        else
                        {
                            String noExisteTipo = string.Empty;
                            if (loteCfds.Doctype.Equals(string.Empty))
                                noExisteTipo = "No tiene asociado el tipo de documento de localización. ";
                            sMsj = noExisteTipo + errFactura.Message + " [CFDComprobanteFiscalDigitalFabrica.ensamblaFacturas] ";
                        }
                        iErr++;
                    }

                    OnProgreso(100 / loteCfds.RowCount, "Doc: " + loteCfds.Sopnumbe + " " + sMsj, docXmlLog);

                    if (iErr > 0) iMaxErr++;
                    sMsj = string.Empty;
                    iErr = 0;

                } while (loteCfds.MoveNext() && iMaxErr < 10);

                OnProgreso(100, _lContenedores.Count().ToString() + " comprobante(s) para enviar al SII. ");
                OnProgreso(100, "...");
            }
            catch (Exception errorGral)
            {
                sMsj = "Excepción desconocida al inicializar el ensamble de facturas. " + errorGral.Message + " [CFDComprobanteFiscalDigitalFabrica.ensamblaFacturas] ";
                iErr++;
                OnProgreso(0, sMsj);
            }
        }

        public void ActualizaCambioEstado(vwCfdTransaccionesDeVenta loteCfds, int evento)
        {
            try
            {
                sMsj = string.Empty;
                iErr = 0;

                if (_lDocumentos.Count() == 0)
                {
                    OnProgreso(0, "No hay documentos válidos para cambiar de estado.");
                    return;
                }

                OnProgreso(0, "Actualizando...");
                foreach (CFDComprobanteFiscalDigital cfd in _lDocumentos)
                {
                    cfd.Actualiza(String.Empty);
                    OnProgreso(0, "Doc: " + cfd.sopnumbe +" "+ cfd.sMsj);
                }
            }
            catch (Exception pc)
            {
                sMsj = "Excepción al actualizar cambios de estado. " + pc.Message + " [CFDComprobanteFiscalDigitalFabrica.ActualizaCambioEstado] ";
                iErr++;
            }
        }

        private CFDComprobanteFiscalDigital preparaDte(vwCfdTransaccionesDeVenta loteCfds, XmlDocument autorizacion)
        {
            iErr = 0;
            sMsj = String.Empty;
            CFDComprobanteFiscalDigital cfd = new CFDComprobanteFiscalDigital(_Conex, _Param, _encoding, loteCfds.EstadoActual, loteCfds.IdxSingleStatus, loteCfds.Voidstts,
                                      loteCfds.Sopnumbe, loteCfds.Soptype, loteCfds.IdImpuestoCliente, loteCfds.CUSTNMBR, loteCfds.NombreCliente, loteCfds.Doctype);
            cfd.modeloDte.AutorizacionXml = autorizacion;
            cfd.modeloDte.criptografo = _encriptador;
            cfd.fechaHora = loteCfds.Fechahora;
            iErr += cfd.iErr;
            sMsj = cfd.sMsj;

            if (iErr == 0)      
            {
                cfd.cicloDeVida.Transiciona(Maquina.eventoEnsamblaLote, _certificados.firma);
                iErr = cfd.cicloDeVida.iErr;
                sMsj = cfd.cicloDeVida.sMsj;
            }

            if (iErr == 0)
            {
                cfd.ensamblaCfd(loteCfds);
                iErr = cfd.iErr;
                sMsj = cfd.sMsj;
            }

            if (iErr == 0)
            {
                //genera el código de barras y guarda el archivo jpg
                cfd.GuardaCodigoBarras();
                iErr = cfd.iErr;
                sMsj = cfd.sMsj;
            }
            if (iErr == 0)
            {
                //registra log de la emisión de factura antes de la impresión
                cfd.Guarda();
                iErr = cfd.iErr;
                sMsj = cfd.sMsj;
            }

            if (iErr == 0)
            {
                cfd.GuardaPdf("B");                                     //B: Original y copia en un solo pdf
                iErr = cfd.iErr;
                sMsj = cfd.sMsj;

                if (sMsj.Contains("Crystal"))                           //no está instalado crystal reports. Se puede continuar pero luego de instalar crystal se debe generar los pdfs
                {
                    iErr = 0;
                    sMsj = "Advertencia. No se ha generado el pdf. " + sMsj;
                }
            }

            return cfd;
        }

        private void preparaContenedor(CFDComprobanteFiscalDigital cfd)
        {
            cfd.cicloDeVida.Transiciona(Maquina.eventoEnviaAlSII, _certificados.envia);
            iErr = cfd.cicloDeVida.iErr;
            sMsj = cfd.cicloDeVida.sMsj;

            if (iErr == 0)
            {
                ensamblaEnvio(cfd, cfd.modeloDte.dteDoc.Encabezado.IdDoc.TipoDTE.ToString());   //crea un nuevo contenedor

                agregaDte(cfd);

                firmaEnvio();

                if (_modeloEnvio.iErr == 0)
                    _lContenedores.Add(_modeloEnvio);

                iErr = _modeloEnvio.iErr;
                sMsj = _modeloEnvio.sMsj;
            }

        }

        private void ensamblaEnvio(CFDComprobanteFiscalDigital cfd, string tipoDoc)
        {
            try
            {
                _modeloEnvio = new EnvioDteModel(cfd.modeloDte, _certificados, cfd.rutaXml, _encoding, cfd.sopnumbe);

                _modeloEnvio.criptografo = _encriptador;

                _modeloEnvio.prepara(_lDocumentos, tipoDoc);

                _modeloEnvio.Serializa();

                _modeloEnvio.Canonicaliza(_modeloEnvio.xDocXml);

                reAjustaAtributos();

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Agrega nodo DTE firmado
        /// </summary>
        /// <param name="cfd"></param>
        private void agregaDte(CFDComprobanteFiscalDigital cfd)
        {
            XmlNamespaceManager ns = new XmlNamespaceManager(_modeloEnvio.xDocXml.NameTable);
            ns.AddNamespace("x", _modeloEnvio.xDocXml.DocumentElement.NamespaceURI);
            XmlNode nodoEnvio = _modeloEnvio.xDocXml.SelectSingleNode("//x:EnvioDTE/x:SetDTE", ns);
            nodoEnvio.AppendChild(nodoEnvio.OwnerDocument.ImportNode(
                                            cfd.modeloDte.xDocXml.DocumentElement, true));

            //_modeloEnvio.xDocXml.DocumentElement.AppendChild(_modeloEnvio.xDocXml.ImportNode(cfd.modeloDte.xDocXml.DocumentElement, true));
            String sEnvio = _modeloEnvio.xDocXml.OuterXml.Replace("</DTE></SetDTE>", "</DTE>\n</SetDTE>");
            sEnvio = sEnvio.Replace(" xmlns=\"\"", "");
            _modeloEnvio.xDocXml.LoadXml(sEnvio);

            _modeloEnvio.lDocumentos.Add(cfd);
        }

        public void firmaEnvio()
        {

            _modeloEnvio.firma(_modeloEnvio.setId, "//EnvioDTE");                   //firma el contenedor anterior

            if (_modeloEnvio.iErr == 0)
                _modeloEnvio.VerificaFirma();

            if (_modeloEnvio.iErr == 0)
                _modeloEnvio.validaXsd(_Param.URLArchivoEnvioXSD);    //agrega contenedor anterior a la lista de contenedores
        }

        public void reAjustaAtributos()
        {
            _modeloEnvio.xDocXml.SelectSingleNode("//EnvioDTE");
            _modeloEnvio.xDocXml.DocumentElement.RemoveAllAttributes();
            _modeloEnvio.xDocXml.DocumentElement.SetAttribute("xmlns", "http://www.sii.cl/SiiDte");
            _modeloEnvio.xDocXml.DocumentElement.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            XmlDocument atr = new XmlDocument();
            XmlAttribute schemaLocation = atr.CreateAttribute("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
            schemaLocation.Value = "http://www.sii.cl/SiiDte EnvioDTE_v10.xsd";
            _modeloEnvio.xDocXml.DocumentElement.SetAttributeNode(schemaLocation);
            _modeloEnvio.xDocXml.DocumentElement.SetAttribute("version", "1.0");

        }

        /// <summary>
        /// Ensambla y firma una lista de documentos dte a partir de un lote de facturas.
        /// _lDocumentos: Lista de dte's
        /// </summary>
        /// <param name="loteCfds">Lote de documentos a procesar</param>
        public void ensamblaLote(vwCfdTransaccionesDeVenta loteCfds)
        {
            try
            {
                OnProgreso(1, "Iniciando...");                              //Notifica al suscriptor

                loteCfds.Rewind();                                          //move to first record

                sMsj = string.Empty;
                iErr = 0;
                int iMaxErr =0;
                CFDComprobanteFiscalDigital cfd;
                XmlDocument autorizacion = new XmlDocument();
                autorizacion.PreserveWhitespace = true;
                string rutaCertsAnterior = string.Empty;
                _lDocumentos.Clear();
                
                do
                {
                    //Obtiene la llave de timbrado por cada nuevo Id de documento de venta
                    if (iErr == 0 && _certificados.firma == 1 && !loteCfds.RutaCerts.Equals(rutaCertsAnterior))
                    {
                        autorizacion.Load(loteCfds.RutaCerts);
                        rutaCertsAnterior = loteCfds.RutaCerts;
                    }
                    cfd = preparaDte(loteCfds, autorizacion);

                    if (iErr == 0)
                        _lDocumentos.Add(cfd);

                    OnProgreso(100 / loteCfds.RowCount, "Doc: " + loteCfds.Sopnumbe + " " + sMsj, cfd.modeloDte.xDocXml.ToString());

                    if (iErr > 0) iMaxErr++;
                    sMsj = string.Empty;
                    iErr = 0;
                } while (loteCfds.MoveNext() && iMaxErr < 10);

                OnProgreso(100, _lDocumentos.Count().ToString() + " comprobante(s) para enviar al SII. ");
            }
            catch (Exception errorGral)
            {
                sMsj = "Excepción encontrada al ensamblar lote. " + errorGral.Message + " [CFDComprobanteFiscalDigitalFabrica.ensamblaLote] ";
                iErr++;
                OnProgreso(0, sMsj);
            }
        }

        /// <summary>
        /// Carga en una lista los dte que ha marcado el usuario. No incluye el xml, sólo la ruta donde está guardado.
        /// </summary>
        /// <param name="loteCfds">Lote de documentos a procesar</param>
        public void cargaLote(vwCfdTransaccionesDeVenta loteCfds, int evento)
        {
            try
            {
                OnProgreso(1, "Preparando comprobantes...");        //Notifica al suscriptor

                loteCfds.Rewind();                                  //move to first record

                sMsj = string.Empty;
                iErr = 0;
                int iMaxErr = 0;
                //string docIdAnterior = string.Empty;
                CFDComprobanteFiscalDigital cfd;
                _lDocumentos.Clear();
                do
                {
                    cfd = new CFDComprobanteFiscalDigital(_Conex, _Param, _encoding, loteCfds.EstadoActual, loteCfds.IdxSingleStatus, loteCfds.Voidstts,
                                                          loteCfds.Sopnumbe, loteCfds.Soptype, loteCfds.IdImpuestoCliente, loteCfds.CUSTNMBR, loteCfds.NombreCliente, loteCfds.Doctype);
                    cfd.rutaXml = loteCfds.RutaXml;
                    cfd.mensaje = loteCfds.Mensaje;
                    iErr += cfd.iErr;

                    if (iErr == 0)
                    {
                        cfd.cicloDeVida.Transiciona(evento, _certificados.envia);
                        iErr = cfd.cicloDeVida.iErr;
                        sMsj = cfd.cicloDeVida.sMsj;
                    }

                    if (iErr == 0)
                        _lDocumentos.Add(cfd);

                    OnProgreso(100 / loteCfds.RowCount, "Doc: " + loteCfds.Sopnumbe + " " + cfd.sMsj.Trim() + sMsj, cfd.modeloDte.xDocXml.ToString());

                    if (iErr > 0) iMaxErr++;
                    sMsj = string.Empty;
                    iErr = 0;
                } while (loteCfds.MoveNext() && iMaxErr < 10);
                OnProgreso(100, _lDocumentos.Count().ToString() + " comprobante(s) preparado(s).");

            }
            catch (Exception errorGral)
            {
                sMsj = "Excepción encontrada al cargar lote de comprobantes. " + errorGral.Message + " [CFDComprobanteFiscalDigitalFabrica.cargaLote] ";
                iErr++;
                OnProgreso(0, sMsj);
            }
        }

        /// <summary>
        /// Prepara un contenedor con varios documentos dte. Utilizado para certificación.
        /// Requisitos: 
        ///     ensamblaLote()
        /// _lDocumentos: lista de documentos dte
        /// _lContenedores: lista de envíos.
        /// </summary>
        public void preparaUnContenedor()
        {
            try
            {
                sMsj = string.Empty;
                iErr = 0;
                OnProgreso(0, sMsj);
                if (_lDocumentos.Count() == 0)
                {
                    OnProgreso(0, "No hay documentos válidos para enviar al SII.");
                    return;
                }
                string clienteAnterior = string.Empty;
                var docsOrdenadosPorCliente = _lDocumentos.OrderBy(o => o.fechaHora);

                OnProgreso(0, "Preparando contenedores...");
                foreach (var cfd in docsOrdenadosPorCliente)
                {
                    //Evento 2: envía al SII
                    if (cfd.cicloDeVida.Transiciona(Maquina.eventoEnviaAlSII, _certificados.envia))
                    {
                        if (clienteAnterior.Equals(String.Empty))
                        {
                            ensamblaEnvio(cfd, String.Empty);             //crea un nuevo contenedor
                            clienteAnterior = cfd.modeloDte.idPersonaRecibe;
                        }

                        agregaDte(cfd);
                    }
                    else
                    {
                        iErr = cfd.cicloDeVida.iErr;
                        sMsj = cfd.cicloDeVida.sMsj;
                    }
                }

                if (_lDocumentos.Count > 0 && iErr == 0)
                {
                    _modeloEnvio.firma(_modeloEnvio.setId, "//EnvioDTE");                   //firma el contenedor anterior

                    if (_modeloEnvio.iErr == 0)
                        _lContenedores.Add(_modeloEnvio);
                    else
                        OnProgreso(0, "Excepción al validar el envío: " + _modeloEnvio.setId + " " + _modeloEnvio.sMsj);
                }
                OnProgreso(0, "Contenedores preparados para envío");

            }
            catch (Exception pc)
            {
                sMsj = "Excepción al preparar los envíos. " + pc.Message + " [CFDComprobanteFiscalDigitalFabrica.preparaContenedoresPorCliente] ";
                iErr++;
                OnProgreso(0, sMsj);
            }
        }

        /// <summary>
        /// Prepara una lista de envíos. Cada envío es llamado un contenedor y corresponde a un cliente. Cada contenedor puede tener varios documentos dte
        /// Requisitos: 
        ///     ensamblaLote()
        /// _lDocumentos: lista de documentos dte
        /// _lContenedores: lista de envíos.
        /// </summary>
        public void preparaContenedoresPorReceptor()
        {
            try
            {
                sMsj = string.Empty;
                iErr = 0;
                OnProgreso(0, sMsj);
                if (_lDocumentos.Count() == 0)
                {
                    OnProgreso(0, "No hay documentos válidos para enviar al SII.");
                    return;
                }
                string clienteAnterior = string.Empty;
                var docsOrdenadosPorCliente = _lDocumentos.OrderBy(o => o.modeloDte.idPersonaRecibe);

                OnProgreso(0, "Preparando contenedores...");
                foreach (var cfd in docsOrdenadosPorCliente)
                {
                    //Evento 2: envía al SII
                    if (cfd.cicloDeVida.Transiciona(Maquina.eventoEnviaAlSII, _certificados.envia))
                    {
                        if (clienteAnterior.Equals(String.Empty))
                        {
                            ensamblaEnvio(cfd, String.Empty);             //crea un nuevo contenedor
                            clienteAnterior = cfd.modeloDte.idPersonaRecibe;
                        }

                        if (cfd.modeloDte.idPersonaRecibe != clienteAnterior)
                        {
                            _modeloEnvio.firma(_modeloEnvio.setId, "//EnvioDTE");           //firma el contenedor anterior

                            if (_modeloEnvio.iErr == 0)
                                _lContenedores.Add(_modeloEnvio);         //agrega contenedor anterior a la lista de contenedores
                            else
                                OnProgreso(0, "Excepción al validar el envío: " + _modeloEnvio.setId + " " + _modeloEnvio.sMsj, _modeloEnvio.xDocXml.ToString());

                            ensamblaEnvio(cfd, String.Empty);             //crea un nuevo contenedor
                        }

                        agregaDte(cfd);

                    }
                    else
                    {
                        iErr = cfd.cicloDeVida.iErr;
                        sMsj = cfd.cicloDeVida.sMsj;
                    }
                    clienteAnterior = cfd.modeloDte.idPersonaRecibe;
                }

                if (_lDocumentos.Count > 0 && iErr == 0)
                {
                    _modeloEnvio.firma(_modeloEnvio.setId, "//EnvioDTE");                   //firma el contenedor anterior

                    if (_modeloEnvio.iErr == 0)
                        _lContenedores.Add(_modeloEnvio);
                    else
                        OnProgreso(0, "Excepción al validar el envío: " + _modeloEnvio.setId + " " + _modeloEnvio.sMsj);
                }
                OnProgreso(0, "Contenedores preparados para envío");

            }
            catch (Exception pc)
            {
                sMsj = "Excepción al preparar los envíos. " + pc.Message + " [CFDComprobanteFiscalDigitalFabrica.preparaContenedoresPorCliente] ";
                iErr++;
                OnProgreso(0, sMsj);
            }
        }

        /// <summary>
        /// Prepara una lista de envíos. Cada envío es llamado un contenedor y corresponde a un documento dte. 
        /// Requisitos: 
        ///     ensamblaLote()
        /// _lDocumentos: lista de documentos dte
        /// _lContenedores: lista de envíos.
        /// </summary>
        public void preparaContenedoresPorFactura()
        {
            try
            {
                sMsj = string.Empty;
                iErr = 0;
                OnProgreso(0, sMsj);
                if (_lDocumentos.Count() == 0)
                {
                    OnProgreso(0, "No hay documentos válidos para enviar al SII.");
                    return;
                }
                //string clienteAnterior = string.Empty;
                var docsOrdenadosPorCliente = _lDocumentos.OrderBy(o => o.modeloDte.dteDoc.Encabezado.Receptor.RUTRecep);
                OnProgreso(0, "Preparando contenedores...");
                foreach (var cfd in docsOrdenadosPorCliente)
                {
                    //Evento 2: envía al SII
                    if (cfd.cicloDeVida.Transiciona(Maquina.eventoEnviaAlSII, _certificados.envia))
                    {
                        ensamblaEnvio(cfd, cfd.modeloDte.dteDoc.Encabezado.IdDoc.TipoDTE.ToString());   //crea un nuevo contenedor

                        agregaDte(cfd); 

                        //cfd.setId = _modeloEnvio.setId;                                                 //set que contiene la factura

                        firmaEnvio();

                        if (_modeloEnvio.iErr == 0)
                            _lContenedores.Add(_modeloEnvio);
                        else
                            OnProgreso(0, "Excepción al validar el envío: " + _modeloEnvio.setId + " " + _modeloEnvio.sMsj, _modeloEnvio.xDocXml.ToString());

                    }
                    else
                    {
                        iErr = cfd.cicloDeVida.iErr;
                        sMsj = cfd.cicloDeVida.sMsj;
                    }
                }

                OnProgreso(0, "Contenedores preparados para envío");
            }
            catch (Exception pc)
            {
                sMsj = "Excepción al preparar los envíos. " + pc.Message + " [CFDComprobanteFiscalDigitalFabrica.preparaContenedoresPorFactura] ";
                iErr++;
                OnProgreso(0, sMsj);
            }
        }

    }
}
