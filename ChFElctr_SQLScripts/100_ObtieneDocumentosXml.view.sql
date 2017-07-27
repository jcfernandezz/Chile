--FACTURA ELECTRONICA GP - CHILE
--Proyectos:		GETTY
--Propósito:		Genera funciones y vistas de FACTURAS para la facturación electrónica en GP - CHILE
--Referencia:		
--		14/1/14 Versión Beta
--		30/11/14 Release
--Utilizado por:	Aplicación C# de generación de factura electrónica CHILE
-----------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdInfoAduaneraXML') IS NOT NULL
   DROP FUNCTION dbo.fCfdInfoAduaneraXML
GO

create function dbo.fCfdInfoAduaneraXML(@ITEMNMBR char(31), @SERLTNUM char(21))
returns xml 
as
--Propósito. Obtiene info aduanera para conceptos de importación
--Requisito. Se asume que todos los artículos importados usan número de serie o lote. De otro modo se consideran nacionales.
--			También se asume que no hay números de serie repetidos por artículo
--17/5/12 jcf Creación
--
begin
	declare @cncp xml;
	select @cncp = null;

	IF isnull(@SERLTNUM, '_NULO') <> '_NULO'	
	begin
		WITH XMLNAMESPACES ('http://www.sat.gob.mx/cfd/3' as "cfdi")
		select @cncp = (
		   select ad.numero, ad.fecha
		   from (
				--En caso de usar número de lote, la info aduanera viene en el número de lote y los atributos del lote
				select top 1 dbo.fCfdReemplazaSecuenciaDeEspacios(ltrim(rtrim(@SERLTNUM)),10) numero, 
						--dbo.fCfdReemplazaSecuenciaDeEspacios(ltrim(rtrim(dbo.fCfdReemplazaCaracteresNI(la.LOTATRB1 +' '+ la.LOTATRB2))),10) numero, 
						replace(convert(varchar(12), la.LOTATRB4, 102), '.', '-') fecha,
						dbo.fCfdReemplazaSecuenciaDeEspacios(ltrim(rtrim(dbo.fCfdReemplazaCaracteresNI(la.LOTATRB3))),10) aduana
				  from iv00301 la				--iv_lot_attributes [ITEMNMBR LOTNUMBR]
				  inner join IV00101 ma			--iv_itm_mstr
					on ma.ITEMNMBR = la.ITEMNMBR
				 where ma.ITMTRKOP = 3			--lote
					and la.ITEMNMBR = @ITEMNMBR
					and la.LOTNUMBR = @SERLTNUM
				union all
				--En caso de usar número de serie, la info aduanera viene de los campos def por el usuario de la recepción de compra
				select top 1 dbo.fCfdReemplazaSecuenciaDeEspacios(ltrim(rtrim(dbo.fCfdReemplazaCaracteresNI(ud.user_defined_text01))),10) numero, 
						replace(convert(varchar(12), ud.user_defined_date01, 102), '.', '-') fecha,
						dbo.fCfdReemplazaSecuenciaDeEspacios(ltrim(rtrim(dbo.fCfdReemplazaCaracteresNI(ud.user_defined_text02))),10) aduana
				  from POP30330	rs				--POP_SerialLotHist [POPRCTNM RCPTLNNM QTYTYPE SLTSQNUM]
					inner JOIN POP10306 ud		--POP_ReceiptUserDefined 			
					on ud.POPRCTNM = rs.POPRCTNM
					inner join IV00101 ma		--iv_itm_mstr
					on ma.ITEMNMBR = rs.ITEMNMBR
				where ma.ITMTRKOP = 2			--serie
					and rs.ITEMNMBR = @ITEMNMBR
					and rs.SERLTNUM = @SERLTNUM
				) ad
			FOR XML raw('cfdi:InformacionAduanera') , type
		)
	end
	return @cncp
end
go

IF (@@Error = 0) PRINT 'Creación exitosa de: fCfdInfoAduaneraXML()'
ELSE PRINT 'Error en la creación de: fCfdInfoAduaneraXML()'
GO

-------------------------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[vwSopLineasTrxVentas]') AND OBJECTPROPERTY(id,N'IsView') = 1)
    DROP view dbo.[vwSopLineasTrxVentas];
GO

