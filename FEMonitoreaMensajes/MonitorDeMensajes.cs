using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using OpenPop.Mime;
using OpenPop.Pop3;
using OpenPop.Common.Logging;
using EstructuraMensajeEMail;
using respuestaEnvioDTE;
using CLConfiguracion;
using cfd.FacturaElectronica;

namespace FEMonitoreaMensajes
{
    public class MonitorDeMensajes
    {

        public string sMsj = "";
        public int iErr = 0;
        private List<IMensajeEMail> _newXmlMessages;
        //private List<String> _newNotXmlMessages;
        private List<string> _seenUids;
        private IParametros _Param;
        private IConexionAFuenteDatos _ConResEnvio;
        //private Encoding _encoding;
        private string _emailPop3;
        private int _emailPortIn;
        private List<string> uids;

        public delegate void reportaProgreso(int iAvance, string sMsj);
        public event reportaProgreso Progreso;
        public void MuestraAvance(int iAvance, string sMsj)
        {
            if (Progreso != null)
                Progreso(iAvance, sMsj);
        }

        List<IMensajeEMail> newMessages
        {
            get { return _newXmlMessages; }
        }

        public MonitorDeMensajes(IParametros Param, IConexionAFuenteDatos Conex, string emailPop3, int emailPortIn)
        {
            _Param = Param;
            _ConResEnvio = Conex;
            _emailPop3 = emailPop3;
            _emailPortIn = emailPortIn;
            //_encoding = Encoding.GetEncoding("ISO-8859-1");

        }

        public void MonitoreaComunicacionesInbound(bool primeraVez, String estado)
        {
            _seenUids = new List<string>();
            CFDServicioDespachoRespuestas respuesta = null;

            //Los uids leídos están en la bd
            CargaUidsLeidos();

            if (iErr == 0)
            {
                MuestraAvance(100, "Verificando accesos para firmar envíos... ");
                respuesta = new CFDServicioDespachoRespuestas(_ConResEnvio, _Param);
                respuesta.Progreso += new CFDServicioDespachoRespuestas.reportaProgreso(MuestraAvance);
                iErr = respuesta.IErr;
                sMsj = respuesta.SMsj;
            }

            if (iErr == 0)
            {
                //El origen de los uids es el servidor de correos
                ObtieneTodosLosUids();
            }

            if (iErr == 0)
            {
                if (!primeraVez)
                {
                    ObtieneMensajesXmlNoLeidos();
                    respuesta.NewXmlMessages = _newXmlMessages;
                    respuesta.Recepciona();                     //Recibe _newXmlMessages. 
                    respuesta.ProcesaRespuestasAlProveedor();    //En el caso de facturas de proveedor finaliza guardando la factura en estado publicado y recibido con el uid incluido
                }

                GuardaUidsNoLeidos(estado);          //Guarda el resto de los uids que no tienen xml adjunto

            }
        }

        /// <summary>
        ///  Ejecutar la primera vez para obtener todos los mensajes no leídos del servidor POP3 y marcarlos como leídos.
        /// </summary>
        /// <param name="hostname">Hostname of the server. For example: pop3.live.com</param>
        /// <param name="port">Host port to connect to. Normally: 110 for plain POP3, 995 for SSL POP3</param>
        /// <param name="useSsl">Whether or not to use SSL to connect to server</param>
        /// <param name="username">Username of the user on the server</param>
        /// <param name="password">Password of the user on the server</param>
        /// <param name="seenUids">List of UID's of all messages seen before. New message UID's will be added to the list. Consider using a HashSet if you are using >= 3.5 .NET
        /// </param>
        /// <returns>A List of new Messages on the server on _newMessages</returns>
        public void ObtieneTodosLosUids() //string hostname, int port, bool useSsl, string username, string password)
        {
            MuestraAvance(100, "Revisando nuevos correos... ");

            try
            {
                iErr = 0;
                sMsj = string.Empty;
                // The client disconnects from the server when being disposed
                using (Pop3Client client = new Pop3Client())
                {
                    // Connect to the server
                    client.Connect(_emailPop3, _emailPortIn, _Param.emailSsl);

                    // Authenticate ourselves towards the server
                    client.Authenticate(_Param.emailUser, _Param.emailPwd);

                    // Fetch all the current uids seen
                    uids = client.GetMessageUids();

                    //var notSeenUids = uids.Except(_seenUids);

                    //foreach (var unv in notSeenUids)
                    //{
                    //    // Add the uid to the seen uids, as it has now been seen
                    //    _seenUids.Add(unv.ToString());
                    //}
                }
            }
            catch (Exception fu)
            {
                iErr++;
                sMsj = "Excepción al leer correos. " + fu.Message + " [MonitorDeMensajes.ObtieneTodosLosUids]";
            }
        }

