-------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdObtieneFolio') IS NOT NULL
   DROP FUNCTION dbo.fCfdObtieneFolio
GO

create function dbo.fCfdObtieneFolio(@sopnumbe varchar(21), @separador char(1))
returns int
--Prop�sito. Obtiene el segundo segmento de una cadena separada por @separador y la convierte a entero
--16/01/14 jcf Creaci�n 
--16/10/14 jcf Prueba tambi�n el separador espacio
--03/11/16 jcf El punto no debe ser un separador. Los elimina.
--
begin
		return
				CONVERT( INT, 
					case when ISNUMERIC(replace(right(@sopnumbe, len(@sopnumbe)-patindex('%'+@separador+'%', @sopnumbe)), '.', '')) = 1 then
										replace(right(@sopnumbe, len(@sopnumbe)-patindex('%'+@separador+'%', @sopnumbe)), '.', '')
						when  ISNUMERIC(replace(right(@sopnumbe, len(@sopnumbe)-patindex('% %', @sopnumbe)), '.', '')) = 1 then				--separador espacio
										replace(right(@sopnumbe, len(@sopnumbe)-patindex('% %', @sopnumbe)), '.', '')
					else 0
					end
					)

				--�til para simular facturas reales (segundo paso de certificaci�n). 
				--inicio simulaci�n
				--case when @sopnumbe like 'FV%' THEN
				--	CONVERT( INT, 
				--		case when ISNUMERIC(right(@sopnumbe, len(@sopnumbe)-patindex('% %', @sopnumbe))) = 1 then
				--							right(@sopnumbe, len(@sopnumbe)-patindex('% %', @sopnumbe))
				--		else 0
				--		end
				--		) - 7961	--empieza en fv 8203. Rango disponible a partir de 242
				--when @sopnumbe like 'NC%' THEN
				--	CONVERT( INT, 
				--		case when ISNUMERIC(right(@sopnumbe, len(@sopnumbe)-patindex('% %', @sopnumbe))) = 1 then
				--							right(@sopnumbe, len(@sopnumbe)-patindex('% %', @sopnumbe))
				--		else 0
				--		end
				--		) - 489		--empieza en nc 642.  Rango disponible a partir de 153
				--when @sopnumbe like 'ND%' THEN
				--	CONVERT( INT, 
				--		case when ISNUMERIC(right(@sopnumbe, len(@sopnumbe)-patindex('% %', @sopnumbe))) = 1 then
				--							right(@sopnumbe, len(@sopnumbe)-patindex('% %', @sopnumbe))
				--		else 0
				--		end 
				--		) +47			--empieza en nd 104. Rango disponible a partir de 151
				--else
				--	CONVERT( INT, 
				--		case when ISNUMERIC(right(@sopnumbe, len(@sopnumbe)-patindex('%'+@separador+'%', @sopnumbe))) = 1 then
				--							right(@sopnumbe, len(@sopnumbe)-patindex('%'+@separador+'%', @sopnumbe))
				--		else 0
				--		end
				--		)
				--end
				--fin simulaci�n
					

end
go
IF (@@Error = 0) PRINT 'Creaci�n exitosa de: fCfdObtieneFolio()'
ELSE PRINT 'Error en la creaci�n de: fCfdObtieneFolio()'
GO
-------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdGetSegmento2') IS NOT NULL
   DROP FUNCTION dbo.fCfdGetSegmento2
GO

