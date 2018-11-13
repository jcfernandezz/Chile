using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using Comun;
using System.Security.Cryptography.Xml;
using System.Xml.Linq;
using Encriptador;
using System.Security.Cryptography;
using cfd.FacturaElectronica;
using Spire.Barcode;
using System.Drawing;
using Reporteador;
using MaquinaDeEstados;

namespace FEChile
{
    public class EnvioDteModel
    {
        public int iErr = 0;
        public string sMsj = string.Empty;
        private XmlDocument _xDocXml;
        private DteModel _modeloDte;
        private vwCfdCertificadosService _certPersonaEnvia;
        private TecnicaDeEncriptacion _criptografo;
        private Encoding _encoding;
        private List<CFDComprobanteFiscalDigital> _lDocumentos;         //contiene los cfd
        private DateTime _ts;
        private string _nomArchivo;

        //********************************************************
        #region Propiedades
        private EnvioDTE _envio;

        public EnvioDTE envio
        {
            get { return _envio; }
            set { _envio = value; }
        }
        public XmlDocument xDocXml
        {
            get { return _xDocXml; }
            set { _xDocXml = value; }
        }
        public TecnicaDeEncriptacion criptografo
        {
            get { return _criptografo; }
            set { _criptografo = value; }
        }
        private string _rutaXml = string.Empty;

        public string RutaXml
        {
            get { return _rutaXml; }
            set { _rutaXml = value; }
        }
        private string _setId = string.Empty;
        public string setId
        {
            get { return _setId; }
            set { _setId = value; }
        }
        public List<CFDComprobanteFiscalDigital> lDocumentos
        {
            get { return _lDocumentos; }
            set { _lDocumentos = value; }
        }
        public String RutEmpresaEmisora
        {
            get { return _envio.SetDTE.Caratula.RutEmisor; }
        }
        public String RutPersonaEnvia
        {
            get { return _envio.SetDTE.Caratula.RutEnvia; }
        }
        #endregion
        //********************************************************

        public EnvioDteModel(DteModel modeloDte, vwCfdCertificadosService certificado, string rutaXml, Encoding encoding, string sopnumbe) 
        {
            try
            {
                iErr = 0;
                sMsj = String.Empty;
                _ts = DateTime.Now;
                _ts = new DateTime(_ts.Year, _ts.Month, _ts.Day, _ts.Hour, _ts.Minute, _ts.Second);
                //_ts = new DateTime(2014, 8, 11, 18, 0, 0);

                _xDocXml = new XmlDocument();
                _xDocXml.PreserveWhitespace = true;

                _modeloDte = modeloDte;
                _certPersonaEnvia = certificado;
                _lDocumentos = new List<CFDComprobanteFiscalDigital>();

                _criptografo = new TecnicaDeEncriptacion();

                _rutaXml = rutaXml;
                _encoding = encoding;

                if (sopnumbe.Equals(string.Empty))
                    _nomArchivo = Utiles.FormatoNombreArchivo(_ts.ToString("yyMMddHHmmss"),
                                                         _modeloDte.dteDoc.Encabezado.Receptor.RznSocRecep.Replace('%', '_').Replace('+', '_'), 20);
                else
                    _nomArchivo = Utiles.FormatoNombreArchivo(sopnumbe + "_" + _modeloDte.dteDoc.Encabezado.Receptor.RUTRecep + "_",
                                                         _modeloDte.dteDoc.Encabezado.Receptor.RznSocRecep.Replace('%', '_').Replace('+', '_'), 20);
                _setId = "E" + _nomArchivo;
            }
            catch (Exception ed)
            {
                iErr++;
                sMsj = "Excepción al iniciar el envío. [EnvioDteModel] " + ed.Message;
                
            }
        }

        public EnvioDteModel()
        {
            _criptografo = new TecnicaDeEncriptacion();

        }

