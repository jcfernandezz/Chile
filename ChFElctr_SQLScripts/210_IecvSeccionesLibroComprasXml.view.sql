IF OBJECT_ID ('dbo.fCfdLibroComprasDetalleXML') IS NOT NULL
   DROP FUNCTION dbo.fCfdLibroComprasDetalleXML
GO

create function dbo.fCfdLibroComprasDetalleXML (@year int, @month smallint, @idImpuestos varchar(200))
returns xml 
as
--Propósito. Detalle de compras
--Requisitos. No incluye anulados. Indica Emisor siempre que sea Devolución - nota de crédito
--21/07/14 jcf Creación fe Chile
--17/3/15 jcf Cambia exento por exentoCalculado
--
begin
	declare @lcv xml;
	WITH XMLNAMESPACES
	(DEFAULT 'http://www.sii.cl/SiiDte'	)
	select @lcv = 
	(
		select tv.lochdoccod				'TpoDoc',
			case when tv.doctype = 4				--devolución
				then '1' else null end		'Emisor',
			tv.folio						'NroDoc',
		--	tv.voidstts						'Anulado',
			abs(tv.tasaIVA)					'TasaImp',
			replace(convert(varchar(20), tv.docdate, 102), '.', '-')	'FchDoc',
			tv.rutClieProvee				'RUTDoc',
			left(tv.RazonSocial, 50)		'RznSoc',
			tv.exentoCalculado				'MntExe',
			tv.neto							'MntNeto',
			tv.iva							'MntIVA',
			case when tv.inr_taxamnt = 0 then
				null
			else tv.codIvaNoRec
			end								'IVANoRec/CodIVANoRec',
			case when tv.inr_taxamnt = 0 then
				null
			else tv.inr_taxamnt				
			end								'IVANoRec/MntIVANoRec',
			tv.iuc_taxamnt					'IVAUsoComun',

			case when tv.ri_taxamnt <> 0 then 
				tv.otrosImp						
			else null
			end								'OtrosImp/CodImp',
			case when tv.ri_taxamnt <> 0 then 
				abs(tv.tasaReteIva)				
			else null
			end								'OtrosImp/TasaImp',
			case when tv.ri_taxamnt <> 0 then 
				abs(tv.ri_taxamnt)				
			else null
			end								'OtrosImp/MntImp',
			cast(tv.total as numeric(18))	'MntTotal'
		from dbo.vwIecvLibroComprasCrudo tv 
		where year(tv.pstgdate) = @year
		and MONTH(tv.pstgdate) = @month
		FOR XML path('Detalle'), type
	)
	return @lcv;
end
go

IF (@@Error = 0) PRINT 'Creación exitosa de la función: fCfdLibroComprasDetalleXML ()'
ELSE PRINT 'Error en la creación de la función: fCfdLibroComprasDetalleXML ()'
GO

------------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdLibroComprasTotOtrosImpXML') IS NOT NULL
   DROP FUNCTION dbo.fCfdLibroComprasTotOtrosImpXML
GO

create function dbo.fCfdLibroComprasTotOtrosImpXML (@lochdoccod varchar(3), @year int, @month smallint)
returns xml 
as
--Propósito. Resumen de otros impuestos de un periodo
--Requisitos. -
--26/08/14 jcf Creación fe Chile
--
begin
	declare @lcv xml;
	WITH XMLNAMESPACES
	(DEFAULT 'http://www.sii.cl/SiiDte'	)
	select @lcv = 
	(
		select 	tv.otrosImp						'CodImp',
			abs(SUM(tv.ri_taxamnt))				'TotMntImp'
		from vwIecvLibroComprasCrudo tv
		where tv.lochdoccod = @lochdoccod
		and year(tv.pstgdate) = @year
		and MONTH(tv.pstgdate) = @month
		group by tv.otrosImp
		having SUM(tv.ri_taxamnt) <> 0
		FOR XML path('TotOtrosImp'), type
	)
	return @lcv;
end
go

IF (@@Error = 0) PRINT 'Creación exitosa de la función: fCfdLibroComprasTotOtrosImpXML ()'
ELSE PRINT 'Error en la creación de la función: fCfdLibroComprasTotOtrosImpXML ()'
GO

------------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdLibroComprasTotIvaNoRecuperableXML') IS NOT NULL
   DROP FUNCTION dbo.fCfdLibroComprasTotIvaNoRecuperableXML
GO

create function dbo.fCfdLibroComprasTotIvaNoRecuperableXML (@lochdoccod varchar(3), @year int, @month smallint)
returns xml 
as
--Propósito. Resumen de iva no recuperable de un periodo
--Requisitos. -
--26/08/14 jcf Creación fe Chile
--
begin
	declare @lcv xml;
	WITH XMLNAMESPACES
	(DEFAULT 'http://www.sii.cl/SiiDte'	)
	select @lcv = 
	(
		select tv.codIvaNoRec						'CodIVANoRec',
			COUNT(case when tv.inr_taxamnt = 0 then null else 1 end)					'TotOpIVANoRec',
			sum(tv.inr_taxamnt)						'TotMntIVANoRec'
		from vwIecvLibroComprasCrudo tv
		where tv.lochdoccod = @lochdoccod
		and year(tv.pstgdate) = @year
		and MONTH(tv.pstgdate) = @month
		group by tv.codIvaNoRec
		having sum(tv.inr_taxamnt) <> 0
		FOR XML path('TotIVANoRec'), type
	)
	return @lcv;
end
go

