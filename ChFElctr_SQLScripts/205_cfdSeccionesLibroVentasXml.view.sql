IF OBJECT_ID ('dbo.fCfdLibroVentasResumenPeriodoXML') IS NOT NULL
   DROP FUNCTION dbo.fCfdLibroVentasResumenPeriodoXML
GO

create function dbo.fCfdLibroVentasResumenPeriodoXML (@year int, @month smallint)
returns xml 
as
--Propósito. Resumen de ventas de un periodo
--Requisitos. -
--17/07/14 jcf Creación fe Chile
--25/08/14 jcf Usa funciones para obtener exento y neto
--22/12/14 jcf Montos deben ser cero cuando la factura está anulada
--			 Agrega temporalmente el cálculo manual de: ('33-00000062', '33-00000071', '33-00000077', '33-00000079', '33-00000082', '61-0000014', '61-0000020', '61-0000022', '61-0000025') 
--19/10/16 jcf Ajusta parámetros para obtener neto y exento
--31/03/17 JCF Excluye facturas marcadas: cstponbr = 'EXCLUIRDELV'. totMntNeto = 0 si es nulo
--
begin
	declare @lcv xml;
	WITH XMLNAMESPACES
	(DEFAULT 'http://www.sii.cl/SiiDte'	)
	select @lcv = 
	(
		select tv.docType							'TotalesPeriodo/TpoDoc',
			count(tv.sopnumbe)						'TotalesPeriodo/TotDoc',
			CASE WHEN SUM(tv.voidstts) > 0 THEN 
				SUM(tv.voidstts)
				ELSE null
				END									'TotalesPeriodo/TotAnulado',
			sum(case when tv.subtotal <> 0 and tv.voidstts = 0 then 
					dbo.fCfdVentasObtieneExento(tv.subtotal, tv.descuento, ex.importe, tv.docType)
				else 0
				end)								'TotalesPeriodo/TotMntExe',
			SUM(case when tv.subtotal <> 0 and tv.voidstts = 0 then 
					isnull(dbo.fCfdVentasObtieneNeto(tv.subtotal, tv.descuento, ex.importe, tv.docType), 0)
				else 0
				end)								'TotalesPeriodo/TotMntNeto',
			sum(case when tv.voidstts = 0 then
					case when tv.sopnumbe in ('33-00000062', '33-00000071', '33-00000077', '33-00000079', '33-00000082', '61-0000014', '61-0000020', '61-0000022', '61-0000025') then 
						cast(round(dbo.fCfdVentasObtieneNeto(tv.subtotal, tv.descuento, ex.importe, tv.docType) * .19, 0) as numeric(18))
					else
						cast(tv.impuesto as numeric(18))
					end 
				else 0
				end
					)								'TotalesPeriodo/TotMntIVA',
			--sum(
			--	case when tv.sopnumbe in ('33-00000062', '33-00000071', '33-00000077', '33-00000079', '33-00000082', '61-0000014', '61-0000020', '61-0000022', '61-0000025') then 
			--		cast(round(dbo.fCfdVentasObtieneNeto(tv.subtotal, tv.descuento, ex.importe) * 1.19, 0) as numeric(18))
			--	else
			--		cast(tv.total as numeric(18))
			--	end
			--	)		'TotalesPeriodo/TotMntTotal'
			sum(case when tv.voidstts = 0 then 
					cast(tv.total as numeric(18)) 
					else 0 
				end)		'TotalesPeriodo/TotMntTotal'
		from vwSopTransaccionesVenta tv
			outer apply dbo.fCfdConceptosSumaExentos(tv.soptype, tv.sopnumbe) ex
		where year(tv.docdate) = @year
		and MONTH(tv.docdate) = @month
		and isnull(tv.cstponbr, '') != 'EXCLUIRDELV'
		
		--and tv.sopnumbe in ('33-0000238', '33-0000239', '33-0000240', '33-0000241', '61-0000150', '61-0000151', '61-0000152', '56-0000150')
		
		group by tv.docType
		FOR XML path(''), type
	)
	return @lcv;
end
go

IF (@@Error = 0) PRINT 'Creación exitosa de la función: fCfdLibroVentasResumenPeriodoXML ()'
ELSE PRINT 'Error en la creación de la función: fCfdLibroVentasResumenPeriodoXML ()'
GO
---------------------------------------------------------------------------------------------

