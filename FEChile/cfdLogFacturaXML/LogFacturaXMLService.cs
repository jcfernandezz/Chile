using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Comun;
using MyGeneration.dOOdads;
using CLConfiguracion;
using MaquinaDeEstados;

namespace cfd.FacturaElectronica
{
    public class LogFacturaXMLService
    {
        private IConexionAFuenteDatos _Conexion = null;

        //*************************************************
        #region Propiedades
        private string _sMsj = "";
        public string sMsj
        {
            get { return _sMsj; }
            set { _sMsj = value; }
        }
        private int _iErr = 0;
        public int iErr
        {
            get { return _iErr; }
            set { _iErr = value; }
        }
        private short _sopType;
        public short SopType
        {
            get { return _sopType; }
            set { _sopType = value; }
        }
        private string _sopnumbe;
        public string Sopnumbe
        {
            get { return _sopnumbe; }
            set { _sopnumbe = value; }
        }

        private String _estadoBase;

        public String EstadoBase
        {
            get { return _estadoBase; }
            set { _estadoBase = value; }
        }

        private String _compoundedBinStatus;
        public String CompoundedBinStatus
        {
            get { return _compoundedBinStatus; }
            set { _compoundedBinStatus = value; }
        }
        private String _idxSingleStatus;
        public String IdxSingleStatus
        {
            get { return _idxSingleStatus; }
            set { _idxSingleStatus = value; }
        }
        private Maquina _cicloDeVida;
        public Maquina CicloDeVida
        {
            get { return _cicloDeVida; }
            set { _cicloDeVida = value; }
        }

        #endregion
        //*************************************************

        public LogFacturaXMLService(IConexionAFuenteDatos conex, String tipoDoc)
        {
            _Conexion = conex;
            _cicloDeVida = new Maquina("emisor", tipoDoc);
        }

        public LogFacturaXMLService(IConexionAFuenteDatos conex, String sopnumbe, short soptype, String estado, String tipoDoc)
        {
            _Conexion = conex;
            _sopnumbe = sopnumbe;
            _sopType = soptype;
            bool trae = TraeEstadoDelIdVentas(_sopnumbe, _sopType, estado);
            if(_iErr==0)
            {
                if (trae)
                    _cicloDeVida = new Maquina(_compoundedBinStatus, _idxSingleStatus.ToString(), 0, "emisor", tipoDoc);
                else
                    _cicloDeVida = new Maquina("emisor", tipoDoc);

                _iErr = _cicloDeVida.iErr;
                _sMsj = _cicloDeVida.sMsj;
            }
        }

        public LogFacturaXMLService(IConexionAFuenteDatos conex, String compoundedBinStatus, String idxSingleStatus, String sopnumbe, short soptype, String tipoDoc)
        {
            _sopnumbe = sopnumbe;
            _sopType = soptype;
            _Conexion = conex;
            _compoundedBinStatus = compoundedBinStatus;
            _idxSingleStatus = idxSingleStatus;
            
            if (!compoundedBinStatus.Equals(String.Empty))
                _cicloDeVida = new Maquina(_compoundedBinStatus, _idxSingleStatus.ToString(), 0, "emisor", tipoDoc);
            else
                _cicloDeVida = new Maquina("emisor", tipoDoc);
            
            _iErr = _cicloDeVida.iErr;
            _sMsj = _cicloDeVida.sMsj;

            //_cicloDeVida = new Maquina(compoundedBinStatus, idxSingleStatus, 0, tipoDoc);
        }

        /// <summary>
        /// Inserta datos de una factura en el log de facturas. 
        /// </summary>
        /// <returns></returns>
        public void Save(short soptype, string sopnumbe, string mensaje, string noAprobacion, string idusuario, string innerxml,
                                            string eBaseNuevo, string eBinarioActual, string mensajeBinActual, string idExterno)
        {
            try
            {
                _sMsj = "";
                _iErr = 0;
                //log de facturas xml emitido y xml anulado
                cfdLogFacturaXML logVenta = new cfdLogFacturaXML(_Conexion.ConnStr);

                logVenta.AddNew();
                logVenta.Soptype = soptype;
                logVenta.Sopnumbe = sopnumbe;

                logVenta.Estado = eBaseNuevo;
                logVenta.EstadoActual = eBinarioActual;
                logVenta.NoAprobacion = noAprobacion;
                logVenta.MensajeEA = Utiles.Derecha(mensajeBinActual, 255);
                logVenta.Mensaje = Utiles.Derecha(mensaje, 255);
                if (!innerxml.Equals(""))
                    logVenta.ArchivoXML = innerxml;
                logVenta.IdExterno = idExterno;

                logVenta.FechaEmision = DateTime.Now;
                logVenta.IdUsuario = Utiles.Derecha(idusuario, 10);
                logVenta.IdUsuarioAnulacion = "-";
                logVenta.FechaAnulacion = new DateTime(1900, 1, 1);

                logVenta.Save();
            }
            catch (Exception eLog)
            {
                _sMsj = "Contacte al administrador. No se puede ingresar la tarea en la Bitácora. [LogFacturaXMLService.Save] " + eLog.Message;
                _iErr++;
                throw;
            }
        }

