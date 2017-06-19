use gchi;

--GETTY
--Pruebas de Factura electrónica México
--
--1. Probar en bd que no tenga los objetos sql instalados
-- 	* Aviso de expiración de Certificado Digital 

-------------------------------------------------------------------------------------------------------
--OBJETOS DTE
use gchi;

select * from fCfdEmisor()
select dbo.fCfdDescuentosXML(100, 1)

select * 
from vwCfdTransaccionesDeVenta 
where sopnumbe like '33-00001368'
--where sopnumbe >= '33-00001158'	-- '33%26%'	--'61%15%'	--
order by sopnumbe

select * from fCfdFolioVigente(3 , 'FACTURA ELCTRNC', 100)
select dbo.fCfdObtieneFolio('33-00000002', '-')
select * from dbo.fCfdCertificadoVigente('1/1/14', 'sa')

select dbo.fCfdConceptosXML(3, '33-00001446')
select dbo.fCfdImpuestosXML(3, 'FV 00003929', 'V-IVA DF')
select * FROM dbo.fCfdImpuestos(3, 'FV 00003929', 'V-IVA DF')
select dbo.fCfdGeneraDocumentoDeVentaXML (3, '33-00003641')	--aceptado

select tv.soptype, tv.rmdtypal, tv.sopnumbe, tv.docdate, tv.sopUserTab01, tv.refrence, tv.sopUserDef2, tv.usrdat02, tv.total, rtrim(tv.CUSTNMBR), tv.cstponbr,
		dbo.fCfdReferenciaXML(tv.soptype, tv.rmdtypal, tv.sopnumbe, tv.docdate, tv.sopUserTab01, tv.refrence, tv.sopUserDef2, tv.usrdat02, tv.total, rtrim(tv.CUSTNMBR), tv.cstponbr)	'Documento'
	from vwSopTransaccionesVenta tv
where tv.sopnumbe like '33-00004363'
and datediff(day, '1/1/1900', usrdat02) = 0

select itemdesc, replace(itemdesc, '’', ''), dbo.fCfdReemplazaCaracteresNI(itemdesc), dbo.fCfdConceptosXML(3, '33-00001446')
from vwSopLineasTrxVentas 
where sopnumbe = '33-00001446'

--update s set refrence = 'DEVOLUCION DE MERCADERIAS' --docncorr = '13:30:00:000'
select docncorr, *
from sop30200 s
where s.sopnumbe = '61-0000108'
datediff(day, '8/14/14', docdate) = 0
order by 1

--insert into loch0004 (LOCHTRXNO,DOCTYPE,LOCHDOCCOD,CUSTVNDR,VNDDOCNM,DOCDATE,MODULE1,DOCAMNT,RPRTTYPE,USERID,DOCCLTYP)
select '61-0000109',DOCTYPE,LOCHDOCCOD,CUSTVNDR,VNDDOCNM,DOCDATE,MODULE1, 1592082, RPRTTYPE,USERID,DOCCLTYP
from loch0004 id 
where id.lochtrxno = '33-00000262'

--update ctrl set userdef2 = '33-0109' -- usrdat02 = '8/12/14'
select *
	from SOP10106 ctrl					--campos def. por el usuario.
	where --ctrl.soptype = @soptype
	ctrl.sopnumbe in ('61-0000104', '56-0000101')
	
-----------------------------------------------------------------------------------------------------
--LOG FACTURA XML venta
--
--insert into cfdlogfacturaxml(soptype,sopnumbe,estado,estadoActual,noAprobacion,mensajeEA,mensaje,archivoXML,idExterno,fechaEmision)
--						values(3, '33-3882', 'aceptado SII', '00000000111100', 8, 'EMITIDO. ENVIADO SII. ACEPTADO SII. ', 'Resultado del SII. ACEPTA', null, '0912719492', '1/23/15'  )
--				select soptype,'33-3882',estado,estadoActual,noAprobacion,rtrim(mensajeEA) + ' (ajustado)',mensaje,archivoXML,idExterno,'3/31/17'

--update lf set estadoActual = '00000000111100', noAprobacion = 8, mensajeEA = 'EMITIDO. ENVIADO SII. ACEPTADO SII. '

select sopnumbe, idexterno, 
*	--into _temp2_cfdlogfacturaxml	--soptype, sopnumbe, estado, mensaje, estadoActual, mensajeEA, noAprobacion indice, idExterno, secuencia
--delete lf
--UPDATE LF SET IDEXTERNO = 2178342062, ESTADOACTUAL= '00000000011100', NOAPROBACION = 9, MENSAJEEA = 'EMITIDO. ENVIADO SII.'
from cfdlogfacturaxml lf
where 
lf.sopnumbe in 
(
'33-00004364',   --   BBDO PUBLICIDAD S.A.
'33-00004365' ,   --  EDITORIAL TIEMPO PRESENTE LTDA.
'33-00004366'  ,   -- BANCO DE CHILE
'33-00004368'
)
and estado = 'emitido'


