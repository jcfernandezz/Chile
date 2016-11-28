--------------------------------------------------------------------------------------------------------

IF OBJECT_ID ('dbo.fCfdDatosAdicionales') IS NOT NULL
   DROP FUNCTION dbo.fCfdDatosAdicionales
GO

create function dbo.fCfdDatosAdicionales(@orpmtrvd numeric(21,5), @soptype smallint, @sopnumbe varchar(21), @custnmbr varchar(15), @prbtadcd varchar(15))
returns table
as
--Propósito. Devuelve datos adicionales de la factura
--Requisitos. 
--30/6/14 jcf Creación FE Chile
--8/7/14 jcf Agrega usrdat02
--
return
( 
	select rtrim(ctrl.USERDEF2) sopUserDef2,
		rtrim(ctrl.usrtab01) sopUserTab01,
		ctrl.USERDEF1 sopUserDef1, ctrl.usrdat02
	from SOP10106 ctrl					--campos def. por el usuario.
	where ctrl.soptype = @soptype
	and ctrl.sopnumbe = @sopnumbe
)
go

IF (@@Error = 0) PRINT 'Creación exitosa de la función: fCfdDatosAdicionales()'
ELSE PRINT 'Error en la creación de la función: fCfdDatosAdicionales()'
GO
--------------------------------------------------------------------------------------------------------

IF OBJECT_ID ('dbo.fCfdUofMSAT') IS NOT NULL
   DROP FUNCTION dbo.fCfdUofMSAT
GO

create function dbo.fCfdUofMSAT(@UOMSCHDL varchar(11), @UOFM varchar(9))
returns table
as
--Propósito. Obtiene la descripción larga de la unidad de medida 
--Requisitos. 
--02/08/12 jcf Creación 
--
return
( 
	select UOFMLONGDESC
	from iv40202	--unidades de medida [UOMSCHDL SEQNUMBR]
	WHERE UOMSCHDL = @UOMSCHDL
	and UOFM = @UOFM 
)
go

IF (@@Error = 0) PRINT 'Creación exitosa de la función: fCfdUofMSAT()'
ELSE PRINT 'Error en la creación de la función: fCfdUofMSAT()'
GO

------------------------------------------------------------------------------------------
--select *
--from dbo.fCfdDatosAdicionales(0, 3, 'FV A0001-00000016', 'C0004', 'PRINCIPAL')

