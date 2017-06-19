-------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdGetSegmentoExcepcion') IS NOT NULL
   DROP FUNCTION dbo.fCfdGetSegmentoExcepcion
GO

create function dbo.fCfdGetSegmentoExcepcion(@numSegmento smallint, @texto varchar(21))
returns varchar(18)
--Propósito. Obtiene el segundo segmento de una cadena separada por @separador 
--21/1/15 jcf Creación 
--
begin
		return 
			case when @numSegmento = 2 then
					case when patindex('%/%', @texto) > 0 then 
							 right(@texto, len(@texto)-patindex('%/%', @texto))
						when patindex('%-%', @texto) > 0 then
							right(@texto, len(@texto)-patindex('%-%', @texto))
					else '0'
					end
				when @numSegmento = 1 then
					case when charindex('/', @texto, 1) > 0 then
						rtrim(replace(left(@texto, charindex('/', @texto, 1)), '/', ''))
					when charindex('-', @texto, 1) > 0 then
						rtrim(replace(left(@texto, charindex('-', @texto, 1)), '-', ''))
					else '0'
					end
			end
					
end
go

IF (@@Error = 0) PRINT 'Creación exitosa de: fCfdGetSegmentoExcepcion()'
ELSE PRINT 'Error en la creación de: fCfdGetSegmentoExcepcion()'
GO

--------------------------------------------------------------------------------------------------------
--PARA SET DE PRUEBAS
			--CASO MONTO = 0
			--inicio set de pruebas
			--select 1 NroLinRef, 'SET' TpoDocRef, dbo.fCfdObtieneFolio(rtrim(@p_sopnumbe), '-') FolioRef,
			--	replace(convert(varchar(10), @p_docdate, 102), '.', '-') FchRef, '2' CodRef, 
			--	'CASO ' + CASE rtrim(@p_sopnumbe) 
			--								WHEN '33-0000238' then '309969-1'
			--								WHEN '33-0000239' then '309969-2'
			--								WHEN '33-0000240' then '309969-3'
			--								WHEN '33-0000241' then '309969-4' 
			--								WHEN '61-0000150' then '309969-5'
			--								WHEN '61-0000151' then '309969-6'
			--								WHEN '61-0000152' then '309969-7'
			--								WHEN '56-0000150' then '309969-8'
			--				else null
			--	end	RazonRef
			--union all
			--fin set de pruebas
			--cambiar el NroLinref a 1

			--CASO NC/ND
				--inicio set de pruebas
				--select 1 NroLinRef, 'SET' TpoDocRef, @p_sopnumbe apdcnm,
				--convert(varchar(10), @p_docdate, 102) apdcdt, '2' CodRef, 
				--'CASO ' + CASE rtrim(@p_sopnumbe) 
				--							WHEN '33-0000238' then '309969-1'
				--							WHEN '33-0000239' then '309969-2'
				--							WHEN '33-0000240' then '309969-3'
				--							WHEN '33-0000241' then '309969-4' 
				--							WHEN '61-0000150' then '309969-5'
				--							WHEN '61-0000151' then '309969-6'
				--							WHEN '61-0000152' then '309969-7'
				--							WHEN '56-0000150' then '309969-8'
				--			else null
				--end	RazonRef
				--union all
				--fin set de pruebas
				--cambiar el NroLinref a 1

--------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdReferencia') IS NOT NULL
   DROP FUNCTION dbo.fCfdReferencia
GO

ALTER function dbo.fCfdReferencia(@soptype smallint, @rmdtypal smallint, @p_sopnumbe varchar(21), @p_docdate datetime,
									@USRTAB01 varchar(21), @REFRENCE VARCHAR(31), @USERDEF2 varchar(21), @USRDAT02 datetime, @DOCAMNT numeric(21,5),
									@cliente varchar(15), @CSTPONBR VARCHAR(21))
