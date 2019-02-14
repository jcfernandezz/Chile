IF OBJECT_ID('SP_cfd_FOL00100','P') IS NOT NULL
DROP PROC dbo.SP_cfd_FOL00100
GO

CREATE PROCEDURE dbo.SP_cfd_FOL00100@SOPTYPE                      smallint,@DOCID                        char(15),@num_folio_desde              int,@num_folio_hasta              int,@ruta_codigo_autorizacion     char(255) = NULLAS
IF EXISTS (SELECT 1 FROM dbo.cfd_FOL00100WHERE SOPTYPE = @SOPTYPE   AND DOCID = @DOCID   --AND num_folio_desde = @num_folio_desde   --AND num_folio_hasta = @num_folio_hasta )
BEGIN
 
DELETE FROM dbo.cfd_FOL00100 WHERE SOPTYPE = @SOPTYPE   AND DOCID = @DOCID   --AND num_folio_desde = @num_folio_desde   --AND num_folio_hasta = @num_folio_hasta 
END
 
INSERT INTO dbo.cfd_FOL00100(SOPTYPE,DOCID,num_folio_desde,num_folio_hasta,ruta_codigo_autorizacion)SELECT @SOPTYPE,@DOCID,@num_folio_desde,@num_folio_hasta,@ruta_codigo_autorizacion
 
 
GO

--sp_statistics cfd_FOL00100