create view dbo.vwSopLineasTrxVentas as
--Propósito. Obtiene todas las líneas de facturas de venta SOP y la info aduanera de importación
--			También obtiene la serie/lote del artículo o kits
--Requisito. No incluye descuentos. Todos los descuentos están sumados en la cabecera. Ver: fCfdGeneraDocumentoDeVentaXML[descuento]
--			Si el tipo de artículo es servicio, y el cliente requiere una unidad específica, usar campo COMMENT2.
--			Si no indica una unidad específica, usar: No aplica.
--			Si el tipo de artículo no es servicio, usar la unidad del artículo.
--			Atención ! Si la compañía vende artículos de inventario DEBE usar unidades de medida listadas en el SAT. 
--			De otro modo, hacer conversiones en esta vista.
--21/01/14 JCF Creación fe Chile
--01/07/14 jcf Agrega IVITMTXB, TAXAMNT
--14/08/14 jcf Agrega tipoItem
--			Modifica importe. En caso de no usar lotes, importe = xTNDPRC
--21/1/15 jcf Agrega CNTCPRSN, CUSTNMBR
--
select 'INTERNO' tipoCodigo,
	dt.soptype, dt.sopnumbe, dt.LNITMSEQ, rtrim(dt.ITEMNMBR) ITEMNMBR, 
	cast(ISNULL(sr.serltqty, dt.QUANTITY) as numeric(19,6)) cantidad, dt.QUANTITY, sr.serltqty, left(dt.UOFM, 4) UOFM, 
	case when ma.ITEMTYPE >= 4 then		--4: Misc Charges, 5:servicios, 6:flat fee
			case when rtrim(cs.COMMENT2) = '' then 'No Aplica' 
			else rtrim(cs.COMMENT2) 
			end
	else um.UOFMLONGDESC
	end UOFMsat, 
	sr.SERLTNUM, 
	left(dbo.fCfdReemplazaEspecialesXml(dbo.fCfdReemplazaSecuenciaDeEspacios(dbo.fCfdReemplazaCaracteresNI(dt.ITEMDESC), 0)), 80) ITEMDESC,
	cast (dt.UNITPRCE as numeric(19,6)) UNITPRCE, dt.XTNDPRCE, 
	cast (dt.ORUNTPRC as numeric(19,4)) ORUNTPRC, dt.OXTNDPRC, dt.CMPNTSEQ, 
	case when isnull(sr.SOPNUMBE, '_nulo')='_nulo' then 
			cast(dt.OXTNDPRC as numeric(19,4)) 
		else 
			cast(sr.SERLTQTY * dt.ORUNTPRC as numeric(19,4)) 
	end importeOrig,
	case when isnull(sr.SOPNUMBE, '_nulo')='_nulo' then 
			cast(dt.XTNDPRCE as numeric(19,6)) 
		else 
			cast(sr.SERLTQTY * dt.UNITPRCE as numeric(19,6)) 
	end importe,
	dt.IVITMTXB, dt.TAXAMNT, 
	CASE WHEN dt.IVITMTXB=1 and dt.TAXAMNT=0 and dt.ITMTSHID <> '' --es gravable, el impuesto es cero y tiene plan de impuestos
		then 1	--exento
		else 0 
	end tipoItem,
	isnull(ma.ITMTRKOP, 1) ITMTRKOP,	--3 lote, 2 serie, 1 nada
	dt.mrkdnamt, dt.CNTCPRSN, cb.CUSTNMBR
from SOP30200 cb
inner join SOP30300 dt
	on cb.SOPNUMBE = dt.SOPNUMBE
	and cb.SOPTYPE = dt.SOPTYPE
INNER join rm00101 cs
	on cs.custnmbr = cb.custnmbr
left join iv00101 ma				--iv_itm_mstr
	on ma.ITEMNMBR = dt.ITEMNMBR
left join sop10201 sr				--SOP_Serial_Lot_WORK_HIST
	on sr.SOPNUMBE = dt.SOPNUMBE
	and sr.SOPTYPE = dt.SOPTYPE
	and sr.CMPNTSEQ = dt.CMPNTSEQ
	and sr.LNITMSEQ = dt.LNITMSEQ
outer apply dbo.fCfdUofMSAT(ma.UOMSCHDL, dt.UOFM ) um

go	

IF (@@Error = 0) PRINT 'Creación exitosa de: vwSopLineasTrxVentas'
ELSE PRINT 'Error en la creación de: vwSopLineasTrxVentas'
GO

-------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdParteXML') IS NOT NULL
   DROP FUNCTION dbo.fCfdParteXML
GO