returns table
as
--Propósito. Obtiene la referencia de una factura, NC o ND. Factura electrónica Chile
--20/1/15 jcf Creación
--			Excepciones: 
--						CAROZZI - 150108 Gchile Emisión Facturas Electronicas Carozzi.by A Gómez.DOC
--15/02/15 jcf Generaliza facturas con OC o NP. El usuario debe ingresar estos prefijos en el campo de orden de compra. 
--			LAN - 141127 GCHILE requerimiento LAN.by A Gómez.PDF, Instructivo factura electronica Cencosud.pdf, 150129 GCHILE RE Despacho Nota de Pedido de Servicios (BuPO90 NroNP00033829).by A Gómez.htm
--13/03/15 jcf Obtiene referencias adicionales de sop10107. El usuario debe ingresar uno de los siguientes prefijos: OC, NP, HES, HEM, CT
--14/04/15 jcf Excluye número de referencia que inicia con DM (número de documento de material de COPEC)
--20/04/15 jcf Agrega prefijos de referencia: OTS, RS, PR
--22/12/15 jcf Agrega prefijo: OS
--20/01/16 jcf Corrige fecha referencia en caso de copec. Aquí debe ingresar la fecha de la orden de compra
--14/07/16 jcf Agrega caso de Embotelladoras chilenas
--17/08/16 jcf Agrega caso de empresas asociadas a Embotelladoras chilenas
--03/10/16 jcf Agrega caso de SET de pruebas. Ingresar el valor SET CASO XXXXX en el número de seguimiento de ventas
--				Agrega caso de nota de débito que aplica a factura
--19/06/17 jcf Agrega parámetros TPODOCREF2, TPODOCREF3 para caso de docs de referencia genéricos. Ej. Claro
--
return(
			select ROW_NUMBER() OVER(ORDER BY tracking_number) NroLinRef, 
				case when rtrim(upper(Tracking_Number)) like 'OC%' then '801'
					when rtrim(upper(Tracking_Number)) like 'NP%' then '802'
					when rtrim(upper(Tracking_Number)) like 'CT%' then '803'	--contrato
					when pr.param2 like '%'+left(rtrim(upper(Tracking_Number)), 3)+'%' then
						left(rtrim(upper(Tracking_Number)), 3)

					when pr.param1 like '%'+left(rtrim(upper(Tracking_Number)), 2)+'%' then
						left(rtrim(upper(Tracking_Number)), 2)

					--when rtrim(upper(Tracking_Number)) like 'HES%' then 'HES'	--hoja de entrada de servicio
					--when rtrim(upper(Tracking_Number)) like 'HEM%' then 'HEM'	--recepción de material
					--when rtrim(upper(Tracking_Number)) like 'OTS%' then 'OTS'	--Orden de trabajo por servicios
					--when rtrim(upper(Tracking_Number)) like 'RS%' then 'RS'		--recepción de servicios
					--when rtrim(upper(Tracking_Number)) like 'PR%' then 'PR'		--presupuesto
					--when rtrim(upper(Tracking_Number)) like 'OS%' then 'OS'		--presupuesto
					--when rtrim(upper(Tracking_Number)) like 'NC%' then 'NC'		--Número de conformidad
					else null
				end TpoDocRef,
				
				case when rtrim(upper(Tracking_Number)) like 'NP%' then
					case when @cliente in ( '000006573',						--Embotelladoras chilenas unidas
											'000005917',						--CIA.CERVECERIAS UNIDAS S.A.                                      
											'000009562',						--CERVECERA CCU CHILE LTDA                                     
											'000019109',						--MANANTIAL SA                                                 
											'000013305',						--CERVECERIA AUSTRAL S.A.                                      
											'000015495'							--AGUAS CCU NESTLE CHILE S.A.
											)
					then 'NP'		
					else '' end
				else '' end +	

				case when pr.param2 like '%'+left(rtrim(upper(Tracking_Number)), 3)+'%' then
						SUBSTRING(rtrim(Tracking_Number), 4, 40 )

					when pr.param1 like '%'+left(rtrim(upper(Tracking_Number)), 2)+'%' then
						SUBSTRING(rtrim(Tracking_Number), 3, 40 )

                --case when rtrim(upper(Tracking_Number)) like 'OC%' or
				--	rtrim(upper(Tracking_Number)) like 'NP%' or
				--	rtrim(upper(Tracking_Number)) like 'CT%' or --contrato
				--	rtrim(upper(Tracking_Number)) like 'RS%' or --recepción de servicios
				--	rtrim(upper(Tracking_Number)) like 'OS%' or 
				--	rtrim(upper(Tracking_Number)) like 'NC%' or --número de conformidad
				--	rtrim(upper(Tracking_Number)) like 'PR%' then 
					--	SUBSTRING(rtrim(Tracking_Number), 3, 40 )
					
					--when rtrim(upper(Tracking_Number)) like 'HES%' or	--hoja de entrada de servicio
					--rtrim(upper(Tracking_Number)) like 'HEM%' or	--recepción de material
					--rtrim(upper(Tracking_Number)) like 'OTS%' then 	--Orden de trabajo por servicios
					--	SUBSTRING(rtrim(Tracking_Number), 4, 40 )

					else null
				end FolioRef,
				case when (datediff(day, '1/1/1900', @USRDAT02)) = 0 then
						replace(convert(varchar(10), @p_docdate, 102), '.', '-') 
					else
						replace(convert(varchar(10), @USRDAT02, 102), '.', '-') 
				end FchRef,
				null				CodRef,	
				null				RazonRef
			from sop10107	--
		      cross apply dbo.fLcLvParametros('TPODOCREF2', 'TPODOCREF3', 'NA', 'NA', 'NA', 'NA') pr
			where soptype = 3 
			AND sopnumbe = @p_sopnumbe
			and soptype = @soptype
			and LEFT(sopnumbe, 2) IN  ('33', '34')
			and left(@USRTAB01, 1) not in ('1', '2')
			and @DOCAMNT <> 0
			and upper(Tracking_Number) not like 'DM%'		--número de documento de material de COPEC
			AND Tracking_Number NOT LIKE 'SET%'				--SET de pruebas para certificación
			
			--NC / ND / factura con monto cero / y set de pruebas
			union all
			select 	ROW_NUMBER() OVER(ORDER BY rf.NroLinRef) NroLinRef, rf.TpoDocRef, 
					convert(varchar(18), dbo.fCfdObtieneFolio(rtrim(rf.apdcnm), '-')) FolioRef, 
					replace(rf.apdcdt, '.', '-') FchRef, rf.CodRef,	rf.RazonRef
			from (
				--SET de pruebas para certificación
				select 1 NroLinRef, 
					'SET' TpoDocRef,
					@p_sopnumbe apdcnm,
					convert(varchar(10), @p_docdate, 102) apdcdt,
					'2'				CodRef,	
					rtrim(upper(SUBSTRING(Tracking_Number, 5, 20))) RazonRef
				from sop10107	--
				where LEFT(sopnumbe, 2) IN  ('56', '61', '33', '34')
				AND sopnumbe = @p_sopnumbe
				and soptype = @soptype
				AND Tracking_Number LIKE 'SET%'

				--caso factura con monto cero
				UNION ALL
				select top 1	2				NroLinRef,
					case when charindex('-', @USERDEF2) >0 then
						rtrim(replace(left(rtrim(@USERDEF2), charindex('-', @USERDEF2)), '-', ''))
					else null
					end					TpoDocRef,
					convert(varchar(18), dbo.fCfdObtieneFolio(rtrim(@USERDEF2), '-')) FolioRef,	--obtiene un entero
					case when convert(varchar(10), @USRDAT02, 102) = '1900.01.01' then 
						null 
					else replace(convert(varchar(10), @USRDAT02, 102), '.', '-') 
					end					FchRef, 
					left(@USRTAB01, 1)	CodRef,	
					rtrim(REPLACE(@REFRENCE, '{REPL}', 'ELECTRONICA')) RazonRef
				from sy00300
				where left(@USRTAB01, 1) in ('1', '2')											--1-anula, 2-corrige texto, y con monto cero
				and @DOCAMNT = 0																

				--ND aplicada a factura
				UNION ALL
				select top 1	2 NroLinRef,
					case when charindex('-', @USERDEF2) >0 then
						rtrim(replace(left(rtrim(@USERDEF2), charindex('-', @USERDEF2)), '-', ''))
					else null
					end					TpoDocRef,
					convert(varchar(18), dbo.fCfdObtieneFolio(rtrim(@USERDEF2), '-')) FolioRef,	--obtiene un entero
					case when convert(varchar(10), @USRDAT02, 102) = '1900.01.01' then 
						null 
					else replace(convert(varchar(10), @USRDAT02, 102), '.', '-') 
					end					FchRef, 
					left(@USRTAB01, 1)	CodRef,	
					rtrim(@REFRENCE)			RazonRef
				from sy00300
				where left(@USRTAB01, 1) in ('3')												--3- modifica monto
				and @DOCAMNT != 0			
				and left(@p_sopnumbe, 2) = '56'													--nd electrónica
				and @soptype = 3
				and not exists (
					select ta.apfrdcnm, ta.apfrdcdt
					from vwRmTrxAplicadas ta
					inner join sop30200 sp
						on sp.sopnumbe = ta.apfrdcnm
						and sp.soptype = 4			--NC
					where ta.aptodcnm = @p_sopnumbe	
					and ta.aptodcty = @rmdtypal		--ND
				)

				--NOTA DE CREDITO
				union all
				select top 1 2 NroLinRef, rtrim(id.lochdoccod) TpoDocRef, dbo.fCfdGetSegmento2(rtrim(ta.aptodcnm), '-') apdcnm,	
					convert(varchar(10), ta.aptodcdt, 102) apdcdt, 
					left(@USRTAB01, 1) CodRef,		--1: sólo en NC o ND para anular un doc
					rtrim(@REFRENCE) RazonRef
				from vwRmTrxAplicadas ta 
				inner join sop30200 sp
					on sp.sopnumbe = ta.aptodcnm
					and sp.soptype = 3			--aplica a una ND o Factura
		        inner join loch0004 id 
					on id.custvndr = sp.custnmbr
					and id.lochtrxno = sp.sopnumbe
					and id.module1 = 2			--2:ventas
				where @soptype = 4				--devolución
				and ta.apfrdcnm = @p_sopnumbe
				and ta.apfrdcty = @rmdtypal		--nc
				
				union all
				--NOTA DE DEBITO aplicada por nc 
				select top 1 2 NroLinRef, rtrim(id.lochdoccod) TpoDocRef, dbo.fCfdGetSegmento2(rtrim(ta.apfrdcnm), '-') apdcnm, 
					convert(varchar(10), ta.apfrdcdt, 102) apdcdt,
					left(@USRTAB01, 1) CodRef,		--1: sólo en NC o ND para anular un doc
					rtrim(@REFRENCE) RazonRef
				from vwRmTrxAplicadas ta
				inner join sop30200 sp
					on sp.sopnumbe = ta.apfrdcnm
					and sp.soptype = 4			--es aplicada por una NC
		        inner join loch0004 id 
					on id.custvndr = sp.custnmbr
					and id.lochtrxno = sp.sopnumbe
					and id.module1 = 2			--2:ventas
		        inner join loch0004 nd
					on nd.custvndr = sp.custnmbr
					and nd.lochtrxno = ta.aptodcnm
					and nd.module1 = 2			--2:ventas
					and nd.lochdoccod = '56'	--nd electrónica
				where @soptype = 3				--invoice
				and ta.aptodcnm = @p_sopnumbe	
				and ta.aptodcty = @rmdtypal		--invoice
				) rf
				
			UNION ALL
			--NOTA DE CREDITO DE CAROZZI
			select top 1 2 NroLinRef, '801' TpoDocRef, 
					rtrim(replace(upper(tracking_number), 'OC', '')) FolioRef, 
					--dbo.fCfdGetSegmentoExcepcion(1, rtrim(sp.CSTPONBR))	FolioRef,				--orden de compra
					replace(convert(varchar(10), ta.aptodcdt, 102), '.', '-') apdcdt, 
					null CodRef,		
					null RazonRef
			from vwRmTrxAplicadas ta 
				inner join sop30200 sp
					on sp.sopnumbe = ta.aptodcnm
					and sp.soptype = 3			--aplica a una ND o Factura
				inner join sop10107	tn
					on tn.SOPTYPE = sp.SOPTYPE
					and tn.SOPNUMBE = sp.SOPNUMBE
					and rtrim(upper(tn.Tracking_Number)) like 'OC%' 
			where @soptype = 4				--devolución
				and ta.apfrdcnm = @p_sopnumbe
				and ta.apfrdcty = @rmdtypal		--nc
				and @cliente = '000006607'		--EMPRESAS CAROZZI S.A.                                            
)	
go

