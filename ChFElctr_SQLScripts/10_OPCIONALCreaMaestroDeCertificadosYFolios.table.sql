/****** Object:  Table [dbo].[cfd_CER00100]    Script Date: 01/07/2011 19:56:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
--Propósito. Estas tablas contienen los datos de los certificados y los rangos de folios de Gila Chile
--Estas tablas son creadas por el producto CNK 10.0.1 del desarrollo de Certificados y folios para dexterity en GP 10.
--
IF not EXISTS (SELECT 1 FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[cfd_FOL00100]') AND OBJECTPROPERTY(id,N'IsTable') = 1)
begin
	CREATE TABLE [dbo].[cfd_FOL00100](
	[SOPTYPE] [smallint] NOT NULL,
	[DOCID] [char](15) NOT NULL,
	[num_folio_desde] [int] NOT NULL,
	[num_folio_hasta] [int] NOT NULL,
	[ruta_codigo_autorizacion] [char](255) NOT NULL,
	[DEX_ROW_ID] [int] IDENTITY(1,1) NOT NULL,
	CONSTRAINT [PKcfd_FOL00100] PRIMARY KEY NONCLUSTERED 
	(
	[SOPTYPE] ASC,
	[DOCID] ASC,
	[num_folio_desde] ASC,
	[num_folio_hasta] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
	ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
end
go
 --drop table cfd_CER00100;
IF not EXISTS (SELECT 1 FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[cfd_CER00100]') AND OBJECTPROPERTY(id,N'IsTable') = 1)
begin
	CREATE TABLE [dbo].[cfd_CER00100](
	[USERID] [char](35) NOT NULL,
	[ACA_RUT] [char](9) NOT NULL,
	[fecha_vig_desde] [datetime] NOT NULL,
	[fecha_vig_hasta] [datetime] NOT NULL,
	[ruta_certificado] [char](255) NOT NULL,
	[ruta_clave] [char](255) NOT NULL,
	[contrasenia_clave] [char](20) NOT NULL,
	[ACA_SolicitaFolio] [tinyint] NOT NULL,
	[ACA_AnulaDocumentos] [tinyint] NOT NULL,
	[ACA_EnviaDocumentos] [tinyint] NOT NULL,
	[ACA_FirmaDocumentos] [tinyint] NOT NULL,
	[ACTIVE] [tinyint] NOT NULL,
	[DEX_ROW_ID] [int] IDENTITY(1,1) NOT NULL,
	CONSTRAINT [PKcfd_CER00100] PRIMARY KEY NONCLUSTERED 
	(
	[USERID] ASC,
	[ACA_RUT] ASC,
	[fecha_vig_desde] ASC,
	[fecha_vig_hasta] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
	ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
end
go
------------------------------------------------------------------------------------------------------------------------
--test
--alter table cfd_cer00100 drop constraint pkcfd_cer00100;
--alter table cfd_cer00100 alter column [USERID] char(35);
--alter table cfd_cer00100 add constraint pkcfd_cer00100 primary key (userid, aca_rut, fecha_vig_desde, fecha_vig_hasta);
--alter table [cfd_CER00100] alter column [contrasenia_clave] char(20) not null

--create index idx1_cfd_cer00100 on cfd_cer00100 (userid, aca_rut, fecha_vig_desde, fecha_vig_hasta);

--select *
--from [cfd_CER00100]

--update cfd_cer00100 set contrasenia_clave = 'interfactura54'
--where userid = 'sa'

--select *
--from cfd_FOL00100
--where soptype = 3
--and DOCID = 'FACTURA ELCTRNC'
--and 2 between num_folio_desde and num_folio_hasta

--sp_statistics [cfd_CER00100]
--sp_columns cfd_cer00100