create function dbo.fCfdParteXML(@soptype smallint, @sopnumbe char(21), @LNITMSEQ int)
returns xml 
as
--Propósito. Obtiene info de componentes de kit e info aduanera
--2/5/12 jcf Creación
--
begin
	declare @cncp xml;
	WITH XMLNAMESPACES ('http://www.sat.gob.mx/cfd/3' as "cfdi")
	select @cncp = (
		select dt.cantidad, 
				case when dt.ITMTRKOP = 2 then --tracking option: serie
					dbo.fCfdReemplazaSecuenciaDeEspacios(ltrim(rtrim(dbo.fCfdReemplazaCaracteresNI(dt.SERLTNUM))),10) 
					else null
				end noIdentificacion, 
				dbo.fCfdReemplazaSecuenciaDeEspacios(ltrim(rtrim(dbo.fCfdReemplazaCaracteresNI(dt.ITEMDESC))), 10) descripcion,
				dbo.fCfdInfoAduaneraXML(dt.ITEMNMBR, dt.SERLTNUM)
		from vwSopLineasTrxVentas dt
		where dt.soptype = @soptype
		and dt.sopnumbe = @sopnumbe
		and dt.LNITMSEQ = @LNITMSEQ
		and dt.CMPNTSEQ <> 0		--a nivel componente de kit
		FOR XML raw('cfdi:Parte') , type
	)
	return @cncp
end
go

IF (@@Error = 0) PRINT 'Creación exitosa de: fCfdParteXML()'
ELSE PRINT 'Error en la creación de: fCfdParteXML()'
GO
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdConceptosXML') IS NOT NULL
   DROP FUNCTION dbo.fCfdConceptosXML
GO
create function dbo.fCfdConceptosXML(@p_soptype smallint, @p_sopnumbe varchar(21), @docType char(3))
returns xml 
as
--Propósito. Obtiene las líneas de una factura en formato xml para CFDI
--			Elimina carriage returns, line feeds, tabs, secuencias de espacios y caracteres especiales.
--21/01/14 jcf Creación fe Chile
--30/06/14 jcf Agrega IndExe (exento)
--21/01/15 jcf Agrega DscItem. Caso LAN
--13/03/15 jcf Agrega parámetros a fCfdDetalleExcepcion. Caso bci
--12/10/16 jcf Agrega caso factura exenta
--
begin
	declare @cncp xml;
	WITH XMLNAMESPACES 
		(DEFAULT 'http://www.sii.cl/SiiDte'	)
	select @cncp = (
		select 
			row_number() over (order by Concepto.lnitmseq)	'NroLinDet',
			Concepto.tipoCodigo								'CdgItem/TpoCodigo',
			Concepto.ITEMNMBR								'CdgItem/VlrCodigo',
			case when @docType = '34' then					--factura exenta
				1
			else
				case when Concepto.tipoItem <> 0 then 
					Concepto.tipoItem						--1:exento
				else null									--  afecto
				end
			end 'IndExe',
			dbo.fCfdReemplazaSecuenciaDeEspacios(ltrim(rtrim(dbo.fCfdReemplazaCaracteresNI(Concepto.ITEMDESC))), 10) 'NmbItem', 
			dbo.fCfdReemplazaSecuenciaDeEspacios(ltrim(rtrim(dbo.fCfdReemplazaCaracteresNI(
									dbo.fCfdDetalleExcepcion(Concepto.CNTCPRSN, Concepto.CUSTNMBR, Concepto.soptype, Concepto.sopnumbe)
													))), 10) 'DscItem',
			case when Concepto.UNITPRCE > 0 then 
				Concepto.cantidad else null end				'QtyItem', 
			case when Concepto.cantidad * Concepto.UNITPRCE <> 0 then 
				Concepto.UNITPRCE else null end				'PrcItem',
			CASE WHEN Concepto.mrkdnamt <> 0 then 
				cast(round(100 * Concepto.mrkdnamt / CASE WHEN Concepto.UNITPRCE > 0 THEN Concepto.UNITPRCE else 1 END, 2) as numeric(5,2)) 
			else null
			end 'DescuentoPct',
			CASE WHEN Concepto.mrkdnamt  <> 0 then 
				cast(round(Concepto.mrkdnamt * Concepto.quantity, 0) as numeric(18)) 
			else null
			end 'DescuentoMonto',
			cast(Concepto.importe as numeric(18))			'MontoItem',
			case when Concepto.ITMTRKOP = 2 then							--tracking option: serie
				dbo.fCfdReemplazaSecuenciaDeEspacios(ltrim(rtrim(dbo.fCfdReemplazaCaracteresNI(Concepto.SERLTNUM))),10) 
				else null
			end 'noIdentificacion',
			dbo.fCfdInfoAduaneraXML(Concepto.ITEMNMBR, Concepto.SERLTNUM),
			dbo.fCfdParteXML(Concepto.soptype, Concepto.sopnumbe, Concepto.LNITMSEQ) 
		from vwSopLineasTrxVentas Concepto
		where CMPNTSEQ = 0																			--a nivel kit
		and Concepto.soptype = @p_soptype
		and Concepto.sopnumbe = @p_sopnumbe
		FOR XML path('Detalle'), type
	)
	--SELECT @cncp
	
	return @cncp
