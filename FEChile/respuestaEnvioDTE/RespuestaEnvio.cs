using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;

using cfd.FacturaElectronica;
using EstructuraMensajeEMail;
using Encriptador;
using MaquinaDeEstados;

namespace respuestaEnvioDTE
{
    public class RespuestaEnvio
    {
        private XNamespace nsDte = "http://www.sii.cl/SiiDte";
        private XNamespace nsSignature = "http://www.w3.org/2000/09/xmldsig#";

        private string _sMsj;
        private int _iErr;
        private RespuestaDTE _respuestaEnvio;
        private RespuestaDTEResultadoRecepcionEnvio _recepcion;
        private RespuestaDTEResultadoRecepcionEnvioRecepcionDTE _recepcionDetalle;
        //private RespuestaDTEResultado _resultado;
        private XmlDocument _xDocXml;
        private Encoding _encoding;
        private int _tieneAcceso;

        private string _rutResponde;    //rut que genera la respuesta
        private string _folio = string.Empty;
        private string _idResultado;

        private string _rutRecibe;      //rut de quien recibe la respuesta
        private string _archivoRecibido;
        private string _connStr;
        private DateTime ts;
        private DateTime _fechaRecepcion;
        private string _EnvioDTEID;
        private string _digest;
        private short _EstadoRecepEnv = 0;
        private string _tipoDTE;
        private DateTime _fchEmis;
        private string _rutEmisor;

        private string _rutReceptor;
        private string _sMntTotal;
        private string _estadoRecepDTE = "0";

        //**********************************************************************************
        #region Propiedades
        private int _evento;

        public int Evento
        {
            get { return _evento; }
            set { _evento = value; }
        }

        private String _usuario;

        public String Usuario
        {
            get { return _usuario; }
            set { _usuario = value; }
        }

        private String _uid;
        public String Uid
        {
            get { return _uid; }
            set { _uid = value; }
        }
        private string _nomEmisor;
        public string NomEmisor
        {
            get { return _nomEmisor; }
            set { _nomEmisor = value; }
        }

        public XmlDocument xDocXml
        {
            get { return _xDocXml; }
            set { _xDocXml = value; }
        }
        private XDocument _xDocProveedor;

        public XDocument XDocProveedor
        {
            get { return _xDocProveedor; }
            set { _xDocProveedor = value; }
        }

        public string EstadoRecepDTE
        {
            get { return _estadoRecepDTE; }
            set { _estadoRecepDTE = value; }
        }
        public int IErr
        {
            get { return _iErr; }
        }
        public string SMsj
        {
            get { return _sMsj; }
        }
        public string RutResponde
        {
            get { return _rutResponde; }
            set { _rutResponde = value; }
        }
        public string RutRecibe
        {
            get { return _rutRecibe; }
            set
            {
                //MatchCollection matchCollection = Regex.Matches(value, "[0-9]+-([0-9]|K)");
                //if (matchCollection.Count == 0)
                //    _iErr++;

                _rutRecibe = value; }
        }
        public string archivoRecibido
        {
            get { return _archivoRecibido; }
            set { _archivoRecibido = value; }
        }
        public DateTime fechaRecepcion
        {
            get { return _fechaRecepcion; }
            set { _fechaRecepcion = value; }
        }
        public string EnvioDTEID
        {
            get { return _EnvioDTEID; }
            set { _EnvioDTEID = value; }
        }
        public string digest
        {
            get { return _digest; }
            set { _digest = value; }
        }
        public short EstadoRecepEnv
        {
            get { return _EstadoRecepEnv; }
            set { _EstadoRecepEnv = value; }
        }
        public string tipoDTE
        {
            get { return _tipoDTE; }
            set { _tipoDTE = value; }
        }

        public string Folio
        {
            get { return _folio; }
            set { _folio = value;            }
        }
        public string idResultado
        {
            get { return _idResultado; }
            set { _idResultado = value; }
        }
        public DateTime fchEmis
        {
            get { return _fchEmis; }
            set { _fchEmis = value; }
        }
        public string rutEmisor
        {
            get { return _rutEmisor; }
            set { _rutEmisor = value; }
        }
        public string rutReceptor
        {
            get { return _rutReceptor; }
            set { _rutReceptor = value; }
        }
        public string sMntTotal
        {
            get { return _sMntTotal; }
            set { _sMntTotal = value; }
        }