select sopnumbe, idexterno, * --into _temp_cfdlogfacturaxml	--soptype, sopnumbe, estado, mensaje, estadoActual, mensajeEA, noAprobacion indice, idExterno, secuencia
from cfdlogfacturaxml lf
where mensajeEA = 'EMITIDO. ENVIADO SII. RECHAZADO SII.'
and exists (select docnumbr from vwRmTransaccionesTodas where voidstts = 0 and docnumbr = lf.sopnumbe)
and estado = 'emitido'

--update so set refrence = 'Razón social distinta'
select custnmbr, custname, *
from sop30200 so
where datediff(day, '8/1/16', docdate) >=0
order by 2
so.sopnumbe in ('33-00002248') --, '33-00000523', '33-00000532')

select *
from sop30300 so
where so.sopnumbe in ('33-00000651') --, '33-00000523', '33-00000532')

select top 1 1 NroLinRef, id.lochdoccod TpoDocRef, dbo.fCfdGetSegmento2(rtrim(ta.aptodcnm), '-') apdcnm,	--el espacio en el caso de facturas manuales de Getty
	convert(varchar(10), ta.aptodcdt, 102) apdcdt 
	--left(@USRTAB01, 1) CodRef,		--1: sólo en NC o ND para anular un doc
	--rtrim(@REFRENCE) RazonRef
from vwRmTrxAplicadas ta 
inner join sop30200 sp
	on sp.sopnumbe = ta.aptodcnm
	and sp.soptype = 3			--aplica a una ND o Factura
inner join loch0004 id 
	on id.custvndr = sp.custnmbr
	and id.lochtrxno = sp.sopnumbe
	and id.module1 = 2			--2:ventas
where --@soptype = 4				--devolución
 ta.apfrdcnm = '33-00003641'
and ta.apfrdcty = 8		--nc

--update du set usrtab01 = '1-Anula documento'
select *
from sop10106 du
where du.sopnumbe like '33-00003745'

----------------------------------------------------------------------------------------------------
--LOG FACTURAS DE COMPRA

--insert into  cfdLogFacturaCompra (tipo,folio,idImpuestoTercero,nombreTercero,fechaRecepcion,estado,estadoActual,idxSingleStatus,mensajeEA,mensaje,archivoXML,archivoPDF,idExterno,fechaAlta,idUsuario,fechaModificacion,idUsuarioModificacion)
--select tipo,folio,idImpuestoTercero,nombreTercero,fechaRecepcion,estado,estadoActual,idxSingleStatus,mensajeEA,mensaje,archivoXML,archivoPDF,idExterno,fechaAlta,idUsuario,fechaModificacion,idUsuarioModificacion
--from tmp_cfdLogFacturaCompra141113
--where idimpuestotercero = '76833750-0'

select * --into tmp_cfdLogFacturaCompra141113
from cfdLogFacturaCompra
where folio like '%3924%'
--where secuencia in (152, 153, 158)
where idimpuestotercero = '76833750-0'	--en el caso de ventas es el id del SII
order by secuencia

--update cfdLogFacturaCompra set estadoActual = '00011000000000', idxSingleStatus = 3, mensajeEA = 'RECIBIDO.'
--where estado = 'publicado'
--and secuencia > 451
--and tipo >0
--and sopnumbe = 'FV 00008205'


------------------------------------------------------------------------------------------------------
--LIBRO DE COMPRAS Y VENTAS

--tabla LOG LIBROS DE COMPRA Y VENTA
select * 
from cfdlogLibroCV 
where tipo = 'LV'
and periodo = 201609

select *
from vwCfdLibroCVLog

select DBO.fCfdLibroComprasXML ('LC', 2016, 3)
select dbo.fCfdLibroVentasXML('LV', 2017, 3)

------------------------------------------------------------------------------------------------------
--docs sin código de documento de loc Chile
--
--revisa docs sin código de documento en AP
--insert into loch0004 (LOCHTRXNO,DOCTYPE,LOCHDOCCOD,CUSTVNDR,VNDDOCNM,DOCDATE,MODULE1,DOCAMNT,RPRTTYPE,USERID,DOCCLTYP)
select a.poprctnm, 1, '33', a.vendorId, a.docnumbr, a.docdate, 1, a.docamnt, 1, 'sa', 1 -- *
  from dbo.vwIecvDetalleComprasAP a
        left outer join loch0004 cdt --loch_boleta_trx_details Indica si es electrónica
             on cdt.custvndr = a.VENDORID
            and cdt.lochtrxno = case when a.poprctnm is null then a.VCHRNMBR 
															else a.poprctnm end	--usar el número de repción cuando es recepción/factura