        /// <summary>
        /// Carga el objeto EnvioDTE. Un envío por cliente.
        /// </summary>
        /// <param name="lDocumentos">lista de cfds</param>
        public void prepara(List<CFDComprobanteFiscalDigital> lDocumentos, string tipoDoc)
        {
            try
            {
                _envio = new EnvioDTE();
                _envio.SetDTE = new EnvioDTESetDTE();
                _envio.SetDTE.Caratula = new EnvioDTESetDTECaratula();
                _envio.SetDTE.DTE = new DTEDefType[1];

                _envio.SetDTE.ID = _setId;
                _envio.SetDTE.Caratula.version = Convert.ToDecimal("1.0");
                _envio.SetDTE.Caratula.RutEmisor = _modeloDte.dteDoc.Encabezado.Emisor.RUTEmisor;        //rut de la compañía 
                _envio.SetDTE.Caratula.RutEnvia = _certPersonaEnvia.idImpuesto;                          //rut de la persona que envía
                _envio.SetDTE.Caratula.RutReceptor = _modeloDte.idPersonaRecibe;                         //rut de la persona que recibe
                _envio.SetDTE.Caratula.FchResol = Convert.ToDateTime(_certPersonaEnvia.fchResol);
                _envio.SetDTE.Caratula.NroResol = _certPersonaEnvia.nroResol;
                _envio.SetDTE.Caratula.TmstFirmaEnv = _ts;

                if (!tipoDoc.Equals(String.Empty))                                                      //uno por contenedor
                {
                    _envio.SetDTE.Caratula.SubTotDTE = new EnvioDTESetDTECaratulaSubTotDTE[1];
                    _envio.SetDTE.Caratula.SubTotDTE[0] = new EnvioDTESetDTECaratulaSubTotDTE()
                    {
                        TpoDTE = (DOCType) Enum.Parse(typeof(DOCType), tipoDoc),
                        NroDTE = "1"
                    };

                }
                else
                {
                    var docsAgrupados = lDocumentos
                    .GroupBy(t =>
                        new
                        {
                            //cliente = t.modeloDte.dteDoc.Encabezado.Receptor.RUTRecep,
                            cliente = t.modeloDte.idPersonaRecibe,
                            tipoDoc = t.modeloDte.dteDoc.Encabezado.IdDoc.TipoDTE
                        })
                    //.Where(grouping => grouping.Key.cliente.Equals(_modeloDte.dteDoc.Encabezado.Receptor.RUTRecep))
                    .Where(grouping => grouping.Key.cliente.Equals(_modeloDte.idPersonaRecibe))
                    .Select(subTot =>
                        new
                        {
                            tDoc = subTot.Key.tipoDoc,
                            nDoc = subTot.Count()
                        });

                    _envio.SetDTE.Caratula.SubTotDTE = new EnvioDTESetDTECaratulaSubTotDTE[docsAgrupados.Count()];
                    int i = 0;
                    foreach (var item in docsAgrupados)
                    {
                        _envio.SetDTE.Caratula.SubTotDTE[i] = new EnvioDTESetDTECaratulaSubTotDTE()
                            {
                                TpoDTE = (DOCType) Enum.Parse(typeof(DOCType), item.tDoc.ToString()),
                                NroDTE = item.nDoc.ToString()
                            };
                        i++;
                    }
                }
            }
            catch (Exception pr)
            {
                sMsj = pr.Message + " [EnvioDteModel.prepara] " + pr.StackTrace;
                iErr++;
                throw;
            }

        }

        /// <summary>
        /// Prepara y serializa el objeto en el string _sDocXml
        /// </summary>
        /// <param name="objeto">Objeto de cualquier tipo</param>
        public void Serializa()
        {
            try
            {
                String _sDocXml = string.Empty;
                XmlSerializer serializer = new XmlSerializer(_envio.GetType());
                _sDocXml = SerializaObjeto(serializer, _encoding, _envio, true);
                _sDocXml = _sDocXml.Replace("iso-8859-1", "ISO-8859-1");
//                _sDocXml = _sDocXml.Replace("<Caratula version=" + "\"" + "1" + "\"" + ">", "<Caratula version=" + "\"" + "1.0" + "\"" + ">");
                String lDocXml = Comun.Utiles.RemoveAllXmlNamespace(_sDocXml);

                _xDocXml.LoadXml(lDocXml);
            }
            catch (Exception so)
            {
                sMsj = "Error al serializar el documento. " + so.Message + " [EnvioDteModel.Serializa] " + so.StackTrace;
                iErr++;
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
                
                MemoryStream ms = new MemoryStream();
                XmlTextWriter xmlTextWriter = new XmlTextWriter(ms, encoding);
                xmlTextWriter.Formatting = Formatting.None;

                serializer.Serialize(xmlTextWriter, objeto, ns);
                ms = (MemoryStream)xmlTextWriter.BaseStream;
                return encoding.GetString(ms.ToArray());

            }
            catch (Exception oj)
            {
                return "Error al serializar el documento. " + oj.Message + oj.InnerException.ToString() + " [EnvioDteModel.Serializa] " + oj.StackTrace;
                throw;
            }
        }

