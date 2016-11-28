using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Xml;
using System.IO;
using System.Security.AccessControl;

using Encriptador;
using MyGeneration.dOOdads;
using EjecutableEncriptador;
using Comun;
using Reporteador;
using MaquinaDeEstados;
//using QRCodeLib;
using FEChile;

namespace cfd.FacturaElectronica
{
    class cfdReglasFacturaXml
    {
        public string ultimoMensaje = "";
        public int numMensajeError=0;
        private ConexionAFuenteDatos _Conexion = null;
        private Parametros _Param = null;
        //CodigoDeBarras codigobb;
        Documento reporte;

        public cfdReglasFacturaXml(ConexionAFuenteDatos conex, Parametros param)
        {
            _Conexion = conex;
            _Param = param;
            reporte = new Documento(_Conexion, _Param);
            //codigobb = new CodigoDeBarras();

            numMensajeError = reporte.numErr;   //+ codigobb.iErr
            ultimoMensaje = reporte.mensajeErr; //+ codigobb.strMensajeErr
        }

        /// <summary>
        /// Filtra la lista de transacciones de venta
        /// </summary>
        /// <param name="filtroFecha"></param>
        /// <param name="desdeF"></param>
        /// <param name="hastaF"></param>
        /// <param name="deFDefault"></param>
        /// <param name="aFDefault"></param>
        /// <param name="filtroNumDoc"></param>
        /// <param name="numDocDe"></param>
        /// <param name="numDocA"></param>
        /// <param name="filtroIdDoc"></param>
        /// <param name="idDoc"></param>
        /// <param name="filtroEstado"></param>
        /// <param name="estado"></param>
        /// <param name="filtroCliente"></param>
        /// <param name="cliente"></param>
        /// <param name="trxVenta"></param>
        public void AplicaFiltroAFacturas(bool filtroFecha, DateTime desdeF, DateTime hastaF, DateTime deFDefault, DateTime aFDefault,
                                    bool filtroNumDoc, string numDocDe, string numDocA,
                                    bool filtroIdDoc, string idDoc, 
                                    bool filtroEstado, string estado,
                                    bool filtroCliente, string cliente,
                                    out vwCfdTransaccionesDeVenta trxVenta)
        {
            trxVenta = new vwCfdTransaccionesDeVenta(_Conexion.ConnStr);
            trxVenta.Query.AddOrderBy(vwCfdTransaccionesDeVenta.ColumnNames.Sopnumbe, WhereParameter.Dir.ASC);

            DateTime desdeFecha = new DateTime(deFDefault.Year, deFDefault.Month, deFDefault.Day, 0, 0, 0);
            DateTime hastaFecha = new DateTime(aFDefault.Year, aFDefault.Month, aFDefault.Day, 23, 59, 59);
            if (filtroFecha)
            {
                //Filtro personalizado
                desdeFecha = new DateTime(desdeF.Year, desdeF.Month, desdeF.Day, 0, 0, 0); ;
                hastaFecha = new DateTime(hastaF.Year, hastaF.Month, hastaF.Day, 23, 59, 59);
                //desdeFecha = desdeF;
                //hastaFecha = hastaF;
            }
            if ((!filtroNumDoc && !filtroIdDoc && !filtroEstado && !filtroCliente) || filtroFecha)
            {   //Filtra los documentos por fecha. De forma predeterminada es la fecha de hoy.
                trxVenta.Where.Fechahora.BetweenBeginValue = desdeFecha;
                trxVenta.Where.Fechahora.BetweenEndValue = hastaFecha; 
                trxVenta.Where.Fechahora.Operator = WhereParameter.Operand.Between;
            }
            if (filtroNumDoc)
            {
                trxVenta.Where.Sopnumbe.BetweenBeginValue = numDocDe.Trim();
                trxVenta.Where.Sopnumbe.BetweenEndValue = numDocA.Trim();
                trxVenta.Where.Sopnumbe.Operator = WhereParameter.Operand.Between;
            }
            if (filtroIdDoc)
            {
                trxVenta.Where.Docid.Value = idDoc.Trim();
                trxVenta.Where.Docid.Operator = WhereParameter.Operand.Equal;
            }
            if (filtroEstado)
            {
                trxVenta.Where.Estado.Value = estado;
                trxVenta.Where.Estado.Operator = WhereParameter.Operand.Equal;
            }
            if (filtroCliente)
            {
                
                trxVenta.Where.NombreCliente.Value = "%" + cliente + "%";
                trxVenta.Where.NombreCliente.Operator = WhereParameter.Operand.Like;
            }
            try
            {
                if (!trxVenta.Query.Load())
                {
                    ultimoMensaje = "No hay datos para el filtro seleccionado.";
                    numMensajeError++;
                }
            }
            catch (Exception eFiltro)
            {
                ultimoMensaje = "Contacte al administrador. No se pudo consultar la base de datos. [cfdReglasFacturaXml.AplicaFiltroAFacturas]" + eFiltro.Message;
                numMensajeError++;
            }

        }