        /// <summary>
        /// Inserta datos de una factura en el log de facturas. 
        /// </summary>
        /// <returns></returns>
         public void Save(string mensaje, string idusuario, string innerxml, string idExterno)
        {
            try
            {
                _sMsj = "";
                _iErr = 0;
                //log de facturas xml emitido y xml anulado
                cfdLogFacturaXML logVenta = new cfdLogFacturaXML(_Conexion.ConnStr);

                logVenta.AddNew();
                logVenta.Soptype = _sopType;
                logVenta.Sopnumbe = _sopnumbe;

                logVenta.Estado = _cicloDeVida.targetSingleStatus;
                logVenta.EstadoActual = _cicloDeVida.targetBinStatus;
                logVenta.NoAprobacion = _cicloDeVida.idxTargetSingleStatus.ToString();
                logVenta.MensajeEA = Utiles.Derecha(_cicloDeVida.EstadoEnPalabras(_cicloDeVida.targetBinStatus), 255);
                logVenta.Mensaje = Utiles.Derecha(mensaje, 255);
                if (!innerxml.Equals(""))
                    logVenta.ArchivoXML = innerxml;
                logVenta.IdExterno = idExterno;

                logVenta.FechaEmision = DateTime.Now;
                logVenta.IdUsuario = Utiles.Derecha(idusuario, 10);
                logVenta.IdUsuarioAnulacion = "-";
                logVenta.FechaAnulacion = new DateTime(1900, 1, 1);

                logVenta.Save();
            }
            catch (Exception eLog)
            {
                _sMsj = "Contacte al administrador. No se puede ingresar la tarea en la Bitácora. [LogFacturaXMLService.Save] " + eLog.Message;
                _iErr++;
                throw;
            }
        }

