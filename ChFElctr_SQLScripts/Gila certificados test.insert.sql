--Inserta datos del usuario y su certificado personal
--

use gchi
go
--atención: los ids de usuario deben estar guardados con minúsculas incluyendo el dominio. Ejemplo: gila\priscilla.parra
insert into cfd_CER00100 
( USERID,ACA_RUT,fecha_vig_desde,fecha_vig_hasta,ruta_certificado,ruta_clave,
	contrasenia_clave,ACA_SolicitaFolio,ACA_AnulaDocumentos,ACA_EnviaDocumentos,ACA_FirmaDocumentos,ACTIVE)
values('sa', '87115925', '1/1/14', '12/31/14', 
		'C:\GPUsuario\GPExpressCfdi\feGilaChiTST\certificado\CERTIFICADO ELECTRONICO MARIANA REBOLLEDO.p12',
		'C:\GPUsuario\GPExpressCfdi\feGilaChiTST\certificado\CERTIFICADO ELECTRONICO MARIANA REBOLLEDO.p12', 
		'GettyCert2014', 1, 1, 1, 1, 1)
GO
insert into cfd_CER00100 
( USERID,ACA_RUT,fecha_vig_desde,fecha_vig_hasta,ruta_certificado,ruta_clave,
	contrasenia_clave,ACA_SolicitaFolio,ACA_AnulaDocumentos,ACA_EnviaDocumentos,ACA_FirmaDocumentos,ACTIVE)
values('gila\priscilla.parra', '87115925', '1/1/14', '12/31/14', 
		'\\gilabasrdb03\GettyCh_FacturaElectronicaCertificados\mrebolledo\CERTIFICADO ELECTRONICO MARIANA REBOLLEDO.p12',
		'\\gilabasrdb03\GettyCh_FacturaElectronicaCertificados\mrebolledo\CERTIFICADO ELECTRONICO MARIANA REBOLLEDO.p12',
		'GettyCert2014', 1, 1, 1, 1, 1)
GO
insert into cfd_CER00100 
( USERID,ACA_RUT,fecha_vig_desde,fecha_vig_hasta,ruta_certificado,ruta_clave,
	contrasenia_clave,ACA_SolicitaFolio,ACA_AnulaDocumentos,ACA_EnviaDocumentos,ACA_FirmaDocumentos,ACTIVE)
values('gila\andrea.gomez', '87115925', '1/1/14', '12/31/14', 
		'\\gilabasrdb03\GettyCh_FacturaElectronicaCertificados\mrebolledo\CERTIFICADO ELECTRONICO MARIANA REBOLLEDO.p12',
		'\\gilabasrdb03\GettyCh_FacturaElectronicaCertificados\mrebolledo\CERTIFICADO ELECTRONICO MARIANA REBOLLEDO.p12',
		'GettyCert2014', 1, 1, 1, 1, 1)
GO

select *
--update c set ruta_clave = REPLACE(ruta_clave, 'gilabasrdb01', 'gilabasrdb04')
--ruta_certificado = REPLACE(ruta_certificado, 'gilabasrdb01', 'gilabasrdb04')
from cfd_CER00100  c
where c.userid = 'gila\priscilla.parra'

--UPDATE CFD_CER00100 SET estado = 0

--Inserta configuración CAF (folios) por tipo de documento
insert into cfd_fol00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)
values(3, 'FACTURA ELECTRN', 238, 285, '\\gilabasrdb03\GettyCh_FacturaElectronicaCertificados\FoliosSII760555683323820141013179.xml');
--values(3, 'FV', 100, 199, '\\gilabasrdb03\GettyCh_FacturaElectronicaCertificados\FoliosSII760555683310020147301051.xml');

insert into cfd_fol00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)
values(3, 'NDEBITO ELECTRN', 150, 157, '\\gilabasrdb03\GettyCh_FacturaElectronicaCertificados\FoliosSII7605556856150201410131714.xml');

insert into cfd_fol00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)
values(4, 'NCREDIT ELECTRN', 150, 161, '\\gilabasrdb03\GettyCh_FacturaElectronicaCertificados\FoliosSII7605556861150201410131715.xml');


--update cfd_fol00100 set num_folio_desde = 238, num_folio_hasta = 285, ruta_codigo_autorizacion = 'C:\GPUsuario\GPExpressCfdi\feGilaChiTST\caf\FoliosSII760555683323820141013179.xml'
--where docid = 'FACTURA ELCTRNC'

--update cfd_fol00100 set num_folio_desde = 150, num_folio_hasta = 157, ruta_codigo_autorizacion = 'C:\GPUsuario\GPExpressCfdi\feGilaChiTST\caf\FoliosSII7605556856150201410131714.xml'
--where docid = 'NDEBITO ELECTRN'


--------------------------------------------------------------------------------------------------------------------------------------
--SET DE PRUEBAS

update cfd_fol00100 set num_folio_desde = 150, num_folio_hasta = 161, ruta_codigo_autorizacion = '\\10.1.1.20\GettyCh_FacturaElectronicaCertificados\FoliosSII7605556861150201410131715.xml'
where docid = 'NCREDIT ELECTRN'

insert into cfd_fol00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)
--values(3, 'FV', 238, 285, 'C:\GPUsuario\GPExpressCfdi\feGilaChiTST\caf\FoliosSII760555683323820141013179.xml');
values(3, 'FAC ELEC EXENTA', 1, 6, '\\10.1.1.20\GettyCh_FacturaElectronicaCertificados\FoliosSII760555683412016103939.xml');

insert into cfd_fol00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)
values(3, 'NDEBITO ELECTRN', 150, 157, '\\10.1.1.20\GettyCh_FacturaElectronicaCertificados\FoliosSII7605556856150201410131714.xml');

------------------------------------------------------------------------------------------------------------------------------
select *
--update fo set ruta_codigo_autorizacion = REPLACE(ruta_codigo_autorizacion, 'gilabasrap05', '10.1.1.18')
from  cfd_FOL00100 fo
where fo.DOCID in ('FACTURA ELCTRN ')	--, 'NDEBITO ELECTRN', 'NC ELECTRONICA')


select *
--update ce set ruta_certificado = replace(ruta_certificado, 'gilabasrap05', '10.1.1.18'), ruta_clave = REPLACE(ruta_clave, 'gilabasrap05', '10.1.1.18')
from cfd_CER00100 ce

