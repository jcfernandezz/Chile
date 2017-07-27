--GETTY
--Propósito. Libro de compras en crudo para factura ELECTRONICA IECV de Chile
----------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.f_obtiene_porcentaje_impuesto') IS NOT NULL
drop FUNCTION f_obtiene_porcentaje_impuesto 
GO

create FUNCTION dbo.f_obtiene_porcentaje_impuesto (@p_idimpuesto varchar(20))
RETURNS numeric(19,2)
AS
BEGIN
   DECLARE @l_TXDTLPCT numeric(19,2)
   select @l_TXDTLPCT = round(TXDTLPCT, 2) from tx00201 where taxdtlid = @p_idimpuesto
   RETURN(@l_TXDTLPCT)
END
go

IF (@@Error = 0) PRINT 'Creación exitosa de: f_obtiene_porcentaje_impuesto ()'
ELSE PRINT 'Error en la creación de: f_obtiene_porcentaje_impuesto ()'
GO

----------------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.vwIecvLibroComprasCrudo') IS NOT NULL
   DROP view dbo.vwIecvLibroComprasCrudo
GO

create view dbo.vwIecvLibroComprasCrudo as
--Propósito. Vista que obtiene los datos fundamentales para armar el archivo IECV de libro de compras
--19/12/07 JCF Creación
--15/5/08 JCF Se quitó el tipo de documento. Ese campo se debe formar en el reporte ya que depende de la configuración de la localización de docs chilenos.
--            Se añadió la tabla loch0002 para obtener la descripción del documento
--1/8/08 JCF Se recalculó un total ficticio para las facturas de Aduana (914 Declaración de Ingreso). Requerimiento legal. 
--3/9/08 JCF Las facturas con número = TARJETA DE CREDITO no deben ser consideradas en el libro
--9/4/09 JCF Se limitó la longitud del docnumbr a 10. (Signature por alguna razón no acepta más de 10)
--18/8/09 jcf Cuando se trata de una recepción/factura obtener el código impositivo a partir del número de recepción
--19/12/14 jcf Agrega pm00200. Emitir con rut 5 los proveedores extranjeros
--17/3/15 jcf Agrega exentoCalculado. Modifica neto.
--13/8/15 jcf Modifica folio. Caso de docnumbr = '1-1-5545454-1-23222' debe obtener el último segmento
--25/8/15 jcf Excluye devoluciones pagadas porque no tiene sentido. Además, no se pueden anular en GP
--21/4/16 jcf Elimina caracteres no imprimibles del folio
--
select a.doctype, a.vchrnmbr, a.docnumbr,

		case when  charindex('-', reverse(a.docnumbr)) > 0 then
					dbo.fCfdObtieneFolio(reverse(left(reverse(rtrim(a.docnumbr)), charindex('-', reverse(rtrim(a.docnumbr))))), '-')
				else
					dbo.fCfdObtieneFolio(right(rtrim(dbo.fCfdReemplazaCaracteresNI(a.docnumbr)), 10), '-')		--long máx 10. req legal de fe
				end folio,
							
		isnull(dbo.f_obtiene_porcentaje_impuesto (a.iva_TAXDTLID), 0) tasaIVA, 
		a.docdate, 
		case when mp.vndclsid in ('EXTRANJERO', 'ROYALTIES') then '55555555-5' else a.rutClieProvee end rutClieProvee, 
		left(a.RazonSocial, 50) RazonSocial, 
		cast(a.exentoCalculado as numeric(18)) exentoCalculado,
		cast(a.exe_taxableAmnt as numeric(18)) exento, 
		cast(a.docamnt - a.iva_taxamnt - iuc_taxamnt - inr_taxamnt - ri_taxamnt - a.exentoCalculado	--a.exe_taxableAmnt
			+ case when isnull(cdt.lochdoccod, '') = '914' or upper(a.pordnmbr) like '%IMPO%' then round(a.iva_taxamnt*100/dbo.f_obtiene_porcentaje_impuesto (a.iva_TAXDTLID), 0) else 0 end 
			as numeric(18)) neto, 
		cast(a.iva_taxamnt as numeric(18)) iva,
		case when a.SHIPMTHD = 'COMPRAS' then '9' else left(a.SHIPMTHD, 1) end codIvaNoRec,
		CAST(a.inr_taxamnt as numeric(18)) inr_taxamnt,
		
		cast(a.iuc_taxamnt as numeric(18)) iuc_taxamnt,
		
		'15' otrosImp,
		isnull(dbo.f_obtiene_porcentaje_impuesto (a.ri_TAXDTLID), 0) tasaReteIva, 
		CAST(a.ri_taxamnt as numeric(18)) ri_taxamnt,
		
		cast(a.docamnt
			+ case when isnull(cdt.lochdoccod, '') = '914' or upper(a.pordnmbr) like '%IMPO%' then round(a.iva_taxamnt*100/dbo.f_obtiene_porcentaje_impuesto (a.iva_TAXDTLID), 0) else 0 end 
			as numeric(18)) total, 
		a.pstgdate, a.voided, rtrim(cdt.lochdoccod) lochdoccod, cde.dscriptn, isnull(cdt.rprttype, 2) rprttype, -- 1:electrónico, 2:no electrónico
		--convert(varchar(10), year(a.docdate)) +'-'+ right('0'+convert(varchar(10), month(a.docdate)), 2) +'-'+ right('0'+convert(varchar(10), day(a.docdate)), 2) docdateTxt,
		a.vendorid, a.bchsourc, a.poprctnm
  from dbo.vwIecvDetalleComprasAP a
		inner join pm00200 mp
			on mp.vendorid = a.vendorid
        left outer join loch0004 cdt --loch_boleta_trx_details Indica si es electrónica
             on cdt.custvndr = a.VENDORID
            and cdt.lochtrxno = case when a.poprctnm is null then a.VCHRNMBR 
															else a.poprctnm end	--usar el número de repción cuando es recepción/factura
        left outer join loch0002 cde --loch_descripcion_documentos
             on cde.lochdoccod = cdt.lochdoccod
            and cde.module1 = cdt.module1
		left outer join pm30200 hi	--devoluciones pagadas
				on hi.doctype = 4
				and hi.ttlpymts != 0
				and hi.Bchsourc like '%PM_Trxent'
				and hi.DOCTYPE = a.DOCTYPE
				and hi.VCHRNMBR = a.VCHRNMBR
 where a.doctype in (1, 4)			--1: Factura y ND, 4: Nota de Crédito
   and a.voided = 0					--sólo vigentes
   and upper(a.docnumbr) != 'TARJETA DE CRÉDITO'
   and hi.VCHRNMBR is null			--excluye devoluciones pagadas

go

IF (@@Error = 0) PRINT 'Creación exitosa de la vista: vwIecvLibroComprasCrudo ()'
ELSE PRINT 'Error en la creación de la vista: vwIecvLibroComprasCrudo ()'
GO

----------------------------------------------------------------------------------------------------------
--select *
--from vwIecvLibroComprasCrudo