        /// <summary>
        /// Actualiza la fecha, estado y observaciones de una factura emitida en el log de facturas. 
        /// </summary>
        /// <returns></returns>
        public void Update(string idusuario, string eBaseAnterior, string eBaseNuevo)
        {
            _sMsj = "";
            _iErr = 0;
            cfdLogFacturaXML xmlEmitido = new cfdLogFacturaXML(_Conexion.ConnStr);
            xmlEmitido.Where.Soptype.Value = _sopType;
            xmlEmitido.Where.Soptype.Operator = WhereParameter.Operand.Equal;
            xmlEmitido.Where.Sopnumbe.Conjuction = WhereParameter.Conj.And;
            xmlEmitido.Where.Sopnumbe.Value = _sopnumbe;
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
                    xmlEmitido.EstadoActual = _cicloDeVida.targetBinStatus;
                    xmlEmitido.MensajeEA = Utiles.Derecha(_cicloDeVida.EstadoEnPalabras(_cicloDeVida.targetBinStatus), 255);
                    xmlEmitido.NoAprobacion = _cicloDeVida.idxTargetSingleStatus.ToString();
                    xmlEmitido.Save();
                }
                else
                {
                    _sMsj = "No se puede actualizar porque no está en la bitácora con estado base.";
                    _iErr++;
                }
            }
            catch (Exception eAnula)
            {
                _sMsj = "Contacte al administrador. Error al acceder a la base de datos. [LogFacturaXMLService.Update] " + eAnula.Message;
                _iErr++;
                throw;
            }
        }

        /// <summary>
        /// Actualiza la fecha, estado y observaciones de una factura emitida en el log de facturas.
        /// </summary>
        /// <param name="Soptype"></param>
        /// <param name="Sopnumbe"></param>
        /// <param name="idusuario"></param>
        /// <param name="eBaseAnterior"></param>
        /// <param name="eBaseNuevo"></param>
        /// <param name="eBinarioActual"></param>
        /// <param name="mensajeEA"></param>
        /// <param name="noAprobacion"></param>
        /// <param name="IdxExterno">Si está vacío no lo actualiza</param>
        public void Update(short Soptype, string Sopnumbe, string idusuario, string eBaseAnterior, string eBaseNuevo, string eBinarioActual, string mensajeEA, string noAprobacion,
                            String IdxExterno)
        {
            _sMsj = "";
            _iErr = 0;
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
                    xmlEmitido.NoAprobacion = noAprobacion;

                    if (!IdxExterno.Equals(String.Empty))
                        xmlEmitido.IdExterno = IdxExterno;

                    xmlEmitido.Save();
                }
                else
                {
                    _sMsj = "No se puede actualizar porque no está en la bitácora con estado 'emitido'.";
                    _iErr++;
                }
            }
            catch (Exception eAnula)
            {
                _sMsj = "Contacte al administrador. Error al acceder a la base de datos. [LogFacturaXMLService.Update] " + eAnula.Message;
                _iErr++;
                throw;
            }
        }

        public void Delete(short Soptype, string Sopnumbe, string eBinario)
        {
            _sMsj = "";
            _iErr = 0;
            cfdLogFacturaXML xmlEmitido = new cfdLogFacturaXML(_Conexion.ConnStr);
            xmlEmitido.Where.Soptype.Value = Soptype;
            xmlEmitido.Where.Soptype.Operator = WhereParameter.Operand.Equal;
            xmlEmitido.Where.Sopnumbe.Conjuction = WhereParameter.Conj.And;
            xmlEmitido.Where.Sopnumbe.Value = Sopnumbe;
            xmlEmitido.Where.Sopnumbe.Operator = WhereParameter.Operand.Equal;
            xmlEmitido.Where.EstadoActual.Conjuction = WhereParameter.Conj.And;
            xmlEmitido.Where.EstadoActual.Value = eBinario;
            xmlEmitido.Where.EstadoActual.Operator = WhereParameter.Operand.Equal;
            try
            {
                if (xmlEmitido.Query.Load())
                {
                    xmlEmitido.MarkAsDeleted();
                    xmlEmitido.Save();
                }
                else
                {
                    _sMsj = "No se puede eliminar porque no está en la bitácora.";
                    _iErr++;
                }
            }
            catch (Exception eAnula)
            {
                _sMsj = "Contacte al administrador. Error al acceder a la base de datos. [LogFacturaXMLService.Delete] " + eAnula.Message;
                _iErr++;
                throw;
            }

        }

        /// <summary>
        /// Debería devolver un solo registro correspondiente al trackid del SII
        /// </summary>
        /// <param name="idExterno">trackid del SII</param>
        /// <param name="estado">estado emitido</param>
        /// <returns></returns>
        public bool TraeLlaves(String idExterno, String estado)
        {
            cfdLogFacturaXML log = new cfdLogFacturaXML(_Conexion.ConnStr);

            log.Where.Estado.Value = estado;
            log.Where.Estado.Operator = WhereParameter.Operand.Equal;
            log.Where.IdExterno.Conjuction = WhereParameter.Conj.And;
            log.Where.IdExterno.Value = idExterno;
            log.Where.IdExterno.Operator = WhereParameter.Operand.Equal;
            bool existe = false;
            try
            {
                if (log.Query.Load())
                {
                    log.Rewind();

                    do
                    {
                        _sopType = log.Soptype;
                        _sopnumbe = log.Sopnumbe;
                        _compoundedBinStatus = log.EstadoActual;
                        _idxSingleStatus = log.NoAprobacion;
                        existe = true;
                    } while (log.MoveNext());
                }
            }
            catch (Exception ePla)
            {
                _sMsj = "Contacte al administrador. Error al obtener log de facturas filtrado por trackid. " + ePla.Message + "[LogFacturaXMLService.TraeLlaves()]";
                _iErr++;
                return existe;
            }
            return existe;
        }

        /// <summary>
        /// Debería devolver un solo registro correspondiente al estado de la factura dado sopType y sopNumbe.
        /// </summary>
        /// <returns></returns>
        public bool TraeEstadoDelIdVentas(String sopnumbe, short soptype, String estadoBase)
        {
            cfdLogFacturaXML log = new cfdLogFacturaXML(_Conexion.ConnStr);
            log.Where.Sopnumbe.Value = sopnumbe;
            log.Where.Sopnumbe.Operator = WhereParameter.Operand.Equal;
            log.Where.Soptype.Conjuction = WhereParameter.Conj.And;
            log.Where.Soptype.Value = soptype;
            log.Where.Soptype.Operator = WhereParameter.Operand.Equal;
            log.Where.Estado.Conjuction = WhereParameter.Conj.And;
            log.Where.Estado.Value = estadoBase;
            log.Where.Estado.Operator = WhereParameter.Operand.Equal;
            try
            {
                if (log.Query.Load())
                {
                    log.Rewind();

                    do
                    {
                        _compoundedBinStatus = log.EstadoActual;
                        _idxSingleStatus = log.NoAprobacion;

                    } while (log.MoveNext());
                }
            }
            catch (Exception ePla)
            {
                _sMsj = "Contacte al administrador. Error al obtener log de facturas filtrado por llave. " + ePla.Message + "[LogFacturaXMLService.GetStatus()]";
                _iErr++;
                return false;
            }
            return true;
        }

    }
}
