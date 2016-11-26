--GETTY - Factura Electrónica CHILE
--Propósito. Tablas y funciones para monitorear la creación de facturas en formato xml
--
---------------------------------------------------------------------------------------
--Propósito. Log de facturas emitidas en formato xml. Sólo debe haber un estado emitido para cada factura.
--23/4/12 jcf Creación cfdi
--27/9/14 jcf Agrega un dígito a valor predeterminado del campo estadoActual
--			Agrega idExterno
--
--drop table cfdLogFacturaXml

IF not EXISTS (SELECT 1 FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[cfdLogFacturaXML]') AND OBJECTPROPERTY(id,N'IsTable') = 1)
begin
	CREATE TABLE dbo.cfdLogFacturaXML (
	  soptype SMALLINT  NOT NULL DEFAULT 0 ,
	  sopnumbe VARCHAR(21)  NOT NULL DEFAULT '' ,
	  secuencia INTEGER  NOT NULL IDENTITY ,
	  estado VARCHAR(20)  NOT NULL DEFAULT 'no emitido' ,	--estado actual
	  estadoActual varchar(20) default '0',					--estado binario compuesto
	  noAprobacion varchar(21) not null default '0',		--índice estado actual
	  mensajeEA varchar(255) default '',					--mensaje estado binario compuesto
	  mensaje VARCHAR(255)  NOT NULL DEFAULT 'no emitido' , --en el estado actual guarda la ruta del archivo xml
	  archivoXML xml default '',							--en el estado actual guarda el xml del dte
	  idExterno varchar(25) not null default '',			--trackid del sistema del SII
	  
	  fechaEmision datetime not null default getdate(), 
	  idUsuario varchar(10) not null default '',
	  fechaAnulacion datetime not null default 0,
	  idUsuarioAnulacion varchar(10) not null default ''
	PRIMARY KEY(soptype, sopnumbe, secuencia));

	--alter table dbo.cfdLogFacturaXML add constraint chk_estado check(estado in ('anulado', 'rechazado', 'aceptado', 'recibido', 'publicado', 'con excepción', 'rechazado SII', 'con reparos SII', 'aceptado SII', 'enviado SII', 'emitido', 'no emitido'));
	create index idx1_cfdLogFacturaXML on dbo.cfdLogFacturaXML(soptype, sopnumbe, estado, idExterno) include (estadoActual, archivoXML);
end;
go

--alter table cfdLogFacturaXml add idExterno varchar(25) default '';

---------------------------------------------------------------------------------------------------------------------------
--PRUEBAS--
--select * from cfdLogFacturaXML

