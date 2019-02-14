--Factura electr�nica 
--Prop�sito. Crea vista de los id de documentos que se incluyen en factura electr�nica.
--

--use [compa��a];
--go
-----------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[vwCfdIdDocumentos]') AND OBJECTPROPERTY(id,N'IsView') = 1)
    DROP view dbo.[vwCfdIdDocumentos];
GO
create view dbo.vwCfdIdDocumentos as
--Prop�sito. Ids. de documento configurados para emitir factura electr�nica. No usar para incluir en el xml, para eso est� loch0004.
--Utilizado por. Factura electr�nica
--24/01/14 jcf Creaci�n
--10/11/14 jcf Filtra s�lo los ids que tienen ingresado el tipo documento en commntid
--05/12/18 jcf Agrega inet8
--
select ds.soptype, ds.docid, ds.SOPNUMBE, ds.commntid, 
	case when isnull(ds.commntid, '') = '34' then 'FACTURA NO AFECTA O EXENTA ELECTR�NICA'
	else isnull(upper(rtrim(lo.dscriptn)), '') 
	end nombreTipoDoc, emi.inet8 carpetaFolio
from sop40200 ds			--sop_id_setp
left join loch0002 lo		--tipos de documento en loc chile
	on lo.lochdoccod = ds.commntid
	and lo.module1 = 2		--2:ventas
outer apply dbo.fCfdEmisor() emi
where soptype in (3, 4)
and ds.commntid <> ''

go

IF (@@Error = 0) PRINT 'Creaci�n exitosa de la vista: vwCfdIdDocumentos'
ELSE PRINT 'Error en la creaci�n de la vista: vwCfdIdDocumentos'
GO

-----------------------------------------------------------------
--test
--select * from vwCfdIdDocumentos;

--sp_columns loch0002