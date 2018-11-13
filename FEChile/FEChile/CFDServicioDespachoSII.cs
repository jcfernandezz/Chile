using Encriptador;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
//using System.Diagnostics;

namespace FEChile
{
    public class CFDServicioDespachoSII
    {
        public static int IErr = 0;
        public static String SMsj = String.Empty;
        public static TecnicaDeEncriptacion Encriptador = new TecnicaDeEncriptacion();
        public static string uri = "https://maullin.sii.cl/cgi_dte/UPL/DTEUpload";
        //public static XDocument xdocument;

        public static String nombreArchivo = String.Empty;  // @"C:\GPUsuario\GPExpressCfdi\feGilaChiTST\E33-0000118_78504500-9_OCHOLIBROSEDITORE.xml";
        public static string rutEmisor = String.Empty;      //"87115925";
        public static string rutEmpresa = String.Empty;      //"760555681";
        public static String trackId = String.Empty;
        public static String status = String.Empty;

        /// <summary>
        /// Carga un archivo al SII vía HTTP
        /// </summary>
        /// <param name="valorToken">Este valor se obtiene ejecutando el método GetToken()</param>
        public static void UploadFile(String valorToken, XmlDocument xDocEnvio)
        {
            IErr = 0;
            try
            {
                //String valorToken = GetToken();

                string pRutEmisor = rutEmisor.Substring(0, (rutEmisor.Length - 2));
                string pDigEmisor = rutEmisor.Substring(rutEmisor.Length - 1);
                string pRutEmpresa = rutEmpresa.Substring(0, (rutEmpresa.Length - 2));
                string pDigEmpresa = rutEmpresa.Substring(rutEmpresa.Length - 1);

                // Header del request a enviar al SII
                StringBuilder data = new StringBuilder();
                data.Append("--7d23e2a11301c4\r\n");
                data.Append("Content-Disposition: form-data; name=\"rutSender\"\r\n");
                data.Append("\r\n");
                data.Append(pRutEmisor + "\r\n");
                data.Append("--7d23e2a11301c4\r\n");
                data.Append("Content-Disposition: form-data; name=\"dvSender\"\r\n");
                data.Append("\r\n");
                data.Append(pDigEmisor + "\r\n");
                data.Append("--7d23e2a11301c4\r\n");
                data.Append("Content-Disposition: form-data; name=\"rutCompany\"\r\n");
                data.Append("\r\n");
                data.Append(pRutEmpresa + "\r\n");
                data.Append("--7d23e2a11301c4\r\n");
                data.Append("Content-Disposition: form-data; name=\"dvCompany\"\r\n");
                data.Append("\r\n");
                data.Append(pDigEmpresa + "\r\n");
                data.Append("--7d23e2a11301c4\r\n");
                data.Append("Content-Disposition: form-data; name=\"archivo\"; filename=\"" + nombreArchivo + "\"\r\n");
                data.Append("Content-Type: text/xml\r\n");
                data.Append("\r\n");
                //XDocument xdocument = XDocument.Load(nombreArchivo, LoadOptions.PreserveWhitespace);

                // Cargue el documento en el objeto secuencia
                //data.Append("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>\r");
                //data.Append(xdocument.ToString(SaveOptions.DisableFormatting));
                data.Append(xDocEnvio.InnerXml);
                data.Append("\r\n--7d23e2a11301c4--\r\n");

                UploadFileViaWebRequest(data, valorToken);

            }
            catch (Exception uf)
            {
                IErr++;
                SMsj = "Excepción al preparar la carga del archivo en el SII. [CFDServicioDespachoSII.UploadFile()]" + uf.Message;
            }
        }

