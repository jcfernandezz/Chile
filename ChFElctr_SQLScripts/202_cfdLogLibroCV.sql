
USE [GCHI]
GO

/****** Object:  StoredProcedure [proc_cfdLogLibroCVLoadByPrimaryKey]    Script Date: 21/07/2014 03:15:07 p.m. ******/
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[proc_cfdLogLibroCVLoadByPrimaryKey]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
    DROP PROCEDURE [proc_cfdLogLibroCVLoadByPrimaryKey];
GO

CREATE PROCEDURE [proc_cfdLogLibroCVLoadByPrimaryKey]
(
	@periodo int,
	@tipo varchar(4),
	@estado varchar(20)
)
AS
BEGIN
	SET NOCOUNT ON
	DECLARE @Err int

	SELECT
		[secuencia],
		[periodo],
		[tipo],
		[estado],
		[mensajeGral],
		[estadoActualBin],
		[idxSingleStatus],
		[mensajeEActual],
		[archivoXML],
		[fechaAlta],
		[idUsuario],
		[fechaModificacion],
		[idUsuarioModificacion]
	FROM [cfdLogLibroCV]
	WHERE
		([periodo] = @periodo) AND
		([tipo] = @tipo) AND
		([estado] = @estado)

	SET @Err = @@Error

	RETURN @Err
END
GO


-- Display the status of Proc creation
IF (@@Error = 0) PRINT 'Procedure Creation: proc_cfdLogLibroCVLoadByPrimaryKey Succeeded'
ELSE PRINT 'Procedure Creation: proc_cfdLogLibroCVLoadByPrimaryKey Error on Creation'
GO

/****** Object:  StoredProcedure [proc_cfdLogLibroCVLoadAll]    Script Date: 21/07/2014 03:15:07 p.m. ******/
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[proc_cfdLogLibroCVLoadAll]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
    DROP PROCEDURE [proc_cfdLogLibroCVLoadAll];
GO

CREATE PROCEDURE [proc_cfdLogLibroCVLoadAll]
AS
BEGIN

	SET NOCOUNT ON
	DECLARE @Err int

	SELECT
		[secuencia],
		[periodo],
		[tipo],
		[estado],
		[mensajeGral],
		[estadoActualBin],
		[idxSingleStatus],
		[mensajeEActual],
		[archivoXML],
		[fechaAlta],
		[idUsuario],
		[fechaModificacion],
		[idUsuarioModificacion]
	FROM [cfdLogLibroCV]

	SET @Err = @@Error

	RETURN @Err
END
GO


-- Display the status of Proc creation
IF (@@Error = 0) PRINT 'Procedure Creation: proc_cfdLogLibroCVLoadAll Succeeded'
ELSE PRINT 'Procedure Creation: proc_cfdLogLibroCVLoadAll Error on Creation'
GO

/****** Object:  StoredProcedure [proc_cfdLogLibroCVUpdate]    Script Date: 21/07/2014 03:15:07 p.m. ******/
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[proc_cfdLogLibroCVUpdate]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
    DROP PROCEDURE [proc_cfdLogLibroCVUpdate];
GO

CREATE PROCEDURE [proc_cfdLogLibroCVUpdate]
(
	@secuencia int,
	@periodo int,
	@tipo varchar(4),
	@estado varchar(20),
	@mensajeGral varchar(255),
	@estadoActualBin varchar(20) = NULL,
	@idxSingleStatus smallint = NULL,
	@mensajeEActual varchar(255) = NULL,
	@archivoXML xml = NULL,
	@fechaAlta datetime,
	@idUsuario varchar(10),
	@fechaModificacion datetime,
	@idUsuarioModificacion varchar(10)
)
AS
BEGIN

	SET NOCOUNT OFF
	DECLARE @Err int

	UPDATE [cfdLogLibroCV]
	SET
		[mensajeGral] = @mensajeGral,
		[estadoActualBin] = @estadoActualBin,
		[idxSingleStatus] = @idxSingleStatus,
		[mensajeEActual] = @mensajeEActual,
		[archivoXML] = @archivoXML,
		[fechaAlta] = @fechaAlta,
		[idUsuario] = @idUsuario,
		[fechaModificacion] = @fechaModificacion,
		[idUsuarioModificacion] = @idUsuarioModificacion
	WHERE
		[periodo] = @periodo
	AND	[tipo] = @tipo
	AND	[estado] = @estado


	SET @Err = @@Error


	RETURN @Err
END
GO


-- Display the status of Proc creation
IF (@@Error = 0) PRINT 'Procedure Creation: proc_cfdLogLibroCVUpdate Succeeded'
ELSE PRINT 'Procedure Creation: proc_cfdLogLibroCVUpdate Error on Creation'
GO




/****** Object:  StoredProcedure [proc_cfdLogLibroCVInsert]    Script Date: 21/07/2014 03:15:07 p.m. ******/
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[proc_cfdLogLibroCVInsert]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
    DROP PROCEDURE [proc_cfdLogLibroCVInsert];
GO

CREATE PROCEDURE [proc_cfdLogLibroCVInsert]
(
	@secuencia int = NULL output,
	@periodo int,
	@tipo varchar(4),
	@estado varchar(20),
	@mensajeGral varchar(255),
	@estadoActualBin varchar(20) = NULL,
	@idxSingleStatus smallint = NULL,
	@mensajeEActual varchar(255) = NULL,
	@archivoXML xml = NULL,
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
	INTO [cfdLogLibroCV]
	(
		[periodo],
		[tipo],
		[estado],
		[mensajeGral],
		[estadoActualBin],
		[idxSingleStatus],
		[mensajeEActual],
		[archivoXML],
		[fechaAlta],
		[idUsuario],
		[fechaModificacion],
		[idUsuarioModificacion]
	)
	VALUES
	(
		@periodo,
		@tipo,
		@estado,
		@mensajeGral,
		@estadoActualBin,
		@idxSingleStatus,
		@mensajeEActual,
		@archivoXML,
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
IF (@@Error = 0) PRINT 'Procedure Creation: proc_cfdLogLibroCVInsert Succeeded'
ELSE PRINT 'Procedure Creation: proc_cfdLogLibroCVInsert Error on Creation'
GO

/****** Object:  StoredProcedure [proc_cfdLogLibroCVDelete]    Script Date: 21/07/2014 03:15:07 p.m. ******/
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[proc_cfdLogLibroCVDelete]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)
    DROP PROCEDURE [proc_cfdLogLibroCVDelete];
GO

CREATE PROCEDURE [proc_cfdLogLibroCVDelete]
(
	@periodo int,
	@tipo varchar(4),
	@estado varchar(20)
)
AS
BEGIN

	SET NOCOUNT OFF
	DECLARE @Err int

	DELETE
	FROM [cfdLogLibroCV]
	WHERE
		[periodo] = @periodo AND
		[tipo] = @tipo AND
		[estado] = @estado
	SET @Err = @@Error

	RETURN @Err
END
GO


-- Display the status of Proc creation
IF (@@Error = 0) PRINT 'Procedure Creation: proc_cfdLogLibroCVDelete Succeeded'
ELSE PRINT 'Procedure Creation: proc_cfdLogLibroCVDelete Error on Creation'
GO