end
go

IF (@@Error = 0) PRINT 'Creación exitosa de: fCfdConceptosXML()'
ELSE PRINT 'Error en la creación de: fCfdConceptosXML()'
GO
-------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdConceptosSumaExentos') IS NOT NULL
   DROP FUNCTION dbo.fCfdConceptosSumaExentos
GO
create function dbo.fCfdConceptosSumaExentos(@p_soptype smallint, @p_sopnumbe varchar(21))
returns TABLE 
as
--Propósito. Obtiene la suma de los items exentos
--02/07/14 jcf Creación
--
return (
		select sum(Concepto.importe)	importe
		from vwSopLineasTrxVentas Concepto
		where Concepto.CMPNTSEQ = 0						--a nivel kit
		and Concepto.soptype = @p_soptype
		and Concepto.sopnumbe = @p_sopnumbe
		and Concepto.tipoItem = 1						--exento
		and Concepto.UNITPRCE > 0
	)
go

IF (@@Error = 0) PRINT 'Creación exitosa de: fCfdConceptosSumaExentos()'
ELSE PRINT 'Error en la creación de: fCfdConceptosSumaExentos()'
GO
-------------------------------------------------------------------------------------------------------

IF OBJECT_ID ('dbo.fCfdDescuentosXML') IS NOT NULL
   DROP FUNCTION dbo.fCfdDescuentosXML
GO

create function dbo.fCfdDescuentosXML(@descuentoNeto numeric(19, 5), @descuentoExento numeric(19, 5), @trdispct numeric(16, 2), @descuento numeric(19,5), @docType char(3))
returns xml 
as
--Propósito. Obtiene los descuentos globales de una factura en formato xml
--24/01/14 jcf Creación fe Chile
--02/07/14 jcf Agrega descuento de ítem exento
--05/10/15 jcf Corrige sección item exento
--12/10/16 jcf Agrega caso factura exenta
--
begin
	declare @cncp xml;
	if @docType = '34'	--factura exenta
	begin
			WITH XMLNAMESPACES 
			(DEFAULT 'http://www.sii.cl/SiiDte'	)
			select @cncp = (
					select 
						1		'DscRcgGlobal/NroLinDR',						
						'D'		'DscRcgGlobal/TpoMov',
						case when @trdispct >0 then '%' else '$' end 'DscRcgGlobal/TpoValor', 
						case when @trdispct >0 then @trdispct else cast(round(@descuento, 2) as numeric(16,2)) end 'DscRcgGlobal/ValorDR',
						1		'DscRcgGlobal/IndExeDR'
				FOR XML path(''), type
			)
	end
	else
	--descuento para items afectos y exentos
	begin
			WITH XMLNAMESPACES 
				(DEFAULT 'http://www.sii.cl/SiiDte'	)
			select @cncp = (
				select row_number() over (order by d.TpoValor) NroLinDR,	
					d.TpoMov,		--'TpoMov',
					d.TpoValor,		--'TpoValor', 
					d.ValorDR,		--'ValorDR',
					d.IndExeDR		--'IndExeDR'
				from (
					select top 1 --item afecto
						'D'		TpoMov,
						case when @trdispct >0 then '%' else '$' end TpoValor,
						case when @trdispct >0 then @trdispct else cast(round(@descuentoNeto, 2) as numeric(16,2)) end ValorDR,
						null	IndExeDR
					from sy01200	--dummy
					where @descuentoNeto > 0
					union all
					select top 1 --item exento
						'D'		TpoMov,				
						case when @trdispct >0 then '%' else '$' end TpoValor,			
						case when @trdispct >0 then @trdispct else cast(round(@descuentoExento, 2) as numeric(16,2)) end ValorDR,
						1		IndExeDR
					from sy01200	--dummy
					where @descuentoExento > 0
					) d
				FOR XML path('DscRcgGlobal'), type
			)
	end
	-----------------------------
	--else			--descuento sólo para items afectos
	--begin
	--	WITH XMLNAMESPACES 
	--		(DEFAULT 'http://www.sii.cl/SiiDte'	)
	--	select @cncp = (
	--			select 
	--				1		'DscRcgGlobal/NroLinDR',						
	--				'D'		'DscRcgGlobal/TpoMov',
	--				case when @trdispct >0 then '%' else '$' end 'DscRcgGlobal/TpoValor', 
	--				case when @trdispct >0 then @trdispct else cast(round(@descuentoNeto, 2) as numeric(16,2)) end 'DscRcgGlobal/ValorDR',
	--				null	'DscRcgGlobal/IndExeDR'
	--		FOR XML path(''), type
	--	)
	--end
	--SELECT @cncp
	return @cncp