where cdt.lochdoccod is null
and year(a.pstgdate) >= 2016
and a.doctype = 1

--revisa docs sin código de documento en SOP
--insert into loch0004 (LOCHTRXNO,DOCTYPE,LOCHDOCCOD,CUSTVNDR,VNDDOCNM,DOCDATE,MODULE1,DOCAMNT,RPRTTYPE,USERID,DOCCLTYP)
select 
a.sopnumbe, 1, '33', a.custnmbr, '', a.docdate, 2, a.docamnt, 2, 'sa', 1 -- *	--factura sop
--a.sopnumbe, a.soptype, cdt.*
  from sop30200 a
        left outer join loch0004 cdt --loch_boleta_trx_details Indica si es electrónica
             on cdt.custvndr = a.custnmbr
            and cdt.lochtrxno = a.sopnumbe
where cdt.lochdoccod is null
and year(a.docdate) >= 2016
and soptype = 3

select top 100 *
from loch0004 
where 
--lochtrxno >= '61-0000570'
lochtrxno like '33%'
and year(docdate) >= 2016

-------------------------------------------------------------------------------------
--PARA IMPRIMIR
select soptype, docid, sopnumbe, fechaHoraEmision, regimenFiscal, rfcReceptor, nombreCliente, total, formaDePago, folioFiscal,
	noCertificadoCSD, [version], selloCFD, selloSAT, cadenaOriginalSAT, noCertificadoSAT, FechaTimbrado, rutaYNomArchivo
from vwCfdDocumentosAImprimir

select *
from fCfdDatosXmlParaImpresion(@archivoXml)

--------------------------------------------------------------------------------------------------------------------
--TABLAS GP Y LOCALIZACION

--actualiza tipo de documento de localización de compras
select *
from loch0004	--CUSTVNDR VNDDOCNM LOCHTRXNO
where 
custvndr like '%23427%' 
and MODULE1 = 1	--1:compras, 2:ventas
and YEAR(docdate ) = 2014

--update loch0004 set lochdoccod = 60, RPRTTYPE = 2	--1:electrónica, 2:no electrónica
where VNDDOCNM = '10992296K-211'
and MODULE1 = 1
and custvndr like '109922%'

SELECT *
FROM sop10100
WHERE SOPNUMBE in ( '33-0000101', '33-0000102', '33-0000103', 'FV 00000082')

SELECT voidstts, *
FROM SOP30200
--update sop30200 set docdate = '7/7/2013'
WHERE SOPNUMBE = '33-00003671'
year(docdate) = 2014
and month(docdate) = 7
and day(docdate) <= 22

select *
--update li set itemdesc  = 'Pañuelo'
from sop30300 li
where li.sopnumbe = '61-0000101'
and itemnmbr = 'SII3'

--SOPNUMBE in ( '33-00000101', '33-00000102', '33-00000103', 'FV 00000082')

select ortxsls, tdttxsls,*	--total venta gravable
from sop10105
WHERE SOPNUMBE in ( '33-00003641') --, '33-00000102', '33-00000103', 'FV 00000082')

SELECT IVITMTXB, TAXAMNT, *
FROM SOP30300
WHERE SOPNUMBE in ( '33-00000101', '33-00000102', '33-00000103', 'FV 00000082')
------------------------------------------------
--update id set id.lochdoccod = '33'
select id.lochdoccod, cab.docid , cab.*
from sop10100 cab
inner join loch0004 id
             on id.custvndr = cab.custnmbr
            and id.lochtrxno = cab.sopnumbe
			and id.module1 = 2					--2:ventas
where datediff(day, '8/1/14', cab.docdate) >= 0
and cab.soptype = 3
and cab.docid = 'FV'

--update id set id.lochdoccod = '61'
select id.lochdoccod, cab.docid , cab.*
from sop30200 cab
inner join loch0004 id
             on id.custvndr = cab.custnmbr
            and id.lochtrxno = cab.sopnumbe
			and id.module1 = 2					--2:ventas
where datediff(day, '5/1/15', cab.docdate) >= 0
and cab.soptype = 4
and cab.docid = 'NC'
AND ID.LOCHDOCCOD != '61'

