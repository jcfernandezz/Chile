using MaquinaDeEstados;
using System;
using System.Collections.Generic;

namespace cfd.FacturaElectronica
{
    public interface ILogFacturaCompraService
    {
        int IErr { get; }
        string SMsj { get; }
        string IdExterno {get;  }
        Maquina CicloDeVida
        {
            get;
            set;
        }
        String CompoundedBinStatus
        {
            get;
            set;
        }
        short IdxSingleStatus
        {
            get;
            set;
        }

        void Save(short tipo, string folio, string idImpuesto, string nomTercero, DateTime fechaRecepcion, string eBaseNuevo, string mensaje, string eBinarioActual, string mensajeBinActual, string innerxml, string pdf, string idExterno, string idUsuario);
        void Update(short tipo, string folio, string idImpuesto, string idusuario, string eBaseAnterior, string eBaseNuevo, string eBinarioActual, string mensajeEA, string idExterno);
        bool Get(String folio, short tipo, String idImpuestoTercero, String estado);
        List<String> GetFiltraEstado(String estado);
        List<String> TraeTodos();
    }
}
