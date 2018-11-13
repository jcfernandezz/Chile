using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using cfd.FacturaElectronica;
using System.Xml;
using Comun;
using Encriptador;
using System.Xml.Linq;
using ReciboMercaderiaServicios;
using respuestaEnvioDTE;
using MaquinaDeEstados;

namespace FEChile
{
    public class CFDFacturasCompraFabrica
    {
        //private List<String> _lDocumentos;         //contiene los cfd
        private Parametros _Param;
        private ConexionAFuenteDatos _Conex;
        private Encoding _encoding;
        private vwCfdCertificadosService _certificados;
        private TecnicaDeEncriptacion _encriptador;
        private XNamespace nsDte = "http://www.sii.cl/SiiDte";
        private XNamespace nsSignature = "http://www.w3.org/2000/09/xmldsig#";
        private CFDReciboMercaServicio _reciboProducto;
        private RespuestaEnvio _respuestaResultado;
        //***********************************************
        #region Propiedades
        private int _iErr;

        public int iErr
        {
            get { return _iErr; }
            set { _iErr = value; }
        }
        private string _sMsj;

        public string SMsj
        {
            get { return _sMsj; }
            set { _sMsj = value; }
        }
        private List<RespuestaEnvio> _lRespuestas;

        public List<RespuestaEnvio> LRespuestas
        {
            get { return _lRespuestas; }
            set { _lRespuestas = value; }
        }
        private List<CFDReciboMercaServicio> _lAcuses;

        public List<CFDReciboMercaServicio> LAcuses
        {
            get { return _lAcuses; }
            set { _lAcuses = value; }
        }
        
        #endregion
        //**************************************************

        public delegate void LogHandler(int iAvance, string sMsj);
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

