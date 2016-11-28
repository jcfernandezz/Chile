using MaquinaDeEstados;
using MyGeneration.dOOdads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cfd.FacturaElectronica
{
    public class LogFacturaCompraService //: ILogFacturaCompraService
    {
        private string _sMsj = "";
        private int _iErr = 0;
        private string _connStr;

        //*************************************************************
        #region Propiedades
        private Maquina _cicloDeVida;
        public Maquina CicloDeVida
        {
            get { return _cicloDeVida; }
            set { _cicloDeVida = value; }
        }
        private String _compoundedBinStatus;
        public String CompoundedBinStatus
        {
            get { return _compoundedBinStatus; }
            set { _compoundedBinStatus = value; }
        }
        private short _idxSingleStatus;
        public short IdxSingleStatus
        {
            get { return _idxSingleStatus; }
            set { _idxSingleStatus = value; }
        }

        public string SMsj
        {
            get { return _sMsj; }
        }

        public int IErr
        {
            get { return _iErr; }
        }

        private String _idExterno;
        public string IdExterno
        {
            get { return _idExterno; }
            set { _idExterno = value; }
        }
        private short _tipo;
        public short Tipo
        {
            get { return _tipo; }
            set { _tipo = value; }
        }
        private String _folio;

        public String Folio
        {
            get { return _folio; }
            set { _folio = value; }
        }
        private String _sDocXml;

        public String SDocXml
        {
            get { return _sDocXml; }
            set { _sDocXml = value; }
        }
        private String _idImpuestoTercero;

        public String IdImpuestoTercero
        {
            get { return _idImpuestoTercero; }
            set { _idImpuestoTercero = value; }
        }
        private String _nombreTercero;

        public String NombreTercero
        {
            get { return _nombreTercero; }
            set { _nombreTercero = value; }
        }
        private DateTime _fechaRecepcion;

        public DateTime FechaRecepcion
        {
            get { return _fechaRecepcion; }
            set { _fechaRecepcion = value; }
        }
        private String _mensaje;

        public String Mensaje
        {
            get { return _mensaje; }
            set { _mensaje = value; }
        }
        private String _pdf;

        public String Pdf
        {
            get { return _pdf; }
            set { _pdf = value; }
        }
        private String _idUsuario;

        public String IdUsuario
        {
            get { return _idUsuario; }
            set { _idUsuario = value; }
        }
        private bool _existeDoc = false;

        public bool ExisteDoc
        {
            get { return _existeDoc; }
            set { _existeDoc = value; }
        }

        #endregion        
        //*************************************************************

        public LogFacturaCompraService(string connStr)
        {
            _connStr = connStr;
        }

        public LogFacturaCompraService(string connStr, String folio, short tipo, String idImpuestoTercero, String estado)
        {
            _connStr = connStr;
            _tipo = tipo;
            _folio = folio;
            _existeDoc = Get(folio, tipo, idImpuestoTercero, estado);
            if (_existeDoc)
                _cicloDeVida = new Maquina(_compoundedBinStatus, _idxSingleStatus.ToString(), 0, "receptor", tipo.ToString());
            else
                _cicloDeVida = new Maquina("receptor", tipo.ToString());

        }

        private string Derecha(string Texto, int Cuantos)
        {
            if (Texto.Length > Cuantos && Cuantos > 0)
            {
                return Texto.Remove(0, Texto.Length - Cuantos);
            }
            else
                return Texto;
        }

        /// <summary>
        /// Inserta datos de una factura en el log de facturas de compra. 
        /// </summary>
        /// <returns></returns>
        public void Save(short tipo, string folio, string idImpuesto, string nomTercero, DateTime fechaRecepcion, string eBaseNuevo, string mensaje, 
                        short idxSingleStatus, string eBinarioActual, string mensajeBinActual, string innerxml, string pdf, string idExterno, string idUsuario)
        {
            try
            {
                _sMsj = "";
                _iErr = 0;
                //log de facturas de compra
                cfdLogFacturaCompra logCompra = new cfdLogFacturaCompra(_connStr);

                logCompra.AddNew();
                logCompra.Tipo = tipo;
                logCompra.Folio = folio;
                logCompra.IdImpuestoTercero = idImpuesto;
                logCompra.NombreTercero = nomTercero;
                logCompra.FechaRecepcion = fechaRecepcion;

                logCompra.IdxSingleStatus = idxSingleStatus;
                logCompra.Estado = eBaseNuevo;
                logCompra.Mensaje = Derecha(mensaje, 255);
                logCompra.EstadoActual = eBinarioActual;
                logCompra.MensajeEA = Derecha(mensajeBinActual, 255);

                if (!innerxml.Equals(string.Empty))
                    logCompra.ArchivoXML = innerxml;

                logCompra.ArchivoPDF = Derecha(pdf, 255);
                logCompra.IdExterno = Derecha(idExterno, 25);

                logCompra.FechaAlta = DateTime.Now;
                logCompra.IdUsuario = Derecha(idUsuario, 10);
                logCompra.IdUsuarioModificacion = "-";
                logCompra.FechaModificacion = new DateTime(1900, 1, 1);

                logCompra.Save();
            }
            catch (Exception eLog)
            {
                _sMsj = "Contacte al administrador. No se puede ingresar la factura de compra en la bitácora. [LogFacturaCompraService.Save] " + eLog.Message;
                _iErr++;
                throw;
            }
        }

        /// <summary>
        /// Inserta datos de una factura en el log de facturas de compra. 
        /// </summary>
        /// <returns></returns>
        public void Save(short tipo, string folio, string idImpuesto, string nomTercero, DateTime fechaRecepcion, string mensaje,
                        string innerxml, string pdf, string idExterno, string idUsuario)
        {
            try
            {
                _sMsj = "";
                _iErr = 0;
                //log de facturas de compra
                cfdLogFacturaCompra logCompra = new cfdLogFacturaCompra(_connStr);

                logCompra.AddNew();
                logCompra.Tipo = tipo;
                logCompra.Folio = folio;
                logCompra.IdImpuestoTercero = idImpuesto;
                logCompra.NombreTercero = nomTercero;
                logCompra.FechaRecepcion = fechaRecepcion;
                logCompra.IdxSingleStatus = Convert.ToInt16( _cicloDeVida.idxTargetSingleStatus);
                logCompra.Estado = _cicloDeVida.targetSingleStatus;
                logCompra.Mensaje = Derecha(mensaje, 255);
                logCompra.EstadoActual = _cicloDeVida.targetBinStatus;
                logCompra.MensajeEA = Derecha(_cicloDeVida.EstadoEnPalabras(_cicloDeVida.targetBinStatus), 255);

                if (!innerxml.Equals(string.Empty))
                    logCompra.ArchivoXML = innerxml;

                logCompra.ArchivoPDF = Derecha(pdf, 255);
                logCompra.IdExterno = Derecha(idExterno, 25);

                logCompra.FechaAlta = DateTime.Now;
                logCompra.IdUsuario = Derecha(idUsuario, 10);
                logCompra.IdUsuarioModificacion = "-";
                logCompra.FechaModificacion = new DateTime(1900, 1, 1);

                logCompra.Save();
            }
            catch (Exception eLog)
            {
                _sMsj = "Contacte al administrador. No se puede ingresar la factura de compra en el log. [LogFacturaCompraService.Save] " + eLog.Message;
                _iErr++;
                throw;
            }
        }

        /// <summary>
        /// Actualiza los datos de una factura publicada en el log de facturas de compra. 
        /// </summary>
        /// <returns></returns>
        public void Update( short tipo, string folio, string idImpuesto, string idusuario, string eBaseAnterior, string eBaseNuevo, 
                            string eBinarioActual, string mensajeEA, short idxSingleStatus, string idExterno)
        {
            _sMsj = "";
            _iErr = 0;
            cfdLogFacturaCompra xmlPublicado = new cfdLogFacturaCompra(_connStr);
            xmlPublicado.Where.Tipo.Value = tipo;
            xmlPublicado.Where.Tipo.Operator = WhereParameter.Operand.Equal;

            xmlPublicado.Where.Folio.Conjuction = WhereParameter.Conj.And;
            xmlPublicado.Where.Folio.Value = folio;
            xmlPublicado.Where.Folio.Operator = WhereParameter.Operand.Equal;

            xmlPublicado.Where.IdImpuestoTercero.Conjuction = WhereParameter.Conj.And;
            xmlPublicado.Where.IdImpuestoTercero.Value = idImpuesto;
            xmlPublicado.Where.IdImpuestoTercero.Operator = WhereParameter.Operand.Equal;

            xmlPublicado.Where.Estado.Conjuction = WhereParameter.Conj.And;
            xmlPublicado.Where.Estado.Value = eBaseAnterior;      // "publicado";
            xmlPublicado.Where.Estado.Operator = WhereParameter.Operand.Equal;
            try
            {
                if (xmlPublicado.Query.Load())
                {
                    if (!eBaseAnterior.Equals(eBaseNuevo))
                        xmlPublicado.Estado = eBaseNuevo;

                    xmlPublicado.EstadoActual = eBinarioActual;
                    xmlPublicado.MensajeEA = Derecha(mensajeEA, 255);
                    xmlPublicado.IdExterno = Derecha(idExterno, 25);

                    xmlPublicado.FechaModificacion = DateTime.Now;
                    xmlPublicado.IdUsuarioModificacion = Derecha(idusuario, 10);
                    xmlPublicado.IdxSingleStatus = idxSingleStatus;
                    xmlPublicado.Save();
                }
                else
                {
                    _sMsj = "No está en la bitácora con estado 'emitido'.";
                    _iErr++;
                }
            }
            catch (Exception eAnula)
            {
                _sMsj = "Contacte al administrador. Error al actualizar la bitácora. [LogFacturaCompraService.Update] " + eAnula.Message;
                _iErr++;
                throw;
            }
        }

        /// <summary>
        /// Actualiza los datos de una factura publicada en el log de facturas de compra. 
        /// </summary>
        /// <returns></returns>
        public void Update(short tipo, string folio, string idImpuesto, string idusuario, string eBaseAnterior, string eBaseNuevo, string idExterno)
        {
            _sMsj = "";
            _iErr = 0;
            cfdLogFacturaCompra xmlPublicado = new cfdLogFacturaCompra(_connStr);
            xmlPublicado.Where.Tipo.Value = tipo;
            xmlPublicado.Where.Tipo.Operator = WhereParameter.Operand.Equal;

            xmlPublicado.Where.Folio.Conjuction = WhereParameter.Conj.And;
            xmlPublicado.Where.Folio.Value = folio;
            xmlPublicado.Where.Folio.Operator = WhereParameter.Operand.Equal;

            xmlPublicado.Where.IdImpuestoTercero.Conjuction = WhereParameter.Conj.And;
            xmlPublicado.Where.IdImpuestoTercero.Value = idImpuesto;
            xmlPublicado.Where.IdImpuestoTercero.Operator = WhereParameter.Operand.Equal;

            xmlPublicado.Where.Estado.Conjuction = WhereParameter.Conj.And;
            xmlPublicado.Where.Estado.Value = eBaseAnterior;      // "publicado";
            xmlPublicado.Where.Estado.Operator = WhereParameter.Operand.Equal;
            try
            {
                if (xmlPublicado.Query.Load())
                {
                    if (!eBaseAnterior.Equals(eBaseNuevo))
                        xmlPublicado.Estado = eBaseNuevo;

                    xmlPublicado.IdxSingleStatus = Convert.ToInt16( _cicloDeVida.idxTargetSingleStatus);
                    xmlPublicado.EstadoActual = _cicloDeVida.targetBinStatus;
                    xmlPublicado.MensajeEA = Derecha(_cicloDeVida.EstadoEnPalabras(_cicloDeVida.targetBinStatus), 255);
                    xmlPublicado.IdExterno = Derecha(idExterno, 25);

                    xmlPublicado.FechaModificacion = DateTime.Now;
                    xmlPublicado.IdUsuarioModificacion = Derecha(idusuario, 10);

                    xmlPublicado.Save();
                }
                else
                {
                    _sMsj = "El estado base no existe";
                    _iErr = -1;
                }
            }
            catch (Exception eAnula)
            {
                _sMsj = "Contacte al administrador. Error al actualizar el log. [LogFacturaCompraService.Update] " + eAnula.Message;
                _iErr++;
                throw;
            }
        }

        public void Delete(String folio, short tipo, String idImpuestoTercero, String estado)
        {
            _sMsj = "";
            _iErr = 0;
            cfdLogFacturaCompra xmlEmitido = new cfdLogFacturaCompra(_connStr);
            xmlEmitido.Where.Folio.Value = folio;
            xmlEmitido.Where.Folio.Operator = WhereParameter.Operand.Equal;
            xmlEmitido.Where.Tipo.Conjuction = WhereParameter.Conj.And;
            xmlEmitido.Where.Tipo.Value = tipo;
            xmlEmitido.Where.Tipo.Operator = WhereParameter.Operand.Equal;
            xmlEmitido.Where.IdImpuestoTercero.Conjuction = WhereParameter.Conj.And;
            xmlEmitido.Where.IdImpuestoTercero.Value = idImpuestoTercero;
            xmlEmitido.Where.IdImpuestoTercero.Operator = WhereParameter.Operand.Equal;
            xmlEmitido.Where.Estado.Conjuction = WhereParameter.Conj.And;
            xmlEmitido.Where.Estado.Value = estado;
            xmlEmitido.Where.Estado.Operator = WhereParameter.Operand.Equal;
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
                _sMsj = "Contacte al administrador. Error al acceder a la base de datos. [LogFacturaCompraService.Delete] " + eAnula.Message;
                _iErr++;
                throw;
            }

        }

        public bool Get(String folio, short tipo, String idImpuestoTercero, String estado)
        {
            cfdLogFacturaCompra log = new cfdLogFacturaCompra(_connStr);
            try
            {
                if (log.LoadByPrimaryKey(tipo, folio, idImpuestoTercero, estado))
                {
                    _idExterno = log.IdExterno;
                    _compoundedBinStatus = log.EstadoActual;
                    _idxSingleStatus = log.IdxSingleStatus;
                }
                else
                    return false;
            }
            catch (Exception ePla)
            {
                _sMsj = "Contacte al administrador. Error al obtener log de facturas de compra por llave primaria. " + ePla.Message + "[LogFacturaCompraService.get()]";
                _iErr++;
                return false;
            }
            return true;
        }

        public List<String> GetFiltraEstado(String estado)
        {
            cfdLogFacturaCompra log = new cfdLogFacturaCompra(_connStr);
            List<String> vistos = new List<String>();

            log.Where.Estado.Value = estado;
            log.Where.Estado.Operator = WhereParameter.Operand.Equal;
            log.Query.AddOrderBy(cfdLogFacturaCompra.ColumnNames.Folio, WhereParameter.Dir.ASC);
            try
            {
                if (log.Query.Load())
                {
                    log.Rewind();

                    do
                    {
                        vistos.Add(log.IdExterno);

                    } while (log.MoveNext());
                }
            }
            catch (Exception ePla)
            {
                _sMsj = "Contacte al administrador. Error al obtener log de facturas de compra filtrado por estado. " + ePla.Message + "[LogFacturaCompraService.GetFiltraEstado()]";
                _iErr++;
                return null;
            }
            return vistos;
        }

        public List<String> TraeTodos()
        {
            cfdLogFacturaCompra log = new cfdLogFacturaCompra(_connStr);
            List<String> vistos = new List<String>();

            //log.Query.AddOrderBy(cfdLogFacturaCompra.ColumnNames.Folio, WhereParameter.Dir.ASC);
            try
            {
                if (log.Query.Load())
                {
                    log.Rewind();

                    do
                    {
                        vistos.Add(log.IdExterno);

                    } while (log.MoveNext());
                }
            }
            catch (Exception ePla)
            {
                _sMsj = "Contacte al administrador. Error al obtener log de facturas de compra. " + ePla.Message + "[LogFacturaCompraService.TraeTodos()]";
                _iErr++;
                return null;
            }
            return vistos;
        }

        public void GuardaYActualiza()
        {
            _iErr = 0;
            _sMsj = String.Empty;

            try
            {
                String xDoc = _sDocXml.Replace("<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\n", "");
                //short tipo = Convert.ToInt16(_tipoDTE);
                //LogFacturaCompraService logReceptor = new LogFacturaCompraService(_connStr, _folio, tipo, _rutReceptor, Maquina.estadoBaseReceptor);

                //if (logReceptor.CicloDeVida.Transiciona(evento, _tieneAcceso))
                //{
                Save(_tipo, _folio, _idImpuestoTercero, _nombreTercero, _fechaRecepcion, _mensaje, xDoc, _pdf, _idExterno, _idUsuario);
                Update(_tipo, _folio, _idImpuestoTercero, _idUsuario, Maquina.estadoBaseReceptor, Maquina.estadoBaseReceptor, _idExterno);

                if (_iErr == -1) //No existe el estado base
                {
                    Save(_tipo, _folio, _idImpuestoTercero, _nombreTercero, _fechaRecepcion, Maquina.estadoBaseReceptor, _mensaje, 
                        Convert.ToInt16( Maquina.binStatusBaseReceptor.IndexOf('1')), Maquina.binStatusBaseReceptor, Maquina.estadoBaseReceptor,
                        xDoc, _pdf, _idExterno, _idUsuario);
                    Update(_tipo, _folio, _idImpuestoTercero, _idUsuario, Maquina.estadoBaseReceptor, Maquina.estadoBaseReceptor, _idExterno);
                }

            }
            catch (Exception pr)
            {
                _sMsj = "Excepción desconocida al guardar en el log. " + pr.Message + " [LogFacturaCompraService.GuardaYActualiza]";
                _iErr++;
            }

        }
    }
}
