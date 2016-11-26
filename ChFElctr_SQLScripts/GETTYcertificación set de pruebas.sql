--Inserta datos del usuario y su certificado personal
--

use gchi
go
--atención: los ids de usuario deben estar guardados con minúsculas incluyendo el dominio. Ejemplo: gila\priscilla.parra

--------------------------------------------------------------------------------------------------------------------------------------
--SET DE PRUEBAS

update cfd_fol00100 set num_folio_desde = 150, num_folio_hasta = 161, ruta_codigo_autorizacion = '\\10.1.1.20\GettyCh_FacturaElectronicaCertificados\FoliosSII7605556861150201410131715.xml'
where docid = 'NCREDIT ELECTRN'

insert into cfd_fol00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)
values(3, 'FAC ELEC EXENTA', 1, 6, '\\10.1.1.20\GettyCh_FacturaElectronicaCertificados\FoliosSII760555683412016103939.xml');

insert into cfd_fol00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)
values(3, 'NDEBITO ELECTRN', 150, 157, '\\10.1.1.20\GettyCh_FacturaElectronicaCertificados\FoliosSII7605556856150201410131714.xml');

------------------------------------------------------------------------------------------------------------------------------
--ruta de certificados y caf

select *
--update c set ruta_certificado=REPLACE(ruta_certificado, '10.1.1.18', '10.1.1.20'), 
--				ruta_clave = REPLACE(ruta_clave, '10.1.1.18', '10.1.1.20')
from cfd_CER00100  c

select *
--update fo set ruta_codigo_autorizacion = REPLACE(ruta_codigo_autorizacion, '10.1.1.18', '10.1.1.20')
from  cfd_FOL00100 fo
where fo.DOCID in ('FACTURA ELCTRN ')	--, 'NDEBITO ELECTRN', 'NC ELECTRONICA')

-----------------------------------------------------------------------------------------------------------------------------
--ruta de xml
select * 
--update fe set inet7 = replace(inet7, 'GILABASRAP05', '10.1.1.20')
from SY01200 fe
-------------------------------------------------------------------------------------------------------------------------------

--SET DE PRUEBAS 
	--Agregar cálculo de descuento para sólo afectos en fCfdGeneraDocumentoDeVentaXML (100_)
	--Agregar cálculo de exento, neto en fCfdVentasObtieneNeto y fCfdVentasObtieneExento (98_fCfdVentasObtieneNeto y Exento.functions.sql)?

--En GP
	--Ingresar casos de prueba. Ejecutar macro ingresar setDePrueba.mac. Cada factura, nc, nd, debe indicar el valor SET CASO XXXXX en el número de seguimiento de ventas
	--Contabilizar casos de prueba
	--Aplicar notas de crédito a facturas y nds

---actualizar la hora para ordenar el set de pruebas en base al número de caso
update fe set docncorr = n.hora
--select n.hora, n.tracking_number,  fe.*
from sop30200 fe
inner join (
	select top 60 f.sopnumbe, f.soptype,
			t.tracking_number, f.docncorr, 
			'16:22:'+ 
			case when len( row_number() over(order by t.tracking_number)) = 1 then '0' else '' end +
			convert(varchar(5), row_number() over(order by t.tracking_number)) hora
	from sop30200 f
	inner join sop10107 t
		on t.sopnumbe = f.sopnumbe
		and t.soptype = f.soptype
	where datediff(day, '10/19/16', f.docdate) = 0
	order by t.tracking_number
	) n
on n.sopnumbe = fe.sopnumbe
and n.soptype = fe.soptype

--actualizar los objetos sql
--ejecutar la aplicación

-------------------------------------
--SET SIMULACION
	--Quitar el cálculo de descuento para sólo afectos en fCfdGeneraDocumentoDeVentaXML (100_)
	--Quitar el cálculo de exento, neto en fCfdVentasObtieneNeto y fCfdVentasObtieneExento (98_fCfdVentasObtieneNeto y Exento.functions.sql)?
