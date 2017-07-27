
/****** Object:  StoredProcedure [proc_cfdLogFacturaCompraLoadByPrimaryKey]    Script Date: 30/09/2014 08:50:21 p.m. ******/
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[proc_cfdLogFacturaCompraLoadByPrimaryKey]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
    DROP PROCEDURE [proc_cfdLogFacturaCompraLoadByPrimaryKey];
GO

CREATE PROCEDURE [proc_cfdLogFacturaCompraLoadByPrimaryKey]
(
	@tipo smallint,
	@folio varchar(21),
	@idImpuestoTercero varchar(15),
	@estado varchar(20)
)
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @Err int

	SELECT
		[secuencia],
		[tipo],
		[folio],
		[idImpuestoTercero],
		[nombreTercero],
		[fechaRecepcion],
		[estado],
		[estadoActual],
		[idxSingleStatus],
		[mensajeEA],
		[mensaje],
		[archivoXML],
		[archivoPDF],
		[idExterno],
		[fechaAlta],
		[idUsuario],
		[fechaModificacion],
		[idUsuarioModificacion]
	FROM [cfdLogFacturaCompra]
	WHERE
		([tipo] = @tipo) AND
		([folio] = @folio) AND
		([idImpuestoTercero] = @idImpuestoTercero) AND
		([estado] = @estado)

	SET @Err = @@Error

	RETURN @Err
END
GO


-- Display the status of Proc creation
IF (@@Error = 0) PRINT 'Procedure Creation: proc_cfdLogFacturaCompraLoadByPrimaryKey Succeeded'
ELSE PRINT 'Procedure Creation: proc_cfdLogFacturaCompraLoadByPrimaryKey Error on Creation'
GO

/****** Object:  StoredProcedure [proc_cfdLogFacturaCompraLoadAll]    Script Date: 30/09/2014 08:50:21 p.m. ******/
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[proc_cfdLogFacturaCompraLoadAll]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
    DROP PROCEDURE [proc_cfdLogFacturaCompraLoadAll];
GO

CREATE PROCEDURE [proc_cfdLogFacturaCompraLoadAll]
AS
BEGIN

	SET NOCOUNT ON
	DECLARE @Err int

	SELECT
		[secuencia],
		[tipo],
		[folio],
		[idImpuestoTercero],
		[nombreTercero],
		[fechaRecepcion],
		[estado],
		[estadoActual],
		[idxSingleStatus],
		[mensajeEA],
		[mensaje],
		[archivoXML],
		[archivoPDF],
		[idExterno],
		[fechaAlta],
		[idUsuario],
		[fechaModificacion],
		[idUsuarioModificacion]
	FROM [cfdLogFacturaCompra]

	SET @Err = @@Error

	RETURN @Err
END
GO


-- Display the status of Proc creation
IF (@@Error = 0) PRINT 'Procedure Creation: proc_cfdLogFacturaCompraLoadAll Succeeded'
ELSE PRINT 'Procedure Creation: proc_cfdLogFacturaCompraLoadAll Error on Creation'
GO

/****** Object:  StoredProcedure [proc_cfdLogFacturaCompraUpdate]    Script Date: 30/09/2014 08:50:21 p.m. ******/
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[proc_cfdLogFacturaCompraUpdate]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
    DROP PROCEDURE [proc_cfdLogFacturaCompraUpdate];
GO

CREATE PROCEDURE [proc_cfdLogFacturaCompraUpdate]
(
	@secuencia int,
	@tipo smallint,
	@folio varchar(21),
	@idImpuestoTercero varchar(15),
	@nombreTercero varchar(35),
	@fechaRecepcion datetime,
	@estado varchar(20),
	@estadoActual varchar(20) = NULL,
	@idxSingleStatus smallint = NULL,
	@mensajeEA varchar(255) = NULL,
	@mensaje varchar(255),
	@archivoXML xml = NULL,
	@archivoPDF varchar(255) = NULL,
	@idExterno varchar(25),
	@fechaAlta datetime,
	@idUsuario varchar(10),
	@fechaModificacion datetime,
	@idUsuarioModificacion varchar(10)
)
AS
BEGIN

	SET NOCOUNT OFF
	DECLARE @Err int

	UPDATE [cfdLogFacturaCompra]
	SET
		[nombreTercero] = @nombreTercero,
		[fechaRecepcion] = @fechaRecepcion,
		[estadoActual] = @estadoActual,
		[idxSingleStatus] = @idxSingleStatus,
		[mensajeEA] = @mensajeEA,
		[mensaje] = @mensaje,
		[archivoXML] = @archivoXML,
		[archivoPDF] = @archivoPDF,
		[idExterno] = @idExterno,
		[fechaAlta] = @fechaAlta,
		[idUsuario] = @idUsuario,
		[fechaModificacion] = @fechaModificacion,
		[idUsuarioModificacion] = @idUsuarioModificacion
	WHERE
		[tipo] = @tipo
	AND	[folio] = @folio
	AND	[idImpuestoTercero] = @idImpuestoTercero
	AND	[estado] = @estado


	SET @Err = @@Error


	RETURN @Err