IF OBJECT_ID ('dbo.fCfdLibroVentasDetalleXML') IS NOT NULL
   DROP FUNCTION dbo.fCfdLibroVentasDetalleXML
GO

create function dbo.fCfdLibroVentasDetalleXML (@year int, @month smallint, @idImpuestos varchar(200))
returns xml 
as
--Propósito. Detalle de ventas en un periodo
--Requisitos. 
--17/07/14 jcf Creación fe Chile
--25/08/14 jcf Usa funciones para obtener exento y neto
--22/12/14 jcf Sólo debe mostrar tipo doc, número y anulado cuando la factura está anulada
--			Agrega temporalmente el cálculo manual de: ('33-00000062', '33-00000071', '33-00000077', '33-00000079', '33-00000082', '61-0000014', '61-0000020', '61-0000022', '61-0000025') 
--02/04/15 jcf Obtiene ruc y razón social del xml
--19/10/16 jcf Ajusta parámetros para obtener neto y exento
--31/03/17 JCF Excluye facturas marcadas: cstponbr = 'EXCLUIRDELV'
--
begin
	declare @lcv xml;
	WITH XMLNAMESPACES
	(DEFAULT 'http://www.sii.cl/SiiDte'	)
	select @lcv = 
	(
		select tv.docType					'TpoDoc',
			tv.folio						'NroDoc',
			case when tv.voidstts = 1 then 'A' else null end							'Anulado',
			case when tv.voidstts = 1 then null else cast(im.tasa as numeric(7,2))	end 'TasaImp',
			case when tv.voidstts = 1 then null else tv.fecha end						'FchDoc',
			case when tv.voidstts = 1 then null else dx.RUTRecep end					'RUTDoc',
			case when tv.voidstts = 1 then null else left(dx.RznSocRecep, 50) end		'RznSoc',
			case when tv.voidstts = 1 then 
				null 
			else 
				case when tv.subtotal <> 0 then 
					dbo.fCfdVentasObtieneExento(tv.subtotal, tv.descuento, ex.importe, tv.docType)
				else 0
				end	
			end																			'MntExe',
			case when tv.voidstts = 1 then 
				null 
			else 
				case when tv.subtotal <> 0 then 
					dbo.fCfdVentasObtieneNeto(tv.subtotal, tv.descuento, ex.importe, tv.docType)
				else 0
				end									
			end																			'MntNeto',
			case when tv.voidstts = 1 then null else 
				case when tv.sopnumbe in ('33-00000062', '33-00000071', '33-00000077', '33-00000079', '33-00000082', '61-0000014', '61-0000020', '61-0000022', '61-0000025') then 
					cast(round(dbo.fCfdVentasObtieneNeto(tv.subtotal, tv.descuento, ex.importe, tv.docType) * .19, 0) as numeric(18))
				else
					cast(tv.impuesto as numeric(18))
				end 
			end																			'MntIVA',
			--case when tv.sopnumbe in ('33-00000062', '33-00000071', '33-00000077', '33-00000079', '33-00000082', '61-0000014', '61-0000020', '61-0000022', '61-0000025') then 
			--	cast(round(dbo.fCfdVentasObtieneNeto(tv.subtotal, tv.descuento, ex.importe) * 1.19, 0) as numeric(18))
			--else
			--	cast(tv.total as numeric(18))		
			--end 'MntTotal'
			case when tv.voidstts = 1 then null else cast(tv.total as numeric(18)) end	'MntTotal'
		from vwSopTransaccionesVenta tv 
			left join cfdlogfacturaxml lf
				on lf.soptype = tv.SOPTYPE
				and lf.sopnumbe = tv.sopnumbe
				and lf.estado = 'emitido'
			outer apply dbo.fCfdConceptosSumaExentos(tv.soptype, tv.sopnumbe) ex
			outer apply dbo.fCfdImpuestos(tv.soptype, tv.sopnumbe, @idImpuestos) im
			outer apply dbo.fCfdDatosXmlParaImpresion(lf.archivoXML) dx
		where year(tv.docdate) = @year
		and MONTH(tv.docdate) = @month
		and isnull(tv.cstponbr, '') != 'EXCLUIRDELV'

		--and tv.sopnumbe in ('33-0000238', '33-0000239', '33-0000240', '33-0000241', '61-0000150', '61-0000151', '61-0000152', '56-0000150')

		FOR XML path('Detalle'), type
	)
	return @lcv;
