IF DATABASE_PRINCIPAL_ID('rol_cfdigital') IS NULL
	create role rol_cfdigital;

--Vistas que usa la impresi�n de factura
grant select on dbo.IMPRIME_COMPROBANTE_DTE to rol_cfdigital;