        /// <summary>
        /// Filtra la lista de libros
        /// </summary>
        /// <param name="filtroPeriodo"></param>
        /// <param name="dePeriodo"></param>
        /// <param name="aPeriodo"></param>
        /// <param name="dePeriodoDefault"></param>
        /// <param name="aPeriodoDefault"></param>
        /// <param name="filtroEstado"></param>
        /// <param name="estado"></param>
        /// <param name="libroCV"></param>
        public void AplicaFiltroALibros(
                            bool filtroPeriodo, int dePeriodo, int aPeriodo, int dePeriodoDefault, int aPeriodoDefault,
                            bool filtroEstado, string estado,
                            out vwCfdLibroCVLog libroCV)
        {
            libroCV = new vwCfdLibroCVLog(_Conexion.ConnStr);
            libroCV.Query.AddOrderBy(vwCfdLibroCVLog.ColumnNames.Periodo, WhereParameter.Dir.ASC);

            int desdePeriodo = dePeriodoDefault;
            int hastaPeriodo = aPeriodoDefault;
            if (filtroPeriodo)
            {
                //Filtro personalizado
                desdePeriodo = dePeriodo;
                hastaPeriodo = aPeriodo;
            }
            if ((!filtroEstado) || filtroPeriodo)
            {   //Filtra los documentos de acuerdo a las opciones del menú. De forma predeterminada es la fecha de hoy.
                libroCV.Where.Periodo.BetweenBeginValue = desdePeriodo;
                libroCV.Where.Periodo.BetweenEndValue = hastaPeriodo;
                libroCV.Where.Periodo.Operator = WhereParameter.Operand.Between;
            }
            if (filtroEstado)
            {
                libroCV.Where.Estado.Value = estado;
                libroCV.Where.Estado.Operator = WhereParameter.Operand.Equal;
            }
            try
            {
                if (!libroCV.Query.Load())
                {
                    ultimoMensaje = "No hay datos para el filtro seleccionado.";
                    numMensajeError++;
                }
            }
            catch (Exception eFiltro)
            {
                ultimoMensaje = "Contacte al administrador. No se pudo consultar la base de datos. [cfdReglasFacturaXml.AplicaFiltroALibros]" + eFiltro.Message;
                numMensajeError++;
            }

        }

        /// <summary>
        /// Filtra la lista de facturas de compra
        /// </summary>
        /// <param name="filtroPeriodo"></param>
        /// <param name="dePeriodo"></param>
        /// <param name="aPeriodo"></param>
        /// <param name="dePeriodoDefault"></param>
        /// <param name="aPeriodoDefault"></param>
        /// <param name="filtroEstado"></param>
        /// <param name="estado"></param>
        /// <param name="facturasCompra"></param>
        public void AplicaFiltroACompras(
                            bool filtroFecha, DateTime desdeF, DateTime hastaF, DateTime deFDefault, DateTime aFDefault,
                            bool filtroNumDoc, string numDocDe, string numDocA,
                            bool filtroIdDoc, string idDoc,
                            bool filtroEstado, string estado,
                            bool filtroProveedor, string proveedor,
                            out vwCfdLogFacturaCompra facturasCompra)
        {
            facturasCompra = new vwCfdLogFacturaCompra(_Conexion.ConnStr);
            facturasCompra.Query.AddOrderBy(vwCfdLogFacturaCompra.ColumnNames.NombreTercero, WhereParameter.Dir.ASC);

            DateTime desdeFecha = new DateTime(deFDefault.Year, deFDefault.Month, deFDefault.Day, 0, 0, 0);
            DateTime hastaFecha = new DateTime(aFDefault.Year, aFDefault.Month, aFDefault.Day, 23, 59, 59);
            if (filtroFecha)
            {
                //Filtro personalizado
                desdeFecha = desdeF;
                hastaFecha = hastaF;
            }
            if ((!filtroNumDoc && !filtroIdDoc && !filtroEstado && !filtroProveedor) || filtroFecha)
            {   //Filtra los documentos por fecha. De forma predeterminada es la fecha de hoy.
                facturasCompra.Where.FechaRecepcion.BetweenBeginValue = desdeFecha;
                facturasCompra.Where.FechaRecepcion.BetweenEndValue = hastaFecha;
                facturasCompra.Where.FechaRecepcion.Operator = WhereParameter.Operand.Between;
            }
            if (filtroNumDoc)
            {
                facturasCompra.Where.Folio.BetweenBeginValue = numDocDe.Trim();
                facturasCompra.Where.Folio.BetweenEndValue = numDocA.Trim();
                facturasCompra.Where.Folio.Operator = WhereParameter.Operand.Between;
            }
            if (filtroIdDoc)
            {
                facturasCompra.Where.Tipo.Value = idDoc.Trim();
                facturasCompra.Where.Tipo.Operator = WhereParameter.Operand.Equal;
            }
            if (filtroEstado)
            {
                facturasCompra.Where.Estado.Value = estado;
                facturasCompra.Where.Estado.Operator = WhereParameter.Operand.Equal;
            }
            if (filtroProveedor)
            {

                facturasCompra.Where.NombreTercero.Value = "%" + proveedor + "%";
                facturasCompra.Where.NombreTercero.Operator = WhereParameter.Operand.Like;
            }
            try
            {
                if (!facturasCompra.Query.Load())
                {
                    ultimoMensaje = "No hay datos para el filtro seleccionado.";
                    numMensajeError++;
                }
            }
            catch (Exception eFiltro)
            {
                ultimoMensaje = "Contacte al administrador. No se pudo consultar la base de datos. [cfdReglasFacturaXml.AplicaFiltroACompras]" + eFiltro.Message;
                numMensajeError++;
            }

        }