create function dbo.fCfdGetSegmento2(@sopnumbe varchar(21), @separador char(1))
returns varchar(18)
--Prop�sito. Obtiene el segundo segmento de una cadena separada por @separador 
--16/01/14 jcf Creaci�n 
--16/10/14 jcf Prueba tambi�n el separador espacio
--
begin
		return 
			--right(@sopnumbe, len(@sopnumbe)-patindex('%'+@separador+'%', @sopnumbe))
					case when ISNUMERIC(right(@sopnumbe, len(@sopnumbe)-patindex('%'+@separador+'%', @sopnumbe))) = 1 then
										right(@sopnumbe, len(@sopnumbe)-patindex('%'+@separador+'%', @sopnumbe))
						when ISNUMERIC(right(@sopnumbe, len(@sopnumbe)-patindex('% %', @sopnumbe))) = 1 then				--separador espacio
										right(@sopnumbe, len(@sopnumbe)-patindex('% %', @sopnumbe))
					else '0'
					end
					
				--�til para simular facturas reales (segundo paso de certificaci�n). resta 8128 para igualar el rango de folios disponible
				--inicio simulaci�n
			--convert(varchar(18),
			--	case when @sopnumbe like 'FV%' THEN
			--		CONVERT( INT, 
			--			case when ISNUMERIC(right(@sopnumbe, len(@sopnumbe)-patindex('% %', @sopnumbe))) = 1 then
			--								right(@sopnumbe, len(@sopnumbe)-patindex('% %', @sopnumbe))
			--			else 0
			--			end
			--			) - 7961	--empieza en fv 8203. Rango disponible a partir de 242
			--	when @sopnumbe like 'NC%' THEN
			--		CONVERT( INT, 
			--			case when ISNUMERIC(right(@sopnumbe, len(@sopnumbe)-patindex('% %', @sopnumbe))) = 1 then
			--								right(@sopnumbe, len(@sopnumbe)-patindex('% %', @sopnumbe))
			--			else 0
			--			end
			--			) - 489		--empieza en nc 642.  Rango disponible a partir de 153
			--	when @sopnumbe like 'ND%' THEN
			--		CONVERT( INT, 
			--			case when ISNUMERIC(right(@sopnumbe, len(@sopnumbe)-patindex('% %', @sopnumbe))) = 1 then
			--								right(@sopnumbe, len(@sopnumbe)-patindex('% %', @sopnumbe))
			--			else 0
			--			end 
			--			) +47			--empieza en nd 104. Rango disponible a partir de 151
			--	else
			--		right(@sopnumbe, len(@sopnumbe)-patindex('%'+@separador+'%', @sopnumbe))
			--	end)
				--fin simulaci�n
end
go

IF (@@Error = 0) PRINT 'Creaci�n exitosa de: fCfdGetSegmento2()'
ELSE PRINT 'Error en la creaci�n de: fCfdGetSegmento2()'
GO

-------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdReemplazaSecuenciaDeEspacios') IS NOT NULL
   DROP FUNCTION dbo.fCfdReemplazaSecuenciaDeEspacios
GO

create function dbo.fCfdReemplazaSecuenciaDeEspacios(@texto nvarchar(max), @repeticiones smallint)
returns NVARCHAR(MAX)
--Prop�sito. Reemplaza toda secuencia de espacios en un texto por un �nico espacio
--10/05/12 jcf Creaci�n (Michael Meierruth)
--
begin
	RETURN   replace(replace(replace(replace(replace(replace(replace(ltrim(rtrim(@texto)),
	  '                                 ',' '),
	  '                 ',' '),
	  '         ',' '),
	  '     ',' '),
	  '   ',' '),
	  '  ',' '),
	  '  ',' ')

--Jeff Moden
--REPLACE(
--            REPLACE(
--                REPLACE(
--                    LTRIM(RTRIM(@texto))
--                ,'  ',' '+CHAR(8))  --Changes 2 spaces to the OX model
--            ,CHAR(8)+' ','')        --Changes the XO model to nothing
--        ,CHAR(8),'') AS CleanString --Changes the remaining X's to nothing

end
go
IF (@@Error = 0) PRINT 'Creaci�n exitosa de: fCfdReemplazaSecuenciaDeEspacios()'
ELSE PRINT 'Error en la creaci�n de: fCfdReemplazaSecuenciaDeEspacios()'
GO
-------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdReemplazaCaracteresNI') IS NOT NULL
   DROP FUNCTION dbo.fCfdReemplazaCaracteresNI
GO

