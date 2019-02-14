using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cfdFolios
{
    public class IntegraBD
    {
        string _connStringTargetEF;

        public IntegraBD(string connStringTarget)
        {
            _connStringTargetEF = connStringTarget;
        }

        private CHI10Entities getDbContextGP()
        {
            if (string.IsNullOrEmpty(_connStringTargetEF))
                return new CHI10Entities();

            return new CHI10Entities(_connStringTargetEF);
        }

        private bool probarConexionDBGP()
        {
            using (var db = this.getDbContextGP())
            {
                return db.Database.Exists();
            }
        }

        public IList<vwCfdIdDocumentos> getIdDocumentos(String tipoDocSii)
        {
            using (var db = this.getDbContextGP())
            {
                // verificar la conexión con el servidor de bd
                if (!this.probarConexionDBGP())
                {
                    throw new InvalidOperationException("No se pudo establecer la conexión con el servidor al tratar de leer los Id de ventas para el tipo de documento del SII: " + tipoDocSii);
                }

                var datos = db.vwCfdIdDocumentos.AsQueryable();
                datos = datos.Where(m => m.commntid == tipoDocSii);
                return datos.ToList();
            }
        }

        public List<string> getFolios()
        {
            using (var db = this.getDbContextGP())
            {
                // verificar la conexión con el servidor de bd
                if (!this.probarConexionDBGP())
                {
                    throw new InvalidOperationException("No se pudo establecer la conexión con el servidor al tratar de leer la configuración de folios.");
                }

                var datos = db.cfd_FOL00100.AsQueryable();

                return datos
                        .Select(f => f.DOCID + ": " + f.num_folio_desde.ToString() + " - " + f.num_folio_hasta.ToString())
                        .ToList();
            }
        }

        public async Task<IList<cfd_FOL00100>> getFoliosAsync()
        {
            using (var db = this.getDbContextGP())
            {
                // verificar la conexión con el servidor de bd
                if (!this.probarConexionDBGP())
                {
                    throw new InvalidOperationException("No se pudo establecer la conexión con el servidor al tratar de leer la configuración de folios.");
                }

                var datos = db.cfd_FOL00100.AsQueryable();

                return await datos.ToListAsync<cfd_FOL00100>();
            }
        }
        public void IngresaRangoFolios(short? soptype, string docid, int? num_folio_desde,  int? num_folio_hasta, string ruta_codigo_autorizacion)
        {
            using (var db = this.getDbContextGP())
            {
                // verificar la conexión con el servidor de bd
                if (!this.probarConexionDBGP())
                {
                    throw new InvalidOperationException("No se pudo establecer la conexión con el servidor al ingresar el nuevo rango de folios del id: " + docid );
                }

                db.SP_cfd_FOL00100(soptype, docid, num_folio_desde, num_folio_hasta, ruta_codigo_autorizacion);

            }
        }

    }
}