        public bool AplicaFiltroParaInformeMes(DateTime deFecha, DateTime aFecha,
                                            out vwCfdInformeMensualVentas infMes)
        {
            infMes = new vwCfdInformeMensualVentas(_Conexion.ConnStr);
            infMes.Query.AddOrderBy(vwCfdInformeMensualVentas.ColumnNames.Sopnumbe, WhereParameter.Dir.ASC);

            //Filtra los documentos por fecha. De forma predeterminada es la fecha de hoy.
            infMes.Where.Fechahora.BetweenBeginValue = string.Format("{0:yyyy-MM-dd}", deFecha) + " 00:00:00.0"; //"2010-08-01 00:00:00.0";
            infMes.Where.Fechahora.BetweenEndValue = string.Format("{0:yyyy-MM-dd}", aFecha) + " 23:59:59.9";
            infMes.Where.Fechahora.Operator = WhereParameter.Operand.Between;

            try
            {
                if (infMes.Query.Load())
                    return true;
                else
                    ultimoMensaje = "No hay datos para el filtro seleccionado.";
            }
            catch (Exception eFiltroMes)
            {
                ultimoMensaje = "[AplicaFiltroInformeMes] No se pudo consultar la base de datos. " + eFiltroMes.Message;
            }
            return false;
        }

        /// <summary>
        /// Inserta datos de una factura en el log de facturas. 
        /// </summary>
        /// <returns></returns>
        public void RegistraLogDeArchivoXML(short soptype, string sopnumbe, string mensaje, string noAprobacion, string idusuario, string innerxml, 
                                            string eBaseNuevo, string eBinarioActual, string mensajeBinActual)
        {
            try
            {
                ultimoMensaje = "";
                numMensajeError = 0;
                //log de facturas xml emitido y xml anulado
                cfdLogFacturaXML logVenta = new cfdLogFacturaXML(_Conexion.ConnStr);
                
                logVenta.AddNew();
                logVenta.Soptype = soptype;
                logVenta.Sopnumbe = sopnumbe;
                logVenta.Mensaje = Utiles.Derecha(mensaje, 255);
                logVenta.Estado = eBaseNuevo;
                logVenta.NoAprobacion = noAprobacion;
                logVenta.FechaEmision = DateTime.Now;
                logVenta.IdUsuario = Utiles.Derecha(idusuario, 10);
                logVenta.IdUsuarioAnulacion = "-";
                logVenta.FechaAnulacion = new DateTime(1900, 1, 1);
                if (!innerxml.Equals("")) 
                    logVenta.ArchivoXML = innerxml;
                logVenta.EstadoActual = eBinarioActual;
                logVenta.MensajeEA = Utiles.Derecha(mensajeBinActual, 255);
                logVenta.Save();
           }
           catch (Exception eLog)
           {
                ultimoMensaje = "Contacte a su administrador. No se puede registrar la tarea en la Bitácora. [RegistraLogDeArchivoXML] " + eLog.Message;
                numMensajeError++;
           }
        }

