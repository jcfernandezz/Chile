--GETTY
--Factura Electr�nica Chile
--Prop�sito. Accesos a objetos de reporte de factura electr�nica
--Requisitos. Ejecutar antes los permisos para factura electr�nica (110) y los permisos a los reportes de impresi�n (120)
--			Para usuario de dominio: Crear login y accesos a bds: Dynamics, [GCOL], INTDB2
--Atenci�n! en el explorador de Windows 2008 del servidor de bd: 
--		Dar permiso a las carpetas de almacenamiento de facturas electr�nicas y Certificados. Usar share, permission level Contributor
--		Dar permiso a la carpeta de aplicaci�n de factura electr�nica. Usar share, permission level Contributor
--		Dar permiso a la carpeta del reporte crystal de la factura 
-------------------------------------------------------------------------------------------
--Permiso a usuarios Windows:
-------------------------------------------------------------------------------------------
--use compa��a; 
use chi10;
if not exists( SELECT *
			FROM sysusers
			where islogin = 1
			and lower(name) = lower('GILA\GICHUSERS')
			)
	create user [GILA\GICHUSERS] for login [GILA\GICHUSERS];

if exists( SELECT *
			FROM sysusers
			where islogin = 1
			and hasdbaccess = 0
			and lower(name) = lower('GILA\GICHUSERS')
			)
begin
	drop schema [GILA\GICHUSERS];
	drop user [GILA\GICHUSERS];
	create user [GILA\GICHUSERS] for login [GILA\GICHUSERS];
end
else
	EXEC sp_addrolemember 'rol_chiLocalizacion', 'GILA\GICHUSERS';

EXEC sp_addrolemember 'rol_cfdigital', 'GILA\GICHUSERS';

EXEC sp_addrolemember 'rol_cfdigital', 'GILA\tiiselam' ;
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\ext-tiiselam4';

EXEC sp_addrolemember 'rol_cfdigital', 'GILA\priscilla.parra';
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\andrea.gomez';

use dynamics;
drop schema [GILA\GICHUSERS];
drop user [GILA\GICHUSERS];
create user [GILA\GICHUSERS] for login [GILA\GICHUSERS];
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\GICHUSERS';
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\tiiselam';
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\ext-tiiselam4';

EXEC sp_addrolemember 'rol_cfdigital', 'GILA\priscilla.parra';
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\andrea.gomez';

use intdb2;
drop schema [GILA\GICHUSERS];
drop user [GILA\GICHUSERS];
create user [GILA\GICHUSERS] for login [GILA\GICHUSERS];
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\GICHUSERS';

EXEC sp_addrolemember 'rol_cfdigital', 'GILA\tiiselam';
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\ext-tiiselam4';

EXEC sp_addrolemember 'rol_cfdigital', 'GILA\priscilla.parra';
EXEC sp_addrolemember 'rol_cfdigital', 'GILA\andrea.gomez';