        private TecnicaDeEncriptacion _criptografo;
        public TecnicaDeEncriptacion criptografo
        {
            get { return _criptografo; }
            set { _criptografo = value; }
        }

        private XElement _xDoc;
        public XElement XDoc
        {
            get { return _xDoc; }
            set { _xDoc = value; }
        }

        private String _eSingleStatus;
        public String ESingleStatus
        {
            get { return _eSingleStatus; }
            set { _eSingleStatus = value; }
        }
        private String _eBinStatus;
        public String EBinStatus
        {
            get { return _eBinStatus; }
            set { _eBinStatus = value; }
        }
        private String _eMensaje;
        public String EMensaje
        {
            get { return _eMensaje; }
            set { _eMensaje = value; }
        }
        private Maquina _cicloDeVida;
        public Maquina CicloDeVida
        {
            get { return _cicloDeVida; }
            set { _cicloDeVida = value; }
        }
        public String IdResultado
        {
            get { return "RESULTADO"; }
        }
        private String _emailProveedor;

        public String EmailProveedor
        {
            get { return _emailProveedor; }
            set { _emailProveedor = value; }
        }
        private String _rutaXml;

        public String RutaXml
        {
            get { return _rutaXml; }
            set { _rutaXml = value; }
        }
        private String _prefijo;

        public String Prefijo
        {
            get { return _prefijo; }
            set { _prefijo = value; }
        }
        public String RutaYNomArchivo
        {
            get { return _rutaXml + _prefijo + _rutEmisor + "_" + _tipoDTE + "_" + _folio; }
        }
        private String _tipoRespuestaResultado;

        public String TipoRespuestaResultado
        {
            get { return _tipoRespuestaResultado; }
            set { _tipoRespuestaResultado = value; }
        }


        #endregion
        //**********************************************************************************

        public delegate void reportaProgreso(int iAvance, string sMsj);
        public event reportaProgreso Progreso;
        public void MuestraAvance(int iAvance, string sMsj)
        {
            if (Progreso != null)
                Progreso(iAvance, sMsj);
        }

        public RespuestaEnvio(string connStr, Encoding encoding, String usuario, int tieneAcceso)
        {
            _connStr = connStr;
            ts = DateTime.Now;
            ts = new DateTime(ts.Year, ts.Month, ts.Day, ts.Hour, ts.Minute, ts.Second);
            _xDocXml = new XmlDocument();
            _xDocXml.PreserveWhitespace = true;
            _encoding = encoding;
            _tieneAcceso = tieneAcceso;
            _usuario = usuario;
        }

        /// <summary>
        /// Arma el objeto RespuestaDTE
        /// </summary>
        /// <param name="max">número máximo de dtes para la respuesta</param>
        public void EnsamblaRecepcionCab(short max)
        {
            _tipoRespuestaResultado = "RESPUESTA";

            _respuestaEnvio = new RespuestaDTE();
            _respuestaEnvio.Resultado = new RespuestaDTEResultado();
            _respuestaEnvio.Resultado.Caratula = new RespuestaDTEResultadoCaratula();
            _respuestaEnvio.Resultado.Items = new object[1];

            _recepcion = new RespuestaDTEResultadoRecepcionEnvio();
            _recepcion.RecepcionDTE = new RespuestaDTEResultadoRecepcionEnvioRecepcionDTE[max];
            //_caratula = new RespuestaDTEResultadoCaratula();

            _respuestaEnvio.Resultado.ID = _idResultado;
            _respuestaEnvio.Resultado.Caratula.RutResponde = _rutReceptor;        //empresa que responde: DE
            _respuestaEnvio.Resultado.Caratula.RutRecibe = _rutEmisor;            //empresa que recibe: PARA
            _respuestaEnvio.Resultado.Caratula.IdRespuesta = "1";                 //obtener correlativo!
            _respuestaEnvio.Resultado.Caratula.NroDetalles = max.ToString();
            _respuestaEnvio.Resultado.Caratula.TmstFirmaResp = ts;

            _recepcion.NmbEnvio = _archivoRecibido;
            _recepcion.FchRecep = _fechaRecepcion;
            _recepcion.CodEnvio = "1";                                            //obtener correlativo!
            _recepcion.EnvioDTEID = _EnvioDTEID;

            if (_EstadoRecepEnv == 0)
            {
                _recepcion.EstadoRecepEnv = RespuestaDTEResultadoRecepcionEnvioEstadoRecepEnv.Item0;    //recibido conforme
                _recepcion.RecepEnvGlosa = "Envío recibido conforme";
            }
            else
            {
                _recepcion.EstadoRecepEnv = RespuestaDTEResultadoRecepcionEnvioEstadoRecepEnv.Item99;   //rechazado - otros
                _recepcion.RecepEnvGlosa = "Envío rechazado - otros";
            }
        }

