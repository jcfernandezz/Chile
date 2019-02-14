﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace cfdFolios
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class CHI10Entities : DbContext
    {
        public CHI10Entities()
            : base("name=CHI10Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<cfd_FOL00100> cfd_FOL00100 { get; set; }
        public virtual DbSet<vwCfdIdDocumentos> vwCfdIdDocumentos { get; set; }
    
        public virtual int SP_cfd_FOL00100(Nullable<short> sOPTYPE, string dOCID, Nullable<int> num_folio_desde, Nullable<int> num_folio_hasta, string ruta_codigo_autorizacion)
        {
            var sOPTYPEParameter = sOPTYPE.HasValue ?
                new ObjectParameter("SOPTYPE", sOPTYPE) :
                new ObjectParameter("SOPTYPE", typeof(short));
    
            var dOCIDParameter = dOCID != null ?
                new ObjectParameter("DOCID", dOCID) :
                new ObjectParameter("DOCID", typeof(string));
    
            var num_folio_desdeParameter = num_folio_desde.HasValue ?
                new ObjectParameter("num_folio_desde", num_folio_desde) :
                new ObjectParameter("num_folio_desde", typeof(int));
    
            var num_folio_hastaParameter = num_folio_hasta.HasValue ?
                new ObjectParameter("num_folio_hasta", num_folio_hasta) :
                new ObjectParameter("num_folio_hasta", typeof(int));
    
            var ruta_codigo_autorizacionParameter = ruta_codigo_autorizacion != null ?
                new ObjectParameter("ruta_codigo_autorizacion", ruta_codigo_autorizacion) :
                new ObjectParameter("ruta_codigo_autorizacion", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("SP_cfd_FOL00100", sOPTYPEParameter, dOCIDParameter, num_folio_desdeParameter, num_folio_hastaParameter, ruta_codigo_autorizacionParameter);
        }
    }
}