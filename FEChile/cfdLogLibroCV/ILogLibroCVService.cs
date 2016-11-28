using System;


namespace cfd.FacturaElectronica
{
    public interface ILogLibroCVService
    {
        int IErr { get; }
        string SMsj { get; }

        void Save(int periodo, string tipo, string estado, string mensaje, short idxStatus,
                        string estadoBinario, string mensajeEstadoBin, string innerxml, string idUsuario);

        void Update(int periodo, string tipo, string estado, short idxStatus,
                            string estadoBinario, string mensajeEA, string idUsuario);

    }
}
