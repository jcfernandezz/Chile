-------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdVentasObtieneNeto') IS NOT NULL
   DROP FUNCTION dbo.fCfdVentasObtieneNeto
GO

create function dbo.fCfdVentasObtieneNeto(@subtotal numeric(21,5), @descuento numeric(21,5), @importe numeric(21,5), @docType char(3))
returns numeric(18)
--Prop�sito. Obtiene el monto neto considerando descuentos globales o por l�nea
--22/08/14 jcf Creaci�n 
--01/11/16 jcf Ajusta doc exento
--
begin
		return
			--descuento global o por l�nea
			case when @docType = '34' then					--doc no afecto o exento no requiere monto neto
				null
			else
				case when @subtotal <> 0 then 
					cast( round(@subtotal - @descuento - (isnull(@importe, 0) * (1-@descuento/@subtotal)), 0) as numeric(18))	
				else 0
				end
			end

			--inicio set de pruebas
			--descuento global s�lo para afectos o por l�nea para SET DE PRUEBAS
			--cast( round(@subtotal - @descuento - isnull(@importe, 0), 0) as numeric(18))	
			--fin set de pruebas
end
go
IF (@@Error = 0) PRINT 'Creaci�n exitosa de: fCfdVentasObtieneNeto()'
ELSE PRINT 'Error en la creaci�n de: fCfdVentasObtieneNeto()'
GO

-------------------------------------------------------------------------------------------------------
IF OBJECT_ID ('dbo.fCfdVentasObtieneExento') IS NOT NULL
   DROP FUNCTION dbo.fCfdVentasObtieneExento
GO

create function dbo.fCfdVentasObtieneExento(@subtotal numeric(21,5), @descuento numeric(21,5), @importe numeric(21,5), @docType char(3))
returns numeric(18)
--Prop�sito. Obtiene el monto exento considerando descuentos globales o por l�nea
--22/08/14 jcf Creaci�n 
--01/11/16 jcf Ajusta doc exento
--
begin
		return
			--descuento global o por l�nea
			case when @docType = '34' then					--doc no afecto o exento
				case when @subtotal <> 0 then 
					cast( round(isnull(@subtotal,0) * (1-@descuento/@subtotal), 0) as numeric(18))
					--dbo.fCfdVentasObtieneExento(tv.subtotal, tv.descuento, tv.subtotal)
				else 
					0
				end
			else
				case when @subtotal <> 0 then 
					cast( round(isnull(@importe,0) * (1-@descuento/@subtotal), 0) as numeric(18))
					--dbo.fCfdVentasObtieneExento(tv.subtotal, tv.descuento, ex.importe)
				else
					null
				end
			end

			--inicio set de pruebas
			--descuento global s�lo para afectos o por l�nea para SET DE PRUEBAS
			--cast( round(isnull(@importe,0), 0) as numeric(18))
			--fin set de pruebas

end
go
IF (@@Error = 0) PRINT 'Creaci�n exitosa de: fCfdVentasObtieneExento()'
ELSE PRINT 'Error en la creaci�n de: fCfdVentasObtieneExento()'
GO
