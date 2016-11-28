using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaquinaDeEstados
{
    public class Estado
    {
        private String _nombre;
        private int _idx;
        private int _idxSuperEstado;

        public Estado(String nombre, int idx, int idxSuperEstado)
        {
            _nombre = nombre;
            _idx = idx;
            _idxSuperEstado = idxSuperEstado;
        }
    }
}