IF (@@Error = 0) PRINT 'Creación exitosa de: fCfdReferencia()'
ELSE PRINT 'Error en la creación de: fCfdReferencia()'
GO

--------------------------------------------------------------------------------------------------------

IF OBJECT_ID ('dbo.fCfdReferenciaXML') IS NOT NULL
   DROP FUNCTION dbo.fCfdReferenciaXML
GO

create function dbo.fCfdReferenciaXML(@soptype smallint, @rmdtypal smallint, @p_sopnumbe varchar(21), @p_docdate datetime,
									@USRTAB01 varchar(21), @REFRENCE VARCHAR(31), @USERDEF2 varchar(21), @USRDAT02 datetime, @DOCAMNT numeric(21,5),
									@cliente varchar(15), @CSTPONBR VARCHAR(21))
returns xml 
as
--Propósito. Obtiene la referencia XML de una NC o ND para anular el documento referenciado
--24/01/14 jcf Creación fe Chile
--30/6/14 jcf Agrega @USRTAB01 (código de referencia), referencia, @USERDEF2 (folio ref), @USRDAT02 (fecha ref)
--			Agrega caso de nc o nd con monto cero
--11/9/14 jcf Corrige nota de débito
--3/12/14 jcf Corrige separador
--21/1/15 jcf Mueve todo a fCfdReferencia
--
begin

	declare @cncp xml;
		WITH XMLNAMESPACES 
			(DEFAULT 'http://www.sii.cl/SiiDte'	)
		select @cncp = (
			select 	rf.NroLinRef, rf.TpoDocRef,  rf.FolioRef, 
					rf.FchRef, rf.CodRef, rf.RazonRef
			from dbo.fCfdReferencia(@soptype, @rmdtypal, @p_sopnumbe, @p_docdate ,
									@USRTAB01 , @REFRENCE , @USERDEF2 , @USRDAT02 , @DOCAMNT ,
									@cliente , @CSTPONBR ) rf
			FOR XML path('Referencia'), type
		)
	
	return @cncp
