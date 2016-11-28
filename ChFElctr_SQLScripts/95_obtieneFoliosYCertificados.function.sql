--------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdCertificadoVigente') IS NOT NULL
   DROP FUNCTION dbo.fCfdCertificadoVigente
GO

create function dbo.fCfdCertificadoVigente(@fecha datetime, @userid varchar(15))
returns table
as
--Propósito. Verifica que la fecha corresponde a un certificado vigente
--			Si existe más de uno o ninguno, devuelve el estado: inconsistente
--			También devuelve el rut
--Requisitos. Los estados posibles para generar o no archivos xml son: no emitido, inconsistente
--16/01/14 jcf Creación para fe chile
--
return
(  
	select top 1 
			fyc.rut, fyc.ruta_certificado, fyc.ruta_clave, fyc.contrasenia_clave, fyc.fila, 
			case when fyc.fila > 1 then 'inconsistente' else 'no emitido' end estado
	from (
		SELECT top 2 rtrim(B.aca_rut) rut, rtrim(B.ruta_certificado) ruta_certificado, rtrim(B.ruta_clave) ruta_clave, 
				rtrim(B.contrasenia_clave) contrasenia_clave, row_number() over (order by B.aca_rut) fila
		FROM cfd_CER00100 B
		WHERE B.ACTIVE = 1
		and B.userid = @userid
		and datediff(day, B.fecha_vig_desde, @fecha) >= 0
		and datediff(day, B.fecha_vig_hasta, @fecha) <= 0
		) fyc
	order by fyc.fila desc
)
go

IF (@@Error = 0) PRINT 'Creación exitosa de la función: fCfdCertificadoVigente()'
ELSE PRINT 'Error en la creación de la función: fCfdCertificadoVigente()'
GO

--------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdFolioVigente') IS NOT NULL
   DROP FUNCTION dbo.fCfdFolioVigente
GO

create function dbo.fCfdFolioVigente(@soptype smallint, @docid char(15), @folio int)
returns table
as
--Propósito. Verifica que el folio es correcto
--Requisitos. 
--16/01/14 jcf Creación para fe chile
--
return
(  
	select top 1 fyc.num_folio_desde, fyc.num_folio_hasta, ruta_codigo_autorizacion,
			case when fyc.fila > 1 then 'inconsistente' else 'no emitido' end estado
	from (
		select num_folio_desde, num_folio_hasta, ruta_codigo_autorizacion, row_number() over (order by num_folio_desde) fila
		from cfd_FOL00100
		where soptype = @soptype
		and DOCID = @docid
		and @folio between num_folio_desde and num_folio_hasta
		) fyc
	order by fyc.fila desc
)
go

IF (@@Error = 0) PRINT 'Creación exitosa de la función: fCfdFolioVigente()'
ELSE PRINT 'Error en la creación de la función: fCfdFolioVigente()'
GO

--test
--select *
--from dbo.fCfdFolioVigente(3, 'FACTURA ELCTRNC', 3)


--------------------------------------------------------------------------------------------------------
--IF OBJECT_ID ('dbo.fCfdCertificadoPAC') IS NOT NULL
--   DROP FUNCTION dbo.fCfdCertificadoPAC
--GO

--create function dbo.fCfdCertificadoPAC(@fecha datetime)
--returns table
--as
----Propósito. Obtiene el certificado del PAC. 
----			Verifica que la fecha corresponde a un certificado vigente y activo
----Requisitos. El id PAC está reservado para registrar el certificado del PAC. 
----23/5/12 jcf Creación
----
--return
--(  
--	--declare @fecha datetime
--	--select @fecha = '5/4/12'
--	SELECT rtrim(B.ID_Certificado) ID_Certificado, rtrim(B.ruta_certificado) ruta_certificado, rtrim(B.ruta_clave) ruta_clave, 
--			rtrim(B.contrasenia_clave) contrasenia_clave
--	FROM cfd_CER00100 B
--	WHERE B.estado = '1'
--		and B.id_certificado = 'PAC'	--El id PAC está reservado para el PAC
--		and datediff(day, B.fecha_vig_desde, @fecha) >= 0
--		and datediff(day, B.fecha_vig_hasta, @fecha) <= 0
--)
--go

--IF (@@Error = 0) PRINT 'Creación exitosa de la función: fCfdCertificadoPAC()'
--ELSE PRINT 'Error en la creación de la función: fCfdCertificadoPAC()'
--GO

--------------------------------------------------------------------------------------------------------
