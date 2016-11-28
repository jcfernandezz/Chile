--GETTY
--Factura Electrónica Chile
--Propósito. Accesos a objetos de reporte de factura electrónica
--Requisitos. Ejecutar antes los permisos para factura electrónica (110) y los permisos a los reportes de impresión (120)
--			Para usuario de dominio: Crear login y accesos a bds: Dynamics, [GCOL], INTDB2
--Atención! en el explorador de Windows 2008 del servidor de bd: 
--		Dar permiso a las carpetas de almacenamiento de facturas electrónicas. Usar share, permission level Contributor
--		Dar permiso a la carpeta de aplicación de factura electrónica. Usar share, permission level Contributor
--		Dar permiso a la carpeta del reporte crystal de la factura 
-------------------------------------------------------------------------------------------
--Permiso a usuarios Windows:
-------------------------------------------------------------------------------------------
--use compañía; 
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\tiiselam' ;
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\ext-tiiselam4';
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\priscilla.parra';
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\andrea.gomez';

use dynamics;
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\tiiselam';
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\ext-tiiselam4';
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\priscilla.parra';
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\andrea.gomez';

use intdb2;
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\tiiselam';
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\ext-tiiselam4';
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\priscilla.parra';
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\andrea.gomez';