create function dbo.fCfdReemplazaCaracteresNI(@texto nvarchar(max))
returns NVARCHAR(MAX)
--Prop�sito. Reemplaza caracteres no imprimibles por espacios
--26/10/10 jcf Creaci�n
--28/09/15 jcf Agrega reemplazo de comilla rara
--
as
begin
	declare @textoModificado nvarchar(max)
	select @textoModificado = @texto
	select @textoModificado = replace(@textoModificado, char(13), ' ')
	select @textoModificado = replace(@textoModificado, char(10), ' ')
	select @textoModificado = replace(@textoModificado, char(9), ' ')
	select @textoModificado = replace(@textoModificado, '|', '')
	select @textoModificado = replace(@textoModificado, '�', '')
	return @textoModificado 
end
go
IF (@@Error = 0) PRINT 'Creaci�n exitosa de: fCfdReemplazaCaracteresNI()'
ELSE PRINT 'Error en la creaci�n de: fCfdReemplazaCaracteresNI()'
GO

---------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdObtienePorcentajeImpuesto') IS NOT NULL
   DROP FUNCTION dbo.fCfdObtienePorcentajeImpuesto
GO

create FUNCTION dbo.fCfdObtienePorcentajeImpuesto (@p_idimpuesto varchar(20))
RETURNS numeric(19,2)
AS
BEGIN
   DECLARE @l_TXDTLPCT numeric(19,5)
   select @l_TXDTLPCT = TXDTLPCT from tx00201 where taxdtlid = @p_idimpuesto
   RETURN(@l_TXDTLPCT)
END
go

IF (@@Error = 0) PRINT 'Creaci�n exitosa de: fCfdObtienePorcentajeImpuesto()'
ELSE PRINT 'Error en la creaci�n de: fCfdObtienePorcentajeImpuesto()'
GO
-------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdReemplazaEspecialesXml') IS NOT NULL
   DROP FUNCTION dbo.fCfdReemplazaEspecialesXml
GO

create function dbo.fCfdReemplazaEspecialesXml(@texto nvarchar(max))
returns NVARCHAR(MAX)
--Prop�sito. Reemplaza caracteres especiales xml por caracteres ascii. 
--			Al convertir una cadena usando for xml, autom�ticamente convierte los caracteres especiales. Por eso se deben volver a convertir a ascii.
--26/10/10 jcf Creaci�n
--
as
begin
	declare @textoModificado nvarchar(max)
	select @textoModificado = @texto
	select @textoModificado = replace(@textoModificado, '&amp;', '&')
	select @textoModificado = replace(@textoModificado, '&lt;', '<')
	select @textoModificado = replace(@textoModificado, '&gt;', '>')
	select @textoModificado = replace(@textoModificado, '&quot;', '"')
	select @textoModificado = replace(@textoModificado, '&#39;', '?')
	--select @textoModificado = replace(@textoModificado, '''', '&apos;')
	
	return @textoModificado 
end
go
IF (@@Error = 0) PRINT 'Creaci�n exitosa de: fCfdReemplazaEspecialesXml()'
ELSE PRINT 'Error en la creaci�n de: fCfdReemplazaEspecialesXml()'
GO
-------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdEsVacio') IS NOT NULL
   DROP FUNCTION dbo.fCfdEsVacio
GO

create function dbo.fCfdEsVacio(@texto nvarchar(max))
returns NVARCHAR(MAX)
--Prop�sito. Devuelve un caracter si el texto es vac�o 
--10/03/11 jcf Creaci�n
--
as
begin
	if @texto = ''
		return '-'

	return @texto
end
go
IF (@@Error = 0) PRINT 'Creaci�n exitosa de: fCfdEsVacio()'
ELSE PRINT 'Error en la creaci�n de: fCfdEsVacio()'
GO
--------------------------------------------------------------------------------------------------------

--	select *
--	from cllc4000	--cllc_localization_setup
--sp_columns cllc4000
	--select *
	--from ncloc145	--ncloc_sucursales_mstr

	--select *
	--from loch0002	--loch_document_code_mstr