end
go

IF (@@Error = 0) PRINT 'Creación exitosa de: fCfdDescuentosXML()'
ELSE PRINT 'Error en la creación de: fCfdDescuentosXML()'
GO

--------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdImpuestosXML') IS NOT NULL
begin
   DROP FUNCTION dbo.fCfdImpuestosXML
   print 'función fCfdImpuestosXML eliminada'
end
GO

create function dbo.fCfdImpuestosXML(@p_soptype smallint, @p_sopnumbe varchar(21), @p_impuestos varchar(150))
returns xml 
as
begin
	declare @impu xml;
	WITH XMLNAMESPACES ('http://www.sat.gob.mx/cfd/3' as "cfdi")
	select @impu = (
		select 	
			case when charindex(RTRIM(imp.taxdtlid), @p_impuestos) > 0 then 'IVA' else '' end impuesto,
			dbo.fCfdObtienePorcentajeImpuesto (imp.taxdtlid) tasa,
			imp.orslstax importe
		from sop10105 imp	--sop_tax_work_hist
 		where imp.SOPTYPE = @p_soptype
		  and imp.SOPNUMBE = @p_sopnumbe
		  and imp.LNITMSEQ = 0
		  and charindex(RTRIM(imp.taxdtlid), @p_impuestos) > 0
		FOR XML raw('cfdi:Traslado'), type
		)
	return @impu
end
go

IF (@@Error = 0) PRINT 'Creación exitosa de la función: fCfdImpuestosXML()'
ELSE PRINT 'Error en la creación de la función: fCfdImpuestosXML()'
GO
--------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdImpuestos') IS NOT NULL
begin
   DROP FUNCTION dbo.fCfdImpuestos
   print 'función fCfdImpuestos eliminada'
end
GO

create function dbo.fCfdImpuestos(@p_soptype smallint, @p_sopnumbe varchar(21), @p_impuestos varchar(150))
returns TABLE 
as
--begin
	RETURN (
		select 	
			case when charindex(RTRIM(imp.taxdtlid), @p_impuestos) > 0 then 'IVA' else '' end impuesto,
			dbo.fCfdObtienePorcentajeImpuesto (imp.taxdtlid) tasa,
			imp.staxamnt, imp.orslstax importe, --impuesto
			imp.tdttxsls, imp.ortxsls			--total venta gravable
		from sop10105 imp						--sop_tax_work_hist
 		where imp.SOPTYPE = @p_soptype
		  and imp.SOPNUMBE = @p_sopnumbe
		  and imp.LNITMSEQ = 0
		  and charindex(RTRIM(imp.taxdtlid), @p_impuestos) > 0
		  )
--end
go

IF (@@Error = 0) PRINT 'Creación exitosa de la función: fCfdImpuestos()'
ELSE PRINT 'Error en la creación de la función: fCfdImpuestos()'
GO

-------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdGeneraDocumentoDeVentaXML') IS NOT NULL
   DROP FUNCTION dbo.fCfdGeneraDocumentoDeVentaXML
GO