        /// <summary>
        /// Forma el detalle de la recepción del dte
        /// </summary>
        /// <param name="i">índice del dte a responder</param>
        public void EnsamblaRecepcionDet(int i)
        {
            _recepcionDetalle = new RespuestaDTEResultadoRecepcionEnvioRecepcionDTE();

            _recepcionDetalle.TipoDTE = (DTEType)Enum.Parse(typeof(DTEType), "Item" + _tipoDTE);
            _recepcionDetalle.Folio = _folio;
            _recepcionDetalle.FchEmis = _fchEmis;
            _recepcionDetalle.RUTEmisor = _rutEmisor;   //empresa que emitió el dte
            _recepcionDetalle.RUTRecep = _rutReceptor;  //empresa que recibió el dte
            _recepcionDetalle.MntTotal = _sMntTotal;

            _recepcionDetalle.EstadoRecepDTE = (RespuestaDTEResultadoRecepcionEnvioRecepcionDTEEstadoRecepDTE)Enum.Parse(typeof(RespuestaDTEResultadoRecepcionEnvioRecepcionDTEEstadoRecepDTE), "Item" + _estadoRecepDTE);
            
            if (_estadoRecepDTE == "0")
                _recepcionDetalle.RecepDTEGlosa = "DTE Recibido OK";
            else
                _recepcionDetalle.RecepDTEGlosa = "DTE No Recibido - Otros";

            _recepcion.RecepcionDTE[i] = _recepcionDetalle;

        }

        public void EnsamblaRecepcionPie()
        {
            _respuestaEnvio.Resultado.Items[0] = _recepcion;

        }

        /// <summary>
        /// Forma el resultado aceptado o rechazado
        /// Requisito. Las propiedades tienen que haber sido asignadas.
        /// </summary>
        /// <param name="xRecepcionFactura"></param>
        /// <param name="resultado">true:aceptado, false:rechazado</param>
        public void EnsamblaResultado(String xRecepcionFactura, Boolean resultado, String motivoRechazo)
        {
            _iErr = 0;
            _sMsj = String.Empty;
            List<XElement> lDte = new List<XElement>();
            _tipoRespuestaResultado = "RESULTADO";

            try
            {
                _xDoc = XElement.Parse(xRecepcionFactura, LoadOptions.PreserveWhitespace);

                _xDoc.Elements(nsSignature + "Signature").Remove();
                _xDoc.Elements().Elements(nsDte + "RecepcionEnvio").Elements(nsDte + "RecepcionDTE").Elements(nsDte + "EstadoRecepDTE").Remove();
                _xDoc.Elements().Elements(nsDte + "RecepcionEnvio").Elements(nsDte + "RecepcionDTE").Elements(nsDte + "RecepDTEGlosa").Remove();
                //inicio intercambio certificación
                //int i = 0;                      
                //fin intercambio certificación
                foreach (XElement r in _xDoc.Elements(nsDte + "Resultado").Elements(nsDte + "RecepcionEnvio").Elements())
                {
                    if (r.Name.Equals(nsDte + "RecepcionDTE"))
                    {
                        r.Name = nsDte + "ResultadoDTE";
                        r.Add(new XElement(nsDte + "CodEnvio", "1"));   //revisar el código a ingresar
                        //inicio intercambio certificación
                        //if (i == 0)
                        //fin intercambio certificación
                        if (resultado)
                        {
                            r.Add(new XElement(nsDte + "EstadoDTE", "0"));
                            r.Add(new XElement(nsDte + "EstadoDTEGlosa", "ACEPTADO OK"));
                        }
                        else
                        {
                            r.Add(new XElement(nsDte + "EstadoDTE", "2"));
                            r.Add(new XElement(nsDte + "EstadoDTEGlosa", "RECHAZADO" + motivoRechazo));
                        }
                        //inicio intercambio certificación
                        //i++;
                        //fin intercambio certificación
                        lDte.Add(r);
                    }
                }

                _xDoc.Elements().Elements(nsDte + "RecepcionEnvio").Remove();

                foreach (XElement e in lDte)
                {
                    _xDoc.Element(nsDte + "Resultado").Add(e);  
                }
                _xDoc.Element(nsDte + "Resultado").Attribute("ID").Value = IdResultado;
                _xDocXml.LoadXml(_xDoc.ToString(SaveOptions.DisableFormatting));
            }
            catch (Exception er)
            {
                _sMsj = "Excepción al formar el resultado. " + er.Message + " [RespuestaEnvio.EnsamblaResultado]";
                _iErr++;
            }
        }