        /// <summary>
        /// Actualiza la fecha, estado y observaciones de una factura emitida en el log de facturas. 
        /// </summary>
        /// <returns></returns>
        public void ActualizaFacturaEmitida(short Soptype, string Sopnumbe, string idusuario, string eBaseAnterior, string eBaseNuevo, string eBinarioActual, string mensajeEA)
        {
            ultimoMensaje = "";
            numMensajeError = 0;
            cfdLogFacturaXML xmlEmitido = new cfdLogFacturaXML(_Conexion.ConnStr);
            xmlEmitido.Where.Soptype.Value = Soptype;
            xmlEmitido.Where.Soptype.Operator = WhereParameter.Operand.Equal;
            xmlEmitido.Where.Sopnumbe.Conjuction = WhereParameter.Conj.And;
            xmlEmitido.Where.Sopnumbe.Value = Sopnumbe;
            xmlEmitido.Where.Sopnumbe.Operator = WhereParameter.Operand.Equal;
            xmlEmitido.Where.Estado.Conjuction = WhereParameter.Conj.And;
            xmlEmitido.Where.Estado.Value = eBaseAnterior;      // "emitido";
            xmlEmitido.Where.Estado.Operator = WhereParameter.Operand.Equal;
            try
            {
                if (xmlEmitido.Query.Load())
                {
                    if (!eBaseAnterior.Equals(eBaseNuevo))
                        xmlEmitido.Estado = eBaseNuevo;         // "anulado";
                    xmlEmitido.FechaAnulacion = DateTime.Now;
                    xmlEmitido.IdUsuarioAnulacion = Utiles.Derecha(idusuario, 10);
                    xmlEmitido.EstadoActual = eBinarioActual;
                    xmlEmitido.MensajeEA = Utiles.Derecha(mensajeEA, 255);
                    xmlEmitido.Save();
                    //ultimoMensaje = "Completado.";
                }
                else
                {
                    ultimoMensaje = "No está en la bitácora con estado 'emitido'.";
                    numMensajeError++;
                }
            }
            catch (Exception eAnula)
            {
                ultimoMensaje = "Contacte al administrador. Error al acceder la base de datos. [ActualizaFacturaEmitida] " + eAnula.Message;
                numMensajeError++;
            }
        }

        /// <summary>
        /// Guarda el archivo xml, lo comprime en zip y anota en la bitácora la factura emitida y el nuevo estado binario.
        /// Luego genera y guarda el código de barras bidimensional y pdf. En caso de error, anota en la bitácora. 
        /// </summary>
        /// <param name="trxVenta">Lista de facturas cuyo índice apunta a la factura que se va procesar.</param>
        /// <param name="comprobante">Documento xml</param>
        /// <param name="mEstados">Nuevo set de estados</param>
        /// <returns>False cuando hay al menos un error</returns>
        //public bool AlmacenaEnRepositorio(vwCfdTransaccionesDeVenta trxVenta, XmlDocument comprobante, ReglasME mEstados)
        //{   
        //    ultimoMensaje = "";
        //    numMensajeError = 0;
        //    try
        //    {   //arma el nombre del archivo xml
        //        string nomArchivo = Utiles.FormatoNombreArchivo(trxVenta.Docid + trxVenta.Sopnumbe + "_" + trxVenta.s_CUSTNMBR, trxVenta.s_NombreCliente, 20);
        //        string rutaYNomArchivo = trxVenta.RutaXml.Trim() + nomArchivo;

        //        //Guarda el archivo xml
        //        comprobante.Save(new XmlTextWriter(rutaYNomArchivo + ".xml", Encoding.UTF8));

        //        //Registra log de la emisión del xml antes de imprimir el pdf, sino habrá error al imprimir
        //        RegistraLogDeArchivoXML(trxVenta.Soptype, trxVenta.Sopnumbe, "Almacenado en " + rutaYNomArchivo, "0", _Conexion.Usuario, comprobante.InnerXml,
        //                                "emitido", mEstados.eBinarioNuevo, mEstados.EnLetras(mEstados.eBinarioNuevo));

        //        if (numMensajeError == 0)
        //        {
        //            //Genera y guarda código de barras bidimensional
        //            //codigobb.GenerarQRBidimensional("?re=" + trxVenta.IdImpuesto + "&rr=" + trxVenta.IdImpuestoCliente + "&tt=" + trxVenta.Total.ToString() + 
        //            //                                "&id=" + trxVenta.UUID, trxVenta.RutaXml.Trim() + "cbb\\" + nomArchivo + ".jpg");
        //            //Genera pdf
        //            if (codigobb.iErr == 0)
        //                reporte.generaEnFormatoPDF(rutaYNomArchivo, trxVenta.Soptype, trxVenta.Sopnumbe, trxVenta.EstadoContabilizado);

