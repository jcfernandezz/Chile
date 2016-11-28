/****** Object:  View [dbo].[IMPRIME_COMPROBANTE_UNICO]    Script Date: 02/05/2014 16:32:49 ******/
IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[IMPRIME_COMPROBANTE_DTE]') AND OBJECTPROPERTY(id,N'IsView') = 1)
    DROP view dbo.[IMPRIME_COMPROBANTE_DTE];
GO

SET QUOTED_IDENTIFIER ON
GO

create VIEW [dbo].[IMPRIME_COMPROBANTE_DTE]
AS
--Propósito. Obtiene los datos de la factura y una copia. El campo modoImpresion controla si se imprime el original, la copia o ambos
--Usado por. Crystal. Imprime original y copia si modoImpresion = 'B'
--03/10/14 JCF Creación. 
--12/12/16 jcf Agrega caso factura 34
--
select 'A' modoImpresion, dte.LINEA, dte.SOPNUMBE, dte.SOPTYPE, dte.DOCDATE, dte.SUBTOTAL, dte.TAXAMNT, dte.DOCAMNT, 
	dte.CUSTNMBR, dte.DUEDATE, dte.PYMTRMID, 
	dte.[user_id], 	dte.Folio,	dte.CUSTNAME, 	dte.ADDRESS1, dte.ADDRESS2, dte.ADDRESS3, 	dte.CITY,	dte.[STATE], 
	dte.COUNTRY, dte.ZIP, dte.PHONE1, 	dte.PHONE2, dte.FAX, 	dte.tax_number, 	dte.[address], 	dte.address_country_code, 
	dte.shipping_address, 	dte.shipping_address_country_code, dte.license_address, 	dte.license_address_country_code,
	dte.article_id, dte.ITEMNMBR, dte.ITEMDESC, 	dte.UNITPRCE, 	dte.XTNDPRCE, dte.QUANTITY, 	dte.info, dte.img_url, dte.CURTEXT, 
	dte.ORDENVTA,	dte.FECHAORDVTA, dte.USERDEF1, dte.TRANSFA, dte.BANCO, 	dte.CUENTA, dte.CBU, dte.TRANSFTIT, dte.CMPNYNAM, dte.ADDRESS1CO,
	dte.ZIPCODECO, dte.CITYCO, dte.TELCO, dte.MAIL1CO, dte.MAIL2CO, dte.EMPRESA, dte.TAXREGTN,
	dte.CURNCYID, dte.TRDISAMT, 	dte.COMMENT1,	dte.ORDCPRA,	dte.TIPOTRABAJO,	dte.PEDIDOPOR,	dte.CLIENTE,	dte.PROMOCION,
	--dte.DIRVTAS,	dte.DIRDIRE,	dte.DIRBCO,	
	dte.memo,	dte.CHEKBKID,	dte.PYMTRCVD, dte.ACCTAMNT, dte.CHEKNMBR,
	dte.REFRENCE, dte.docType, dte.nombreTipoDoc, dte.TpoDocRef, dte.FolioRef,  dte.FchRef, dte.nombreTipoDocRef,
	dte.codigoBarras,	dte.nroResol, dte.fchResol,	dte.sTabla
from dbo.fCfdIMPRIME_COMPROBANTE_DTE() dte
union all
select 'B' modoImpresion, dte.LINEA, dte.SOPNUMBE, dte.SOPTYPE, dte.DOCDATE, dte.SUBTOTAL, dte.TAXAMNT, dte.DOCAMNT, 
	dte.CUSTNMBR, dte.DUEDATE, dte.PYMTRMID, 
	dte.[user_id], 	dte.Folio,	dte.CUSTNAME, 	dte.ADDRESS1, dte.ADDRESS2, dte.ADDRESS3, 	dte.CITY,	dte.[STATE], 
	dte.COUNTRY, dte.ZIP, dte.PHONE1, 	dte.PHONE2, dte.FAX, 	dte.tax_number, 	dte.[address], 	dte.address_country_code, 
	dte.shipping_address, 	dte.shipping_address_country_code, dte.license_address, 	dte.license_address_country_code,
	dte.article_id, dte.ITEMNMBR, dte.ITEMDESC, 	dte.UNITPRCE, 	dte.XTNDPRCE, dte.QUANTITY, 	dte.info, dte.img_url, dte.CURTEXT, 
	dte.ORDENVTA,	dte.FECHAORDVTA, dte.USERDEF1, dte.TRANSFA, dte.BANCO, 	dte.CUENTA, dte.CBU, dte.TRANSFTIT, dte.CMPNYNAM, dte.ADDRESS1CO,
	dte.ZIPCODECO, dte.CITYCO, dte.TELCO, dte.MAIL1CO, dte.MAIL2CO, dte.EMPRESA, dte.TAXREGTN,
	dte.CURNCYID, dte.TRDISAMT, 	dte.COMMENT1,	dte.ORDCPRA,	dte.TIPOTRABAJO,	dte.PEDIDOPOR,	dte.CLIENTE,	dte.PROMOCION,
	--dte.DIRVTAS,	dte.DIRDIRE,	dte.DIRBCO,	
	dte.memo,	dte.CHEKBKID,	dte.PYMTRCVD, dte.ACCTAMNT, dte.CHEKNMBR,
	dte.REFRENCE, dte.docType, dte.nombreTipoDoc, dte.TpoDocRef, dte.FolioRef,  dte.FchRef, dte.nombreTipoDocRef,
	dte.codigoBarras,	dte.nroResol, dte.fchResol,	dte.sTabla
from dbo.fCfdIMPRIME_COMPROBANTE_DTE() dte
where dte.docType in ( '33', '34')


go
IF (@@Error = 0) PRINT 'Creación exitosa de la vista: [IMPRIME_COMPROBANTE_DTE]  '
ELSE PRINT 'Error en la creación de la vista: [IMPRIME_COMPROBANTE_DTE] '
GO

---------------------------------------------------------------------------------------------------------
select top 10 *
from [IMPRIME_COMPROBANTE_DTE]
where sopnumbe = '33-00001919          '
order by sopnumbe


