--Inserta datos de certificados de personas en producción
--

use gchi
go
--atención: los ids de usuario deben estar guardados con minúsculas incluyendo el dominio. Ejemplo: gila\priscilla.parra
insert into cfd_CER00100 
( USERID,ACA_RUT,fecha_vig_desde,fecha_vig_hasta,ruta_certificado,ruta_clave,
	contrasenia_clave,ACA_SolicitaFolio,ACA_AnulaDocumentos,ACA_EnviaDocumentos,ACA_FirmaDocumentos,ACTIVE)
values('gila\priscilla.parra', '87115925', '1/1/14', '12/31/14', 
		'\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\mrebolledo\CERTIFICADO ELECTRONICO MARIANA REBOLLEDO.p12',
		'\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\mrebolledo\CERTIFICADO ELECTRONICO MARIANA REBOLLEDO.p12',
		'GettyCert2014', 1, 1, 1, 1, 1)
GO
insert into cfd_CER00100 
( USERID,ACA_RUT,fecha_vig_desde,fecha_vig_hasta,ruta_certificado,ruta_clave,
	contrasenia_clave,ACA_SolicitaFolio,ACA_AnulaDocumentos,ACA_EnviaDocumentos,ACA_FirmaDocumentos,ACTIVE)
values('gila\andrea.gomez', '87115925', '1/1/14', '12/31/14', 
		'\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\mrebolledo\CERTIFICADO ELECTRONICO MARIANA REBOLLEDO.p12',
		'\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\mrebolledo\CERTIFICADO ELECTRONICO MARIANA REBOLLEDO.p12',
		'GettyCert2014', 1, 1, 1, 1, 1)
GO
insert into cfd_CER00100 
( USERID,ACA_RUT,fecha_vig_desde,fecha_vig_hasta,ruta_certificado,ruta_clave,
	contrasenia_clave,ACA_SolicitaFolio,ACA_AnulaDocumentos,ACA_EnviaDocumentos,ACA_FirmaDocumentos,ACTIVE)
values('gila\tiiselam', '87115925', '1/1/14', '12/31/14', 
		'\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\mrebolledo\CERTIFICADO ELECTRONICO MARIANA REBOLLEDO.p12',
		'\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\mrebolledo\CERTIFICADO ELECTRONICO MARIANA REBOLLEDO.p12',
		'GettyCert2014', 1, 1, 1, 1, 1)
GO
insert into cfd_CER00100 
( USERID,ACA_RUT,fecha_vig_desde,fecha_vig_hasta,ruta_certificado,ruta_clave,
	contrasenia_clave,ACA_SolicitaFolio,ACA_AnulaDocumentos,ACA_EnviaDocumentos,ACA_FirmaDocumentos,ACTIVE)
values('gila\ext-tiiselam4', '87115925', '1/1/14', '12/31/14', 
		'\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\mrebolledo\CERTIFICADO ELECTRONICO MARIANA REBOLLEDO.p12',
		'\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\mrebolledo\CERTIFICADO ELECTRONICO MARIANA REBOLLEDO.p12',
		'GettyCert2014', 1, 1, 1, 1, 1)
GO

select *
--update c set ruta_certificado = '\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\pparra\Certificado_PriscillaParra.p12',
--			ruta_clave = '\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\pparra\Certificado_PriscillaParra.p12',
--			contrasenia_clave = '1234'
from cfd_CER00100 c
where USERID  = 'gila\priscilla.parra'

---------------------------------------------------------------------------------------------------------------------------
--Inserta configuración CAF (folios) por tipo de documento
---------------------------------------------------------------------------------------------------------------------------

--insert into cfd_fol00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)
--values(3, 'FACTURA ELCTRN', 1, 1400, '\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\FoliosSII7605556833120141151622.xml');

--insert into cfd_fol00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)
--values(3, 'FACTURA ELCTRN', 1401, 1947, '\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\FoliosSII7605556833140120159231538.xml');

--insert into cfd_fol00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)
--values(3, 'FACTURA ELCTRN', 1948, 2107, '\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\FoliosSII76055568331948201512221314.xml');

--insert into cfd_fol00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)
--values(3, 'FACTURA ELCTRN', 2108, 2257, '\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\FoliosSII7605556833210820162191612.xml');

insert into cfd_fol00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)
values(3, 'FACTURA ELCTRN', 2258, 3937, '\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\FoliosSII7605556833225820163211018.xml');

--insert into cfd_fol00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)
--values(3, 'NDEBITO ELECTRN', 150, 157, '\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\FoliosSII7605556856150201410131714.xml');

--insert into cfd_fol00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)
--values(4, 'NCREDIT ELECTRN', 1, 280, '\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\FoliosSII7605556861120141151625.xml');

--insert into cfd_fol00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)
--values(4, 'NCREDIT ELECTRN', 281, 370, '\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\FoliosSII760555686128120151091319.xml');

--insert into cfd_fol00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)
--values(4, 'NCREDIT ELECTRN', 371, 410, '\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\FoliosSII760555686137120162101132.xml');

insert into cfd_fol00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)
values(4, 'NCREDIT ELECTRN', 411, 710, '\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\FoliosSII76055568614112016328125.xml');

insert into cfd_fol00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)
values(3, 'FAC ELEC EXENTA', 1, 50, '\\gilabasrap05\GettyCh_FacturaElectronicaCertificados\FoliosSII76055568341201611141653.xml');

-----------------------------------------------
select *
--delete f
from  cfd_FOL00100 f
where f.soptype = 4
and f.docid = 'NCREDIT ELECTRN'	--'NCREDIT ELECTRN'

--update cfd_fol00100 set ruta_codigo_autorizacion = REPLACE(ruta_codigo_autorizacion, 'gilabasrap05', 'gilabasrap05')

