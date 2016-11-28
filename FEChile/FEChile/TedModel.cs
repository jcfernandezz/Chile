using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using Comun;
using Encriptador;
using System.Text.RegularExpressions;

namespace FEChile
{
    public class TedModel
    {
        private Encoding _encoding;

        public int iErr = 0;
        public string sMsj = "";

        string _tedDD = string.Empty;
        XmlDocument _xDocXml = null;
        TecnicaDeEncriptacion _criptografo = null;

        public DTEDefTypeDocumentoTED ted = null;

        public TedModel(Encoding encoding)
        {
            _encoding = encoding;
            _criptografo = new TecnicaDeEncriptacion();

            _xDocXml = new XmlDocument();
            _xDocXml.PreserveWhitespace = true;
        }

        #region Propiedades
        public XmlDocument xDocXml
        {
            get { return _xDocXml; }
        }
        
        #endregion

        public void prepara(DTEDefTypeDocumento dteDoc, DateTime ts)
        {
            try
            {
                sMsj = "";
                iErr = 0;

                ted = new DTEDefTypeDocumentoTED();
                ted.DD = new DTEDefTypeDocumentoTEDDD();
                
                //test:
                //ted.DD.RE = "97975000-5"; //dteDoc.Encabezado.Emisor.RUTEmisor;
                //ted.DD.TD = dteDoc.Encabezado.IdDoc.TipoDTE;
                //ted.DD.F = "27";  // dteDoc.Encabezado.IdDoc.Folio;
                //ted.DD.FE = new DateTime(2003, 9, 8); //dteDoc.Encabezado.IdDoc.FchEmis;
                //ted.DD.RR = "8414240-9";  //dteDoc.Encabezado.Receptor.RUTRecep;
                //ted.DD.RSR = "JORGE GONZALEZ LTDA";    // dteDoc.Encabezado.Receptor.RznSocRecep;
                //ted.DD.MNT = 502946;
                //ted.DD.IT1 = "Cajon AFECTO";

                ted.DD.RE = dteDoc.Encabezado.Emisor.RUTEmisor;
                ted.DD.TD = dteDoc.Encabezado.IdDoc.TipoDTE;
                ted.DD.F = dteDoc.Encabezado.IdDoc.Folio;
                ted.DD.FE = dteDoc.Encabezado.IdDoc.FchEmis;
                ted.DD.RR = dteDoc.Encabezado.Receptor.RUTRecep;
                ted.DD.RSR = Utiles.Izquierda(dteDoc.Encabezado.Receptor.RznSocRecep, 40);
                ted.DD.MNT = Convert.ToUInt64(dteDoc.Encabezado.Totales.MntTotal);
                ted.DD.IT1 = Utiles.Izquierda(dteDoc.Detalle[0].NmbItem, 40);
            }
            catch(Exception pr)
            {
                sMsj = "Es probable que el documento no tenga datos en el detalle. " + pr.Message +" [TedModel.prepara] " + pr.StackTrace;
                iErr++;
                throw;
            }
        }

        /// <summary>
        /// Prepara y serializa el objeto en el string _sDocXml
        /// </summary>
        public void Serializa()
        {
            try
            {
                string sDocXml = string.Empty;
                XmlSerializer serializer = new XmlSerializer(ted.GetType());
                sDocXml = SerializaObjeto(serializer, _encoding, ted, true);

                sDocXml = Regex.Replace(sDocXml, "DTEDefTypeDocumentoTED", "TED");

                _xDocXml.LoadXml(Comun.Utiles.RemoveAllXmlNamespace(sDocXml));
            }
            catch (Exception so)
            {
                sMsj = "Error al serializar el documento. " + so.Message + " [TedModel.Serializa] " + so.StackTrace;
                iErr++;
            }
        }