end
go

IF (@@Error = 0) PRINT 'Creación exitosa de: fCfdReferenciaXML()'
ELSE PRINT 'Error en la creación de: fCfdReferenciaXML()'
GO

--------------------------------------------------------------------------------------------------------

IF OBJECT_ID ('dbo.fCfdDetalleExcepcion') IS NOT NULL
   DROP FUNCTION dbo.fCfdDetalleExcepcion
GO

create function dbo.fCfdDetalleExcepcion(@contacto VARCHAR(61), @cliente varchar(15), @soptype smallint, @sopnumbe varchar(21))
returns varchar(100) 
as
--Propósito. Obtiene el contacto en el caso de LAN
--Referencia. LAN - 141127 GCHILE requerimiento LAN.by A Gómez.PDF
--21/1/15 jcf Creación 
--13/3/15 jcf Agrega bci
--14/4/15 jcf Agrega copec
--
begin
	declare @excep varchar(100) 
	select  @excep = null;
	if @cliente = '000007649'	--LAN
		select  @excep = @contacto
	
	if @cliente in ( '000009941',	--bci
					 '000003190'	--copec
					 )
		select @excep =
		   replace(
			 replace(
			   replace(
					(
					select rtrim(tracking_number) trackn
					from sop10107
					where sopnumbe = @sopnumbe
					and soptype = @soptype
					for xml path('')
				),
				 '</trackn><trackn>',
				 ' ' -- delimiter
			   ),
			   '</trackn>',
			   ''
			 ),
			 '<trackn>',
			 ''
		  )
	
	return @excep
