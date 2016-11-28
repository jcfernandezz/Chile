using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Security.Cryptography;
using System.Xml.Linq;
using Encriptador;

namespace FEChile
{
    public class DteModel 
    {
        public int iErr = 0;
        public string sMsj = "";
        private DTEDefType _dte;
        private DTEDefTypeDocumento _dteDoc;
        private string _idPersonaRecibe;
        private Encoding _encoding;
        public TedModel modeloTed = null;
        public TecnicaDeEncriptacion criptografo = null;

        XmlDocument _xDocXml = null;
        XmlDocument _autorizacionXml = null;

        public DteModel(Encoding encoding)
        {
            _encoding = encoding;
            _dteDoc = new DTEDefTypeDocumento();
            modeloTed = new TedModel(encoding);
            _autorizacionXml = new XmlDocument();
            _autorizacionXml.PreserveWhitespace = true;

            _xDocXml = new XmlDocument();
            _xDocXml.PreserveWhitespace = true;

            criptografo = new TecnicaDeEncriptacion();
        }

        //**********************************************
        #region Propiedades
        public DTEDefTypeDocumento dteDoc
        {
            get { return _dteDoc; }
            set { _dteDoc = value; }
        }
        
        public string idPersonaRecibe
        {
            get { return _idPersonaRecibe; }
            set { _idPersonaRecibe = value; }
        }
        
        public XmlDocument xDocXml
        {
            get { return _xDocXml; }
            set { _xDocXml = value; }
        }

        public XmlDocument AutorizacionXml
        {
            get
            {
                return _autorizacionXml;
            }
            set
            {
                _autorizacionXml = value;
            }
        }

        private short _sopType;

        public short SopType
        {
            get { return _sopType; }
            set { _sopType = value; }
        }
        private String _sopNumbe;

        public String SopNumbe
        {
            get { return _sopNumbe; }
            set { _sopNumbe = value; }
        }

        #endregion
        //**********************************************

        public void prepara()
        {
            try
            {

            }
            catch (Exception)
            {
                
                throw;
            }
        }

        /// <summary>
        /// Ensambla el nodo TED.
        /// Requisito. Debe existir el objeto DTE y la Autorización para usar sus datos en el ensamblaje de TED.
        /// </summary>
        public void ensamblaTed(DateTime ts)
        {
            try
            {
                iErr = 0;
                sMsj = string.Empty;

                modeloTed.prepara(_dteDoc, ts);

                modeloTed.Serializa();

                modeloTed.AgregaCAF(_autorizacionXml, ts);

                modeloTed.firma(_autorizacionXml);
            }
            catch (Exception td)
            {
                sMsj = "Excepción al ensamblar nodo TED. [CFDComprobanteFIscalDigital.ensamblaTed] " + td.Message + " " + modeloTed.sMsj;
                iErr++;
                throw;
            }
        }

        public void agregaTED(TedModel modeloTed)
        {
            try
            {
                iErr = 0;
                sMsj = string.Empty;

                //Eliminar nodo TSTED vacío
                //XmlNode _xmlNode = _xDocXml.SelectSingleNode("//DTEDefTypeDocumentoTED/DD/TSTED");
                //_xmlNode.ParentNode.RemoveChild(_xmlNode);
                //dteDoc
                //XmlDocumentFragment caf = _xDocXml.CreateDocumentFragment();
                //var x = autorizacionXml.DocumentElement.SelectSingleNode("//AUTORIZACION/CAF");
                //caf.InnerXml = x.OuterXml;
                //_xDocXml.DocumentElement.FirstChild.AppendChild(caf);

                ////Agregar TSTED
                //XmlDocumentFragment timeStamp = _xDocXml.CreateDocumentFragment();
                //timeStamp.InnerXml = "<TSTED>" + ts.ToString("s") + "</TSTED>";
                ////timeStamp.InnerXml = "<TSTED>2003-09-08T12:28:31</TSTED>";
                //_xDocXml.DocumentElement.FirstChild.AppendChild(timeStamp);

                ////Obtener nodo TED/DD
                //_tedDD = _xDocXml.SelectSingleNode("//DTEDefTypeDocumentoTED/DD").OuterXml;

            }
            catch (Exception cf)
            {
                sMsj = "Excepción al agregar CAF. [TedModel.AgregaCAF] " + cf.Message + " " + cf.StackTrace;
                iErr++;
                throw;
            }
        }

