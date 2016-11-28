using cfd.FacturaElectronica;
using Encriptador;
using MaquinaDeEstados;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ReciboMercaderiaServicios
{
    public class CFDReciboMercaServicio
    {
        private XmlElement xFirma;
        private XNamespace nsDte = "http://www.sii.cl/SiiDte";
        private XNamespace nsSig = "http://www.w3.org/2000/09/xmldsig#";
        private TecnicaDeEncriptacion _criptografo;
        private String _connStr;
        private Encoding _encoding;
        //********************************************************************
        #region Propiedades
        private int _iErr;
        public int iErr
        {
            get { return _iErr; }
            set { _iErr = value; }
        }
        private String _sMsj;
        public String sMsj
        {
            get { return _sMsj; }
            set { _sMsj = value; }
        }
        private String _usuario;

        public String Usuario
        {
            get { return _usuario; }
            set { _usuario = value; }
        }
        private Maquina _cicloDeVida;
        public Maquina CicloDeVida
        {
            get { return _cicloDeVida; }
            set { _cicloDeVida = value; }
        }

        private String _RutFirma;
        public String RutFirma
        {
            get { return _RutFirma; }
            set { _RutFirma = value; }
        }
        private String _sXDoc;
        public String SXDoc
        {
            get { return _sXDoc; }
            set { _sXDoc = value; }
        }
        private XElement _xDoc;
        public XElement XDoc
        {
            get { return _xDoc; }
            set { _xDoc = value; }
        }
        private XmlDocument _xDocXml;
        public XmlDocument XDocXml
        {
            get { return _xDocXml; }
            set { _xDocXml = value; }
        }
        private String _estado;
        public String Estado
        {
            get { return _estado; }
            set { _estado = value; }
        }

        private String _eBinStatus;
        public String EBinStatus
        {
            get { return _eBinStatus; }
            set { _eBinStatus = value; }
        }

        private short _tipoDte;
        public short TipoDte
        {
            get { return _tipoDte; }
            set { _tipoDte = value; }
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
        private String _nombreTercero;
        public String NombreTercero
        {
            get { return _nombreTercero; }
            set { _nombreTercero = value; }
        }

        private String _idExterno;
        public String IdExterno
        {
            get { return _idExterno; }
            set { _idExterno = value; }
        }
        private String _mensaje;
        public String Mensaje
        {
            get { return _mensaje; }
            set { _mensaje = value; }
        }

        public String IdRecibo
        {
            get { return "R"+_tipoDte.ToString()+_folio; }
        }
        public String IdSetRecibos
        {
            get { return "SERVICIO"; }
        }
        private String _emailProveedor;
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

        public String EmailProveedor
        {
            get { return _emailProveedor; }
            set { _emailProveedor = value; }
        }
        public String RutaYNomArchivo
        {
            get { return _rutaXml + _prefijo + _idImpuestoTercero + "_" + _tipoDte + "_" + _folio; }
        }
        private int _evento;

        public int Evento
        {
            get { return _evento; }
            set { _evento = value; }
        }

        #endregion
        //********************************************************************

        public CFDReciboMercaServicio(TecnicaDeEncriptacion criptografo, String connStr, String usuario, Encoding encoding, String compoundedBinStatus, short idxSingleStatus, short voidStts, short tipo)
        {
            _xDocXml = new XmlDocument();
            _xDocXml.PreserveWhitespace = true;
            _usuario = usuario;
            _criptografo = criptografo;
            _connStr = connStr;
            _encoding = encoding;
            _tipoDte = tipo;
            _cicloDeVida = new Maquina(compoundedBinStatus, idxSingleStatus.ToString(), voidStts, "receptor", tipo.ToString());
        }

        /// <summary>
        /// Convierte la Respuesta envío dte de factura recibida en Recibo de mercadería servicio
        /// Requisito. Asignar las propiedades _tipoDte, _folio, etc...
        /// </summary>
        /// <param name="sRespuesta">xml de la respuesta envío de factura recibida</param>
        public void Ensambla(String sRespuesta)
        {
            _iErr = 0;
            _sMsj = String.Empty;
            List<XElement> lDocRecibo = new List<XElement>();

            try
            {
                sRespuesta = sRespuesta.Replace("RespuestaEnvioDTE_v10.xsd", "EnvioRecibos_v10.xsd");
                sRespuesta = sRespuesta.Replace("RespuestaDTE", "EnvioRecibos");
                sRespuesta = sRespuesta.Replace("Resultado", "SetRecibos");
                sRespuesta = sRespuesta.Replace("RecepcionDTE", "DocumentoRecibo");
                sRespuesta = sRespuesta.Replace("TipoDTE", "TipoDoc");

                _xDoc = XElement.Parse(sRespuesta);
                _xDoc.Elements(nsSig + "Signature").Remove();
                _xDoc.Elements(nsDte + "SetRecibos").Elements(nsDte + "Caratula").Elements(nsDte + "IdRespuesta").Remove();
                _xDoc.Elements(nsDte + "SetRecibos").Elements(nsDte + "Caratula").Elements(nsDte + "NroDetalles").Remove();
                _xDoc.Elements(nsDte + "SetRecibos").Elements(nsDte + "Caratula").Elements(nsDte + "TmstFirmaResp").Remove();

                foreach (XElement e in _xDoc.Elements(nsDte + "SetRecibos").Elements(nsDte + "RecepcionEnvio").Elements())
                {
                    //inicio intercambio
                    //if (e.Name.Equals(nsDte + "DocumentoRecibo") && e.Element(nsDte + "EstadoRecepDTE").Value == "0")
                    //fin intercambio
                    if (e.Name.Equals(nsDte + "DocumentoRecibo"))     
                    {
                        e.Add(new XAttribute("ID", IdRecibo));
                        e.Add(new XElement(nsDte + "Recinto", "Oficina central"));
                        e.Add(new XElement(nsDte + "RutFirma", _RutFirma));
                        e.Add(new XElement(nsDte + "Declaracion", "El acuse de recibo que se declara en este acto, de acuerdo a lo dispuesto en la letra b) del Art. 4, y la letra c) del Art. 5 de la Ley 19.983, acredita que la entrega de mercaderias o servicio(s) prestado(s) ha(n) sido recibido(s)."));
                        e.Add(new XElement(nsDte + "TmstFirmaRecibo", String.Format("{0:yyyy-MM-ddTHH:mm:ssZ}", DateTime.Now)));
                        lDocRecibo.Add(e);
                    }
                }

                _xDoc.Elements(nsDte + "SetRecibos").Elements(nsDte + "RecepcionEnvio").Remove();

                foreach (XElement doc in lDocRecibo)
                {
                    _xDoc.Element(nsDte + "SetRecibos").Add(new XElement(nsDte + "Recibo", new XAttribute("version", "1.0"), doc));
                }
                _xDoc.Element(nsDte + "SetRecibos").Element(nsDte + "Caratula").Add(new XElement(nsDte + "TmstFirmaEnv", String.Format("{0:yyyy-MM-ddTHH:mm:ssZ}", DateTime.Now)));
                _xDoc.Element(nsDte + "SetRecibos").Attribute("ID").Value = IdSetRecibos;

                _xDoc.Elements(nsDte + "SetRecibos").Elements(nsDte + "Recibo").Elements(nsDte + "DocumentoRecibo").Elements(nsDte + "EstadoRecepDTE").Remove();
                _xDoc.Elements(nsDte + "SetRecibos").Elements(nsDte + "Recibo").Elements(nsDte + "DocumentoRecibo").Elements(nsDte + "RecepDTEGlosa").Remove();

            }
            catch (Exception er)
            {
                _sMsj = "Excepción al formar el recibo de mercadería/servicio. " + er.Message + " [CFDReciboMercaServicio.Ensambla]";
                _iErr++;
            }
        }

        public void Canonicaliza()
        {
            _sXDoc = _xDoc.ToString().Replace("><", ">\n<");

            if (_sXDoc.Substring(0, 1).Equals("\n"))
                _xDocXml.LoadXml(_sXDoc);
            else
                _xDocXml.LoadXml("\n" + _sXDoc);

            _xDoc = XElement.Parse(_sXDoc, LoadOptions.PreserveWhitespace);
        }

        public void Firma(String sUri)
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
                xFirma = signedXml.GetXml();

                // Inserta la firma en el documento 
                //_xDocXml.DocumentElement.AppendChild(_xDocXml.ImportNode(xFirma, true));

            }
            catch (Exception fr)
            {
                _sMsj = "Excepción al firmar el recibo de mercadería/servicio. " + fr.Message + " [CFDRecibo.Firma]";
                _iErr++;
            }

        }
        public void InsertaFirma(String sNodo)
        {
            // Inserta la firma en el nodo
            XmlNamespaceManager ns = new XmlNamespaceManager(_xDocXml.NameTable);
            ns.AddNamespace("a", "http://www.sii.cl/SiiDte");

            XmlNode nodoEnvio = _xDocXml.SelectSingleNode("//a:" + sNodo, ns);
            nodoEnvio.AppendChild(nodoEnvio.OwnerDocument.ImportNode(xFirma, true));

        }

        /// <summary>
        /// Guarda la recepción del producto
        /// Requisito. Debe haber transicionado.
        /// </summary>
        public void Save()
        {
            try
            {
                //String xDoc = _xDocXml.InnerXml.Replace("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>\n", "");
                String xDoc = _xDocXml.InnerXml.Replace("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\n", "");
                LogFacturaCompraService bitacoraCompra = new LogFacturaCompraService(_connStr);
                bitacoraCompra.CicloDeVida = _cicloDeVida;
                bitacoraCompra.Save(_tipoDte, _folio, _idImpuestoTercero, _nombreTercero, DateTime.Now, _mensaje, xDoc, String.Empty, _idExterno, _usuario);
                bitacoraCompra.Update(_tipoDte, _folio, _idImpuestoTercero, _usuario, Maquina.estadoBaseReceptor, Maquina.estadoBaseReceptor, _idExterno);

            }
            catch (Exception eAFE)
            {
                _sMsj = "No se puede registrar la Bitácora. " + eAFE.Message + " [CFDReciboMercaServicio.Save] " + eAFE.StackTrace;
                _iErr++;
            }

        }

        public void SaveFile()
        {
            try
            {  

                CustomXmlTextWriter tw = new CustomXmlTextWriter(RutaYNomArchivo + ".xml", _encoding.WebName.ToUpper());
                _xDocXml.Save(tw);
                tw.Close();
            }
            catch (DirectoryNotFoundException)
            {
                _sMsj = "Verifique la existencia de la ruta indicada en la configuración de Ruta de archivos Xml. No existe la ruta actual: " + _rutaXml.Trim() + " [CFDReciboMercaServicio.SaveFile]";
                _iErr++;
            }
            catch (IOException)
            {
                _sMsj = "Verifique permisos de escritura en: " + _rutaXml.Trim() + ". No se pudo guardar el archivo xml ni registrar el documento en la bitácora. [CFDReciboMercaServicio.SaveFile]";
                _iErr++;
            }
            catch (Exception eAFE)
            {
                if (eAFE.Message.Contains("denied"))
                    _sMsj = "Elimine el archivo xml, luego vuelva a intentar. " + eAFE.Message + " [CFDReciboMercaServicio.SaveFile]";
                else
                    _sMsj = "No se puede guardar el archivo XML. " + eAFE.Message + " [CFDReciboMercaServicio.SaveFile] " + eAFE.StackTrace;
                _iErr++;
            }
        }

    }
}