create function dbo.fCfdGeneraDocumentoDeVentaXML (@soptype smallint, @sopnumbe varchar(21))
returns xml 
as
--Propósito. Elabora un comprobante xml para factura electrónica
--Requisitos. -
--22/01/14 jcf Creación fe Chile
--30/06/14 jcf Agrega parámetros sopUserTab01 (código de referencia)
--			También ver 140701 Cálculo de monto neto y exento con descuento en factura electrónica.xlsx
--25/08/14 jcf Usa funciones para obtener exento y neto
--21/01/15 jcf Modifica parámetros de ReferenciaXml
--04/02/15 jcf Agrega contacto
--05/10/15 jcf Corrige monto neto, iva, tasa iva, para factura exenta
--25/08/16 jcf Agrega CorreoRecep
--12/10/16 jcf Agrega caso factura exenta
--
begin
	declare @cfd xml;
	WITH XMLNAMESPACES
	(DEFAULT 'http://www.sii.cl/SiiDte'	)
	select @cfd = 
	(
	select 
		emi.[version]										'@version',
		'D'+rtrim(replace(tv.sopnumbe, ' ', ''))			'Documento/@ID',
		tv.docType											'Documento/Encabezado/IdDoc/TipoDTE',
		tv.folio											'Documento/Encabezado/IdDoc/Folio',
		tv.fecha											'Documento/Encabezado/IdDoc/FchEmis',
		--'1'													'Documento/Encabezado/IdDoc/FmaPago',		--1:contado, 2:crédito, 3:sin costo
		STUFF(emi.idImpuesto, len(emi.idImpuesto), 0, '-')	'Documento/Encabezado/Emisor/RUTEmisor',	
		emi.RazonSocial										'Documento/Encabezado/Emisor/RznSoc',
		emi.GiroEmpresa										'Documento/Encabezado/Emisor/GiroEmis',
		emi.CodActivEconom									'Documento/Encabezado/Emisor/Acteco',
		emi.calle											'Documento/Encabezado/Emisor/DirOrigen',
		emi.colonia											'Documento/Encabezado/Emisor/CmnaOrigen',
		emi.ciudad											'Documento/Encabezado/Emisor/CiudadOrigen',
		tv.idImpuestoCliente								'Documento/Encabezado/Receptor/RUTRecep',
		tv.RazonSocial										'Documento/Encabezado/Receptor/RznSocRecep', 
		left(tv.GiroEmpresa, 40)							'Documento/Encabezado/Receptor/GiroRecep', 
		case when tv.contacto = '' then null else tv.contacto end 'Documento/Encabezado/Receptor/Contacto', 
		case when tv.EmailToAddress = '' then null else tv.EmailToAddress end 'Documento/Encabezado/Receptor/CorreoRecep', 
		tv.address1											'Documento/Encabezado/Receptor/DirRecep',
		tv.[STATE]											'Documento/Encabezado/Receptor/CmnaRecep',
		tv.city												'Documento/Encabezado/Receptor/CiudadRecep',
		dbo.fCfdVentasObtieneNeto(tv.subtotal, tv.descuento, ex.importe, tv.docType) 'Documento/Encabezado/Totales/MntNeto',
		--case when tv.docType = '34' then					--doc no afecto o exento no requiere monto neto
		--	null
		--else
		--	case when tv.subtotal <> 0 then 
		--		dbo.fCfdVentasObtieneNeto(tv.subtotal, tv.descuento, ex.importe)
		--	else 0
		--	end
		--end													
		dbo.fCfdVentasObtieneExento(tv.subtotal, tv.descuento, ex.importe, tv.docType) 'Documento/Encabezado/Totales/MntExe',
		--case when tv.docType = '34' then					--doc no afecto o exento
		--	case when tv.subtotal <> 0 then 
		--		dbo.fCfdVentasObtieneExento(tv.subtotal, tv.descuento, tv.subtotal)
		--	else 
		--		0
		--	end
		--else
		--	case when tv.subtotal <> 0 then 
		--		dbo.fCfdVentasObtieneExento(tv.subtotal, tv.descuento, ex.importe)
		--	else
		--		null
		--	end
		--end													
		case when tv.docType = '34' then					--exento
			null
		else
			cast(im.tasa as numeric(7,2))						
		end													'Documento/Encabezado/Totales/TasaIVA',
		case when tv.docType = '34' then					--exento
			null
		else
			cast(tv.impuesto as numeric(18))					
		end													'Documento/Encabezado/Totales/IVA',
		cast(tv.total as numeric(18))						'Documento/Encabezado/Totales/MntTotal',

		dbo.fCfdConceptosXML(tv.soptype, tv.sopnumbe, tv.docType)		'Documento',

		case when tv.descuento <> 0 and tv.subtotal <> 0 then 
			--inicio SET PRUEBAS descuento global o por línea sólo para afectos 
			--dbo.fCfdDescuentosXML(	(tv.subtotal - isnull(ex.importe, 0)) * .18,	--descuento del neto   (descuento global items afectos 18%)
			--						0,												--descuento del exento
			--						18.00)
			--fin SET PRUEBAS
			--descuento global o por línea para afectos o exentos
			dbo.fCfdDescuentosXML(	(tv.subtotal - isnull(ex.importe, 0)) * (tv.descuento/tv.subtotal),	--descuento del neto
									isnull(ex.importe, 0) * (tv.descuento/tv.subtotal),					--descuento del exento
									tv.trdispct/100, tv.descuento, tv.docType)
		else null 
		end	'Documento',
		
		dbo.fCfdReferenciaXML(tv.soptype, tv.rmdtypal, tv.sopnumbe, tv.docdate, tv.sopUserTab01, tv.refrence, tv.sopUserDef2, tv.usrdat02, tv.total, rtrim(tv.CUSTNMBR), tv.cstponbr)	'Documento'

	from vwSopTransaccionesVenta tv
		cross join dbo.fCfdEmisor() emi
		outer apply dbo.fCfdImpuestos(tv.soptype, tv.sopnumbe, emi.impuestos) im
		outer apply dbo.fCfdConceptosSumaExentos(tv.soptype, tv.sopnumbe) ex
	where tv.sopnumbe =	@sopnumbe		
	and tv.soptype = @soptype
	FOR XML path('DTE'), type
	)
	return @cfd;