        /// <summary>
        /// Canonicaliza el DTE sin el CAF. Luego agrega el CAF del xml original sin modificarlo.
        /// </summary>
        /// <param name="sDte"></param>
        /// <param name="ts"></param>
        public void Canonicaliza(string sDte, DateTime ts)
        {
            try
            {
                sMsj = string.Empty;
                iErr = 0;

                //Eliminar CAF de TED para que no sea canonicalizado
                XmlNode _xmlNode = modeloTed.xDocXml.SelectSingleNode("//TED/DD/CAF");
                _xmlNode.ParentNode.RemoveChild(_xmlNode);

                //Agrega TED a DTE/Documento
                XmlDocument xDte = new XmlDocument();
                xDte.PreserveWhitespace = true;
                xDte.LoadXml(Comun.Utiles.RemoveAllXmlNamespace(sDte));
                XmlDocumentFragment tedFragment = xDte.CreateDocumentFragment();
                tedFragment.InnerXml = modeloTed.xDocXml.DocumentElement.SelectSingleNode("//TED").OuterXml;
                xDte.DocumentElement.FirstChild.AppendChild(tedFragment);

                //Agregar TmstFirma
                XmlDocumentFragment timeStamp = xDte.CreateDocumentFragment();
                timeStamp.InnerXml = "<TmstFirma>" + ts.ToString("s") + "</TmstFirma>";
                xDte.DocumentElement.FirstChild.AppendChild(timeStamp);

                //Canonicalizar. Comenta para set de pruebas
                XmlDsigC14NTransform t = new XmlDsigC14NTransform();
                t.LoadInput(xDte);
                Stream s = (Stream)t.GetOutput(typeof(Stream));
                _xDocXml.Load(s);

                string sDocXml = _xDocXml.OuterXml.Replace("><", ">\n<");   //agrega line breaks
                //string sDocXml = xDte.OuterXml.Replace("><", ">\n<");   //agrega line breaks
                //string sDocXml = xDte.OuterXml;   
                _xDocXml.LoadXml(sDocXml);

                //Agregar CAF
                XmlDocumentFragment caf = _xDocXml.CreateDocumentFragment();
                caf.InnerXml = _autorizacionXml.DocumentElement.SelectSingleNode("//AUTORIZACION/CAF").OuterXml;
                XmlNode it1 = _xDocXml.DocumentElement.SelectSingleNode("//DTE/Documento/TED/DD/IT1");
                XmlNode parent = _xDocXml.DocumentElement.SelectSingleNode("//DTE/Documento/TED/DD");
                parent.InsertAfter(caf, it1);

                //XmlNode caf = _autorizacionXml.DocumentElement.SelectSingleNode("//AUTORIZACION/CAF");
                //XmlNode it1 = _xDocXml.DocumentElement.SelectSingleNode("//DTE/Documento/TED/DD/IT1");
                //XmlNode parent = _xDocXml.DocumentElement.SelectSingleNode("//DTE/Documento/TED/DD");
                //parent.InsertAfter(caf, it1);

                sDocXml = _xDocXml.OuterXml.Replace("</IT1>", "</IT1>\n");  //agrega line break
                //sDocXml = _xDocXml.OuterXml; 

                _xDocXml.LoadXml(sDocXml);

            }
            catch(Exception cn)
            {
                iErr++;
                sMsj = cn.Message + " [DteModel.Canonicaliza]";
                throw;
            }
        }

        public void firma(string referenciaUri)
        {
            try
            {
                sMsj = string.Empty;
                iErr = 0;

                SignedXml signedXml = new SignedXml(_xDocXml);

                // Agrega la clave privada al objeto signedXml
                signedXml.SigningKey = criptografo.certificado.PrivateKey;

                // Recupera el objeto signature desde signedXml
                Signature XMLSignature = signedXml.Signature;

                // Crea la referencia al documento DTE. Formato: '#reference'
                Reference reference = new Reference();
                reference.Uri = "#" + referenciaUri;

                //XmlDsigC14NTransform transform = new XmlDsigC14NTransform();
                //reference.AddTransform(transform);

                // Agrega la referencia al objeto signature
                XMLSignature.SignedInfo.AddReference(reference);
                KeyInfo keyInfo = new KeyInfo();
                keyInfo.AddClause(new RSAKeyValue((RSA)criptografo.certificado.PrivateKey));

                // Agrega información del certificado x509
                keyInfo.AddClause(new KeyInfoX509Data(criptografo.certificado));
                XMLSignature.KeyInfo = keyInfo;

                // Calcula la firma y recupere la representacion de la firma en un objeto xmlElement
                signedXml.ComputeSignature();
                XmlElement xFirma = signedXml.GetXml();

                // Inserta la firma en el documento DTE
                _xDocXml.DocumentElement.AppendChild(_xDocXml.ImportNode(xFirma, true));
            }
            catch(Exception fr)
            {
                sMsj = "Excepción al firmar DTE. " +fr.Message + " [DteModel.firma]";
                iErr++;
                throw;
            }
        }