        public static string SerializaObjetoError(XmlSerializer serializer, Encoding encoding, object objeto, bool omitDeclaration)
        {
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.OmitXmlDeclaration = true;

                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                //ns.Add(String.Empty, String.Empty);

                MemoryStream ms = new MemoryStream();
                using (XmlWriter writer = XmlWriter.Create(ms, settings))
                {
                    serializer.Serialize(writer, objeto, ns);
                }

                return encoding.GetString(ms.ToArray());

            }
            catch (Exception oj)
            {
                return "Error al serializar el documento. " + oj.Message + oj.InnerException.ToString() + " [EnvioDteModel.Serializa] " + oj.StackTrace;
                throw;
            }
        }

        public void Canonicaliza(XmlDocument xEnvioDte)
        {
            try
            {
                sMsj = string.Empty;
                iErr = 0;

                //Canonicalizar: deshabilitado para set de pruebas
                //XmlDsigC14NTransform t = new XmlDsigC14NTransform();
                //t.LoadInput(xEnvioDte);
                //Stream s = (Stream)t.GetOutput(typeof(Stream));
                //_xDocXml.Load(s);

                //string sDocXml = _xDocXml.OuterXml.Replace("><", ">\n<");   //agrega line breaks
                //if (xEnvioDte.FirstChild.NodeType == XmlNodeType.XmlDeclaration)
                //    xEnvioDte.RemoveChild(xEnvioDte.FirstChild);

                string sDocXml = xEnvioDte.OuterXml.Replace("><", ">\n<");   //agrega line breaks
                //string sDocXml = xEnvioDte.OuterXml;   
                _xDocXml.LoadXml(sDocXml);


            }
            catch (Exception cn)
            {
                iErr++;
                sMsj = cn.Message + " [EnvioDteModel.Canonicaliza]";
                throw;
            }
        }

        public void firma(string sUri, String sNodo)
        {
            try
            {
                sMsj = string.Empty;
                iErr = 0;

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

                // Inserta la firma en el documento DTE
                //XmlNamespaceManager ns = new XmlNamespaceManager(_xDocXml.NameTable);
                //ns.AddNamespace("x", _xDocXml.DocumentElement.NamespaceURI);
                //XmlNode nodoEnvio = _xDocXml.SelectSingleNode(sNodo, ns);
                //nodoEnvio.AppendChild(nodoEnvio.OwnerDocument.ImportNode(xFirma, true));

                _xDocXml.DocumentElement.AppendChild(_xDocXml.ImportNode(xFirma, true));
                //xEnv.DocumentElement.AppendChild(xEnv.ImportNode(exFirma, true));

            }
            catch (Exception fr)
            {
                sMsj = "Excepción al firmar el envío DTE. Uri: "+ sUri+" " + fr.Message + " [EnvioDteModel.firma]";
                iErr++;
                throw new InvalidOperationException(sMsj);
            }
        }

        public void VerificaFirma()
        {
            try
            {
                iErr = 0;
                sMsj = String.Empty;

                SignedXml signedDoc = new SignedXml(_xDocXml);
                //XmlNodeList nodeList = _xDocXml.SelectNodes("/EnvioDTE/Signature");
                XmlNodeList nodeList = _xDocXml.GetElementsByTagName("Signature");

                signedDoc.LoadXml((XmlElement)nodeList[1]); //verifica el segundo nodo Signature

                if (!signedDoc.CheckSignature((RSA)criptografo.certificado.PublicKey.Key))
                {
                    sMsj = "La firma del DTE no concuerda con la llave pública. [DteModel.Verifica]";
                    iErr++;
                }

            }
            catch (Exception vf)
            {
                sMsj = "Excepción al verificar la firma del DTE. " + vf.Message + " [DteModel.Verifica]";
                iErr++;
                throw;
            }
        }

        public void validaXsd(String URLArchivoXSD)
        {
            iErr = 0;
            sMsj = String.Empty;

            //Validar el esquema del archivo xml
            ValidadorXML validadorxml = new ValidadorXML(URLArchivoXSD);
            sMsj = validadorxml.sMsj;
            iErr = validadorxml.iErr;

            if (validadorxml.iErr == 0)
            {
                validadorxml.ValidarXSD(_xDocXml);
                iErr = validadorxml.iErr;
                sMsj = validadorxml.sMsj;
            }

            if (iErr != 0)
                sMsj += " " + URLArchivoXSD;
        }