end
go

IF (@@Error = 0) PRINT 'Creación exitosa de la función: fCfdGeneraDocumentoDeVentaXML ()'
ELSE PRINT 'Error en la creación de la función: fCfdGeneraDocumentoDeVentaXML ()'
GO
-----------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[vwCfdTransaccionesDeVenta]') AND OBJECTPROPERTY(id,N'IsView') = 1)
    DROP view dbo.[vwCfdTransaccionesDeVenta];
GO

create view dbo.vwCfdTransaccionesDeVenta as
--Propósito. Lista todos los documentos de venta: facturas y notas de crédito. 
--			Si el documento no fue emitido, genera el comprobante xml en el campo comprobanteXml
--Usado por. App Factura digital (doodads)
--Requisitos. El estado "no emitido" indica que no se ha emitido el archivo xml pero que está listo para ser generado.
--			El estado "inconsistente" indica que existe un problema en el folio o certificado, por tanto no puede ser generado.
--			El estado "emitido" indica que la factura ha sido procesada.
--16/01/14 jcf Creación fe Chile
--30/6/14 jcf Modifica campo sopUserDef1
--10/9/14 jcf Cuando está en lote, el estado actual es cero
--30/9/14 jcf Agrega dos dígitos a estadoActual
--16/12/14 jcf Corrige mensaje cuando factura está Anulada
--10/7/17 jcf Agrega idexterno
--
select tv.estadoContabilizado, tv.soptype, tv.docid, tv.sopnumbe, tv.fechahora, 
	tv.CUSTNMBR, tv.RazonSocial nombreCliente, tv.idImpuestoCliente, tv.total, tv.voidstts, 
	
	isnull(lf.estado, isnull(fv.estado, 'inconsistente')) estado,
			
	tv.doctype, tv.nombreTipoDoc, tv.folio, tv.fecha,
	
	case when isnull(lf.estado, isnull(fv.estado, 'inconsistente')) = 'inconsistente' 
		then 'folio inconsistente'
		else ISNULL(lf.mensaje, tv.estadoContabilizado)
	end mensaje,
	
	case when isnull(lf.noAprobacion, '11') in ('11', '10')				--no emitido, emitido
		then dbo.fCfdGeneraDocumentoDeVentaXML (tv.soptype, tv.sopnumbe) 
		--else isnull(lf.archivoXML, cast('' as xml))
		else cast('' as xml)
	end comprobanteXml,
	
	tv.RutRepLegal idClienteRepLegal, 
	emi.nroResol, emi.fchResol,
	emi.idImpuesto, isnull(fv.ruta_codigo_autorizacion, '_noexiste') rutaCerts, emi.rutaXml, 
	
	case when tv.estadoContabilizado = 'en lote' then
		'00000000000000'
	else
		isnull(lf.estadoActual, '00000000000100') 
	end estadoActual, 
	
	case when isnull(lf.mensajeEA, '@no_existe') = '@no_existe' then
		tv.estadoContabilizado
	else
		lf.mensajeEA + ' id:' + isnull(lf.idexterno, '')
	end + 
	case when tv.voidstts = 0 then '' else ' ANULADO.' end mensajeEA,

	case when tv.estadoContabilizado = 'en lote' then
		'0'
	else
		isnull(lf.noAprobacion, '11')		--indicar el índice del estado inicial
	end idxSingleStatus,
	tv.sopUserDef1 USERDEF1