end
go

IF (@@Error = 0) PRINT 'Creación exitosa de la función: fCfdLibroVentasDetalleXML ()'
ELSE PRINT 'Error en la creación de la función: fCfdLibroVentasDetalleXML ()'
GO
---------------------------------------------------------------------------------------------

IF OBJECT_ID ('dbo.fCfdLibroVentasXML') IS NOT NULL
   DROP FUNCTION dbo.fCfdLibroVentasXML
GO

create function dbo.fCfdLibroVentasXML (@tipo varchar(4), @year int, @month smallint)
returns xml 
as
--Propósito. Elabora un comprobante xml libro de ventas electrónico de Chile
--Requisitos. 
--17/07/14 jcf Creación fe Chile
--   atención! El timestamp es una hora menos siempre
begin
	declare @lcv xml;
	WITH XMLNAMESPACES
	(DEFAULT 'http://www.sii.cl/SiiDte',
			'http://www.w3.org/2001/XMLSchema-instance' as "xsi"
	)
	select @lcv = 
	(
		select 'http://www.sii.cl/SiiDte LibroCV_v10.xsd'	'@xsi:schemaLocation',
			emi.[version]				'@version',
			@tipo+convert(varchar(6), @year*100+@month)					'EnvioLibro/@ID',
			STUFF(emi.idImpuesto, len(emi.idImpuesto), 0, '-')			'EnvioLibro/Caratula/RutEmisorLibro',
			''							'EnvioLibro/Caratula/RutEnvia',					--se carga en la aplicación
			convert(varchar(4), @year) +'-'+  right('0'+convert(varchar(2), @month), 2) 'EnvioLibro/Caratula/PeriodoTributario',
			emi.fchResol				'EnvioLibro/Caratula/FchResol',
			emi.nroResol				'EnvioLibro/Caratula/NroResol',
			'VENTA'						'EnvioLibro/Caratula/TipoOperacion',
			emi.tipoLibro				'EnvioLibro/Caratula/TipoLibro',
			'TOTAL'						'EnvioLibro/Caratula/TipoEnvio',
			case when emi.tipoLibro = 'ESPECIAL' then 1 else null end	'EnvioLibro/Caratula/FolioNotificacion',
			dbo.fCfdLibroVentasResumenPeriodoXML (@year, @month)		'EnvioLibro/ResumenPeriodo',
			dbo.fCfdLibroVentasDetalleXML(@year, @month, emi.impuestos)	'EnvioLibro',
			replace(convert(varchar(20), GETDATE(), 102), '.', '-')+'T'+rtrim(LEFT(convert(varchar(12), dateadd(hh,-1,getdate()), 114), 8)) 'EnvioLibro/TmstFirma'
			--'2014-08-20T18:00:00' 'EnvioLibro/TmstFirma'
		from dbo.fCfdEmisor() emi
		FOR XML path('LibroCompraVenta'), type
	)
	return @lcv;
end
go

IF (@@Error = 0) PRINT 'Creación exitosa de la función: fCfdLibroVentasXML ()'
ELSE PRINT 'Error en la creación de la función: fCfdLibroVentasXML ()'
GO
---------------------------------------------------------------------------------------------

--test
--select *
--from vwCfdLibroCVLog
--select dbo.fCfdLibroVentasXML('x', 2015, 6)
--select dbo.fCfdLibroVentasResumenPeriodoXML (2014 , 6)
--select dbo.fCfdLibroVentasDetalleXML (2013, 10, '')

--select CONVERT(datetime, replace(convert(varchar(20), GETDATE(), 102), '.', '-')+'T'+'00:00:00' ,126),
--	convert(varchar(12), dateadd(hh,-1,getdate()), 114),
--	rtrim(LEFT(convert(varchar(12), dateadd(hh,-1,getdate()), 114), 8)),
--	replace(convert(varchar(20), GETDATE(), 102), '.', '-')+'T'+rtrim(LEFT(convert(varchar(12), dateadd(hh,-1,getdate()), 114), 8))
	