IF (@@Error = 0) PRINT 'Creación exitosa de la función: fCfdLibroComprasTotIvaNoRecuperableXML ()'
ELSE PRINT 'Error en la creación de la función: fCfdLibroComprasTotIvaNoRecuperableXML ()'
GO

------------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdLibroComprasResumenPeriodoXML') IS NOT NULL
   DROP FUNCTION dbo.fCfdLibroComprasResumenPeriodoXML
GO

create function dbo.fCfdLibroComprasResumenPeriodoXML (@year int, @month smallint)
returns xml 
as
--Propósito. Resumen de compras de un periodo
--Requisitos. -
--22/07/14 jcf Creación fe Chile
--17/3/15 jcf Cambia exento por exentoCalculado
--
begin
	declare @lcv xml;
	WITH XMLNAMESPACES
	(DEFAULT 'http://www.sii.cl/SiiDte'	)
	select @lcv = 
	(
		select tv.lochdoccod						'TotalesPeriodo/TpoDoc',
			count(tv.vchrnmbr)						'TotalesPeriodo/TotDoc',
			sum(tv.exentoCalculado)					'TotalesPeriodo/TotMntExe',
			SUM(tv.neto)							'TotalesPeriodo/TotMntNeto',
			SUM(tv.iva)								'TotalesPeriodo/TotMntIVA',

			dbo.fCfdLibroComprasTotIvaNoRecuperableXML (tv.lochdoccod, @year, @month) 'TotalesPeriodo',

			case when SUM(tv.iuc_taxamnt) <> 0 then
				SUM(tv.iuc_taxamnt)
			else null
			end										'TotalesPeriodo/TotIVAUsoComun',
			case when SUM(tv.iuc_taxamnt) <> 0 then
				0.60
			else null
			end										'TotalesPeriodo/FctProp',
			case when SUM(tv.iuc_taxamnt)=0 then 
				null 
			else cast(round(0.60 * SUM(tv.iuc_taxamnt), 0) as numeric(18))
			end										'TotalesPeriodo/TotCredIVAUsoComun',

			dbo.fCfdLibroComprasTotOtrosImpXML (tv.lochdoccod, @year, @month) 'TotalesPeriodo',

			sum(cast(tv.total as numeric(18)))		'TotalesPeriodo/TotMntTotal'
			
		from vwIecvLibroComprasCrudo tv
		where year(tv.pstgdate) = @year
		and MONTH(tv.pstgdate) = @month
		group by tv.lochdoccod
		FOR XML path(''), type
	)
	return @lcv;
end
go

IF (@@Error = 0) PRINT 'Creación exitosa de la función: fCfdLibroComprasResumenPeriodoXML ()'
ELSE PRINT 'Error en la creación de la función: fCfdLibroComprasResumenPeriodoXML ()'
GO
---------------------------------------------------------------------------------------------

IF OBJECT_ID ('dbo.fCfdLibroComprasXML') IS NOT NULL
   DROP FUNCTION dbo.fCfdLibroComprasXML
GO

create function dbo.fCfdLibroComprasXML (@tipo varchar(4), @year int, @month smallint)
returns xml 
as
--Propósito. Elabora un comprobante xml libro de compras electrónico de Chile
--Requisitos. 
--21/07/14 jcf Creación fe Chile
--   atención! El timestamp es una hora menos siempre
begin
	declare @lcv xml;
	WITH XMLNAMESPACES
	(DEFAULT 'http://www.sii.cl/SiiDte',
			'http://www.w3.org/2001/XMLSchema-instance' as "xsi"
	)
	select @lcv = 
	(
		select 'http://www.sii.cl/SiiDte LibroCVS_v10.xsd'	'@xsi:schemaLocation',
			emi.[version]				'@version',
			@tipo+convert(varchar(6), @year*100+@month)					'EnvioLibro/@ID',
			STUFF(emi.idImpuesto, len(emi.idImpuesto), 0, '-')			'EnvioLibro/Caratula/RutEmisorLibro',
			''							'EnvioLibro/Caratula/RutEnvia',					--se carga en la aplicación
			convert(varchar(4), @year) +'-'+  right('0'+convert(varchar(2), @month), 2) 'EnvioLibro/Caratula/PeriodoTributario',
			emi.fchResol				'EnvioLibro/Caratula/FchResol',
			emi.nroResol				'EnvioLibro/Caratula/NroResol',
			'COMPRA'					'EnvioLibro/Caratula/TipoOperacion',
			emi.tipoLibro				'EnvioLibro/Caratula/TipoLibro',
			'TOTAL'						'EnvioLibro/Caratula/TipoEnvio',
			case when emi.tipoLibro = 'ESPECIAL' then 2 else null end	'EnvioLibro/Caratula/FolioNotificacion',
			dbo.fCfdLibroComprasResumenPeriodoXML (@year, @month)		'EnvioLibro/ResumenPeriodo',
			dbo.fCfdLibroComprasDetalleXML(@year, @month, emi.impuestos) 'EnvioLibro',
			replace(convert(varchar(20), GETDATE(), 102), '.', '-')+'T'+rtrim(LEFT(convert(varchar(12), dateadd(hh,-1,getdate()), 114), 8)) 'EnvioLibro/TmstFirma'
		from dbo.fCfdEmisor() emi
		FOR XML path('LibroCompraVenta'), type
	)
	return @lcv;
end
go

IF (@@Error = 0) PRINT 'Creación exitosa de la función: fCfdLibroComprasXML ()'
ELSE PRINT 'Error en la creación de la función: fCfdLibroComprasXML ()'
GO


----------------------------------------------------------------------------------

--select dbo.fCfdLibroComprasXML ('', 2014, 11)
