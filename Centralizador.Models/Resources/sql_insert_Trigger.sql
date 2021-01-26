IF NOT EXISTS (
		SELECT *
		FROM sys.objects
		WHERE name = 'IW_GSAEN_REF_DTE_CEN'
			AND type = 'TR'
		)
	EXEC dbo.sp_executesql @statement = 
		N'

CREATE TRIGGER [softland].[IW_GSAEN_REF_DTE_CEN] ON [softland].[iw_gsaen_refdte]
AFTER INSERT
AS
DECLARE @TipoDte VARCHAR(1);

SELECT @TipoDte = (
		SELECT i.Tipo
		FROM inserted i
		);--F / N

IF @TipoDte = ''F''
BEGIN
	SET NOCOUNT ON;
	DECLARE @FolioRef VARCHAR(40);-- DE04457A19C47
	DECLARE @RazonRef VARCHAR(40);--SEN_[]
	DECLARE @FchRef DATETIME;
	DECLARE @NroInt INT;
	DECLARE @RefNode VARCHAR(max);
	DECLARE @FolioF INT;
	DECLARE @rut VARCHAR(15);

	SELECT @NroInt = (
			SELECT i.NroInt
			FROM inserted i
			);

	SELECT @FolioRef =  (
			SELECT TOP (1) nv.codlugardesp
			FROM softland.iw_gsaen_refdte n
			JOIN softland.iw_gsaen g ON g.nroint = n.nroint
				AND n.tipo = g.tipo
			JOIN softland.nw_nventa nv ON nv.nvnumero = g.nvnumero
			WHERE n.nroint = @NroInt
				AND n.Tipo = ''F''
			)

	SELECT @RazonRef = ''SEN_'' + (
			SELECT TOP (1) nv.SolicitadoPor
			FROM softland.iw_gsaen_refdte n
			JOIN softland.iw_gsaen g ON g.nroint = n.nroint
				AND n.tipo = g.tipo
			JOIN softland.nw_nventa nv ON nv.nvnumero = g.nvnumero
			WHERE n.nroint = @NroInt
				AND n.Tipo = ''F''
			)

	SELECT @FchRef = (
			SELECT TOP (1) nv.nvFem
			FROM softland.iw_gsaen_refdte n
			JOIN softland.iw_gsaen g ON g.nroint = n.nroint
				AND n.tipo = g.tipo
			JOIN softland.nw_nventa nv ON nv.nvnumero = g.nvnumero
			WHERE n.nroint = @NroInt
				AND n.Tipo = ''F''
			)

	SELECT @FolioF = (
			SELECT TOP (1) g.Folio
			FROM softland.iw_gsaen_refdte n
			JOIN softland.iw_gsaen g ON g.nroint = n.nroint
				AND n.tipo = g.tipo
			JOIN softland.nw_nventa nv ON nv.nvnumero = g.nvnumero
			WHERE n.nroint = @NroInt
				AND n.Tipo = ''F''
			)

	SELECT @rut = (
			SELECT RutEmisor
			FROM softland.soempre
			)

	SELECT @RefNode = CONCAT (
			''</Referencia><Referencia><NroLinRef>2</NroLinRef><TpoDocRef>SEN</TpoDocRef><FolioRef>'',
			@FolioRef,
			''</FolioRef><FchRef>'',
			@FchRef,
			''</FchRef><RazonRef>'',
			@RazonRef,
			''</RazonRef></Referencia><TED version="1.0">''
			)

	-- INSERT REF IN IW_GSaEn_RefDTE
	IF NOT EXISTS (
			SELECT *
			FROM softland.IW_GSaEn_RefDTE r,
				inserted
			WHERE r.NroInt = inserted.NroInt
				AND r.Tipo = ''F''
				AND r.CodRefSII = ''SEN''
			)
	BEGIN
		INSERT INTO softland.IW_GSaEn_RefDTE (
			Tipo,
			NroInt,
			LineaRef,
			CodRefSII,
			FolioRef,
			FechaRef,
			Glosa
			)
		VALUES (
			''F'',
			@NroInt,
			2,
			''SEN'',
			@FolioRef,
			@FchRef,
			@RazonRef
			)
	END

	--UPDATE DTE FILE
	BEGIN
		UPDATE softland.DTE_Archivos
		SET Archivo = CAST(REPLACE(cast(archivo AS NVARCHAR(max)), ''</Referencia><TED version="1.0">'', @RefNode) AS NTEXT)
		WHERE Tipo = ''F''
			AND NroInt = @NroInt
			AND TipoXML = ''D''
			AND TipoDTE = 33
	END

	--INSERT REF IN DTE_DocRef
	IF NOT EXISTS (
			SELECT *
			FROM softland.DTE_DocRef
			WHERE Folio = @FolioF
				AND TipoDTE = 33
				AND TpoDocRef = ''SEN''
			)
	BEGIN
		INSERT INTO softland.DTE_DocRef (
			RUTEmisor,
			TipoDTE,
			Folio,
			NroLinRef,
			TpoDocRef,
			FolioRef,
			FchRef,
			RazonRef
			)
		VALUES (
			@rut,
			33,
			@FolioF,
			2,
			''SEN'',
			@FolioRef,
			@FchRef,
			@RazonRef
			)
	END
END

'