        public void VerificaFirma()
        {
            try
            {
                SignedXml signedDoc = new SignedXml(_xDocXml);
                XmlNodeList nodeList = _xDocXml.GetElementsByTagName("Signature");

                signedDoc.LoadXml((XmlElement)nodeList[0]);

                if (!signedDoc.CheckSignature((RSA)criptografo.certificado.PublicKey.Key))
                {
                    sMsj = "La firma del DTE no concuerda con la llave pública. [DteModel.Verifica]";
                    iErr++;
                }

            }
            catch (Exception vf)
            {
                sMsj = "Excepción al verificar la firma del DTE. "+vf.Message+" [DteModel.Verifica]";
                iErr++;
                throw;
            }
        }
            
        public void serializa()
        { }

        /// <summary>
        /// Deserializa el el nodo DTE
        /// </summary>
        /// <param name="objetoXml"></param>
        public void DeSerializa(string objetoXml)
        {
            try
            {
                //Encoding encoding = Encoding.GetEncoding("ISO-8859-1");
                byte[] byteArray = Encoding.UTF8.GetBytes(objetoXml);

                //  Deserialize the XML file into a LibraryType object
                XmlSerializer serializer = new XmlSerializer(typeof(DTEDefType));
                _dte = (DTEDefType)serializer.Deserialize(new StreamReader(new MemoryStream(byteArray)));

                _dteDoc = (DTEDefTypeDocumento)_dte.Item;
            }
            catch (Exception ds)
            {
                sMsj = "Revise los datos, es probable que el tipo de documento asignado no sea electrónico. " + ds.Message + "\nExcepción al deserializar el siguiente doc:\n" + objetoXml + "\n[CFDComprobanteFiscalDigital.DeSerializa] ";
                iErr++;
                throw;
            }
        }

        public void DeserializaManualmente(string objetoXml, string idClienteRepLegal)
        {
            try
            {
                XElement dte = XElement.Parse(objetoXml, LoadOptions.PreserveWhitespace);

                XElement emisor = dte.Element("Emisor");
                _dteDoc.Encabezado = new DTEDefTypeDocumentoEncabezado();
                _dteDoc.Encabezado.Emisor = new DTEDefTypeDocumentoEncabezadoEmisor();
                _dteDoc.Encabezado.Receptor = new DTEDefTypeDocumentoEncabezadoReceptor();
                _dteDoc.Encabezado.IdDoc = new DTEDefTypeDocumentoEncabezadoIdDoc();

                foreach (var rec in dte.Descendants("Documento").Descendants("Encabezado").Descendants("Emisor").Elements("RUTEmisor"))
                    _dteDoc.Encabezado.Emisor.RUTEmisor = rec.Value;

                foreach (var rec in dte.Descendants("Documento").Descendants("Encabezado").Descendants("Receptor").Elements("RUTRecep"))
                    _dteDoc.Encabezado.Receptor.RUTRecep = rec.Value;

                foreach (var rec in dte.Descendants("Documento").Descendants("Encabezado").Descendants("Receptor").Elements("RznSocRecep"))
                    _dteDoc.Encabezado.Receptor.RznSocRecep = rec.Value;

                foreach (var rec in dte.Descendants("Documento").Descendants("Encabezado").Descendants("IdDoc").Elements("TipoDTE"))
                {
                    if (rec.Value.Equals("33"))
                    _dteDoc.Encabezado.IdDoc.TipoDTE = DTEType.Item33;
                    if (rec.Value.Equals("34"))
                        _dteDoc.Encabezado.IdDoc.TipoDTE = DTEType.Item34;
                    if (rec.Value.Equals("46"))
                        _dteDoc.Encabezado.IdDoc.TipoDTE = DTEType.Item46;
                    if (rec.Value.Equals("52"))
                        _dteDoc.Encabezado.IdDoc.TipoDTE = DTEType.Item52;
                    if (rec.Value.Equals("56"))
                        _dteDoc.Encabezado.IdDoc.TipoDTE = DTEType.Item56;
                    if (rec.Value.Equals("61"))
                        _dteDoc.Encabezado.IdDoc.TipoDTE = DTEType.Item61;
                }
                idPersonaRecibe = idClienteRepLegal;

            }
            catch (Exception dm)
            {
                iErr++;
                sMsj = "Excepción al obtener datos del DTE. " + dm.Message + " [DteModel.DeserializaManualmente]";
            }
        }
    }
}
