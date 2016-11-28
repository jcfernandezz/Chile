using System;

namespace cfd.FacturaElectronica
{
    public class cfdLogFacturaCompra : _cfdLogFacturaCompra
    {
        public cfdLogFacturaCompra (string connstr)
        {
            this.ConnectionString = connstr;
        }
    }

}