end
go



IF (@@Error = 0) PRINT 'Creación exitosa de: fCfdDetalleExcepcion()'
ELSE PRINT 'Error en la creación de: fCfdDetalleExcepcion()'
GO
--------------------------------------------------------------------------------------------------------
--select dbo.fCfdGetSegmentoExcepcion(1, '767676-987')
--case when dbo.fCfdGetSegmentoExcepcion(1, '767676') = '0' then 'devuelve núm' else 'no hacer nada' end

--select dbo.fCfdDetalleExcepcion('' , '000009941', 3, '33-00000266')

--declare @CSTPONBR varchar(20)
--select @CSTPONBR = ' NP 2987' --
--select	TOP 1 *
--from sy00300
--where rtrim(upper(@CSTPONBR)) like 'NP%' 
--			and dbo.fCfdGetSegmentoExcepcion(1, rtrim(@CSTPONBR)) = '0'

--select dbo.fCfdReferenciaXML(3, 1, sopnumbe, '5/5/2016',
--									'usertab1', 'reference', 'userdef2', '5/5/16', 1500,
--									custnmbr, 'ponmbr')
--from sop30200
--where custnmbr ='000019109'	-- '000006573'	--embotelladoras chilenas
--and sopnumbe = '33-00000662          '	--'33-00002533          '

