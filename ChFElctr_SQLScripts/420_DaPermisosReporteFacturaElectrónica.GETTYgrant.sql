--Getty Chile
--Factura Electr�nica
--Prop�sito. Rol que da accesos a objetos de reporte de factura electr�nica
--Requisitos. Ejecutar antes los permisos para factura electr�nica 
--
--23/10/14 JCF Creaci�n
-----------------------------------------------------------------------------------
--use [bd compa��a]

------------------------------------------------------------------------------------------
use dynamics;

--Objetos que usa la impresi�n de factura
grant execute on dbo.fncNUMLET to rol_cfdigital, dyngrp;
grant execute on dbo.fncNUMLET_ENG to rol_cfdigital, dyngrp;
grant select on dbo.mc40200 to rol_cfdigital;

-------------------------------------------------------------------------------------------
use intdb2

IF DATABASE_PRINCIPAL_ID('rol_cfdigital') IS NULL
	create role rol_cfdigital;

--Objetos que usa la impresi�n de factura
grant select on dbo.ERP_Country_Description  to rol_cfdigital;
grant select on dbo.ERP_Invoice  to rol_cfdigital;