        //            //Comprime el archivo xml
        //            if (_Param.zip)
        //                Utiles.Zip(rutaYNomArchivo, ".xml");

        //            numMensajeError = codigobb.iErr + reporte.numErr + Utiles.numErr;
        //            ultimoMensaje = codigobb.strMensajeErr + " " + reporte.mensajeErr + " " + Utiles.msgErr;

        //            //Si hay error en cbb o pdf o zip anota en la bitácora
        //            if (numMensajeError != 0)
        //                ActualizaFacturaEmitida(trxVenta.Soptype, trxVenta.Sopnumbe, _Conexion.Usuario,
        //                                        "emitido", "emitido", mEstados.eBinActualConError,
        //                                        mEstados.EnLetras(mEstados.eBinActualConError) + ultimoMensaje.Trim());
        //        }
        //        return numMensajeError==0;
        //    }
        //    catch (DirectoryNotFoundException)
        //    {
        //        ultimoMensaje = "Verifique la existencia de la ruta indicada en la configuración de Ruta de archivos Xml. La ruta no pudo ser encontrada: " + trxVenta.RutaXml;
        //        numMensajeError++;
        //        return false;
        //    }
        //    catch (IOException)
        //    {
        //        ultimoMensaje = "Verifique permisos de escritura en: " + trxVenta.RutaXml + ". No se pudo guardar el archivo xml ni registrar el documento en la bitácora. ";
        //        numMensajeError++;
        //        return false;
        //    }
        //    catch (Exception eAFE)
        //    {
        //        if (eAFE.Message.Contains("denied"))
        //            ultimoMensaje = "Elimine el archivo xml antes de volver a generar uno nuevo. Luego vuelva a intentar. " + eAFE.Message;
        //        else
        //            ultimoMensaje = "Contacte a su administrador. No se pudo guardar el archivo XML ni registrar la Bitácora. " + eAFE.Message;
        //        numMensajeError++;
        //        return false;
        //    }
        //}

        /// <summary>
        /// Genera el código de barras bidimensional y guarda el archivo pdf. 
        /// Luego anota en la bitácora la factura impresa y el nuevo estado binario
        /// </summary>
        /// <param name="trxVenta"></param>
        /// <param name="eBase"></param>
        /// <param name="eBinario"></param>
        /// <param name="enLetras"></param>
        /// <returns></returns>
        //public bool AlmacenaEnRepositorio(vwCfdTransaccionesDeVenta trxVenta, string eBase, string eBinario, string enLetras)
        //{
        //    ultimoMensaje = "";
        //    numMensajeError = 0;

        //    try
        //    {
        //        string nomArchivo = Utiles.FormatoNombreArchivo(trxVenta.Docid + trxVenta.Sopnumbe + "_" + trxVenta.s_CUSTNMBR, trxVenta.s_NombreCliente, 20);
        //        string rutaYNomArchivo = trxVenta.RutaXml.Trim() + nomArchivo;

        //        //Genera y guarda código de barras bidimensional
        //        //codigobb.GenerarQRBidimensional("?re=" + trxVenta.IdImpuesto + "&rr=" + trxVenta.IdImpuestoCliente + "&tt=" + trxVenta.Total.ToString() + 
        //        //                                "&id=" + trxVenta.UUID, trxVenta.RutaXml.Trim() + "cbb\\" + nomArchivo + ".jpg");

        //        //Genera pdf
        //        if (codigobb.iErr == 0)
        //            reporte.generaEnFormatoPDF(rutaYNomArchivo, trxVenta.Soptype, trxVenta.Sopnumbe, trxVenta.EstadoContabilizado);

        //        numMensajeError =  reporte.numErr + codigobb.iErr;
        //        ultimoMensaje = reporte.mensajeErr + codigobb.strMensajeErr;

        //        if (reporte.numErr==0)
        //            RegistraLogDeArchivoXML(trxVenta.Soptype, trxVenta.Sopnumbe, "Almacenado en " + rutaYNomArchivo, "0", 
        //                                _Conexion.Usuario, "", eBase, eBinario, enLetras);

        //        return ultimoMensaje.Equals(string.Empty);
        //    }
        //    catch (Exception eAFE)
        //    {
        //        ultimoMensaje = "Contacte a su administrador. No se pudo guardar el archivo PDF ni registrar la Bitácora. [AlmacenaEnRepositorio()] " + eAFE.Message;
        //        numMensajeError++;
        //        return false;
        //    }
        //}
    }
}