        ///// <summary>
        ///// Arma el objeto RespuestaDTE
        ///// Requisito. Arma una respuesta por DTE
        ///// </summary>
        //public void EnsamblaRecepcion()
        //{
        //    _respuestaEnvio = new RespuestaDTE();
        //    _respuestaEnvio.Resultado = new RespuestaDTEResultado();
        //    _respuestaEnvio.Resultado.Caratula = new RespuestaDTEResultadoCaratula();
        //    _respuestaEnvio.Resultado.Items = new object[1];
            
        //    _recepcion = new RespuestaDTEResultadoRecepcionEnvio();
        //    _recepcion.RecepcionDTE = new RespuestaDTEResultadoRecepcionEnvioRecepcionDTE[1];
        //    //_caratula = new RespuestaDTEResultadoCaratula();
        //    _recepcionDetalle = new RespuestaDTEResultadoRecepcionEnvioRecepcionDTE();

        //    _respuestaEnvio.Resultado.Caratula.RutResponde = _rutResponde;       //persona que responde: DE
        //    _respuestaEnvio.Resultado.Caratula.RutRecibe = _rutRecibe;           //persona que recibe: PARA
        //    _respuestaEnvio.Resultado.Caratula.IdRespuesta = "0";                //obtener correlativo!
        //    _respuestaEnvio.Resultado.Caratula.NroDetalles = "1";
        //    _respuestaEnvio.Resultado.Caratula.TmstFirmaResp = ts;

        //    _recepcion.NmbEnvio = _archivoRecibido;
        //    _recepcion.FchRecep = _fechaRecepcion;
        //    _recepcion.CodEnvio = "0";                  //obtener correlativo!
        //    _recepcion.EnvioDTEID = _EnvioDTEID;
        //    //_recepcion.Digest = Convert.FromBase64String(_digest);
        //    if (_EstadoRecepEnv == 0)
        //    {
        //        _recepcion.EstadoRecepEnv = RespuestaDTEResultadoRecepcionEnvioEstadoRecepEnv.Item0;    //recibido conforme
        //        _recepcion.RecepEnvGlosa = "Envío recibido conforme";
        //    }
        //    else
        //    {
        //        _recepcion.EstadoRecepEnv = RespuestaDTEResultadoRecepcionEnvioEstadoRecepEnv.Item99;   //rechazado - otros
        //        _recepcion.RecepEnvGlosa = "Envío rechazado - otros";
        //    }
        //    //_recepcion.NroDTE = "1";

