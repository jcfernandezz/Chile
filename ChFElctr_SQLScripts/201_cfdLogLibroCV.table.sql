--GETTY - Factura Electrónica CHILE
--
---------------------------------------------------------------------------------------
--Propósito. Tabla para monitorear el estado de envío de los libros de compras y ventas
--17/7/14 jcf Creación
--
--drop TABLE dbo.cfdLogLibroCV 
IF not EXISTS (SELECT 1 FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[cfdLogLibroCV]') AND OBJECTPROPERTY(id,N'IsTable') = 1)
begin
	CREATE TABLE dbo.cfdLogLibroCV (
	  secuencia INTEGER  NOT NULL IDENTITY ,
	  periodo INTEGER  NOT NULL DEFAULT 0,
	  tipo VARCHAR(4) NOT NULL DEFAULT '',
	  estado VARCHAR(20)  NOT NULL DEFAULT '' , 

	  mensajeGral VARCHAR(255)  NOT NULL DEFAULT '' ,
	  estadoActualBin varchar(20) default '000000000001', 
	  idxSingleStatus smallint default 11,
	  mensajeEActual varchar(255) default '',
	  archivoXML xml default '',
	  
	  fechaAlta datetime not null default getdate(), 
	  idUsuario varchar(10) not null default '',
	  fechaModificacion datetime not null default 0,
	  idUsuarioModificacion varchar(10) not null default ''
	PRIMARY KEY(periodo, tipo, estado));

end;
go



---------------------------------------------------------------------------------------------------------------------------

