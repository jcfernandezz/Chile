--LIBRO DE COMPRAS ELECTRONICO
--Proyectos:     GETTY
--Propósito:     Crea vistas y funciones para la generación del libro de compras para Chile
--Pre-condición: Localización chilena instalada, tabla cllc0002 Maestro de proveedores y clientes
--Utilizado por: Reportes crystal del libro de compras de Chile
--				 Reporting services del libro de compras de Chile
-----------------------------------------------------------------------
IF OBJECT_ID ('dbo.vwIecvDetalleComprasAP') IS NOT NULL
   DROP view dbo.vwIecvDetalleComprasAP
GO

create VIEW dbo.vwIecvDetalleComprasAP
--Propósito. Detalle de transacciones de Payables Management abiertas e históricas
--21/7/14 JCF Creación
--17/3/15 jcf Corrige exentoCalculado
--
AS

	--Transacciones parcialmente aplicadas (abiertas) de Payables Management
       SELECT trx.pstgdate, trx.docdate, trx.DOCTYPE, 
              trx.VCHRNMBR, trx.docnumbr, trx.trxsorce, trx.VOIDED, trx.VENDORID, trx.pordnmbr, 
              case trx.voided when 0 then dbo.fCfdReemplazaEspecialesXml(dbo.fCfdEsVacio(dbo.fCfdReemplazaSecuenciaDeEspacios(dbo.fCfdReemplazaCaracteresNI(cl.razonSocial), 10))) else '(*) NULA' end razonSocial,
              case trx.voided when 0 then STUFF(rtrim(cl.rutClieProvee), len(rtrim(cl.rutClieProvee)), 0, '-')
                               else '' 
               end rutClieProvee, rtrim(cl.rutClieProvee) rut,
			case trx.voided when 0 then
				case when exe.tdttxpur is null then 
						trx.PRCHAMNT				-- exento = subtotal - (cualquier iva) + retención
						- case when isnull(iva.taxamnt, 0) <> 0 then isnull(iva.tdttxpur, 0) else 0 end
						- case when isnull(iuc.taxamnt, 0) <> 0 then isnull(iuc.tdttxpur, 0) else 0 end
						- case when isnull(inr.taxamnt, 0) <> 0 then isnull(inr.tdttxpur, 0) else 0 end 
						+ case when isnull(ri.taxamnt, 0) <> 0 then isnull(ri.tdttxpur, 0) else 0 end
				else	isnull(exe.tdttxpur, 0) 
				end 
			else 0 
			end exentoCalculado,
            case trx.voided when 0 then isnull(exe.tdttxpur, 0) else 0 end exe_taxableAmnt, 
            case trx.voided when 0 then isnull(iva.taxamnt, 0) else 0 end iva_taxamnt, --monto impuesto iva
            case trx.voided when 0 then isnull(iuc.taxamnt, 0) else 0 end iuc_taxamnt, 
            case trx.voided when 0 then isnull(inr.taxamnt, 0) else 0 end inr_taxamnt, 
            case trx.voided when 0 then isnull(ri.taxamnt, 0) else 0 end ri_taxamnt, 
            case trx.voided when 0 then trx.DOCAMNT else 0 end docamnt, 
			trx.SHIPMTHD, trx.bchsourc, rcp.poprctnm, pr.param2 iva_TAXDTLID, ri.TAXDTLID ri_TAXDTLID
         from PM20000 trx --pm_transaction_open
					left outer join
				POP30300 rcp	--POP_receiptHist
					on rcp.POPTYPE = 3	--recibo/factura
					AND rcp.VCHRNMBR = trx.VCHRNMBR
					and rcp.VENDORID = trx.VENDORID
					and rcp.VNDDOCNM = trx.docnumbr
                  left outer join 
              cllc0002 cl  --maestro de proveedores localización chilena
                    on cl.custvndr = trx.vendorid
                   and cl.tipClieProvee = 2 --proveedor

		      cross apply dbo.fLcLvParametros('IMCEXENTO', 'IMCIVA', 'IMCUSOCOMUN', 'IMCNORECUP', 'IMCRETEIVA', 'na') pr
				   
			  outer apply dbo.fLcPmTaxWork(trx.VCHRNMBR, trx.DOCTYPE, pr.param1) exe	--exento

			  outer apply dbo.fLcPmTaxWork(trx.VCHRNMBR, trx.DOCTYPE, pr.param2) iva	--iva

			  outer apply dbo.fLcPmTaxWork(trx.VCHRNMBR, trx.DOCTYPE, pr.param3) iuc	--iva uso común

			  outer apply dbo.fLcPmTaxWork(trx.VCHRNMBR, trx.DOCTYPE, pr.param4) inr	--iva no recuperable

			  outer apply dbo.fLcPmTaxWork(trx.VCHRNMBR, trx.DOCTYPE, pr.param5) ri		--retención de iva

        UNION all
        
       --Transacciones totalmente aplicadas (históricas) de Payables Management
       SELECT trx.pstgdate, trx.docdate, trx.DOCTYPE, 
              trx.VCHRNMBR, trx.docnumbr, trx.trxsorce, trx.VOIDED, trx.VENDORID, trx.pordnmbr, 
              case trx.voided when 0 then dbo.fCfdReemplazaEspecialesXml(dbo.fCfdEsVacio(dbo.fCfdReemplazaSecuenciaDeEspacios(dbo.fCfdReemplazaCaracteresNI(cl.razonSocial), 10))) else '(*) NULA' end razonSocial,
              case trx.voided when 0 then STUFF(rtrim(cl.rutClieProvee), len(rtrim(cl.rutClieProvee)), 0, '-')
                               else '' 
               end rutClieProvee, rtrim(cl.rutClieProvee) rut,
			case trx.voided when 0 then
				case when exe.tdttxpur is null then 
						trx.PRCHAMNT				-- exento = subtotal - (cualquier iva) + retención
						- case when isnull(iva.taxamnt, 0) <> 0 then isnull(iva.tdttxpur, 0) else 0 end
						- case when isnull(iuc.taxamnt, 0) <> 0 then isnull(iuc.tdttxpur, 0) else 0 end
						- case when isnull(inr.taxamnt, 0) <> 0 then isnull(inr.tdttxpur, 0) else 0 end 
						+ case when isnull(ri.taxamnt, 0) <> 0 then isnull(ri.tdttxpur, 0) else 0 end
				else	isnull(exe.tdttxpur, 0) 
				end 
			else 0 
			end exentoCalculado,
            case trx.voided when 0 then isnull(exe.tdttxpur, 0) else 0 end exe_taxableAmnt, 
            case trx.voided when 0 then isnull(iva.taxamnt, 0) else 0 end iva_taxamnt, --monto impuesto iva
            case trx.voided when 0 then isnull(iuc.taxamnt, 0) else 0 end iuc_taxamnt, 
            case trx.voided when 0 then isnull(inr.taxamnt, 0) else 0 end inr_taxamnt, 
            case trx.voided when 0 then isnull(ri.taxamnt, 0) else 0 end ri_taxamnt, 
            case trx.voided when 0 then trx.DOCAMNT else 0 end docamnt, 

			trx.SHIPMTHD, trx.bchsourc, rcp.poprctnm, pr.param2 iva_TAXDTLID, ri.TAXDTLID ri_TAXDTLID
         from PM30200 trx --pm_paid_transaction_hist
					left outer join
				POP30300 rcp	--POP_receiptHist
					on rcp.POPTYPE = 3	--recibo/factura
					AND rcp.VCHRNMBR = trx.VCHRNMBR
					and rcp.VENDORID = trx.VENDORID
					and rcp.VNDDOCNM = trx.docnumbr
                 left outer join 
              cllc0002 cl  --maestro de proveedores localización chilena
                    on cl.custvndr = trx.vendorid
                   and cl.tipClieProvee = 2 --proveedor

		      cross apply dbo.fLcLvParametros('IMCEXENTO', 'IMCIVA', 'IMCUSOCOMUN', 'IMCNORECUP', 'IMCRETEIVA', 'na') pr
				   
			  outer apply dbo.fLcPmTaxHist(trx.VCHRNMBR, trx.DOCTYPE, pr.param1) exe	--exento

			  outer apply dbo.fLcPmTaxHist(trx.VCHRNMBR, trx.DOCTYPE, pr.param2) iva	--iva

			  outer apply dbo.fLcPmTaxHist(trx.VCHRNMBR, trx.DOCTYPE, pr.param3) iuc	--iva uso común

			  outer apply dbo.fLcPmTaxHist(trx.VCHRNMBR, trx.DOCTYPE, pr.param4) inr	--iva no recuperable

			  outer apply dbo.fLcPmTaxHist(trx.VCHRNMBR, trx.DOCTYPE, pr.param5) ri		--retención de iva