        public void CargaUidsLeidos()
        {
            MuestraAvance(100, "Identificando correos procesados... ");

            LogFacturaCompraService log = new LogFacturaCompraService(_ConResEnvio.ConnStr);
            try
            {
                _seenUids = log.TraeTodos();
                iErr = log.IErr;
                sMsj = log.SMsj;
            }
            catch (Exception l)
            {
                Console.WriteLine(l.Message);
            }
        }

        /// <summary>
        /// Guarda en la bd los uids no leídos que no tienen xml adjunto: todos los uids excepto los ya leídos
        /// </summary>
        /// <param name="estado"></param>
        public void GuardaUidsNoLeidos( String estado)
        {
            iErr = 0;
            sMsj = String.Empty;
            MuestraAvance(100, "Guardando correos procesados... ");

            try
            {
                LogFacturaCompraService log = new LogFacturaCompraService(_ConResEnvio.ConnStr);
                //var notSeenUids = uids.Except(_seenUids).Where(x => int.Parse(x)<= 19674); //Antes de este Id de correo no se toman en cuenta para factura electrónica
                var notSeenUids = uids.Except(_seenUids);

                foreach (var r in notSeenUids)
                {
                    try
                    {
                        log.Save(0, r.ToString(), "Carga inicial", "-", DateTime.Now, estado, "email no tiene xml adjunto", 0, "-", "Carga inicial", String.Empty, String.Empty, r.ToString(), "-");
                        MuestraAvance(100, "Guardando otros correos... " + r.ToString());
                    }
                    catch (Exception l)
                    {
                        Console.WriteLine(l.Message);
                    }
                }

            }
            catch (Exception nl)
            {
                iErr++;
                sMsj = "Excepción al guardar emails no leídos. [MonitorDeMensajes.GuardaUidsNoLeidos()] Status: "+ estado +" " + nl.Message;
                MuestraAvance(100, sMsj);

            }
        }

        /// <summary>
        ///  Obtiene mensajes no leídos con adjunto xml del servidor POP3 en la lista _newXmlMessages
        ///  Los mensajes no leídos que no tienen un xml adjunto están en la lista _seenUids
        ///    (notice that the POP3 protocol cannot see if a message has been read on the server
        ///     before. Therefore the client need to maintain this state for itself)
        /// 5/2/15 jcf Si encuentra un mail defectuoso lo salta y continúa con el resto
        /// </summary>
        /// <param name="hostname">Hostname of the server. For example: pop3.live.com</param>
        /// <param name="port">Host port to connect to. Normally: 110 for plain POP3, 995 for SSL POP3</param>
        /// <param name="useSsl">Whether or not to use SSL to connect to server</param>
        /// <param name="username">Username of the user on the server</param>
        /// <param name="password">Password of the user on the server</param>
        /// <param name="seenUids">List of UID's of all messages seen before. New message UID's will be added to the list. Consider using a HashSet if you are using >= 3.5 .NET
        /// </param>
        /// <returns>A List of new Messages on the server on _newMessages</returns>
        public void ObtieneMensajesXmlNoLeidos()
        {
            try
            {
                iErr = 0;
                sMsj = string.Empty;
                _newXmlMessages = new List<IMensajeEMail>();
                                // The client disconnects from the server when being disposed
                using (Pop3Client client = new Pop3Client())
                {
                    // Connect to the server
                    client.Connect(_emailPop3, _emailPortIn, _Param.emailSsl);

                    // Authenticate ourselves towards the server
                    client.Authenticate(_Param.emailUser, _Param.emailPwd);
                    var notSeenUids = uids.Except(_seenUids);
                    //notSeenUids = notSeenUids.Where(x => int.Parse(x) > 19674); //test
                    int numCorreos = notSeenUids.Count();
                    foreach (var unv in notSeenUids)
                    {
                        string currentUidOnServer = unv.ToString();
                        try
                        {
                            // the uids list is in messageNumber order - meaning that the first
                            // uid in the list has messageNumber of 1, and the second has 
                            // messageNumber 2. Therefore we can fetch the message using
                            // i + 1 since messageNumber should be in range [1, messageCount]
                            int posiMensaje = uids.IndexOf(currentUidOnServer);
                            Message unseenMessage = client.GetMessage(posiMensaje + 1);
                            IMensajeEMail mensajeRecibido = (IMensajeEMail)new MensajeEMail(unseenMessage);
                            mensajeRecibido.Uid = currentUidOnServer;
                            bool existeXml = mensajeRecibido.FindXmlInMessage();
                            iErr = mensajeRecibido.IErr;
                            // Add the message to the new messages
                            if (iErr == 0 && existeXml)
                            {
                                _newXmlMessages.Add(mensajeRecibido);
                                // Add the uid to the seen uids, as it has now been seen
                                _seenUids.Add(currentUidOnServer);

                                MuestraAvance(100 / numCorreos, "Correo recibido el " +mensajeRecibido.dateSent.ToString()+ " Adjunto: " + mensajeRecibido.nombreArchivoXml);
                            }
                            else
                            {
                                //_newNotXmlMessages.Add(currentUidOnServer);
                                MuestraAvance(100 / numCorreos, "Correo recibido sin adjunto el " + mensajeRecibido.dateSent.ToString());
                            }
                        }
                        catch(Exception em)
                        {
                            sMsj = "Excepción desconocida al abrir el correo con Id: " + currentUidOnServer + "[MonitorDeMensajes.ObtieneMensajesXmlNoLeidos] " + em.Message;
                            //_seenUids.Add(currentUidOnServer);
                            MuestraAvance(100 / numCorreos, sMsj);
                        }
                    }
                }
            }
            catch (Exception fu)
            {
                iErr++;
                sMsj = "Excepción al leer correos. " + fu.Message + "[MonitorDeMensajes.ObtieneMensajesXmlNoLeidos]";
            }
        }

