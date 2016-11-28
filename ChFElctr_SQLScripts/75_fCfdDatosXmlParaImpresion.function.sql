IF OBJECT_ID ('dbo.fCfdDatosXmlParaImpresion') IS NOT NULL
   drop function dbo.fCfdDatosXmlParaImpresion
go

create function dbo.fCfdDatosXmlParaImpresion(@archivoXml xml)
--Propósito. Obtiene los datos de la factura electrónica
--Usado por. vwCfdTransaccionesDeVenta
--Requisitos. dte
--05/02/14 jcf Creación Fe Chile
--27/09/14 jcf Agrega FchEmis
--12/10/16 jcf Agrega segunda fila de referencias
--
returns table
return(
	select
	@archivoXml.value('(/DTE/Documento/Encabezado/Receptor/RznSocRecep/text())[1]', 'varchar(61)') RznSocRecep,
	@archivoXml.value('(/DTE/Documento/Encabezado/Receptor/DirRecep/text())[1]', 'varchar(61)') DirRecep,
	@archivoXml.value('(/DTE/Documento/Encabezado/Receptor/GiroRecep/text())[1]', 'varchar(41)') GiroRecep,
	@archivoXml.value('(/DTE/Documento/Encabezado/Receptor/RUTRecep/text())[1]', 'varchar(11)') RUTRecep,
	@archivoXml.value('(/DTE/Documento/Encabezado/Receptor/CmnaRecep/text())[1]', 'varchar(20)') CmnaRecep,
	@archivoXml.value('(/DTE/Documento/Encabezado/Receptor/CiudadRecep/text())[1]', 'varchar(20)') CiudadRecep,
	@archivoXml.value('(/DTE/Documento/Encabezado/IdDoc/Folio/text())[1]', 'varchar(20)') Folio,
	@archivoXml.value('(/DTE/Documento/Encabezado/IdDoc/FchEmis/text())[1]', 'varchar(20)') FchEmis,
	@archivoXml.value('(/DTE/Documento/Referencia/TpoDocRef/text())[1]', 'varchar(10)') TpoDocRef,
	@archivoXml.value('(/DTE/Documento/Referencia/FolioRef/text())[1]', 'varchar(20)') FolioRef,
	@archivoXml.value('(/DTE/Documento/Referencia/FchRef/text())[1]', 'varchar(20)') FchRef,
	@archivoXml.value('(/DTE/Documento/Referencia/TpoDocRef/text())[2]', 'varchar(10)') TpoDocRef2,
	@archivoXml.value('(/DTE/Documento/Referencia/FolioRef/text())[2]', 'varchar(20)') FolioRef2,
	@archivoXml.value('(/DTE/Documento/Referencia/FchRef/text())[2]', 'varchar(20)') FchRef2
	)
go

IF (@@Error = 0) PRINT 'Creación exitosa de: fCfdDatosXmlParaImpresion()'
ELSE PRINT 'Error en la creación de: fCfdDatosXmlParaImpresion()'
GO

--------------------------------------------------------------------------------------
