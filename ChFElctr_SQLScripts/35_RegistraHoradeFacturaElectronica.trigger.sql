----------------------------------------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('trgins_sop10100_registraHora','TR') IS NOT NULL
   DROP TRIGGER dbo.trgins_sop10100_registraHora
GO

CREATE TRIGGER dbo.trgins_sop10100_registraHora ON dbo.sop10100
AFTER INSERT
AS
--CHILE
--Propósito. Ingresa la hora de la transacción
--24/11/10 JCF Creación. 
--27/08/13 JCF Getty ejecuta una integración en la madrugada desde Argentina. 
--14/01/14 jcf Todas las facturas ingresan con diferencia -1
--
begin try
	UPDATE dbo.SOP10100 set DOCNCORR = 
				--case when datepart(hh, dateadd(hh,-3,getdate())) > 22 then 
				--	convert(varchar(12), getdate(), 114)	--hora Argentina
				--else 
					convert(varchar(12), dateadd(hh,-1,getdate()), 114)
				--end	
	 FROM dbo.SOP10100, inserted 
	 WHERE SOP10100.SOPTYPE = inserted.SOPTYPE 
	 AND SOP10100.SOPNUMBE = inserted.SOPNUMBE 

end try
BEGIN catch
	declare @l_error nvarchar(2048)
	select @l_error = 'Error al ingresar la hora de la factura. [trgins_sop10100_registraHora] ' + error_message()
	RAISERROR (@l_error , 16, 1)
end catch
go
----------------------------------------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('trgins_sop30200_registraHora','TR') IS NOT NULL
   DROP TRIGGER dbo.trgins_sop30200_registraHora
GO

CREATE TRIGGER dbo.trgins_sop30200_registraHora ON dbo.sop30200
AFTER INSERT
AS
--CHILE
--Propósito. Ingresa la hora de la transacción
--10/11/14 JCF Creación. 
--
begin try
	UPDATE dbo.sop30200 set DOCNCORR = convert(varchar(12), dateadd(hh,-1,getdate()), 114)
	 FROM dbo.sop30200, inserted 
	 WHERE sop30200.SOPTYPE = inserted.SOPTYPE 
	 AND sop30200.SOPNUMBE = inserted.SOPNUMBE 

end try
BEGIN catch
	declare @l_error nvarchar(2048)
	select @l_error = 'Error al ingresar la hora de la factura. [trgins_sop30200_registraHora] ' + error_message()
	RAISERROR (@l_error , 16, 1)
end catch
go

-------------------------------------------------------------------------------------------------
--IF OBJECT_ID ('trgupd_sop10100_registraHora','TR') IS NOT NULL
--   DROP TRIGGER dbo.trgupd_sop10100_registraHora
--GO

--CREATE TRIGGER dbo.trgupd_sop10100_registraHora ON dbo.sop10100
--AFTER UPDATE
--AS
----CHILE
----Propósito. Revisa la hora de la transacción en los documentos de venta habilitados en vwCfdIdDocumentos
----Requisito. Debe existir la suspensión: SIN HORA
----Utiliza. vwCfdIdDocumentos
----25/11/10 JCF Creación. 
----01/12/10 jcf Agrega control por id de documento. Sólo procesa documentos listados en la vista vwCfdIdDocumentos
----10/11/14 JCF deprecated. La hora se guarda al contabilizar la factura en sop30200
----
--begin TRY
--	declare @existeDocncorr varchar(1)
	
--	select @existeDocncorr = top 1 SUBSTRING(i.DOCNCORR, 3, 1)
--		from vwCfdIdDocumentos id
--		inner join inserted i 
--			on i.soptype = id.soptype
--			and i.docid = id.docid

--	if (isnull(@existeDocncorr, ':')) <> ':'
--		begin	--agregar suspensión
--			if (select count(sop10104.soptype)
--				FROM dbo.sop10104
--					inner join inserted 
--					on sop10104.SOPTYPE = inserted.SOPTYPE 
--					AND sop10104.SOPNUMBE = inserted.SOPNUMBE
--					and sop10104.prchldid = 'SIN HORA' ) > 0
	
--				UPDATE dbo.sop10104 set DELETE1 = 0
--				 FROM dbo.sop10104, inserted 
--				 WHERE sop10104.SOPTYPE = inserted.SOPTYPE 
--				 AND sop10104.SOPNUMBE = inserted.SOPNUMBE
--				 and sop10104.prchldid = 'SIN HORA' 
--			else
--				insert into sop10104 (soptype, sopnumbe, prchldid, delete1 )
--				select soptype, sopnumbe, 'SIN HORA', 0
--				from inserted
--		end
--end TRY
--BEGIN catch
--	RAISERROR ('Error al revisar la hora de la factura. [trgupd_sop10100_registraHora]', 16, 1)
--end catch
--go

-------------------------------------------------------------------------------------------------
--Crea una suspensión
--if not exists(select * from SOP00100 where PRCHLDID = 'SIN HORA') 
--	insert into SOP00100 (PRCHLDID, DSCRIPTN,[PASSWORD],XFERPHOL,POSTPHOL,FUFIPHOL,PRINPHOL,WORKFLOWHOLD,USER2ENT,CREATDDT,MODIFDT)
--	values('SIN HORA', 'Fac. electrónica requiere hora', 'sin hora', 1, 1, 0, 0, 0, '', 0, 0)
--go
-------------------------------------------------------------------------------------------------
--delete from sop00100
--where prchldid = 'SIN HORA'