        /// <summary>
        /// Carga el xml en el sitio web del SII
        /// </summary>
        /// <param name="data">datos a cargar</param>
        /// <param name="valorToken">Valor asignado por el SII</param>
        private static void UploadFileViaWebRequest(StringBuilder data, string valorToken)
        {
            // Parametros del header.
            string pMethod = "POST";
            string pAccept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg,application/vnd.ms-powerpoint, application/ms-excel,application/msword, */*";
            string pReferer = "www.hefestosDte.cl";
            string pToken = "TOKEN={0}";

            // Cree un nuevo request para iniciar el proceso
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = pMethod;
            request.Accept = pAccept;
            request.Referer = pReferer;

            // Agregar el content-type
            request.ContentType = "multipart/form-data: boundary=7d23e2a11301c4";
            request.ContentLength = data.Length;

            // Defina manualmente los headers del request
            request.Headers.Add("Accept-Language", "es-cl");
            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("Cookie", string.Format(pToken, valorToken));

            // Defina el user agent
            request.UserAgent = "Mozilla/4.0 (compatible; PROG 1.0; Windows NT 5.0; YComp 5.0.2.4)";
            request.KeepAlive = true;

            string respuestaSii = string.Empty;

            try
            {

                // Recupere el streamwriter para escribir la secuencia
                using (StreamWriter sw = new StreamWriter(request.GetRequestStream(), Encoding.GetEncoding("ISO-8859-1")))
                {
                    String sData = data.ToString();
                    sw.Write(sData);
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                    {
                        respuestaSii = sr.ReadToEnd().Trim();
                        RevisaRespuestaUploadDelSII(respuestaSii);
                    }

                }
                if (string.IsNullOrEmpty(respuestaSii))
                {
                    IErr++;
                    SMsj = "La respuesta del SII es null. Espere un momento y vuelva a intentar. [CFDServicioDespachoSII.UploadFileViaWebRequest()] ";
                }
            }
            catch (Exception ex)
            {
                IErr++;
                SMsj = "Excepción desconocida al cargar el archivo en el SII. [CFDServicioDespachoSII.UploadFileViaWebRequest()] " + ex.Message;
            }
        }

        private static void UploadFileViaHttp(StringBuilder data, string valorToken)
        {
            IErr = 0;
            //string data = string.Empty;
            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "es-cl");
            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "multipart/form-data: boundary=7d23e2a11301c4");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cache-Control", "no-cache");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Cookie", "TOKEN=" + valorToken.Trim());
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/4.0 (compatible; PROG 1.0; Windows NT 5.0; YComp 5.0.2.4)");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "Keep-Alive");

            try
            {
                httpClient.BaseAddress = new Uri(uri);
                var response = httpClient.PostAsync(uri, new StringContent(data.ToString())).Result;

                //response.EnsureSuccessStatusCode();
                //string content = await response.Content.ReadAsStringAsync();

                String responseBody = httpClient.GetStringAsync(uri).Result;

                RevisaRespuestaUploadDelSII(responseBody);

                //return await Task.Run(() =&gt; JsonObject.Parse(content));
                //Console.WriteLine(responseBody);
            }
            catch (Exception e)
            {
                IErr++;
                SMsj = "Excepción al cargar el archivo en el SII. [CFDServicioDespachoSII.UploadFileViaHttp()] " + e.Message;
            }

            // Need to call dispose on the HttpClient object 
            // when done using it, so the app doesn't leak resources
            httpClient.Dispose();

        }

        public static string GetToken()
        {

            IErr = 0;
            try
            {
                String valorSemilla = String.Empty;
                using (var svc = new WebRefCrSeed.CrSeedService())
                {
                    String sSemilla = svc.getSeed();

                    var xDoc = XElement.Parse(sSemilla);
                    var xSemilla = xDoc.Elements().Where(r => r.Name == "{http://www.sii.cl/XMLSchema}RESP_BODY")
                                        .Elements().Where(s => s.Name == "SEMILLA");

                    foreach (XElement e in xSemilla)
                        valorSemilla = e.Value;
                }

                String valorToken = String.Empty;
                string sToken = GetSignedToken(valorSemilla);

                if (IErr == 0)
                {
                    var xToken = XElement.Parse(sToken);
                    var eToken = xToken.Elements().Where(r => r.Name == "{http://www.sii.cl/XMLSchema}RESP_BODY")
                            .Elements().Where(s => s.Name == "TOKEN");

                    foreach (XElement e in eToken)
                        valorToken = e.Value;
                }

                return valorToken;

            }
            catch (Exception gt)
            {
                IErr++;
                SMsj = "Excepción al obtener permiso del SII. [CFDServicioDespachoSII.GetToken()] " + gt.Message;
            }
            return String.Empty;
        }

        /// <summary>
        /// Firma la semilla para poder validarla en el SII
        /// </summary>
        private static string GetSignedToken(string seed)
        {
            IErr = 0;
            //// Construya el cuerpo del documento en formato string.
            string signedSeed = string.Empty;
            String token = String.Empty;
            string body = "<getToken><item><Semilla>" + seed + "</Semilla></item></getToken>";
            //string body = string.Format("<getToken><item><Semilla>{0}</Semilla></item></getToken>", double.Parse(seed).ToString());

            try
            {
                signedSeed = FirmarSemilla(body);

                if (IErr == 0)
                {
                    //// asigne el valor al metodo GetToken()
                    using (var gToken = new WebRefGetToken.GetTokenFromSeedService())
                    {
                        token = gToken.getToken(signedSeed);
                    }
                }
            }
            catch (Exception gt)
            {
                IErr++;
                SMsj = "Excepción al firmar token [CFDServicioDespachoSII.GetSignedToken()]" + gt.Message;
                signedSeed = string.Empty;
            }

            return token;
        }

        /// <summary>
        /// Firma el documento xml semilla
        /// </summary>
        /// <param name="documento"></param>
        /// <returns></returns>
        public static string FirmarSemilla(string documento)
        {
            //Encoding _encoding = Encoding.GetEncoding("ISO-8859-1");
            //TecnicaDeEncriptacion _encriptador = new TecnicaDeEncriptacion();
            //_encriptador.PreparaEncriptacion("", "GettyCert2014", @"C:\GPUsuario\GPExpressCfdi\feGilaChiTST\certificado\CERTIFICADO ELECTRONICO MARIANA REBOLLEDO.p12", "");

            //// Cree un nuevo documento xml y defina sus caracteristicas
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = false;
            doc.LoadXml(documento);

            //// Cree el objeto XMLSignature.
            SignedXml signedXml = new SignedXml(doc);

            //// Agregue la clave privada al objeto xmlSignature.
            signedXml.SigningKey = Encriptador.certificado.PrivateKey; // certificado.PrivateKey;

            //// Obtenga el objeto signature desde el objeto SignedXml.
            Signature XMLSignature = signedXml.Signature;

            //// Cree una referencia al documento que va a firmarse
            //// si la referencia es "" se firmara todo el documento
            Reference reference = new Reference("");

            //// Representa la transformación de firma con doble cifrado para una firma XML  digital que define W3C.
            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            //// Agregue el objeto referenciado al obeto firma.
            XMLSignature.SignedInfo.AddReference(reference);

            //// Agregue RSAKeyValue KeyInfo  ( requerido para el SII ).
            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new RSAKeyValue((RSA)Encriptador.certificado.PrivateKey));

            //// Agregar información del certificado x509
            keyInfo.AddClause(new KeyInfoX509Data(Encriptador.certificado));

            //// Agregar KeyInfo al objeto Signature 
            XMLSignature.KeyInfo = keyInfo;

            //// Cree la firma
            signedXml.ComputeSignature();

            //// Recupere la representacion xml de la firma
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            //// Agregue la representacion xml de la firma al documento xml
            doc.DocumentElement.AppendChild(doc.ImportNode(xmlDigitalSignature, true));

            //// Limpie el documento xml de la declaracion xml ( Opcional, pera para nuestro proceso es valido  )
            if (doc.FirstChild is XmlDeclaration)
            {
                doc.RemoveChild(doc.FirstChild);
            }

            //// Regrese el valor de retorno
            return doc.InnerXml;
        }

        private static void RevisaRespuestaUploadDelSII(String respuesta)
        {
            IErr = 0;
            SMsj = String.Empty;
            //String sdoc = "<?xml version=\"1.0\" ?><RECEPCIONDTE><RUTSENDER>1-9</RUTSENDER><RUTCOMPANY >3-5</RUTCOMPANY><FILE>EnvioEjemplo.xml</FILE><TIMESTAMP>2002-11-25 18:51:44</TIMESTAMP><STATUS>0</STATUS><TRACKID>39</TRACKID><DETAIL><ERROR>LSX-00265: attribute \"version\" value \"3.2\" is wrong (must be \".2\")</ERROR><ERROR>LSX-00213: only 0 occurrences of particle \"sequence\", minimum is 1</ERROR></DETAIL></RECEPCIONDTE>";

            try
            {
                XElement xRespuestaSII = XElement.Parse(respuesta);
                status = xRespuestaSII.Element("STATUS").Value;
                String sArchivo = xRespuestaSII.Element("FILE").Value;
                trackId = xRespuestaSII.Element("TRACKID").Value;
                var errores = xRespuestaSII.Elements("DETAIL").Elements();

                if (!status.Equals("0"))
                {
                    foreach (var r in errores)
                        SMsj += r.Value;
                    IErr++;
                    SMsj += " Status: "+status;
                }
            }
            catch (NullReferenceException nr)
            {
                string res = respuesta == null ? "" : respuesta;
                if (status.Equals("5"))
                    res += " No está autenticado. Revise si el certificado está vigente. ";
                SMsj = string.Concat("No se encuentra el trackId del SII. " , nr.Message , " [CFDServicioDespachoSII.RevisaRespuestaUploadDelSII] " , res) ;
                
                IErr++;
            }
            catch (Exception re)
            {
                string res = respuesta == null ? "" : respuesta;
                SMsj = "Excepción desconocida al revisar la respuesta del SII luego de cargar el archivo. " + re.Message + " [CFDServicioDespachoSII.RevisaRespuestaUploadDelSII] " + res;
                IErr++;
            }
        }

    }
}
