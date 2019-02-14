using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cfdFolios
{
    public partial class CHI10Entities : DbContext
    {
        public CHI10Entities(String connectionString) : base(connectionString)
        {

        }
    }

}
