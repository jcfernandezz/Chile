using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Serialization;
using System.IO;

using Encriptador;
using Comun;
using cfd.FacturaElectronica;
using MaquinaDeEstados;
using Spire.Barcode;
using System.Drawing;
using Reporteador;

namespace FEChile
{
    public class CFDComprobanteFiscalDigital
    {
        public int iErr = 0;
        public string sMsj = string.Empty;

        private string _sDocXml = string.Empty;
        private string _sopnumbe = string.Empty;
        private short _soptype;
        private string _idCliente;
        private string _custnmbr;
        private string _nombreCliente;
        private string _rutaXml = string.Empty;
        private string _mensaje = string.Empty;
        private string _nomArchivo;
        private string _setId;
        private DateTime _fechaHora = DateTime.Now;
        private Encoding _encoding;
        private Parametros _Param;
        private ConexionAFuenteDatos _Conex;
        private Maquina _cicloDeVida;
        TransformerXML loader = new TransformerXML();
        //XslCompiledTransform xslCompilado;
        //ValidadorXML validadorxml;

        public DateTime ts = DateTime.Now;
        private DteModel _modeloDte;

        private LogFacturaXMLService _bitacora;

        public CFDComprobanteFiscalDigital(ConexionAFuenteDatos conex, Parametros Param, Encoding encoding, string compoundedBinStatus, string idxSingleStatus, short voidStts,
                                            string sopnumbe, short soptype, string idCliente, string custNmbr, string nombreCliente, String tipoDocumento)
        {
            try
            {
                iErr=0;
                sMsj = string.Empty;

                _Param = Param;
                _Conex = conex;
                _encoding = encoding;
                _modeloDte = new DteModel(encoding);
                _sopnumbe = sopnumbe;
                _soptype = soptype;
                _idCliente = idCliente;
                _custnmbr = custNmbr;
                _nombreCliente = nombreCliente;
                _nomArchivo = Utiles.FormatoNombreArchivo(_sopnumbe + "_" + _idCliente + "_", _nombreCliente, 20);
                _bitacora = new LogFacturaXMLService(_Conex, tipoDocumento);
                _cicloDeVida = new Maquina(compoundedBinStatus, idxSingleStatus, voidStts, "emisor", tipoDocumento);
                ts = DateTime.Now;
                ts = new DateTime(ts.Year, ts.Month, ts.Day, ts.Hour, ts.Minute, ts.Second);

            }
            catch (Exception ini)
            {
                sMsj = "Excepción al inicializar cfd. " + ini.Message + " [CFDComprobanteFiscalDigital constructor]";
                iErr++;
                throw;
            }
        }
        //**********************************************************
        #region Propiedades
        public DteModel modeloDte
        {
            get { return _modeloDte; }
            set { _modeloDte = value; }
        }
        public Maquina cicloDeVida
        {
            get { return _cicloDeVida; }
            set { _cicloDeVida = value; }
        }
        public string sopnumbe
        {
            get { return _sopnumbe; }
            set { _sopnumbe = value; }
        }
        public short soptype
        {
            get { return _soptype; }
            set { _soptype = value; }
        }
        public string idCliente
        {
            get { return _idCliente; }
            set { _idCliente = value; }
        }
        public string custnmbr
        {
            get { return _custnmbr; }
            set { _custnmbr = value; }
        }
        public string nombreCliente
        {
            get { return _nombreCliente; }
            set { _nombreCliente = value; }
        }
        public string rutaXml
        {
            get { return _rutaXml; }
            set { _rutaXml = value; }
        }
        public string mensaje
        {
            get { return _mensaje; }
            set { _mensaje = value; }
        }
        public string nomArchivo
        {
            get { return _nomArchivo; }
        }
        public string setId
        {
            get { return _setId; }
            set { _setId = value; }
        }
        public DateTime fechaHora
        {
            get { return _fechaHora; }
            set { _fechaHora = value; }
        }

