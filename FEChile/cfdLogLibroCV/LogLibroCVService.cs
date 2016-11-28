using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyGeneration.dOOdads;

namespace cfd.FacturaElectronica
{
    public class LogLibroCVService : ILogLibroCVService
    {
        private string _sMsj = "";
        private int _iErr = 0;
        private string _connStr;

        public string SMsj
        {
            get { return _sMsj; }
        }
        public int IErr
        {
            get { return _iErr; }
        }

        public LogLibroCVService(string connStr)
        {
            _connStr = connStr;
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
        /// Ingresa el log de la emisión del libro de compra-venta
        /// </summary>
        /// <returns></returns>
        public void Save(int periodo, string tipo, string estado, string mensaje, short idxStatus,
                        string estadoBinario, string mensajeEstadoBin, string innerxml, string idUsuario)
        {
            try
            {
                _sMsj = "";
                _iErr = 0;
                //log de libros de compra - venta
                cfdLogLibroCV logLibro = new cfdLogLibroCV(_connStr);

                logLibro.AddNew();
                logLibro.Periodo = periodo;
                logLibro.Tipo = tipo;
                logLibro.Estado = estado;

                logLibro.MensajeGral = Derecha(mensaje, 255);
                logLibro.EstadoActualBin = estadoBinario;
                logLibro.IdxSingleStatus = idxStatus;
                logLibro.MensajeEActual = Derecha(mensajeEstadoBin, 255);

                if (!innerxml.Equals(string.Empty))
                    logLibro.ArchivoXML = innerxml;

                logLibro.FechaAlta = DateTime.Now;
                logLibro.IdUsuario = Derecha(idUsuario, 10);
                logLibro.IdUsuarioModificacion = "-";
                logLibro.FechaModificacion = new DateTime(1900, 1, 1);

                logLibro.Save();
            }
            catch (Exception eLog)
            {
                _sMsj = "Contacte al administrador. No se puede ingresar el libro "+tipo+" en la bitácora. [LogLibroCVService.Save] " + eLog.Message;
                _iErr++;
                throw;
            }
        }

        /// <summary>
        /// Actualiza el estado del libro en el log. 
        /// </summary>
        /// <returns></returns>
        public void Update(int periodo, string tipo, string estado, short idxStatus,
                            string estadoBinario, string mensajeEA, string idUsuario)
        {
            _sMsj = "";
            _iErr = 0;
            cfdLogLibroCV logLibro = new cfdLogLibroCV(_connStr);
            logLibro.Where.Periodo.Value = periodo;
            logLibro.Where.Periodo.Operator = WhereParameter.Operand.Equal;

            logLibro.Where.Tipo.Conjuction = WhereParameter.Conj.And;
            logLibro.Where.Tipo.Value = tipo;
            logLibro.Where.Tipo.Operator = WhereParameter.Operand.Equal;

            logLibro.Where.Estado.Conjuction = WhereParameter.Conj.And;
            logLibro.Where.Estado.Value = estado;
            logLibro.Where.Estado.Operator = WhereParameter.Operand.Equal;
            try
            {
                if (logLibro.Query.Load())
                {
                    //logLibro.MensajeGral = Derecha(mensaje, 255);
                    logLibro.EstadoActualBin = estadoBinario;
                    logLibro.IdxSingleStatus = idxStatus;
                    logLibro.MensajeEActual = Derecha(mensajeEA, 255);

                    logLibro.FechaModificacion = DateTime.Now;
                    logLibro.IdUsuarioModificacion = Derecha(idUsuario, 10);

                    logLibro.Save();
                }
                else
                {
                    _sMsj = "No está en la bitácora en estado: " + estado;
                    _iErr++;
                }
            }
            catch (Exception eUpd)
            {
                _sMsj = "Contacte al administrador. Error al actualizar la bitácora del libro " + tipo + " [LogLibroCVService.Update] " + eUpd.Message;
                _iErr++;
                throw;
            }
        }

    }
}