        /// <summary>
        /// Serializa un objeto
        /// </summary>
        /// <param name="serializer">Inicializado con el objeto</param>
        /// <param name="objeto">Objeto de cualquier tipo</param>
        /// <param name="objeto"></param>
        /// <returns></returns>
        public static string SerializaObjeto(XmlSerializer serializer, Encoding encoding, object objeto, bool omitDeclaration)
        {
            try
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                MemoryStream ms = new MemoryStream();
                XmlTextWriter xmlTextWriter = new XmlTextWriter(ms, encoding);
                xmlTextWriter.Formatting = Formatting.None;
                //xmlTextWriter.Settings.OmitXmlDeclaration = omitDeclaration;

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
        /// Agrega el nodo CAF al nodo TED/DD
        /// </summary>
        /// <param name="autorizacionXml">Archivo que contiene el nodo CAF</param>
        /// <param name="ts">time stamp</param>
        public void AgregaCAF(XmlDocument autorizacionXml, DateTime ts)
        {
            try
            {
                iErr = 0;
                sMsj = string.Empty;

                //Eliminar nodo TSTED vacío
                XmlNode _xmlNode = _xDocXml.SelectSingleNode("//TED/DD/TSTED");
                _xmlNode.ParentNode.RemoveChild(_xmlNode);

                //Agregar CAF
                XmlDocumentFragment caf = _xDocXml.CreateDocumentFragment();
                //var x = autorizacionXml.DocumentElement.SelectSingleNode("//AUTORIZACION/CAF");
                caf.InnerXml = autorizacionXml.DocumentElement.SelectSingleNode("//AUTORIZACION/CAF").OuterXml;
                _xDocXml.DocumentElement.FirstChild.AppendChild(caf);

                //Agregar TSTED
                XmlDocumentFragment timeStamp = _xDocXml.CreateDocumentFragment();
                timeStamp.InnerXml = "<TSTED>" + ts.ToString("s") + "</TSTED>";
                _xDocXml.DocumentElement.FirstChild.AppendChild(timeStamp);

            }
            catch(Exception cf)
            {
                sMsj = "Excepción al agregar CAF. [TedModel.AgregaCAF] " + cf.Message + " " + cf.StackTrace;
                iErr++;
                throw;
            }
        }

        public void pruebaFirma(XmlDocument autorizacionXml)
        {
            try
            {
                iErr = 0;
                sMsj = string.Empty;

                /////////////////////////////////////////////////
                string DD = string.Empty;
                DD += "<DD><RE>97975000-5</RE><TD>33</TD><F>27</F><FE>2003-09-08</FE>";
                DD += "<RR>8414240-9</RR><RSR>JORGE GONZALEZ LTDA</RSR><MNT>502946</M";
                DD += "NT><IT1>Cajon AFECTO</IT1><CAF version=\"1.0\"><DA><RE>97975000-";
                DD += "5</RE><RS>RUT DE PRUEBA</RS><TD>33</TD><RNG><D>1</D><H>200</H>";
                DD += "</RNG><FA>2003-09-04</FA><RSAPK><M>0a4O6Kbx8Qj3K4iWSP4w7KneZYe";
                DD += "J+g/prihYtIEolKt3cykSxl1zO8vSXu397QhTmsX7SBEudTUx++2zDXBhZw==<";
                DD += "/M><E>Aw==</E></RSAPK><IDK>100</IDK></DA><FRMA algoritmo=\"SHA1";
                DD += "withRSA\">g1AQX0sy8NJugX52k2hTJEZAE9Cuul6pqYBdFxj1N17umW7zG/hAa";
                DD += "vCALKByHzdYAfZ3LhGTXCai5zNxOo4lDQ==</FRMA></CAF><TSTED>2003-09";
                DD += "-08T12:28:31</TSTED></DD>";

                ////
                //// Representa la clave privada rescatada desde el CAF que envía el SII
                //// para la prueba propuesta por ellos.
                ////
                string pk = string.Empty;
                pk += "-----BEGIN RSA PRIVATE KEY-----";
                pk += "MIIBOwIBAAJBANGuDuim8fEI9yuIlkj+MOyp3mWHifoP6a4oWLSBKJSrd3MpEsZd";
                pk += "czvL0l7t/e0IU5rF+0gRLnU1Mfvtsw1wYWcCAQMCQQCLyV9FxKFLW09yWw7bVCCd";
                pk += "xpRDr7FRX/EexZB4VhsNxm/vtJfDZyYle0Lfy42LlcsXxPm1w6Q6NnjuW+AeBy67";
                pk += "AiEA7iMi5q5xjswqq+49RP55o//jqdZL/pC9rdnUKxsNRMMCIQDhaHdIctErN2hC";
                pk += "IP9knS3+9zra4R+5jSXOvI+3xVhWjQIhAJ7CF0R0S7SIHHKe04NUURf/7RvkMqm1";
                pk += "08k74sdnXi3XAiEAlkWk2vc2HM+a1sCqQxNz/098ketqe7NuidMKeoOQObMCIQCk";
                pk += "FAMS9IcPcMjk7zI2r/4EEW63PSXyN7MFAX7TYe25mw==";
                pk += "-----END RSA PRIVATE KEY-----";

                const string HTIMBRE = "pqjXHHQLJmyFPMRvxScN7tYHvIsty0pqL2LLYaG43jMmnfiZfllLA0wb32lP+HBJ/tf8nziSeorvjlx410ZImw==";

                _criptografo.PreparaEncriptacion(pk);
                //_criptografo.PreparaEncriptacion(autorizacionXml.DocumentElement.SelectSingleNode("//AUTORIZACION/RSASK/text()").Value);

                ///////////////////////////////////////////////////
                //Obtener nodo TED/DD y quitar line feeds, tabulaciones y espacios entre el tag de cierre y el tag de inicio. 
                //También quitar referencias a namespaces.
                _tedDD = _xDocXml.SelectSingleNode("//TED/DD").OuterXml;
                _tedDD = _tedDD.Replace("\n", String.Empty).Replace("\t", String.Empty);

                //Agregar sello
                XmlDocumentFragment sello = _xDocXml.CreateDocumentFragment();
                //var x = autorizacionXml.DocumentElement.SelectSingleNode("//AUTORIZACION/CAF");
                //                sello.InnerXml = "<FRMT algoritmo=\"SHA1withRSA\">" + _criptografo.obtieneSello(_tedDD, _encoding) + "</FRMT>";
                string FRMT1 = _criptografo.obtieneSello(DD, _encoding);
                sello.InnerXml = "<FRMT algoritmo=\"SHA1withRSA\">" + FRMT1 + "</FRMT>";
                

                if (HTIMBRE.Equals(FRMT1))
                {
                    Console.WriteLine("Comprobacion OK");
                }
                else
                {
                    Console.WriteLine("Comprobacion NOK");
                }

                _xDocXml.SelectSingleNode("//TED");
                _xDocXml.DocumentElement.AppendChild(sello);

                //XmlNode nodoTed = _xDocXml.SelectSingleNode("//TED");
                //XmlNode childNode = nodoTed.SelectSingleNode("//TED/DD");
                //childNode.InnerText = nodeValue.ToString();

                //SetNodeValue(nodoCaratula, "//TED/DD", rutEnvia.Insert(rutEnvia.Length - 1, "-"));
                //nodoTed.AppendChild(nodoTed.OwnerDocument.CreateTextNode(nodeValue.ToString()));

            }
            catch (Exception fr)
            {
                sMsj = "Excepción al firmar el nodo TED. [TedModel.firma] " + fr.Message + " " + fr.StackTrace;
                iErr++;
                throw;
            }
 
        }

        /// <summary>
        /// Calcula la firma del nodo TED/DD
        /// Requisito. El nodo TED/DD debe estar listo
        /// </summary>
        /// <param name="autorizacionXml">Archivo CAF que contiene la llave privada</param>
        public void firma(XmlDocument autorizacionXml)
        {
            try
            {
                iErr = 0;
                sMsj = string.Empty;

                _criptografo.PreparaEncriptacion(autorizacionXml.DocumentElement.SelectSingleNode("//AUTORIZACION/RSASK/text()").Value);

                //Obtener nodo TED/DD y quitar line feeds, tabulaciones y espacios entre el tag de cierre y el tag de inicio. 
                //También quitar referencias a namespaces.
                _tedDD = _xDocXml.SelectSingleNode("//TED/DD").OuterXml;
                _tedDD = _tedDD.Replace("\n", String.Empty).Replace("\t", String.Empty).Replace("\r", String.Empty);

                //Agregar sello
                XmlDocumentFragment nodoSello = _xDocXml.CreateDocumentFragment();
                String sello = _criptografo.obtieneSello(_tedDD, _encoding);
                nodoSello.InnerXml = "<FRMT algoritmo=\"SHA1withRSA\">" + sello + "</FRMT>";
                _xDocXml.SelectSingleNode("//TED");
                _xDocXml.DocumentElement.AppendChild(nodoSello);

            }
            catch(Exception fr)
            {
                sMsj = "Excepción al firmar el nodo TED. [TedModel.firma] "+ fr.Message + " " +fr.StackTrace;
                iErr++;
                throw;
            }
        }
    }
}