        //    _recepcionDetalle.TipoDTE = (DTEType)Enum.Parse(typeof(DTEType), "Item"+_tipoDTE);
        //    _recepcionDetalle.Folio = _folio;
        //    _recepcionDetalle.FchEmis = _fchEmis;
        //    _recepcionDetalle.RUTEmisor = _rutEmisor;   //empresa que emitió el doc
        //    _recepcionDetalle.RUTRecep = _rutReceptor;  //empresa que recibió el doc
        //    _recepcionDetalle.MntTotal = _sMntTotal;
        //    _recepcionDetalle.EstadoRecepDTE = (RespuestaDTEResultadoRecepcionEnvioRecepcionDTEEstadoRecepDTE)Enum.Parse(typeof(RespuestaDTEResultadoRecepcionEnvioRecepcionDTEEstadoRecepDTE), "Item" + _estadoRecepDTE);
        //    if (_estadoRecepDTE == "0")
        //        _recepcionDetalle.RecepDTEGlosa = "DTE Recibido OK";
        //    else
        //        _recepcionDetalle.RecepDTEGlosa = "DTE No Recibido - Otros";

        //    _recepcion.RecepcionDTE[0] = _recepcionDetalle;
        //    _respuestaEnvio.Resultado.Items[0] = _recepcion;

        //}

        /// <summary>
        /// Prepara y serializa el objeto en el string _sDocXml
        /// </summary>
        /// <param name="objeto">Objeto de cualquier tipo</param>
        public void Serializa(Encoding encoding)
        {
            try
            {
                string _sDocXml = string.Empty;
                XmlSerializer serializer = new XmlSerializer(_respuestaEnvio.GetType());
                _sDocXml = SerializaObjeto(serializer, encoding, _respuestaEnvio, true);

                _xDocXml.LoadXml(RemoveAllXmlNamespace(_sDocXml));
            }
            catch (Exception so)
            {
                _sMsj = "Error al serializar el documento. " + so.Message + " [RespuestaEnvio.Serializa] " + so.StackTrace;
                _iErr++;
            }
        }

        public void Canonicaliza()
        {
            try
            {
                _sMsj = string.Empty;
                _iErr = 0;

                String sDocXml = _xDocXml.OuterXml.Replace("><", ">\n<");   //agrega line breaks
                //String sResp = sDocXml.Replace("</Resultado></RespuestaDTE>", "</Resultado>\n</RespuestaDTE>");

                _xDocXml.LoadXml(sDocXml);

            }
            catch (Exception cn)
            {
                _iErr++;
                _sMsj = cn.Message + " [RespuestaEnvio.Canonicaliza]";
                throw;
            }
        }

        public void reAjustaAtributos()
        {
            _xDocXml.SelectSingleNode("//RespuestaDTE");
            _xDocXml.DocumentElement.RemoveAllAttributes();
            _xDocXml.DocumentElement.SetAttribute("xmlns", "http://www.sii.cl/SiiDte");
            _xDocXml.DocumentElement.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            XmlDocument atr = new XmlDocument();
            XmlAttribute schemaLocation = atr.CreateAttribute("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
            schemaLocation.Value = "http://www.sii.cl/RespuestaEnvioDTE_v10.xsd";
            _xDocXml.DocumentElement.SetAttributeNode(schemaLocation);
            _xDocXml.DocumentElement.SetAttribute("version", "1.0");

        }

        public void firma(string sUri)
        {
            try
            {
                _sMsj = string.Empty;
                _iErr = 0;

                SignedXml signedXml = new SignedXml(_xDocXml);

                // Agrega la clave privada al objeto signedXml
                signedXml.SigningKey = _criptografo.certificado.PrivateKey;

                // Recupera el objeto signature desde signedXml
                Signature XMLSignature = signedXml.Signature;

                // Crea la referencia al documento DTE. Formato: '#reference'
                Reference reference = new Reference();
                reference.Uri = "#" + sUri;

                //XmlDsigC14NTransform transform = new XmlDsigC14NTransform();
                //reference.AddTransform(transform);

                // Agrega la referencia al objeto signature
                XMLSignature.SignedInfo.AddReference(reference);
                KeyInfo keyInfo = new KeyInfo();
                keyInfo.AddClause(new RSAKeyValue((RSA)_criptografo.certificado.PrivateKey));

                // Agrega información del certificado x509
                keyInfo.AddClause(new KeyInfoX509Data(_criptografo.certificado));
                XMLSignature.KeyInfo = keyInfo;

                // Calcula la firma y recupere la representacion de la firma en un objeto xmlElement
                signedXml.ComputeSignature();
                XmlElement xFirma = signedXml.GetXml();

                // Inserta la firma en el documento 
                _xDocXml.DocumentElement.AppendChild(_xDocXml.ImportNode(xFirma, true));

            }
            catch (Exception fr)
            {
                _sMsj = "Excepción al firmar el acuse. " + fr.Message + " [RespuestaEnvio.firma]";
                _iErr++;
                throw;
            }
        }

        /// <summary>
        /// Serializa un objeto
        /// </summary>
        /// <param name="serializer">Inicializado con el objeto</param>
        /// <param name="encoding"></param>
        /// <param name="objeto"></param>
        /// <returns></returns>
        public static string SerializaObjeto(XmlSerializer serializer, Encoding encoding, object objeto, bool omitDeclaration)
        {
            try
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                //ns.Add("xmlns", "http://www.sii.cl/SiiDte");

                MemoryStream ms = new MemoryStream();
                XmlTextWriter xmlTextWriter = new XmlTextWriter(ms, encoding);
                xmlTextWriter.Formatting = Formatting.None;

                serializer.Serialize(xmlTextWriter, objeto, ns);
                ms = (MemoryStream)xmlTextWriter.BaseStream;
                return encoding.GetString(ms.ToArray());

            }
            catch (Exception)
            {
                return "";
                throw;
            }
        }

