IF (OBJECT_ID ('dbo.vwSopTransaccionesVenta', 'V') IS NULL)
   exec('create view dbo.vwSopTransaccionesVenta as SELECT 1 as t');
go

--IF OBJECT_ID ('dbo.vwSopTransaccionesVenta') IS NOT NULL
--   DROP view dbo.vwSopTransaccionesVenta
--GO

alter view dbo.vwSopTransaccionesVenta
--Propósito. Obtiene las transacciones de venta SOP. 
--Utiliza:	vwRmTransaccionesTodas
--Requisitos. No muestra facturas ingresadas en cuentas por cobrar. 
--14/01/14 jcf Creación para Chile
--30/06/14 jcf Modifica campos de fCfdDatosAdicionales, agrega refrence
--			Modifica subtotal para no incluir descuento
--			Agrega usrdat02
--23/07/14 jcf Obtiene docType de localización
--12/11/14 jcf El rut del representante legal se cambia por el rut del cliente
--21/01/15 jcf Modifica default de sopUserTab01
--04/02/15 jcf Agrega cntcprsn
--09/02/15 jcf Agrega cspornbr
--25/08/16 jcf Agrega EmailToAddress
--
AS
SELECT	'contabilizado' estadoContabilizado,
		case when isnull(cl.rutClieProvee, '') = '' 
			then rtrim(STUFF(replace(replace(cab.TXRGNNUM, '.', ''), '-', ''), 
						len(rtrim(replace(replace(cab.TXRGNNUM, '.', ''), '-', ''))),
						0, '-'))	
			else rtrim(STUFF(cl.rutClieProvee, len(rtrim(cl.rutClieProvee)), 0, '-'))	
		end idImpuestoCliente,
		cab.CUSTNMBR,
		dbo.fCfdReemplazaEspecialesXml(dbo.fCfdReemplazaSecuenciaDeEspacios(dbo.fCfdReemplazaCaracteresNI(cab.CUSTNAME), 10)) nombreCliente,
		rtrim(cab.docid) docid, 
		rtrim(id.lochdoccod) docType,
		cde.dscriptn nombreTipoDoc,
		cab.SOPTYPE, 
		rtrim(cab.sopnumbe) sopnumbe, 
		dbo.fCfdObtieneFolio(rtrim(cab.sopnumbe), '-') folio, 
		--inicio simulación
		--case when datediff (day, '8/20/14', cab.docdate) >= 0 then getdate() else cab.docdate end docdate,
		--CONVERT(datetime, 
		--		replace(convert(varchar(20), case when datediff (day, '8/20/14', cab.docdate) >= 0 then getdate() else cab.docdate end, 102), '.', '-')+'T'+
		--		case when substring(cab.DOCNCORR, 3, 1) = ':' then rtrim(LEFT(cab.docncorr, 8))	
		--		else '00:00:00' end,
		--		126) fechaHora,
		--replace(convert(varchar(20), case when datediff (day, '8/20/14', cab.docdate) >= 0 then getdate() else cab.docdate end, 102), '.', '-') fecha,
		--fin simulación
		cab.docdate, 
		CONVERT(datetime, 
				replace(convert(varchar(20), cab.DOCDATE, 102), '.', '-')+'T'+
				case when substring(cab.DOCNCORR, 3, 1) = ':' then rtrim(LEFT(cab.docncorr, 8))	
				else '00:00:00' end,
				126) fechaHora,
		replace(convert(varchar(20), cab.DOCDATE, 102), '.', '-') fecha,
		cast(cab.ORDOCAMT as numeric(19,6)) totalOrig,														--se requieren 6 decimales fijos para generar el código de barras
		cab.ORSUBTOT, cab.ORTAXAMT impuestoOrig, cab.ORMRKDAM, cab.ORTDISAM, cab.ORTDISAM descuentoOrig, 
		cab.docamnt total, 
		cab.SUBTOTAL, cab.TAXAMNT impuesto, cab.trdisamt descuento, cab.trdispct,
		cab.orpmtrvd, rtrim(cab.curncyid) curncyid, 
		case when cab.xchgrate <= 0 then 1 else cab.xchgrate end xchgrate, 
		cab.voidStts + isnull(rmx.voidstts, 0) voidstts, 
		rmx.rmdtypal,
		
		--isnull(rtrim(STUFF(cl.RutRepLegal, len(rtrim(cl.RutRepLegal)), 0, '-')), '') RutRepLegal, 
		--isnull(rtrim(STUFF(cl.rutClieProvee, len(rtrim(cl.rutClieProvee)), 0, '-')), '') RutRepLegal, 
		'60803000-K' RutRepLegal, 
		
		dbo.fCfdReemplazaEspecialesXml(dbo.fCfdEsVacio(dbo.fCfdReemplazaSecuenciaDeEspacios(dbo.fCfdReemplazaCaracteresNI(cl.NomRepLeg), 10))) NomRepLeg,
		dbo.fCfdReemplazaEspecialesXml(dbo.fCfdEsVacio(dbo.fCfdReemplazaSecuenciaDeEspacios(dbo.fCfdReemplazaCaracteresNI(cl.RazonSocial), 10))) RazonSocial, 
		dbo.fCfdReemplazaEspecialesXml(dbo.fCfdEsVacio(dbo.fCfdReemplazaSecuenciaDeEspacios(dbo.fCfdReemplazaCaracteresNI(cl.GiroEmpresa), 10))) GiroEmpresa, 
		dbo.fCfdReemplazaEspecialesXml(dbo.fCfdEsVacio(dbo.fCfdReemplazaSecuenciaDeEspacios(dbo.fCfdReemplazaCaracteresNI(cab.cntcprsn), 10))) contacto,
		dbo.fCfdReemplazaEspecialesXml(dbo.fCfdEsVacio(dbo.fCfdReemplazaSecuenciaDeEspacios(dbo.fCfdReemplazaCaracteresNI(cn.address1), 10))) address1, 
		dbo.fCfdReemplazaEspecialesXml(left(dbo.fCfdEsVacio(dbo.fCfdReemplazaSecuenciaDeEspacios(dbo.fCfdReemplazaCaracteresNI(cn.address2), 10)), 20)) address2, 
		dbo.fCfdEsVacio(dbo.fCfdReemplazaSecuenciaDeEspacios(dbo.fCfdReemplazaCaracteresNI(cn.address3), 10)) address3, 
		left(dbo.fCfdEsVacio(dbo.fCfdReemplazaSecuenciaDeEspacios(dbo.fCfdReemplazaCaracteresNI(cn.city), 10)), 20) city, 
		dbo.fCfdReemplazaEspecialesXml(left(dbo.fCfdEsVacio(dbo.fCfdReemplazaSecuenciaDeEspacios(dbo.fCfdReemplazaCaracteresNI(cn.[STATE]), 10)), 20)) [state], 
		dbo.fCfdEsVacio(dbo.fCfdReemplazaSecuenciaDeEspacios(dbo.fCfdReemplazaCaracteresNI(isnull(cc.cCodeDesc, cn.country)), 10)) country, 
		right('00000'+dbo.fCfdReemplazaSecuenciaDeEspacios(dbo.fCfdReemplazaCaracteresNI(cn.zip), 10), 5) zipcode, 
		
		cab.duedate, cab.pymtrmid, cab.glpostdt, cab.refrence,
		isnull(da.sopUserDef1, '') sopUserDef1,
		isnull(da.sopUserDef2, '') sopUserDef2,
		--'Pago en una sola exhibición' formaDePago,
		isnull(da.sopUserTab01, '0') sopUserTab01,		--código de referencia caso nc o nd
		isnull(da.usrdat02, '1-1-1900') usrdat02,
		dbo.fCfdReemplazaSecuenciaDeEspacios(dbo.fCfdReemplazaCaracteresNI(
			rmx.cspornbr
		), 10) cstponbr, isnull(convert(varchar(250), ia.EmailToAddress), '') EmailToAddress
  from	sop30200 cab							--sop_hdr_hist
		inner join vwCfdIdDocumentos cnf		--docs configurados para fe
			on cnf.docid = cab.DOCID
        left outer join loch0004 id 
             on id.custvndr = cab.custnmbr
            and id.lochtrxno = cab.sopnumbe
			and id.module1 = 2					--2:ventas
        left outer join loch0002 cde			--loch_descripcion_documentos
             on cde.lochdoccod = id.lochdoccod
            and cde.module1 = id.module1
        left outer join RM00101 cn				--rm_customer_mstr
			on cn.CUSTNMBR = cab.CUSTNMBR
		left outer join vwRmTransaccionesTodas rmx
             ON rmx.RMDTYPAL in (1, 8)			-- 1 invoice, 8 return
            and rmx.bchsourc = 'Sales Entry'	-- incluye sop
            and (cab.sopType-2 = rmx.rmdTypAl or cab.sopType+4 = rmx.rmdTypAl) --elimina la posibilidad de repetidos
            and cab.sopnumbe = rmx.DOCNUMBR
        left outer join cllc0002 cl				--razón social de clientes y proveedores (localización chilena)
             on rtrim(cab.custnmbr) = rtrim(cl.custvndr)
            and cl.tipClieProvee = 1			--cliente
		OUTER APPLY dbo.fCfdDatosAdicionales(cab.orpmtrvd, cab.soptype, cab.sopnumbe, cab.custnmbr, cab.prbtadcd) da
        left outer join vat10001 cc				--vat_country_code_mstr
			on cc.ccode = cab.country
		left join sy01200	ia	--Master_Type, Master_ID, ADRSCODE
			on ia.Master_Type = 'CUS'
			AND ia.Master_ID = cn.custnmbr
			and ia.ADRSCODE = cn.ADRSCODE

 where cab.soptype in (3, 4)					--3 invoice, 4 return
 
 union all
 
 select 'en lote' estadoContabilizado, cab.custnmbr idImpuestoCliente, cab.CUSTNMBR, cab.CUSTNAME nombreCliente,
		rtrim(cab.docid) docid, 
		rtrim(id.lochdoccod) docType,
		cde.dscriptn nombreTipoDoc,
		cab.SOPTYPE, 
		rtrim(cab.sopnumbe) sopnumbe, 
		1 folio, 
		cab.docdate, cab.docdate fechaHora, replace(convert(varchar(20), cab.DOCDATE, 102), '.', '-') fecha,
		cab.ORDOCAMT totalOrig, cab.ORSUBTOT subtotalOrig, cab.ORTAXAMT impuestoOrig, 0, cab.ORTDISAM, cab.ORTDISAM descuentoOrig, 
		cab.docamnt total, 
		cab.SUBTOTAL subtotal, cab.TAXAMNT impuesto, cab.trdisamt descuento, cab.trdispct,
		cab.orpmtrvd, rtrim(cab.curncyid) curncyid, 
		cab.xchgrate, 
		cab.voidStts, 0	rmdtypal,
		'', '', 
		'', '', cab.cntcprsn, cab.address1, cab.address2, cab.address3, cab.city, cab.[STATE], cab.country, cab.zipcode, 
		cab.duedate, cab.pymtrmid, cab.glpostdt, cab.refrence,
		ctrl.USERDEF1, ctrl.userdef2,
		--'Pago en una sola exhibición' formaDePago,
		isnull(ctrl.usrtab01, '0') sopUserTab01,
		isnull(ctrl.usrdat02, '1-1-1900') usrdat02,
		cab.cstponbr, '' EmailToAddress
 from  SOP10100 cab								--sop_hdr_work
		inner join vwCfdIdDocumentos cnf		--docs configurados para fe
			on cnf.docid = cab.DOCID
        left outer join SOP10106 ctrl			--campos def. por el usuario.
            on ctrl.SOPTYPE = cab.SOPTYPE
            and ctrl.SOPNUMBE = cab.SOPNUMBE
        left outer join loch0004 id 
             on id.custvndr = cab.custnmbr
            and id.lochtrxno = cab.sopnumbe
			and id.module1 = 2					--2:ventas
        left outer join loch0002 cde			--loch_descripcion_documentos
             on cde.lochdoccod = id.lochdoccod
            and cde.module1 = id.module1
 where cab.SOPTYPE in (3, 4)					--3 invoice, 4 return
go

IF (@@Error = 0) PRINT 'Creación exitosa de: vwSopTransaccionesVenta'
ELSE PRINT 'Error en la creación de: vwSopTransaccionesVenta'
GO

-------------------------------------------------------------------------------------------------------
--test
--select cstponbr, *
--from vwSopTransaccionesVenta
--where sopnumbe in ( '33-00000370', '33-00000371', '33-00000372', '33-00000378')

--select *
--from loch0002
--where module1=2

--update loch0002 set dscriptn = 'FACTURA ELECTRONICA'
--WHERE LOCHDOCCOD = '33'
--AND MODULE1 = 2

--update loch0002 set dscriptn = 'NOTA DE DEBITO ELECTRONICA'
--WHERE LOCHDOCCOD = '56'
--AND MODULE1 = 2

--update loch0002 set dscriptn = 'NOTA DE CREDITO ELECTRONICA'
--WHERE LOCHDOCCOD = '61'
--AND MODULE1 = 2
