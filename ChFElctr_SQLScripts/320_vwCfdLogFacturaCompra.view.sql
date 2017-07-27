
---------------------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[vwCfdLogFacturaCompra]') AND OBJECTPROPERTY(id,N'IsView') = 1)
    DROP view dbo.vwCfdLogFacturaCompra;
GO

CREATE view dbo.vwCfdLogFacturaCompra as
--Propósito. Obtiene todas las facturas electrónicas de compra recibidas
--Usado por. App Factura digital (doodads)
--Requisitos. 
--			El registro del estado "publicado" contiene info del estado actual
--			En el caso de tipo = 0 indica que son mails recibidos que no tienen que ver con compras
--07/10/14 jcf Creación fe Chile
--
select f.secuencia,f.tipo,f.folio,f.idImpuestoTercero,f.nombreTercero,f.fechaRecepcion,f.estado, f.estadoActual, f.idxSingleStatus, f.mensajeEA, f.mensaje, 
	f.archivoXML,f.archivoPDF,f.idExterno,f.fechaAlta,f.idUsuario,f.fechaModificacion,f.idUsuarioModificacion
from dbo.cfdLogFacturaCompra f
--	cross join dbo.fCfdEmisor() emi
where f.estado = 'publicado'
and f.tipo > 0

go

IF (@@Error = 0) PRINT 'Creación exitosa de la vista: dbo.vwCfdLogFacturaCompra'
ELSE PRINT 'Error en la creación de la vista: dbo.vwCfdLogFacturaCompra'
GO

-----------------------------------------------------------------------------------------

--test

--select dbo.fCfdLibroVentasResumenPeriodoXML (2014 , 6)
--select dbo.fCfdLibroVentasDetalleXML (2013, 10, '')

--select CONVERT(datetime, replace(convert(varchar(20), GETDATE(), 102), '.', '-')+'T'+'00:00:00' ,126),
--	convert(varchar(12), dateadd(hh,-1,getdate()), 114),
--	rtrim(LEFT(convert(varchar(12), dateadd(hh,-1,getdate()), 114), 8)),
--	replace(convert(varchar(20), GETDATE(), 102), '.', '-')+'T'+rtrim(LEFT(convert(varchar(12), dateadd(hh,-1,getdate()), 114), 8))
	
	