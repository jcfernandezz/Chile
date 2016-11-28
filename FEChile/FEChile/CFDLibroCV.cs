using Comun;
using cfd.FacturaElectronica;
using MaquinaDeEstados;
using Encriptador;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
using System.IO;

namespace FEChile
{
    public class CFDLibroCV
    {
        private Parametros _Param;
        private ConexionAFuenteDatos _Conex;
        private Encoding _encoding;
        private string _nomArchivo;
        private LogLibroCVService _bitacora;
        private Maquina _cicloDeVida;
        private int _periodo;
        private string _tipo;
        private XmlDocument _xDocXml;
        private TecnicaDeEncriptacion _criptografo;
        private vwCfdCertificadosService _certificados;
        private String _idLibro;
        private String _rutaXml = string.Empty;
        private int _iErr = 0;
        private string _sMsj = string.Empty;
        private String _sDeclaracionXml;

        //public DateTime ts = DateTime.Now;

        public CFDLibroCV(ConexionAFuenteDatos conex, Parametros Param, Encoding encoding,
                          string compoundedBinStatus, string idxSingleStatus, int periodo, String tipo, String rutaXml)
        {
            try
            {
                _iErr=0;
                _sMsj = string.Empty;
                _Param = Param;
                _Conex = conex;
                _rutaXml = rutaXml;
                _encoding = encoding;
                _periodo = periodo;
                _tipo = tipo;
                _idLibro = _tipo + _periodo.ToString();
                _nomArchivo = Utiles.FormatoNombreArchivo(tipo + "_", periodo.ToString(), 20);
                _bitacora = new LogLibroCVService(_Conex.ConnStr);
                _cicloDeVida = new Maquina(compoundedBinStatus, idxSingleStatus, 0, "emisor", "LIBRO");

                //ts = DateTime.Now;
                //ts = new DateTime(ts.Year, ts.Month, ts.Day, ts.Hour, ts.Minute, ts.Second);
                //ts = new DateTime(2014, 08, 20, 18, 00, 00 );
                _xDocXml = new XmlDocument();
                _xDocXml.PreserveWhitespace = true;
                _criptografo = new TecnicaDeEncriptacion();

                _sDeclaracionXml = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>";
            }
            catch (Exception ini)
            {
                _sMsj = "Excepción al inicializar cfd. " + ini.Message + " [CFDLibroCV.constructor]";
                _iErr++;
                throw;
            }
        }

        #region Propiedades
        public string sMsj
        {
            get { return _sMsj; }
        }
        public int iErr
        {
            get { return _iErr; }
        }

        public int periodo
        {
            get { return _periodo; }
            set { _periodo = value; }
        }

        public string tipo
        {
            get { return _tipo; }
            set { _tipo = value; }
        }

        public string nomArchivo
        {
            get { return _nomArchivo; }
        }
        public Maquina cicloDeVida
        {
            get { return _cicloDeVida; }
            set { _cicloDeVida = value; }
        }
        public TecnicaDeEncriptacion criptografo
        {
            get { return _criptografo; }
            set { _criptografo = value; }
        }
        public vwCfdCertificadosService certificados
        {
            get { return _certificados; }
            set { _certificados = value; }
        }
        

        #endregion

        /// <summary>
        /// Arma y firma un envío de libro 
        /// </summary>
        /// <param name="sDocXml">Xml a completar y firmar</param>
        public void Ensambla(String sDocXml, Encoding encoding)
        {
            try
            {
                _iErr = 0;
                _sMsj = string.Empty;
                String rutEnvia = _certificados.idImpuesto.Trim();

                //quita namespaces antes de cargar xml
                String lDocXml = Comun.Utiles.RemoveAllXmlNamespace(sDocXml);

                _xDocXml.LoadXml(_sDeclaracionXml + lDocXml);

                //Agrega el rut del que envía
                XmlNode nodoCaratula = _xDocXml.SelectSingleNode("//Caratula");
                SetNodeValue(nodoCaratula, "//Caratula/RutEnvia", rutEnvia);

                //Canonicaliza documento original
                //Canonicaliza(loteLibro.ComprobanteXml, ts);

                //Reajusta atributos de namespace
                _xDocXml.SelectSingleNode("//LibroCompraVenta");
                _xDocXml.DocumentElement.RemoveAllAttributes();
                _xDocXml.DocumentElement.SetAttribute("xmlns", "http://www.sii.cl/SiiDte");
                _xDocXml.DocumentElement.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                XmlDocument atr = new XmlDocument();
                XmlAttribute schemaLocation = atr.CreateAttribute("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance");
                schemaLocation.Value = "http://www.sii.cl/SiiDte LibroCV_v10.xsd";
                _xDocXml.DocumentElement.SetAttributeNode(schemaLocation);
                _xDocXml.DocumentElement.SetAttribute("version", "1.0");

                lDocXml = _xDocXml.OuterXml.Replace("><", ">\n<");

                _xDocXml.LoadXml(lDocXml);

                firma(_idLibro);

                validaXsd(_Param.URLArchivoLibroCVXSD);
            }
            catch (Exception ex)
            {
                _sMsj += _sMsj + " Excepción al ensamblar libro. " + ex.Message + " [CFDLibroCV.Ensambla] " + ex.StackTrace;
                _iErr++;
            }
        }

