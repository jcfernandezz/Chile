--GETTY - Factura Electrónica CHILE
--
---------------------------------------------------------------------------------------
--Propósito. Tabla para monitorear la recepción de facturas de compra
--20/3/14 jcf Creación
--28/9/14 jcf Cambia llave primaria
--
--drop TABLE dbo.cfdLogFacturaCompra 
IF not EXISTS (SELECT 1 FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[cfdLogFacturaCompra]') AND OBJECTPROPERTY(id,N'IsTable') = 1)
begin
	CREATE TABLE dbo.cfdLogFacturaCompra (
	  secuencia INTEGER  NOT NULL IDENTITY ,
	  tipo SMALLINT NOT NULL DEFAULT 0 ,
	  folio VARCHAR(21)  NOT NULL DEFAULT '',
	  idImpuestoTercero varchar(15) not null default '',
	  nombreTercero varchar(35) not null default '',
	  fechaRecepcion datetime not null default getdate(),
	  
	  estado VARCHAR(20)  NOT NULL DEFAULT '' , 
	  estadoActual varchar(20) default '0',
	  idxSingleStatus smallint default 0, 
	  mensajeEA varchar(255) default '',
	  mensaje VARCHAR(255)  NOT NULL DEFAULT '' ,
	  archivoXML xml default '',
	  archivoPDF varchar(255) default '',
	  idExterno varchar(25) not null default '',
	  
	  fechaAlta datetime not null default getdate(), 
	  idUsuario varchar(10) not null default '',
	  fechaModificacion datetime not null default 0,
	  idUsuarioModificacion varchar(10) not null default ''
	PRIMARY KEY(folio, tipo, idImpuestoTercero, estado));

	create index idx1_cfdLogFacturaCompra on dbo.cfdLogFacturaCompra(idExterno, estado) include (estadoActual, secuencia);
end;
go

---------------------------------------------------------------------------------------------------------------------------
--modificaciones en test
--alter table cfdLogFacturaCompra add idxSingleStatus smallint null;

--ALTER TABLE cfdLogFacturaCompra
--ADD CONSTRAINT PK_cfdLogFacturaCompra PRIMARY KEY (folio, tipo, idImpuestoTercero, estado, secuencia);
--go

--select *
--from [cfdLogFacturaCompra]
--where tipo > 0

--update [cfdLogFacturaCompra] set estado = 'publicado'
--where tipo = 0


---------------------------------------------------------------------------------------------------------------------------