        public CFDFacturasCompraFabrica(ConexionAFuenteDatos Conex, Parametros Param)
        {
            try
            {
                _sMsj = string.Empty;
                _iErr = 0;
                _Param = Param;
                _Conex = Conex;
                _lAcuses = new List<CFDReciboMercaServicio>();
                _lRespuestas = new List<RespuestaEnvio>();

                _encoding = Encoding.GetEncoding(_Param.encoding);

                preparaCertificado();                                       //carga certificados y _encriptador

            }
            catch(Exception cf)
            {
                _sMsj = "Excepción al inicializar fábrica de cfds. " + cf.Message + " [CFDComprobanteFiscalDigitalFabrica]";
                _iErr++;
            }
        }

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
                    //_encriptador.PreparaEncriptacion("", _certificados.clave.Trim(), @"C:\GPUsuario\GPExpressCfdi\feGilaChiTST\certificado\CERTIFICADO ELECTRONICO MARIANA REBOLLEDO.p12", "");

                }
                else
                {
                    _iErr = 50;
                    _sMsj = "El usuario no tiene asignado un certificado de firma. Ingrese al Mantenimiento de certificados y agregue la ruta del certificado para " + _Conex.Usuario + " " + _certificados.sMsj;
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Ensambla la aceptación o rechazo de un lote de facturas de compra
        /// _lDocumentos: Lista de dte's
        /// </summary>
        /// <param name="loteFacturasCompra">Lote de documentos a procesar</param>
        public void EnsamblaLote(vwCfdLogFacturaCompra loteFacturasCompra, int evento, String motivoRechazo)
        {
            String sTipo = String.Empty;
            try
            {
                OnProgreso(1, "Iniciando...");                              //Notifica al suscriptor

                loteFacturasCompra.Rewind();                                //move to first record
                _sMsj = string.Empty;
                _iErr = 0;
                int iMaxErr = 0;
                _lAcuses.Clear();
                _lRespuestas.Clear();
                do
                {
                    //RespuestaEnvio cfdCompra = new RespuestaEnvio() ;
                    //CFDReciboMercaServicio cfdRecibo = new CFDReciboMercaServicio() ;
                    if (evento == Maquina.eventoAcuseProducto)
                    {
                        ArmaRecepcionProducto(loteFacturasCompra, evento, _Param.RutaArchivosTemp);
                        sTipo = "RECIBO";
                        if (_iErr == 0)
                            _lAcuses.Add(_reciboProducto);
                    }
                    if (evento == Maquina.eventoResultadoAceptado || evento == Maquina.eventoResultadoRechazado)
                    {
                        ArmaResultadoDte(loteFacturasCompra, evento, _Param.RutaArchivosTemp, motivoRechazo);        //evento resultado: acepta 
                        sTipo = "RESULTADO";
                        if (_iErr == 0)
                            _lRespuestas.Add(_respuestaResultado);
                    }

                    OnProgreso(100 / loteFacturasCompra.RowCount, "Doc. compra: " + loteFacturasCompra.Folio + " " + _sMsj);

                    if (_iErr > 0) iMaxErr++;
                    _sMsj = String.Empty;
                    _iErr = 0;

                } while (loteFacturasCompra.MoveNext() && iMaxErr < 10);

                int numDocs = _lRespuestas.Count() + _lAcuses.Count();
                OnProgreso(100, numDocs.ToString() + " comprobante(s) procesados. ");
            }
            catch (Exception errorGral)
            {
                _sMsj = "Excepción encontrada al formar la respuesta tipo: " + sTipo + " " + errorGral.Message + " [CFDFacturasCompraFabrica.EnsamblaLote] ";
                _iErr++;
                OnProgreso(0, _sMsj);
            }
        }

        //private CFDFacturaCompra ArmaRespuestaRecepcionFactura(vwCfdLogFacturaCompra cfdCompra)
        //{
        //    _iErr = 0;
        //    _sMsj = String.Empty;

        //    CFDFacturaCompra facturaCompra = new CFDFacturaCompra(_encriptador, _Conex.ConnStr, _encoding);
        //    facturaCompra.TipoDte = cfdCompra.Tipo;
        //    facturaCompra.Folio = cfdCompra.Folio;
        //    facturaCompra.IdImpuestoTercero = cfdCompra.IdImpuestoTercero;
        //    facturaCompra.NombreTercero = cfdCompra.NombreTercero;
        //    facturaCompra.Estado = cfdCompra.Estado;
        //    facturaCompra.EBinStatus = cfdCompra.EstadoActual;
        //    facturaCompra.IdExterno = cfdCompra.IdExterno;
        //    facturaCompra.Mensaje = cfdCompra.Mensaje;

        //    facturaCompra.EnsamblaRespuesta(cfdCompra);

        //    _iErr += facturaCompra.iErr;
        //    _sMsj = facturaCompra.sMsj;

        //    //if (iErr == 0)      //Evento 0: ensambla lote, emite factura
        //    //{
        //    //    facturaCompra.cicloDeVida.Transiciona(0, _certificados.firma);
        //    //    iErr = facturaCompra.cicloDeVida.iErr;
        //    //    sMsj = facturaCompra.cicloDeVida.sMsj;
        //    //}

        //    if (_iErr == 0)
        //    {
        //        //Canonicaliza documento original
        //        facturaCompra.Canonicaliza();

        //        //firma al enviar
        //        facturaCompra.Firma("RESULTADO");

        //        //facturaCompra.VerificaFirma();

        //        _iErr = facturaCompra.iErr;
        //        _sMsj = facturaCompra.sMsj;
        //    }

        //    if (_iErr == 0)
        //    {
        //        facturaCompra.SaveFile();

        //        //Anota en la bitácora
        //        facturaCompra.Save();

        //        facturaCompra.Update();

        //        _iErr = facturaCompra.iErr;
        //        _sMsj = facturaCompra.sMsj;
        //    }

        //    return facturaCompra;
        //}

        private RespuestaEnvio ArmaResultadoDte(vwCfdLogFacturaCompra cfdCompra, int evento, String ruta, String motivoRechazo)
        {
            _iErr = 0;
            _sMsj = String.Empty;

            _respuestaResultado = new RespuestaEnvio(_Conex.ConnStr, _encoding, _Conex.Usuario, _certificados.envia);
            _respuestaResultado.Evento = evento;
            _respuestaResultado.RutaXml = ruta;
            _respuestaResultado.criptografo = _encriptador;
            _respuestaResultado.tipoDTE = cfdCompra.Tipo.ToString();
            _respuestaResultado.Folio = cfdCompra.Folio;
            _respuestaResultado.rutEmisor = cfdCompra.IdImpuestoTercero;
            _respuestaResultado.NomEmisor = cfdCompra.NombreTercero;
            _respuestaResultado.ESingleStatus = cfdCompra.Estado;
            _respuestaResultado.EBinStatus = cfdCompra.EstadoActual;
            _respuestaResultado.Uid = cfdCompra.IdExterno;
            _respuestaResultado.archivoRecibido = "-";
            //_respuestaResultado.EnvioDTEID = cfdCompra.Mensaje;
            _respuestaResultado.EmailProveedor = cfdCompra.Mensaje;

            _respuestaResultado.EnsamblaResultado(cfdCompra.ArchivoXML, evento == Maquina.eventoResultadoAceptado, motivoRechazo);

            _iErr += _respuestaResultado.IErr;
            _sMsj = _respuestaResultado.SMsj;

            //if (_iErr == 0)
            //{
                //_respuestaResultado.CicloDeVida.TipoDoc = cfdCompra.Folio;
                //_respuestaResultado.CicloDeVida.Transiciona(evento, _certificados.envia);
                //_iErr = _respuestaResultado.CicloDeVida.iErr;
                //_sMsj = _respuestaResultado.CicloDeVida.sMsj;
            //}

            if (_iErr == 0)
            {
                //Canonicaliza documento original
                _respuestaResultado.Canonicaliza();

                //firma al enviar
                _respuestaResultado.firma(_respuestaResultado.IdResultado);

                //respuestaResultado.VerificaFirma();

                _iErr = _respuestaResultado.IErr;
                _sMsj = _respuestaResultado.SMsj;
            }

            return _respuestaResultado;
        }

        private CFDReciboMercaServicio ArmaRecepcionProducto(vwCfdLogFacturaCompra cfdCompra, int evento, String ruta)
        {
            _iErr = 0;
            _sMsj = String.Empty;

            _reciboProducto = new CFDReciboMercaServicio(_encriptador, _Conex.ConnStr, _Conex.Usuario, _encoding, cfdCompra.EstadoActual, cfdCompra.IdxSingleStatus, 0, cfdCompra.Tipo);
            _reciboProducto.Evento = evento;
            _reciboProducto.RutaXml = ruta;
            _reciboProducto.TipoDte = cfdCompra.Tipo;
            _reciboProducto.Folio = cfdCompra.Folio;
            _reciboProducto.IdImpuestoTercero = cfdCompra.IdImpuestoTercero;
            _reciboProducto.NombreTercero = cfdCompra.NombreTercero;
            _reciboProducto.Estado = cfdCompra.Estado;
            _reciboProducto.EBinStatus = cfdCompra.EstadoActual;
            _reciboProducto.IdExterno = cfdCompra.IdExterno;
            //_reciboProducto.Mensaje = cfdCompra.Mensaje;
            _reciboProducto.RutFirma = _certificados.idImpuesto;
            _reciboProducto.Prefijo = "ACU";
            _reciboProducto.EmailProveedor = cfdCompra.Mensaje;

            _reciboProducto.Ensambla(cfdCompra.ArchivoXML);

            _iErr += _reciboProducto.iErr;
            _sMsj = _reciboProducto.sMsj;

            if (_iErr == 0)
            {
                //Canonicaliza documento original
                _reciboProducto.Canonicaliza();

                //firma al enviar
                _reciboProducto.Firma(_reciboProducto.IdRecibo);
                _reciboProducto.InsertaFirma("Recibo");

                _reciboProducto.Firma(_reciboProducto.IdSetRecibos);
                _reciboProducto.InsertaFirma("EnvioRecibos");

                //facturaCompra.VerificaFirma();

                _iErr = _reciboProducto.iErr;
                _sMsj = _reciboProducto.sMsj;
            }

            //if (_iErr == 0)
            //{
            //    //inicio intercambio
            //    //_reciboProducto.SaveFile();
            //    //fin intercambio

            //    _iErr = _reciboProducto.iErr;
            //    _sMsj = _reciboProducto.sMsj;
            //}

            return _reciboProducto;
        }

    }
}