go 

IF (@@Error = 0) PRINT 'Creación exitosa de la vista: vwIecvDetalleComprasAP'
ELSE PRINT 'Error en la creación de la vista: vwIecvDetalleComprasAP'
GO

------------------------------------------------------------------------------------------------------------

-- INICIO PRUEBAS**********************************
--Libro de Compras detallado (1261)
-- select --dbo.fnFcLCCorrelativo ( convert(varchar(2),a.DOCTYPE) + convert(char(8), a.docdate, 112)+ a.VCHRNMBR, year (a.pstgdate)*100 + month(a.pstgdate)) secuencial, 
--        a.pstgdate, a.DOCDATE, a.DOCTYPE, a.tipoDoc, a.VCHRNMBR, a.docnumbr, a.VOIDED, a.VENDORID, a.razonSocial, a.rutClieProvee, 
--        case a.doctype when 5 then (-1)*(a.total-a.iva-a.exento) else a.total-a.iva-a.exento end afecto,
--        case a.doctype when 5 then (-1)*a.exento else a.exento end exento,
--        case a.doctype when 5 then (-1)*a.iva else a.iva end iva,
--        case a.doctype when 5 then (-1)*a.total else a.total end total,
--        0 otros
--   from vwIecvDetalleComprasAP a
--  where --month(a.pstgdate) = 10
--    year (a.pstgdate) = 2005
--    and a.doctype in (1,5) --1 Factura, 5 Nota de Crédito
-- --   and docnumbr like '%243882'--in ('000010' , '1')
-- order by 4, 3, 6
-- 
-- --Resumen de libro de compras
-- select convert(char(10), pstgdate, 112), tipoDoc, count(docnumbr) num, 
--        sum(case doctype when 5 then (-1)*(total-iva-exento) else total-iva-exento end ) afecto,
--        sum(case doctype when 5 then (-1)*exento else exento end) exento,
--        sum(case doctype when 5 then (-1)*iva else iva end) iva,
--        sum(case doctype when 5 then (-1)*total else total end) total,
--        0 otros
--   from vwIecvDetalleComprasAP
--  where --convert(char(10), pstgdate, 112) = '20050223'
-- --   and pstgdate = '1/31/2003'
--    doctype in (1,5) --1 Factura, 5 Nota de Crédito
--  group by convert(char(10), pstgdate, 112), tipoDoc
--  order by 1
-- 
-- 
----------------------------------------------------
--select *
--from cllc0002 
----UPDATE cllc0002 SET RUTCLIEPROVEE = UPPER(RUTCLIEPROVEE)
--where CUSTVNDR = '77332610K'
-- --TIPCLIEPROVEE = 2
--rutclieprovee like '7733%'
