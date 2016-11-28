
---------------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[vwCfdLibroCVLog]') AND OBJECTPROPERTY(id,N'IsView') = 1)
    DROP view dbo.[vwCfdLibroCVLog];
GO

CREATE view dbo.vwCfdLibroCVLog as
--Propósito. Lista todos los periodos indicando el estado de los libros de compras y Ventas. 
--Usado por. App Factura digital (doodads)
--Requisitos. El estado no emitido indica que no fue ingresado en la tabla de log
--			El registro del estado "emitido" contiene info del estado actual
--			
--17/07/14 jcf Creación fe Chile
--
select pr.YEAR1, pr.PERIODID, pr.YEAR1 * 100 + pr.periodid periodo,
	isnull(li.tipo, 'LV') tipo, 
	isnull(li.estado, 'no emitido') estado,
	isnull(li.mensajeGral, 'no emitido') mensajeGral,	
	isnull(li.estadoActualBin, '00000000000100') estadoActualBin, 
	isnull(li.idxSingleStatus, 11) idxSingleStatus,							--índice de 0 a 11
	isnull(li.mensajeEActual, '') mensajeEActual,
	case when isnull(li.idxSingleStatus, 11) = 8							--aceptado SII
		then isnull(li.archivoXML, cast('' as xml))							--mostrar el archivo
		else dbo.fCfdLibroVentasXML ('LV', pr.YEAR1, pr.PERIODID)
	end comprobanteXml,
	emi.rutaXml
from SY40100 pr					--sy_period_setp
	left join cfdLogLibroCV li
		on li.periodo = pr.year1*100+pr.periodid
		and li.estado = 'emitido'
		and li.tipo = 'LV'
	cross join dbo.fCfdEmisor() emi
where pr.FORIGIN = 1
and pr.YEAR1 * 100 + pr.PERIODID >= 201407
and pr.SERIES = 0
and pr.ODESCTN = ''
and pr.PERIODID > 0

UNION ALL

select pr.YEAR1, pr.PERIODID, pr.YEAR1 * 100 + pr.periodid periodo,
	isnull(li.tipo, 'LC') tipo, 
	isnull(li.estado, 'no emitido') estado,
	isnull(li.mensajeGral, 'no emitido') mensajeGral,	
	isnull(li.estadoActualBin, '00000000000100') estadoActualBin, 
	isnull(li.idxSingleStatus, 11) idxSingleStatus,							--índice de 0 a 11
	isnull(li.mensajeEActual, '') mensajeEActual,
	case when isnull(li.idxSingleStatus, 11) = 8							--aceptado SII
		then isnull(li.archivoXML, cast('' as xml))							--mostrar el archivo
		else dbo.fCfdLibroComprasXML ('LC', pr.YEAR1, pr.PERIODID)
	end comprobanteXml,
	emi.rutaXml
from SY40100 pr					--sy_period_setp
	left join cfdLogLibroCV li
		on li.periodo = pr.year1*100+pr.periodid
		and li.estado = 'emitido'
		and li.tipo = 'LC'
	cross join dbo.fCfdEmisor() emi
where pr.FORIGIN = 1
and pr.YEAR1 * 100 + pr.PERIODID >= 201407
and pr.SERIES = 0
and pr.ODESCTN = ''
and pr.PERIODID > 0

go

IF (@@Error = 0) PRINT 'Creación exitosa de la vista: vwCfdLibroCVLog'
ELSE PRINT 'Error en la creación de la vista: vwCfdLibroCVLog'
GO

-----------------------------------------------------------------------------------------

--test
--select *
--from vwCfdLibroCVLog
--where year1 = 2015
--select dbo.fCfdLibroVentasXML(2013, 9)
--select dbo.fCfdLibroVentasResumenPeriodoXML (2014 , 6)
--select dbo.fCfdLibroVentasDetalleXML (2013, 10, '')

--select CONVERT(datetime, replace(convert(varchar(20), GETDATE(), 102), '.', '-')+'T'+'00:00:00' ,126),
--	convert(varchar(12), dateadd(hh,-1,getdate()), 114),
--	rtrim(LEFT(convert(varchar(12), dateadd(hh,-1,getdate()), 114), 8)),
--	replace(convert(varchar(20), GETDATE(), 102), '.', '-')+'T'+rtrim(LEFT(convert(varchar(12), dateadd(hh,-1,getdate()), 114), 8))
	