        /// <summary>
        ///  - obtiene mensajes no leídos del servidor POP3
        ///  - how to download messages not seen before
        ///    (notice that the POP3 protocol cannot see if a message has been read on the server
        ///     before. Therefore the client need to maintain this state for itself)
        /// </summary>
        /// <param name="hostname">Hostname of the server. For example: pop3.live.com</param>
        /// <param name="port">Host port to connect to. Normally: 110 for plain POP3, 995 for SSL POP3</param>
        /// <param name="useSsl">Whether or not to use SSL to connect to server</param>
        /// <param name="username">Username of the user on the server</param>
        /// <param name="password">Password of the user on the server</param>
        /// <param name="seenUids">List of UID's of all messages seen before. New message UID's will be added to the list. Consider using a HashSet if you are using >= 3.5 .NET
        /// </param>
        /// <returns>A List of new Messages on the server</returns>
        public List<IMensajeEMail> FetchUnseenMessages(string hostname, int port, bool useSsl, string username, string password, List<string> seenUids)
        {
        try 
	        {
                iErr = 0;
                sMsj = string.Empty;
	            // The client disconnects from the server when being disposed
                using (Pop3Client client = new Pop3Client())
                {
                    // Connect to the server
                    client.Connect(hostname, port, useSsl);

                    // Authenticate ourselves towards the server
                    client.Authenticate(username, password);

                    // Fetch all the current uids seen
                    List<string> uids = client.GetMessageUids();

                    // Create a list we can return with all new messages
                    //_lMensajes = new List<XDocument>();
                    _newXmlMessages = new List<IMensajeEMail>();

                    // Messages are numbered in the interval: [1, messageCount]
                    // Ergo: message numbers are 1-based.
                    // Most servers give the latest message the highest number
                    int messageCount = uids.Count();
                    int primeros = 5;
                    for (int i = messageCount; i > 0 && i > messageCount - primeros; i--)
                    {
                        string currentUidOnServer = uids[i-1];
                        if (!seenUids.Contains(currentUidOnServer))
                        {
                            // the uids list is in messageNumber order - meaning that the first
                            // uid in the list has messageNumber of 1, and the second has 
                            // messageNumber 2. Therefore we can fetch the message using
                            // i + 1 since messageNumber should be in range [1, messageCount]
                            Message unseenMessage = client.GetMessage(i);
                            
                            IMensajeEMail mensajeRecibido = (IMensajeEMail) new MensajeEMail(unseenMessage);
                            mensajeRecibido.FindXmlInMessage();
                            iErr = mensajeRecibido.IErr;

                            // Add the message to the new messages
                            if (iErr == 0)
                            {
                                _newXmlMessages.Add(mensajeRecibido);
                                //_lMensajes.Add(_docXml);
                            }

                            // Add the uid to the seen uids, as it has now been seen
                            seenUids.Add(currentUidOnServer);
                        }
                    }

                    // Return our new found messages
                    return _newXmlMessages;
                }
	        }
	        catch (Exception fu)
	        {
                iErr++;
                sMsj = "Excepción al leer correos. " + fu.Message + "[MonitorDeMensajes.FetchUnseenMessages]";
                return null;
	        }
        }