        /// <summary>
        /// Remove all the xml namespaces (xmlns) attributes in the xml string
        /// </summary>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        public string RemoveAllXmlNamespace(string xmlData)
        {
            string xmlnsPattern = "\\s+xmlns\\s*(:\\w)?\\s*=\\s*\\\"(?<url>[^\\\"]*)\\\"";
            MatchCollection matchCol = Regex.Matches(xmlData, xmlnsPattern);

            foreach (Match m in matchCol)
            {
                xmlData = xmlData.Replace(m.ToString(), "");
            }
            return xmlData;
        }

        public void SaveFile()
        {
            //String _rutaXml = @"C:\GPUsuario\GPExpressCfdi\feGilaChiTST\";
            try
            {   //arma el nombre del archivo xml

                CustomXmlTextWriter tw = new CustomXmlTextWriter(RutaYNomArchivo + ".xml", _encoding.WebName.ToUpper());
                _xDocXml.Save(tw);
                tw.Close();
            }
            catch (DirectoryNotFoundException)
            {
                _sMsj = "Verifique la existencia de la ruta indicada en la configuración de Ruta de archivos Xml. No existe la ruta actual: " + _rutaXml.Trim() + " [RespuestaEnvio.SaveFile]";
                _iErr++;
            }
            catch (IOException)
            {
                _sMsj = "Verifique permisos de escritura en: " + _rutaXml.Trim() + ". No se pudo guardar el archivo xml ni registrar el documento en la bitácora. [RespuestaEnvio.SaveFile]";
                _iErr++;
            }
            catch (Exception eAFE)
            {
                if (eAFE.Message.Contains("denied"))
                    _sMsj = "Elimine el archivo xml, luego vuelva a intentar. " + eAFE.Message + " [RespuestaEnvio.SaveFile]";
                else
                    _sMsj = "No se puede guardar el archivo XML. " + eAFE.Message + " [RespuestaEnvio.SaveFile] " + eAFE.StackTrace;
                _iErr++;
            }
        }

        public void GuardaArchivoDelProveedor()
        {
            try
            {
                CustomXmlTextWriter tw = new CustomXmlTextWriter(_rutaXml + _archivoRecibido, _encoding.WebName.ToUpper());
                _xDocProveedor.Save(tw);
                tw.Close();
            }
            catch (DirectoryNotFoundException)
            {
                _sMsj = "Verifique la existencia de la ruta indicada en la configuración de Ruta de archivos Xml. No existe la ruta actual: " + _rutaXml.Trim() + " [RespuestaEnvio.GuardaArchivoDelProveedor]";
                _iErr++;
            }
            catch (IOException)
            {
                _sMsj = "Verifique permisos de escritura en: " + _rutaXml.Trim() + ". No se pudo guardar el archivo xml ni registrar el documento en la bitácora. [RespuestaEnvio.GuardaArchivoDelProveedor]";
                _iErr++;
            }
            catch (Exception eAFE)
            {
                if (eAFE.Message.Contains("denied"))
                    _sMsj = "Elimine el archivo xml, luego vuelva a intentar. " + eAFE.Message + " [RespuestaEnvio.GuardaArchivoDelProveedor]";
                else
                    _sMsj = "No se puede guardar el archivo XML. " + eAFE.Message + " [RespuestaEnvio.GuardaArchivoDelProveedor] " + eAFE.StackTrace;
                _iErr++;
            }
        }

