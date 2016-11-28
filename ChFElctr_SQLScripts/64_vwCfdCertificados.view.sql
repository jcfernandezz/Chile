-----------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[vwCfdCertificados]') AND OBJECTPROPERTY(id,N'IsView') = 1)
    DROP view dbo.[vwCfdCertificados];
GO

create view dbo.vwCfdCertificados as
--Propósito. Lista todos los usuarios con certificados y privilegios
--Requisitos. El rut se guarda en la tabla sin puntos ni guiones
--Usado por. App Factura digital (doodads)
--30/01/14 jcf Creación fe Chile
--
SELECT B.USERID, STUFF(rtrim(B.ACA_RUT), len(rtrim(B.ACA_RUT)), 0, '-') ACA_RUT, B.fecha_vig_desde, B.fecha_vig_hasta, B.ruta_certificado, B.ruta_clave, B.contrasenia_clave,
	B.ACA_SolicitaFolio, B.ACA_AnulaDocumentos, B.ACA_EnviaDocumentos, B.ACA_FirmaDocumentos, B.ACTIVE,
	emi.nroResol, emi.fchResol
FROM cfd_CER00100 B
	cross join dbo.fCfdEmisor() emi
WHERE B.ACTIVE = 1
		
go

IF (@@Error = 0) PRINT 'Creación exitosa de la vista: vwCfdCertificados'
ELSE PRINT 'Error en la creación de la vista: vwCfdCertificados'
GO

--sp_columns cfd_CER00100
--select *
--from vwCfdCertificados 
--set ACA_RUT = '109694770' where USERID = 'sa'

--delete from cfd_cer00100 

--INSERT INTO cfd_cer00100 (userid, aca_rut, fecha_vig_desde, fecha_vig_hasta, ruta_certificado, ruta_clave, contrasenia_clave, 
--					aca_solicitaFolio, aca_anulaDocumentos, aca_enviaDocumentos, aca_firmaDocumentos, active)
--			values('sa', '87115925', '1-1-2014', '12-31-2016', '\\gilabasrdb03\GettyCh_FacturaElectronicaCertificados\mrebolledo\CERTIFICADO ELECTRONICO MARIANA REBOLLEDO.p12', '-', 'GettyCert2014',
--			1, 1, 1, 1, 1)

