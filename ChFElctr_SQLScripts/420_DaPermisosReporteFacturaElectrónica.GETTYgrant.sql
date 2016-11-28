--Getty Chile
--Factura Electrónica
--Propósito. Rol que da accesos a objetos de reporte de factura electrónica
--Requisitos. Ejecutar antes los permisos para factura electrónica 
--
--23/10/14 JCF Creación
-----------------------------------------------------------------------------------
--use [bd compañía]

------------------------------------------------------------------------------------------
use dynamics;

--Objetos que usa la impresión de factura
grant execute on dbo.fncNUMLET to rol_cfdigital, dyngrp;
grant execute on dbo.fncNUMLET_ENG to rol_cfdigital, dyngrp;
grant select on dbo.mc40200 to rol_cfdigital;

-------------------------------------------------------------------------------------------
use intdb2

IF DATABASE_PRINCIPAL_ID('rol_cfdigital') IS NULL
	create role rol_cfdigital;

--Objetos que usa la impresión de factura
grant select on dbo.ERP_Country_Description  to rol_cfdigital;
grant select on dbo.ERP_Invoice  to rol_cfdigital;