        #endregion
        //************************************************************************
        /// <summary>
        /// Arma un cfd vigente (no anulado)
        /// </summary>
        /// <param name="loteCfds">Está posicionado en el cfd que se requiere</param>
        public void ensamblaCfd(vwCfdTransaccionesDeVenta loteCfds)
        {
            try
            {
                iErr = 0;
                sMsj = string.Empty;
                _rutaXml = loteCfds.RutaXml;
                _modeloDte.idPersonaRecibe = loteCfds.IdClienteRepLegal;        //persona que recibe facturas por parte del cliente

                //Obtiene objeto Dte
                _modeloDte.DeSerializa(loteCfds.ComprobanteXml);

                //Crea y firma nodo ted
                _modeloDte.ensamblaTed(ts);

                //Canonicaliza documento original
                _modeloDte.Canonicaliza(loteCfds.ComprobanteXml, ts);
                
                //firma al enviar
                _modeloDte.firma(_modeloDte.dteDoc.ID);

                _modeloDte.VerificaFirma();

                iErr = _modeloDte.iErr;
                sMsj = _modeloDte.sMsj;

                //Validar el esquema del archivo xml
                ValidadorXML validadorxml = new ValidadorXML(_Param.URLArchivoXSD);
                if (iErr == 0)
                {
                    iErr = validadorxml.iErr;
                    sMsj = validadorxml.sMsj;
                }

                if (iErr == 0)
                {
                    validadorxml.ValidarXSD(_modeloDte.xDocXml);    
                    iErr = validadorxml.iErr;
                    sMsj = validadorxml.sMsj;
                }

                //Otras validaciones
                //if (iErr==0)
                //    validaClientesEspeciales(_modeloDte.xDocXml);

            }
            catch (Exception ex)
            {
                sMsj = "Excepción al ensamblar factura. " + ex.Message + " [CFDComprobanteFiscalDigital.ensamblaCfd] " + _modeloDte.sMsj;
                iErr++;
            }
        }

        private void validaClientesEspeciales(XmlDocument docAValidar)
        {
            iErr = 0;
            sMsj = string.Empty;

            XmlNode elemento = docAValidar.DocumentElement;
            String referencia = String.Empty;
            try 
	        {	
		        referencia = elemento.SelectSingleNode("//EnvioDTE/SetDTE/DTE/Documento/Referencia/NroLinRef/text()").Value;
                if (!referencia.Equals(String.Empty))
                {
                    referencia = elemento.SelectSingleNode("//EnvioDTE/SetDTE/DTE/Documento/Referencia/TpoDocRef/text()").Value;
                }
	        }
	        catch (Exception)
	        {
		
		        throw;
	        }

        }

        public void ensamblaDte(string sDte)
        {
            try
            {
                iErr = 0;
                sMsj = string.Empty;
               
            }
            catch (Exception td)
            {
                sMsj = "Excepción al ensamblar nodo DTE. [CFDComprobanteFIscalDigital.ensamblaDte] " + td.Message + " " + td.StackTrace;
                iErr++;
                throw;
            }
        }