from vwSopTransaccionesVenta tv
	cross join dbo.fCfdEmisor() emi
	outer apply dbo.fCfdFolioVigente(tv.soptype, tv.docid, dbo.fCfdObtieneFolio(tv.sopnumbe, '-')) fv	
	left join cfdlogfacturaxml lf
		on lf.soptype = tv.SOPTYPE
		and lf.sopnumbe = tv.sopnumbe
		and lf.estado = 'emitido'
go

IF (@@Error = 0) PRINT 'Creación exitosa de la vista: vwCfdTransaccionesDeVenta'
ELSE PRINT 'Error en la creación de la vista: vwCfdTransaccionesDeVenta'
GO

-----------------------------------------------------------------------------------------
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[vwCfdDocumentosAImprimir]') AND OBJECTPROPERTY(id,N'IsView') = 1)
    DROP view dbo.[vwCfdDocumentosAImprimir];
GO

create view dbo.vwCfdDocumentosAImprimir as
--Propósito. Lista los documentos de venta que están listos para imprimirse: facturas y notas de crédito. 
--05/02/14 jcf Creación Fe Chile
--27/09/14 jcf Agrega FchEmis
--12/10/16 jcf Agrega caso de SET de pruebas
--
select tv.soptype, tv.docid, tv.docType, upper(tv.nombreTipoDoc) nombreTipoDoc, tv.sopnumbe, 
	tv.idImpuestoCliente rfcReceptor, tv.nombreCliente, tv.total, tv.sopUserDef1 USERDEF1, 
	dx.Folio, convert (datetime, dx.FchEmis) FchEmis, dx.RznSocRecep, dx.DirRecep, dx.GiroRecep, dx.RUTRecep, dx.CmnaRecep, dx.CiudadRecep,

	--en caso de set de pruebas ignorar la primera referencia
	case when isnull(dx.TpoDocRef, '') = 'SET' THEN isnull(dx.TpoDocRef2, '')		else isnull(dx.TpoDocRef, '')		end TpoDocRef, 
	case when isnull(dx.TpoDocRef, '') = 'SET' THEN	dx.FolioRef2					else dx.FolioRef					end FolioRef, 
	case when isnull(dx.TpoDocRef, '') = 'SET' THEN	convert (datetime, dx.FchRef2)	else convert (datetime, dx.FchRef)	end FchRef, 
	
	--isnull(dx.TpoDocRef, '') TpoDocRef, 
	--dx.FolioRef,  
	--convert (datetime, dx.FchRef) FchRef, 

	cde.dscriptn nombreTipoDocRef,
	emi.nroResol, CONVERT(datetime, emi.fchResol) fchResol,
	'file://'+replace(emi.rutaxml, '\', '/')+'cbb/' + replace(RIGHT( lf.mensaje, CHARINDEX( '\', REVERSE( lf.mensaje ) + '\' ) - 1 ), '.xml', '.png') rutaYNomArchivo, 
	emi.rutaxml + 'cbb\' + replace(RIGHT( lf.mensaje, CHARINDEX( '\', REVERSE( lf.mensaje ) + '\' ) - 1 ), '.xml', '.png') rutaYNomArchivoNet
from vwSopTransaccionesVenta tv
	inner join cfdlogfacturaxml lf
		on lf.soptype = tv.SOPTYPE
		and lf.sopnumbe = tv.sopnumbe
	outer apply dbo.fCfdEmisor() emi
	outer apply dbo.fCfdFolioVigente(tv.soptype, tv.docid, dbo.fCfdObtieneFolio(tv.sopnumbe, '-')) fv
	outer apply dbo.fCfdDatosXmlParaImpresion(lf.archivoXML) dx
    left join loch0002 cde			--loch_descripcion_documentos
		on cde.lochdoccod = case when isnull(dx.TpoDocRef, '') = 'SET' THEN dx.TpoDocRef2 else dx.TpoDocRef end
       and cde.module1 = 2			--ventas
where lf.estado = 'emitido'

go
IF (@@Error = 0) PRINT 'Creación exitosa de la vista: vwCfdDocumentosAImprimir  '
ELSE PRINT 'Error en la creación de la vista: vwCfdDocumentosAImprimir '
GO
-----------------------------------------------------------------------------------------

-- FIN DE SCRIPT ***********************************************

--select * from vwCfdDocumentosAImprimir 
--where sopnumbe in 		('33-0000238', '33-0000239', '33-0000240', '33-0000241', '61-0000150', '61-0000151', '61-0000152', '56-0000150')