        /// <summary>
        /// Registra el log del receptor
        /// </summary>
        //public void Save()
        //{
        //    _iErr = 0;
        //    _sMsj = String.Empty;

        //    try
        //    {
        //        String xDoc = _xDocXml.InnerXml.Replace("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\n", "");
        //        short tipo = Convert.ToInt16(_tipoDTE);

        //        LogFacturaCompraService logReceptor = new LogFacturaCompraService(_connStr);
        //        logReceptor.CicloDeVida = _cicloDeVida;
        //        logReceptor.Save(tipo, _folio, _rutEmisor, _nomEmisor, DateTime.Now, _EnvioDTEID, xDoc, String.Empty, _uid, _usuario);
        //        logReceptor.Update(tipo, _folio, _rutEmisor, _usuario, Maquina.estadoBaseReceptor, Maquina.estadoBaseReceptor, _uid);

        //        if (logReceptor.IErr == -1) //No existe el estado base
        //            logReceptor.Save(tipo, _folio, _rutEmisor, _nomEmisor, _fechaRecepcion, Maquina.estadoBaseReceptor, _EnvioDTEID, Maquina.binStatusBaseReceptor, Maquina.estadoBaseReceptor,
        //                             xDoc, String.Empty, _uid, _usuario);

        //        _iErr = logReceptor.IErr;
        //        _sMsj = logReceptor.SMsj;

        //    }
        //    catch (Exception pr)
        //    {
        //        _sMsj = "Excepción desconocida al guardar en el log. " + pr.Message + " [RespuestaEnvio.Save]";
        //        _iErr++;
        //    }

        //}
        /// <summary>
        /// Obtiene el status del documento, verifica la transición y guarda en el log del receptor.
        /// </summary>
        /// <param name="evento"></param>
        //public void Guarda(int evento)
        //{
        //    _iErr = 0;
        //    _sMsj = String.Empty;
            
        //    try
        //    {       
        //        String xDoc = _xDocXml.InnerXml.Replace("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\n", "");
        //        short tipo = Convert.ToInt16( _tipoDTE);
        //        LogFacturaCompraService logReceptor = new LogFacturaCompraService(_connStr, _folio, tipo, _rutReceptor, Maquina.estadoBaseReceptor);

        //        if (logReceptor.CicloDeVida.Transiciona(evento, _tieneAcceso))
        //        {
        //            logReceptor.Save(tipo, _folio, _rutEmisor, _nomEmisor, _fechaRecepcion, _emailProveedor, xDoc, _archivoRecibido, _uid, _usuario);
        //            logReceptor.Update(tipo, _folio, _rutEmisor, _usuario, Maquina.estadoBaseReceptor, Maquina.estadoBaseReceptor, _uid);

        //            if (logReceptor.IErr == -1) //No existe el estado base
        //                logReceptor.Save(tipo, _folio, _rutEmisor, _nomEmisor, _fechaRecepcion, Maquina.estadoBaseReceptor, _emailProveedor, Maquina.binStatusBaseReceptor, Maquina.estadoBaseReceptor,
        //                                 xDoc, _archivoRecibido, _uid, _usuario);
        //        }
        //        _iErr = logReceptor.IErr;
        //        _sMsj = logReceptor.SMsj;

        //    }
        //    catch (Exception pr)
        //    {
        //        _sMsj = "Excepción desconocida al guardar en el log. " + pr.Message + " [RespuestaEnvio.TransicionaYGuarda]";
        //        _iErr++;
        //    }
        //}

    }
}