        /// <summary>
        /// Example showing:
        ///  - how to find a MessagePart with a specified MediaType
        ///  - how to get the body of a MessagePart as a string
        /// </summary>
        /// <param name="message">The message to examine for xml</param>
        //public MensajeEMail FindXmlInMessage(Message message)
        //{
        //    MensajeEMail mensajeLeido = new MensajeEMail();
        //    try
        //    {
        //        iErr = 0;
        //        sMsj = string.Empty;
        //        //XDocument docXml = new XDocument();
        //        //string nomArchivo = string.Empty;

        //        MessagePart xml = message.FindFirstMessagePartWithMediaType("text/xml");
        //        if (xml != null)
        //        {
        //            mensajeLeido.nombreArchivoXml = xml.FileName;
        //            // Get out the XML string from the email
        //            string xmlString = xml.GetBodyAsText();

        //            // Load in the XML read from the email
        //            //_docXml.LoadXml(xmlString);
        //            mensajeLeido.DocumentoXml = XDocument.Parse(xmlString, LoadOptions.PreserveWhitespace);
        //            // Save the xml to the filesystem
        //            //doc.Save("test.xml");
        //            mensajeLeido.Mensaje = message;
        //        }
        //        else
        //            iErr--;
        //        return mensajeLeido;

        //    }
        //    catch (Exception fx)
        //    {
        //        iErr++;
        //        sMsj = "Excepción al buscar archivo xml adjunto en el correo. " + fx.Message + "[FEMonitorDeMensajes.FindXmlInMessage]";
        //        return mensajeLeido;
        //    }
        //}
        /// <summary>
        ///  - how to fetch all messages from a POP3 server
        /// </summary>
        /// <param name="hostname">Hostname of the server. For example: pop3.live.com</param>
        /// <param name="port">Host port to connect to. Normally: 110 for plain POP3, 995 for SSL POP3</param>
        /// <param name="useSsl">Whether or not to use SSL to connect to server</param>
        /// <param name="username">Username of the user on the server</param>
        /// <param name="password">Password of the user on the server</param>
        /// <returns>All Messages on the POP3 server</returns>
        //public static List<Message> FetchAllMessages(string hostname, int port, bool useSsl, string username, string password)
        //{
        //    // The client disconnects from the server when being disposed
        //    using (Pop3Client client = new Pop3Client())
        //    {
        //        // Connect to the server
        //        client.Connect(hostname, port, useSsl);

        //        // Authenticate ourselves towards the server
        //        client.Authenticate(username, password);

        //        // Get the number of messages in the inbox
        //        int messageCount = client.GetMessageCount();

        //        // We want to download all messages
        //        List<Message> allMessages = new List<Message>(messageCount);

        //        // Messages are numbered in the interval: [1, messageCount]
        //        // Ergo: message numbers are 1-based.
        //        // Most servers give the latest message the highest number
        //        for (int i = messageCount; i > 0; i--)
        //        {
        //            allMessages.Add(client.GetMessage(i));
        //        }

        //        // Now return the fetched messages
        //        return allMessages;
        //    }
        //}

        /// <summary>
        /// Example showing:
        ///  - How to change logging
        ///  - How to implement your own logger
        /// </summary>
        public static void ChangeLogging()
        {
            // All logging is sent trough logger defined at DefaultLogger.Log
            // The logger can be changed by calling DefaultLogger.SetLog(someLogger)

            // By default all logging is sent to the System.Diagnostics.Trace facilities.
            // These are not very useful if you are not debugging
            // Instead, lets send logging to a file:
            DefaultLogger.SetLog(new FileLogger());
            FileLogger.LogFile = new FileInfo("MyLoggingFile.log");

            // It is also possible to implement your own logging:
            //DefaultLogger.SetLog(new MyOwnLogger());
        }
    }
}
