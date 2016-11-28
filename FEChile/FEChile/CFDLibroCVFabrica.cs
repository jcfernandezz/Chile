using Comun;
using MaquinaDeEstados;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEChile
{
    public class CFDLibroCVFabrica
    {
        private Parametros _Param;
        private ConexionAFuenteDatos _Conex;
        private Encoding _encoding;
        private CFDComprobanteFiscalDigitalFabrica cfdFabrica;

        public int iErr;
        public string sMsj;
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

        public CFDLibroCVFabrica(ConexionAFuenteDatos Conex, Parametros Param)
        {
            try
            {
                sMsj = string.Empty;
                iErr = 0;
                _Param = Param;
                _Conex = Conex;

                _encoding = Encoding.GetEncoding("ISO-8859-1");
                //_lContenedores = new List<EnvioDteModel>();
                //lDocumentos = new List<CFDComprobanteFiscalDigital>();

                OnProgreso(1, "Preparando certificados...");                //Notifica al suscriptor
                cfdFabrica = new CFDComprobanteFiscalDigitalFabrica(Conex, Param);

                if (cfdFabrica.iErr != 0)
                {
                    iErr= cfdFabrica.iErr;
                    sMsj = cfdFabrica.sMsj;
                }
            }
            catch(Exception cf)
            {
                sMsj = "Excepción al inicializar emisión de libros. " + cf.Message + " [CFDLibroCVFabrica.constructor]";
                iErr++;
            }
        }

        /// <summary>
        /// Ensambla y firma una lista de libros a partir de un lote de libros.
        /// _lDocumentos: Lista de libros
        /// </summary>
        /// <param name="loteLibrosCV">Lote de documentos a procesar</param>
        public void ensamblaLote(vwCfdLibroCVLog loteLibrosCV)
        {
            try
            {
                OnProgreso(1, "Iniciando...");                              //Notifica al suscriptor

                loteLibrosCV.Rewind();                                      //move to first record

                sMsj = string.Empty;
                iErr = 0;
                int iMaxErr = 0;
                CFDLibroCV libro;
                //string docIdAnterior = string.Empty;
                //_lDocumentos.Clear();
                do
                {
                    libro = new CFDLibroCV(_Conex, _Param, _encoding, loteLibrosCV.EstadoActualBin, loteLibrosCV.IdxSingleStatus.ToString(), 
                                        loteLibrosCV.Periodo, loteLibrosCV.Tipo, loteLibrosCV.RutaXml);

                    //cfd.modeloDte.AutorizacionXml = autorizacion;
                    libro.criptografo = cfdFabrica.encriptador;
                    libro.certificados = cfdFabrica.certificados;
                    iErr += libro.iErr;

                    if (iErr == 0)      
                        //Evento 20: ensambla lote, emite factura y envía al SII
                        if (!libro.cicloDeVida.Transiciona(Maquina.eventoEnsamblaLote, cfdFabrica.certificados.firma))
                        {
                            iErr = libro.cicloDeVida.iErr;
                            sMsj = libro.cicloDeVida.sMsj;
                        }

                    if (iErr == 0)
                    {
                        libro.Ensambla(loteLibrosCV.ComprobanteXml, _encoding);
                        iErr += libro.iErr;
                    }

                    if (iErr == 0)
                    {
                        //_lDocumentos.Add(cfd)
                        //enviar al SII;
                        //Anota en la bitácora
                        libro.Guarda();
                        iErr += libro.iErr;
                    }

                    OnProgreso(100 / loteLibrosCV.RowCount, "Libro: " + loteLibrosCV.Tipo + " " + libro.sMsj.Trim() + sMsj);

                    if (iErr > 0) iMaxErr++;
                    sMsj = string.Empty;
                    iErr = 0;
                } while (loteLibrosCV.MoveNext() && iMaxErr < 10);

                OnProgreso(100, loteLibrosCV.RowCount.ToString() + " comprobante(s) enviados al SII. ");
            }
            catch (Exception errorGral)
            {
                sMsj = "Excepción encontrada al ensamblar lote de libros. " + errorGral.Message + " [CFDLibroCVFabrica.ensamblaLote] ";
                iErr++;
                OnProgreso(0, sMsj);
            }
        }

    }
}
