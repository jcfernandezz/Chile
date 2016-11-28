
/****** Object:  StoredProcedure [proc_cfdLogFacturaXMLLoadByPrimaryKey]    Script Date: 27/09/2014 10:17:15 p.m. ******/
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[proc_cfdLogFacturaXMLLoadByPrimaryKey]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
    DROP PROCEDURE [proc_cfdLogFacturaXMLLoadByPrimaryKey];
GO

CREATE PROCEDURE [proc_cfdLogFacturaXMLLoadByPrimaryKey]
(
	@soptype smallint,
	@sopnumbe varchar(21),
	@secuencia int
)
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @Err int

	SELECT
		[soptype],
		[sopnumbe],
		[secuencia],
		[estado],
		[mensaje],
		[estadoActual],
		[mensajeEA],
		[noAprobacion],
		[fechaEmision],
		[idUsuario],
		[fechaAnulacion],
		[idUsuarioAnulacion],
		[archivoXML],
		[idExterno]
	FROM [cfdLogFacturaXML]
	WHERE
		([soptype] = @soptype) AND
		([sopnumbe] = @sopnumbe) AND
		([secuencia] = @secuencia)

	SET @Err = @@Error

	RETURN @Err
END
GO


-- Display the status of Proc creation
IF (@@Error = 0) PRINT 'Procedure Creation: proc_cfdLogFacturaXMLLoadByPrimaryKey Succeeded'
ELSE PRINT 'Procedure Creation: proc_cfdLogFacturaXMLLoadByPrimaryKey Error on Creation'
GO

/****** Object:  StoredProcedure [proc_cfdLogFacturaXMLLoadAll]    Script Date: 27/09/2014 10:17:15 p.m. ******/
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[proc_cfdLogFacturaXMLLoadAll]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
    DROP PROCEDURE [proc_cfdLogFacturaXMLLoadAll];
GO

CREATE PROCEDURE [proc_cfdLogFacturaXMLLoadAll]
AS
BEGIN

	SET NOCOUNT ON
	DECLARE @Err int

	SELECT
		[soptype],
		[sopnumbe],
		[secuencia],
		[estado],
		[mensaje],
		[estadoActual],
		[mensajeEA],
		[noAprobacion],
		[fechaEmision],
		[idUsuario],
		[fechaAnulacion],
		[idUsuarioAnulacion],
		[archivoXML],
		[idExterno]
	FROM [cfdLogFacturaXML]

	SET @Err = @@Error

	RETURN @Err
END
GO


-- Display the status of Proc creation
IF (@@Error = 0) PRINT 'Procedure Creation: proc_cfdLogFacturaXMLLoadAll Succeeded'
ELSE PRINT 'Procedure Creation: proc_cfdLogFacturaXMLLoadAll Error on Creation'
GO

/****** Object:  StoredProcedure [proc_cfdLogFacturaXMLUpdate]    Script Date: 27/09/2014 10:17:15 p.m. ******/
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[proc_cfdLogFacturaXMLUpdate]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
    DROP PROCEDURE [proc_cfdLogFacturaXMLUpdate];
GO

CREATE PROCEDURE [proc_cfdLogFacturaXMLUpdate]
(
	@soptype smallint,
	@sopnumbe varchar(21),
	@secuencia int,
	@estado varchar(20),
	@mensaje varchar(255),
	@estadoActual varchar(20) = NULL,
	@mensajeEA varchar(255) = NULL,
	@noAprobacion varchar(21),
	@fechaEmision datetime,
	@idUsuario varchar(10),
	@fechaAnulacion datetime,
	@idUsuarioAnulacion varchar(10),
	@archivoXML xml = NULL,
	@idExterno varchar(25) = NULL
)
AS
BEGIN

	SET NOCOUNT OFF
	DECLARE @Err int

	UPDATE [cfdLogFacturaXML]
	SET
		[estado] = @estado,
		[mensaje] = @mensaje,
		[estadoActual] = @estadoActual,
		[mensajeEA] = @mensajeEA,
		[noAprobacion] = @noAprobacion,
		[fechaEmision] = @fechaEmision,
		[idUsuario] = @idUsuario,
		[fechaAnulacion] = @fechaAnulacion,
		[idUsuarioAnulacion] = @idUsuarioAnulacion,
		[archivoXML] = @archivoXML,
		[idExterno] = @idExterno
	WHERE
		[soptype] = @soptype
	AND	[sopnumbe] = @sopnumbe
	AND	[secuencia] = @secuencia


	SET @Err = @@Error


	RETURN @Err