        /// <summary>
        /// Prepara y serializa el objeto en un string
        /// </summary>
        /// <param name="objeto">Objeto de cualquier tipo</param>
        public void SerializaObjeto(object objeto)
        {
            try
            {
                //XmlDocument _xDocXml = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(objeto.GetType());
                Encoding encoding = Encoding.GetEncoding("ISO-8859-1");
                _sDocXml = Serialize(serializer, encoding, objeto);
                //_xDocXml.LoadXml(_sDocXml);
            }
             catch(Exception so)
            {
                sMsj = "Error al serializar el documento. " + so.Message + " [SerializaObjeto] " + so.StackTrace;
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
        public static string Serialize(XmlSerializer serializer, Encoding encoding, object objeto)
         {
             try
             {
                 MemoryStream ms = new MemoryStream();
                 XmlTextWriter xmlTextWriter = new XmlTextWriter(ms, encoding);
                 xmlTextWriter.Formatting = Formatting.Indented;
                 serializer.Serialize(xmlTextWriter, objeto);
                 ms = (MemoryStream)xmlTextWriter.BaseStream;
                 return encoding.GetString(ms.ToArray());
                 
             }
             catch(Exception)
             {
                 return "";
                 throw;
             }
         }

        public void anula(short soptType, string sopNumbe, string eBinarioNuevo)
        {
            try
            {
                sMsj = "";
                iErr = 0;
                _bitacora.Save(soptType, sopNumbe, "Anulado en GP", "0", _Conex.Usuario, "", "emitido", eBinarioNuevo, "Anulado en GP y marcado como emitido.", "0");
            }
            catch(Exception an)
            {
                sMsj = "Excepción encontrada al anular factura. [CFDComprobanteFiscalDigital.anula] " + an.StackTrace;
                iErr++;
            }
        }

        public void GuardaPdf(String modoImpresion)
        {
            sMsj = "";
            iErr = 0;
            string rutaYNomArchivo = _rutaXml.Trim() + _nomArchivo;
            try
            {
                Documento reporte = new Documento(_Conex, _Param);
                reporte.guardaPDF(rutaYNomArchivo, _soptype, _sopnumbe, modoImpresion);
                iErr = reporte.numErr;
                sMsj = reporte.mensajeErr;

            }
            catch (DirectoryNotFoundException)
            {
                sMsj = "Verifique la existencia de la ruta indicada en la configuración de Ruta de archivos Xml. No existe la ruta actual: " + _rutaXml.Trim() + " [CFDComprobanteFiscalDigital.GuardaPdf]";
                iErr++;
            }
            catch (IOException)
            {
                sMsj = "Verifique permisos de escritura en: " + _rutaXml.Trim() + ". No se pudo guardar el pdf. [CFDComprobanteFiscalDigital.GuardaPdf]";
                iErr++;
            }
            catch (Exception eAFE)
            {
                if (eAFE.Message.Contains("denied"))
                    sMsj = "Elimine el archivo xml, luego vuelva a intentar. " + eAFE.Message + " [CFDComprobanteFiscalDigital.GuardaPdf]";
                else
                    sMsj = "No se puede guardar el pdf. " + eAFE.Message + " [CFDComprobanteFiscalDigital.GuardaPdf] " + eAFE.StackTrace;
                iErr++;
            }

        }

        public void Limpia()
        { 
            sMsj = "";
            iErr = 0;
            try
            {
                //Arma el estado binario inicial
                String eBinarioInicial = Maquina.binStatusBaseEmisor; //revisar: debería iniciar una máquina y transicionar a emitido y comparar ese binStatus

                //Elimina si el estado es el inicial
                if (eBinarioInicial.Equals(_cicloDeVida.binStatus))
                    _bitacora.Delete(_soptype, _sopnumbe, eBinarioInicial);
            }
            catch (Exception el)
            {
                sMsj = "No se puede limpiar el documento de la Bitácora. " + el.Message + " [CFDComprobanteFiscalDigital.Limpia] " + _bitacora.sMsj;
                iErr++;
            }
        }

        /// <summary>
        /// Anota en la bitácora la factura emitida y el nuevo estado binario.
        /// Luego genera y guarda el código de barras bidimensional y pdf. En caso de error, anota en la bitácora. 
        /// </summary>
        /// <param name="trxVenta">Lista de facturas cuyo índice apunta a la factura que se va procesar.</param>
        /// <param name="comprobante">Documento xml</param>
        /// <param name="mEstados">Nuevo set de estados</param>
        /// <returns>False cuando hay al menos un error</returns>
        public void Guarda()
        {
            sMsj = "";
            iErr = 0;
            try
            {   //arma el nombre del archivo xml
                string rutaYNomArchivo = _rutaXml + _nomArchivo;

                //elimina registro duplicado
                _bitacora.Delete(_soptype, _sopnumbe, _cicloDeVida.targetBinStatus);

                //Registra log de la emisión del xml antes de imprimir el pdf, sino habrá error al imprimir
                _bitacora.Save(_soptype, _sopnumbe, rutaYNomArchivo + ".xml",
                            _cicloDeVida.idxTargetSingleStatus.ToString(), _Conex.Usuario, _modeloDte.xDocXml.InnerXml, //Importante. los datos de este xml se muestran en la impresión
                            _cicloDeVida.targetSingleStatus, _cicloDeVida.targetBinStatus, _cicloDeVida.EstadoEnPalabras(_cicloDeVida.targetBinStatus), "0");
            }
            catch (Exception eAFE)
            {
                if (eAFE.Message.Contains("denied"))
                    sMsj = "Elimine el archivo xml, luego vuelva a intentar. " + eAFE.Message + " [CFDComprobanteFiscalDigital.Guarda]";
                else
                    sMsj = "No se puede guardar el archivo XML ni registrar la Bitácora. " + eAFE.Message + " [CFDComprobanteFiscalDigital.Guarda] " + _bitacora.sMsj;
                iErr++;
            }
        }

        public void GuardaActualiza(String trackid)
        {
            sMsj = "";
            iErr = 0;
            try
            {   //arma el nombre del archivo xml
                string rutaYNomArchivo = _rutaXml + _nomArchivo;

                //Registra log de la emisión del xml antes de imprimir el pdf, sino habrá error al imprimir
                _bitacora.Save(_soptype, _sopnumbe, rutaYNomArchivo + ".xml",
                            _cicloDeVida.idxTargetSingleStatus.ToString(), _Conex.Usuario, String.Empty, //no guarda el envío xml para que sea más rápido
                            _cicloDeVida.targetSingleStatus, _cicloDeVida.targetBinStatus, _cicloDeVida.EstadoEnPalabras(_cicloDeVida.targetBinStatus), trackid);

                _bitacora.Update(_soptype, _sopnumbe, _Conex.Usuario, Maquina.estadoBaseEmisor, Maquina.estadoBaseEmisor, _cicloDeVida.targetBinStatus,
                            _cicloDeVida.EstadoEnPalabras(_cicloDeVida.targetBinStatus), _cicloDeVida.idxTargetSingleStatus.ToString(), trackid);

            }
            catch (Exception eAFE)
            {
                if (eAFE.Message.Contains("denied"))
                    sMsj = "Elimine el archivo xml, luego vuelva a intentar. " + eAFE.Message + " [CFDComprobanteFiscalDigital.GuardaActualiza]";
                else
                    sMsj = "No se puede guardar la Bitácora. Track Id: "+ trackid + " " + eAFE.Message + " [CFDComprobanteFiscalDigital.GuardaActualiza] " + _bitacora.sMsj;
                iErr++;
            }
        }

        public void GuardaActualizaMensaje(String sMensaje)
        {
            sMsj = "";
            iErr = 0;
            try
            {
                //Registra log de la emisión del xml antes de imprimir el pdf, sino habrá error al imprimir
                _bitacora.Save(_soptype, _sopnumbe, "ok",
                            _cicloDeVida.idxTargetSingleStatus.ToString(), _Conex.Usuario, sMensaje,
                            _cicloDeVida.targetSingleStatus, _cicloDeVida.targetBinStatus, _cicloDeVida.EstadoEnPalabras(_cicloDeVida.targetBinStatus), "0");

                _bitacora.Update(_soptype, _sopnumbe, _Conex.Usuario, Maquina.estadoBaseEmisor, Maquina.estadoBaseEmisor, _cicloDeVida.targetBinStatus,
                            _cicloDeVida.EstadoEnPalabras(_cicloDeVida.targetBinStatus), _cicloDeVida.idxTargetSingleStatus.ToString(), String.Empty);

            }
            catch (Exception eAFE)
            {
                sMsj = "No se puede guardar la Bitácora. " + eAFE.Message + " [CFDComprobanteFiscalDigital.GuardaActualiza] " + _bitacora.sMsj;
                iErr++;
            }
        }

        public void Actualiza(String trackid)
        {
            sMsj = "";
            iErr = 0;
            try
            {   
                _bitacora.Update(_soptype, _sopnumbe, _Conex.Usuario, Maquina.estadoBaseEmisor, Maquina.estadoBaseEmisor, _cicloDeVida.targetBinStatus,
                            _cicloDeVida.EstadoEnPalabras(_cicloDeVida.targetBinStatus), _cicloDeVida.idxTargetSingleStatus.ToString(), trackid);

            }
            catch (Exception eAFE)
            {
                if (eAFE.Message.Contains("denied"))
                    sMsj = "Elimine el archivo xml, luego vuelva a intentar. " + eAFE.Message + " [CFDComprobanteFiscalDigital.GuardaActualiza]";
                else
                    sMsj = "No se puede registrar la Bitácora. " + eAFE.Message + " [CFDComprobanteFiscalDigital.GuardaActualiza] " + _bitacora.sMsj;
                iErr++;
            }
        }

        public void GuardaCodigoBarras()
        {
            string rutaYNomArchivo = _rutaXml.Trim() + "cbb\\" + _nomArchivo + ".png";
            sMsj = "";
            iErr = 0;
            try
            {
                //Genera y guarda código de barras bidimensional
                BarcodeSettings settings = new BarcodeSettings();
                settings.Pdf417DataMode = Pdf417DataMode.Byte;
                settings.Pdf417Truncated = false;
                settings.Pdf417ECL = Pdf417ECL.Level5;
                settings.DpiX = 100;
                settings.DpiY = 100;

                settings.ImageHeight = 1;
                settings.ImageWidth = 3;
                settings.XYRatio = 3;

                settings.Data2D = _modeloDte.modeloTed.xDocXml.OuterXml;
                settings.Data = _modeloDte.modeloTed.xDocXml.OuterXml;
                settings.Type = (BarCodeType)Enum.Parse(typeof(BarCodeType), "Pdf417");
                settings.ShowText = false;
                settings.BarHeight = 15;
                settings.ShowCheckSumChar = true;
                settings.ForeColor = Color.FromName("Black");

                //generate the barcode use the settings
                BarCodeGenerator generator = new BarCodeGenerator(settings);
                Image barcode = generator.GenerateImage();

                //save the barcode as an image
                barcode.Save(rutaYNomArchivo);

            }
            catch (DirectoryNotFoundException)
            {
                sMsj = "Verifique la existencia de la ruta indicada en la configuración de Ruta de archivos Xml. No existe la ruta actual: " + _rutaXml.Trim() + " [CFDComprobanteFiscalDigital.GuardaCodigoBarras]";
                iErr++;
            }
            catch (IOException)
            {
                sMsj = "Verifique permisos de escritura en: " + _rutaXml.Trim() + ". No se pudo guardar el código de barras. [CFDComprobanteFiscalDigital.GuardaCodigoBarras]";
                iErr++;
            }
            catch (Exception eAFE)
            {
                if (eAFE.Message.Contains("denied"))
                    sMsj = "Elimine el archivo xml, luego vuelva a intentar. " + eAFE.Message + " [CFDComprobanteFiscalDigital.GuardaCodigoBarras]";
                else
                    sMsj = "No se puede guardar el código de barras. " + eAFE.Message + " [CFDComprobanteFiscalDigital.GuardaCodigoBarras] " + eAFE.StackTrace;
                iErr++;
            }
        }
    }
}