----------------------------------------------------------
--actualiza rut de clientes con guión
--update cl set cl.rutclieprovee = '97036000K'  --replace(cl.rutClieProvee, '-', '')
--update cl set RutClieProvee =	ltrim(RutClieProvee)	--, razonSocial = 'VTR BANDA ANCHA CHILE S.A.'	--correcto: '761141430', razonSocial = 'VTR COMUNICACIONES SPA'
--insert into cllc0002 (CUSTVNDR,RutClieProvee,TipClieProvee,GiroEmpresa,RazonSocial,RutRepLegal,NomRepLeg,Activo)
select CUSTVNDR,RutClieProvee,TipClieProvee,GiroEmpresa,RazonSocial,RutRepLegal,NomRepLeg,Activo
from cllc0002 CL	--razón social de clientes y proveedores (localización chilena)
where --cl.rutclieprovee like '%.%'
cl.custvndr in ('000011447', '000019226')
like '%11447%' --= '000005844'

SELECT TXRGNNUM, *
--UPDATE RM SET TXRGNNUM = '767357702'
FROM RM00101 RM
WHERE RM.CUSTNMBR = '000018253'

--Indicar rut del SII como receptor
--update ru set rutclieprovee = '968069802' -- rutreplegal = '60803000K', nomrepleg = 'SII'
select *
--delete ru
from cllc0002 ru --razón social de clientes y proveedores (localización chilena)
where --ru.custvndr like '%761871%'
rutclieprovee like '%761871%' --= '000005844'
and tipclieprovee = 2

	select *
	from cllc4000	--cllc_localization_setup

	select *
	from ncloc145	--ncloc_sucursales_mstr

	select *
	from loch0002	--loch_document_code_mstr

--corrige giro de la empresa
select cl.comment1, cl.custnmbr, cl.custname, lo.giroEmpresa 
--update lo set  lo.giroEmpresa = cl.comment1
from rm00101 cl
inner join cllc0002 lo
	on lo.custvndr = cl.custnmbr
where --lo.giroEmpresa = lo.razonsocial
 lo.tipClieProvee = 1
 and cl.comment1 != lo.giroEmpresa

------------------------------------------------------------------------------------------------------------------
--agregar código de documento del SII a factura
--insert into loch0004 (LOCHTRXNO,DOCTYPE,LOCHDOCCOD,CUSTVNDR,VNDDOCNM,DOCDATE,MODULE1,DOCAMNT,RPRTTYPE,USERID,DOCCLTYP)
select s.sopnumbe, case when soptype = 3 then 1 else 8 end, 
	left(s.sopnumbe, 2), s.custnmbr, '', s.docdate, 2, s.docamnt, 1, 'sa', 1
from sop30200 s
left join loch0004 id 
	on id.lochtrxno = s.sopnumbe
where id.lochtrxno is null
and year(s.docdate) >= 2016
and s.sopnumbe = '33-00002142'


--update s set refrence = 'Facturar a otra empresa'
select refrence, *
from sop30200 s
where soptype = 3
and sopnumbe in (
'33-00000644'
)

--insert into sop10106 (soptype, sopnumbe, usrtab01, cmmttext)
values (4, '61-0000060', '1-Anula documento', '')
--update ctrl set usrdat02 = '10/30/15', userdef2 = '33-1611'		-- usrtab01 = '1-Anula documento'
select *
from SOP10106 ctrl	--campos def. por el usuario.
where ctrl.soptype = 3
and ctrl.sopnumbe = '33-00003745'	--'61-0000303'
and ctrl.usrtab01 = ''

--update sl set subtotal = 4609656, orsubtot= 4609656, remsubto= 4609656, oremsubt= 4609656
--update sl set docdate = '11/30/14' -- bchsourc= 'Sales Entry    ', bachnumb = 'NCREDIT ELECTRN', custnmbr = '000005642      ', custname = 'BANCO DE CHILE                                                   '
--select *
from sop10100 sl
where sl.sopnumbe in (
'61-0000038'        
  )


select *
--update s set tracking_number = 'OC458928'
--DELETE s
from sop10107 s		--números de seguimiento
where s.sopnumbe = '33-00004264'
AND TRACKING_NUMBER = 'OC'

--update rm set txrgnnum = '765102340'
select *
from rm00101 rm
where rm.custnmbr = '000021037'

select *	--sum(iva)
from vwIecvLibroComprasCrudo
where year(docdate) = 2015
and month(docdate) = 7

------------------------------------------------------------------------------------
--Para excluir la factura del libro de ventas
select cspornbr, *
--update r set cspornbr = 'EXCLUIRDELV'
from rm20101 r
where r.docnumbr = '33-00003882'