END
GO


-- Display the status of Proc creation
IF (@@Error = 0) PRINT 'Procedure Creation: proc_cfdLogFacturaCompraUpdate Succeeded'
ELSE PRINT 'Procedure Creation: proc_cfdLogFacturaCompraUpdate Error on Creation'
GO




/****** Object:  StoredProcedure [proc_cfdLogFacturaCompraInsert]    Script Date: 30/09/2014 08:50:21 p.m. ******/
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[proc_cfdLogFacturaCompraInsert]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
    DROP PROCEDURE [proc_cfdLogFacturaCompraInsert];
GO

CREATE PROCEDURE [proc_cfdLogFacturaCompraInsert]
(
	@secuencia int = NULL output,
	@tipo smallint,
	@folio varchar(21),
	@idImpuestoTercero varchar(15),
	@nombreTercero varchar(35),
	@fechaRecepcion datetime,
	@estado varchar(20),
	@estadoActual varchar(20) = NULL,
	@idxSingleStatus smallint = NULL,
	@mensajeEA varchar(255) = NULL,
	@mensaje varchar(255),
	@archivoXML xml = NULL,
	@archivoPDF varchar(255) = NULL,
	@idExterno varchar(25),
	@fechaAlta datetime,
	@idUsuario varchar(10),
	@fechaModificacion datetime,
	@idUsuarioModificacion varchar(10)
)
AS
BEGIN

	SET NOCOUNT OFF
	DECLARE @Err int

	INSERT
	INTO [cfdLogFacturaCompra]
	(
		[tipo],
		[folio],
		[idImpuestoTercero],
		[nombreTercero],
		[fechaRecepcion],
		[estado],
		[estadoActual],
		[idxSingleStatus],
		[mensajeEA],
		[mensaje],
		[archivoXML],
		[archivoPDF],
		[idExterno],
		[fechaAlta],
		[idUsuario],
		[fechaModificacion],
		[idUsuarioModificacion]
	)
	VALUES
	(
		@tipo,
		@folio,
		@idImpuestoTercero,
		@nombreTercero,
		@fechaRecepcion,
		@estado,
		@estadoActual,
		@idxSingleStatus,
		@mensajeEA,
		@mensaje,
		@archivoXML,
		@archivoPDF,
		@idExterno,
		@fechaAlta,
		@idUsuario,
		@fechaModificacion,
		@idUsuarioModificacion
	)

	SET @Err = @@Error

	SELECT @secuencia = SCOPE_IDENTITY()

	RETURN @Err
END
GO


-- Display the status of Proc creation
IF (@@Error = 0) PRINT 'Procedure Creation: proc_cfdLogFacturaCompraInsert Succeeded'
ELSE PRINT 'Procedure Creation: proc_cfdLogFacturaCompraInsert Error on Creation'
GO

/****** Object:  StoredProcedure [proc_cfdLogFacturaCompraDelete]    Script Date: 30/09/2014 08:50:21 p.m. ******/
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[proc_cfdLogFacturaCompraDelete]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
    DROP PROCEDURE [proc_cfdLogFacturaCompraDelete];
GO

CREATE PROCEDURE [proc_cfdLogFacturaCompraDelete]
(
	@tipo smallint,
	@folio varchar(21),
	@idImpuestoTercero varchar(15),
	@estado varchar(20)
)
AS
BEGIN

	SET NOCOUNT OFF
	DECLARE @Err int

	DELETE
	FROM [cfdLogFacturaCompra]
	WHERE
		[tipo] = @tipo AND
		[folio] = @folio AND
		[idImpuestoTercero] = @idImpuestoTercero AND
		[estado] = @estado
	SET @Err = @@Error

	RETURN @Err
END
GO


-- Display the status of Proc creation
IF (@@Error = 0) PRINT 'Procedure Creation: proc_cfdLogFacturaCompraDelete Succeeded'
ELSE PRINT 'Procedure Creation: proc_cfdLogFacturaCompraDelete Error on Creation'
GO
