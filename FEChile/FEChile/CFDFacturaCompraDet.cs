using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEChile
{
    public class CFDFacturaCompraDet
    {
        private String _NmbItem;

        public String NmbItem
        {
            get { return _NmbItem; }
            set { _NmbItem = value; }
        }
        private Decimal _QtyItem;

        public Decimal QtyItem
        {
            get { return _QtyItem; }
            set { _QtyItem = value; }
        }

        private Decimal _PrcItem;

        public Decimal PrcItem
        {
            get { return _PrcItem; }
            set { _PrcItem = value; }
        }

        private Decimal _MontoItem;

        public Decimal MontoItem
        {
            get { return _MontoItem; }
            set { _MontoItem = value; }
        }

    }
}