         private void firma(string idLibro)
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
                reference.Uri = "#" + idLibro;

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

                // Inserta la firma en el documento DTE
                _xDocXml.DocumentElement.AppendChild(_xDocXml.ImportNode(xFirma, true));
            }
            catch (Exception fr)
            {
                _sMsj = "Excepción al firmar el libro. " + fr.Message + " [CFDLibroCV.firma]";
                _iErr++;
                throw;
            }
        }

        public static void SetNodeValue(XmlNode parentNode, string xpath, object nodeValue)
        {
            if (parentNode == null)
                throw new ArgumentNullException("parentNode");

            XmlNode childNode = parentNode.SelectSingleNode(xpath);

            if (childNode == null)
                throw new ArgumentException(String.Format("No se encuentra ningún nodo en la ruta especificada '{0}'", xpath), "xpath");

            if (childNode.ChildNodes.Count > 0)
                throw new XmlException("El nodo hijo no es un nodo texto porque tiene nodos hijo.");

            childNode.InnerText = nodeValue.ToString();

        }
        
        public static void SetNodeValue(XmlNode node, object nodeValue)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.NodeType != XmlNodeType.Element)
                throw new XmlException("Node is not an element.");

            if (node.ChildNodes.Count > 0)
                throw new XmlException("Node is not a text node as it has child nodes.");

            node.AppendChild(node.OwnerDocument.CreateTextNode(nodeValue.ToString()));
        }

        public void validaXsd(String URLArchivoXSD)
        {
            _iErr = 0;
            _sMsj = String.Empty;

            //Validar el esquema del archivo xml
            ValidadorXML validadorxml = new ValidadorXML(URLArchivoXSD);
            _sMsj = validadorxml.sMsj;
            _iErr = validadorxml.iErr;

            if (validadorxml.iErr == 0)
            {
                validadorxml.ValidarXSD(_xDocXml);
                _iErr = validadorxml.iErr;
                _sMsj = validadorxml.sMsj;
            }
        }

        /// <summary>
        /// Guarda el archivo xml, anota en la bitácora el libro emitido y el nuevo estado binario.
        /// </summary>
        public string Guarda()
        {
            _sMsj = "";
            _iErr = 0;
            string rutaYNomArchivo = string.Empty;
            try
            {   //arma el nombre del archivo xml
                rutaYNomArchivo = _rutaXml.Trim() + _nomArchivo;

                //Guarda el archivo xml
                CustomXmlTextWriter tw = new CustomXmlTextWriter(rutaYNomArchivo + ".xml", _encoding.WebName.ToUpper());
                //tw.Formatting = Formatting.Indented;
                _xDocXml.Save(tw);
                tw.Close();

                _bitacora.Save(_periodo, _tipo, _cicloDeVida.targetSingleStatus, rutaYNomArchivo + ".xml",
                                Convert.ToInt16(_cicloDeVida.idxTargetSingleStatus), _cicloDeVida.targetBinStatus,
                                _cicloDeVida.EstadoEnPalabras(_cicloDeVida.targetBinStatus), _xDocXml.InnerXml.Replace(_sDeclaracionXml, String.Empty), _Conex.Usuario);

                return rutaYNomArchivo;

                //if (iErr == 0)
                //{
                //    //Comprime el archivo xml
                //    if (_Param.zip)
                //        Utiles.Zip(rutaYNomArchivo, ".xml");

                //    iErr = codigobb.iErr + reporte.numErr + Utiles.numErr;
                //    sMsj = codigobb.strMensajeErr + " " + reporte.mensajeErr + " " + Utiles.msgErr;

                //    //Si hay error en cbb o pdf o zip anota en la bitácora
                //    if (iErr != 0)
                //        ActualizaFacturaEmitida(trxVenta.Soptype, trxVenta.Sopnumbe, _Conexion.Usuario,
                //                                "emitido", "emitido", mEstados.eBinActualConError,
                //                                mEstados.EnLetras(mEstados.eBinActualConError) + sMsj.Trim());
                //}
            }
            catch (DirectoryNotFoundException)
            {
                _sMsj = "Verifique la existencia de la ruta indicada en la configuración de Ruta de archivos Xml. No existe la ruta actual: " + _rutaXml.Trim() + " [CFDLibroCV.Guarda]";
                _iErr++;
                throw;
            }
            catch (IOException)
            {
                _sMsj = "Verifique permisos de escritura en: " + _rutaXml.Trim() + ". No se pudo guardar el archivo xml ni registrar el documento en la bitácora. [CFDLibroCV.Guarda]";
                _iErr++;
                throw;
            }
            catch (Exception eAFE)
            {
                if (eAFE.Message.Contains("denied"))
                    _sMsj = "Elimine el archivo xml, luego vuelva a intentar. " + eAFE.Message + " [CFDLibroCV.Guarda]";
                else
                    _sMsj = "No se puede guardar el archivo XML ni registrar la Bitácora. " + eAFE.Message + " [CFDLibroCV.Guarda] " + eAFE.StackTrace;
                _iErr++;
                throw;
            }
        }

    }
}