END
GO


-- Display the status of Proc creation
IF (@@Error = 0) PRINT 'Procedure Creation: proc_cfdLogFacturaXMLUpdate Succeeded'
ELSE PRINT 'Procedure Creation: proc_cfdLogFacturaXMLUpdate Error on Creation'
GO




/****** Object:  StoredProcedure [proc_cfdLogFacturaXMLInsert]    Script Date: 27/09/2014 10:17:15 p.m. ******/
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[proc_cfdLogFacturaXMLInsert]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
    DROP PROCEDURE [proc_cfdLogFacturaXMLInsert];
GO

CREATE PROCEDURE [proc_cfdLogFacturaXMLInsert]
(
	@soptype smallint,
	@sopnumbe varchar(21),
	@secuencia int = NULL output,
	@estado varchar(20),
	@mensaje varchar(255),
	@estadoActual varchar(20) = NULL,
	@mensajeEA varchar(255) = NULL,
	@noAprobacion varchar(21),
	@fechaEmision datetime,
	@idUsuario varchar(10),
	@fechaAnulacion datetime,
	@idUsuarioAnulacion varchar(10),
	@archivoXML xml = NULL,
	@idExterno varchar(25) = NULL
)
AS
BEGIN

	SET NOCOUNT OFF
	DECLARE @Err int

	INSERT
	INTO [cfdLogFacturaXML]
	(
		[soptype],
		[sopnumbe],
		[estado],
		[mensaje],
		[estadoActual],
		[mensajeEA],
		[noAprobacion],
		[fechaEmision],
		[idUsuario],
		[fechaAnulacion],
		[idUsuarioAnulacion],
		[archivoXML],
		[idExterno]
	)
	VALUES
	(
		@soptype,
		@sopnumbe,
		@estado,
		@mensaje,
		@estadoActual,
		@mensajeEA,
		@noAprobacion,
		@fechaEmision,
		@idUsuario,
		@fechaAnulacion,
		@idUsuarioAnulacion,
		@archivoXML,
		@idExterno
	)

	SET @Err = @@Error

	SELECT @secuencia = SCOPE_IDENTITY()

	RETURN @Err
END
GO


-- Display the status of Proc creation
IF (@@Error = 0) PRINT 'Procedure Creation: proc_cfdLogFacturaXMLInsert Succeeded'
ELSE PRINT 'Procedure Creation: proc_cfdLogFacturaXMLInsert Error on Creation'
GO

/****** Object:  StoredProcedure [proc_cfdLogFacturaXMLDelete]    Script Date: 27/09/2014 10:17:15 p.m. ******/
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[proc_cfdLogFacturaXMLDelete]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
    DROP PROCEDURE [proc_cfdLogFacturaXMLDelete];
GO

CREATE PROCEDURE [proc_cfdLogFacturaXMLDelete]
(
	@soptype smallint,
	@sopnumbe varchar(21),
	@secuencia int
)
AS
BEGIN

	SET NOCOUNT OFF
	DECLARE @Err int

	DELETE
	FROM [cfdLogFacturaXML]
	WHERE
		[soptype] = @soptype AND
		[sopnumbe] = @sopnumbe AND
		[secuencia] = @secuencia
	SET @Err = @@Error

	RETURN @Err
END
GO


-- Display the status of Proc creation
IF (@@Error = 0) PRINT 'Procedure Creation: proc_cfdLogFacturaXMLDelete Succeeded'
ELSE PRINT 'Procedure Creation: proc_cfdLogFacturaXMLDelete Error on Creation'
GO