        /// <summary>
        /// Guarda el archivo xml, lo comprime en zip y anota en la bitácora la factura emitida y el nuevo estado binario.
        /// </summary>
        /// <param name="trxVenta">Lista de facturas cuyo índice apunta a la factura que se va procesar.</param>
        /// <param name="comprobante">Documento xml</param>
        /// <param name="mEstados">Nuevo set de estados</param>
        /// <returns>False cuando hay al menos un error</returns>
        public string GuardaArchivo()
        {
            sMsj = "";
            iErr = 0;
            //string nomArchivo = string.Empty;
            string rutaYNomArchivo = string.Empty;
            try
            {   //arma el nombre del archivo xml
                rutaYNomArchivo = _rutaXml.Trim() + _nomArchivo;

                CustomXmlTextWriter tw = new CustomXmlTextWriter(rutaYNomArchivo + ".xml", _encoding.WebName.ToUpper());
                _xDocXml.Save(tw);
                tw.Close();
                    //Comprime el archivo xml
                    //if (_Param.zip)
                    //    Utiles.Zip(rutaYNomArchivo, ".xml");

                    //iErr = codigobb.iErr + reporte.numErr + Utiles.numErr;
                    //sMsj = codigobb.strMensajeErr + " " + reporte.mensajeErr + " " + Utiles.msgErr;

                //    //Si hay error en cbb o pdf o zip anota en la bitácora
                //    if (iErr != 0)
                //        ActualizaFacturaEmitida(trxVenta.Soptype, trxVenta.Sopnumbe, _Conexion.Usuario,
                //                                "emitido", "emitido", mEstados.eBinActualConError,
                //                                mEstados.EnLetras(mEstados.eBinActualConError) + sMsj.Trim());
                //}
                return rutaYNomArchivo;

            }
            catch (DirectoryNotFoundException)
            {
                sMsj = "Verifique la existencia de la ruta indicada en la configuración de Ruta de archivos Xml. No existe la ruta actual: " + _rutaXml.Trim() + " [EnvioDteModel.Guarda]";
                iErr++;
                return rutaYNomArchivo;
            }
            catch (IOException)
            {
                sMsj = "Verifique permisos de escritura en: " + _rutaXml.Trim() + ". No se pudo guardar el archivo xml ni registrar el documento en la bitácora. [EnvioDteModel.Guarda]";
                iErr++;
                return rutaYNomArchivo;
            }
            catch (Exception eAFE)
            {
                if (eAFE.Message.Contains("denied"))
                    sMsj = "Elimine el archivo xml, luego vuelva a intentar. " + eAFE.Message + " [EnvioDteModel.Guarda]";
                else
                    sMsj = "No se puede guardar el archivo XML ni registrar la Bitácora. " + eAFE.Message + " [EnvioDteModel.Guarda] " + eAFE.StackTrace;
                iErr++;
                return rutaYNomArchivo;
            }
        }

        public void Transiciona(int evento)
        {
            iErr = 0;
            try
            {
                foreach (var rec in _lDocumentos)
                {
                    rec.cicloDeVida.Transiciona(evento, _certPersonaEnvia.envia);  //Evento: enviar al SII
                    iErr = rec.cicloDeVida.iErr;
                    sMsj = "Dte: " + rec.modeloDte.SopNumbe + " " + rec.cicloDeVida.sMsj;
                    if (iErr != 0) break;
                }

            }
            catch (Exception tr)
            {
                iErr++;
                sMsj = "Excepción al transicionar los dte del envío. [EnvioDteModel.Transiciona()]" + tr.Message;
            }
        }
        public void GuardaActualiza(String trackid)
        {
            iErr = 0;
            try
            {
                foreach (var rec in _lDocumentos)
                {
                    //String xDoc = envio.xDocXml.InnerXml.Replace("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>", "");
                    rec.GuardaActualiza(trackid);
                    iErr = rec.iErr;
                    sMsj = " " + rec.sopnumbe + " " + rec.sMsj;
                    if (iErr != 0) break;
                }
            }
            catch (Exception tr)
            {
                iErr++;
                sMsj = "Excepción al guardar el estado de los dte del envío en el log. [EnvioDteModel.GuardaActualiza()]" + tr.Message;
            }
        }
    }
}
