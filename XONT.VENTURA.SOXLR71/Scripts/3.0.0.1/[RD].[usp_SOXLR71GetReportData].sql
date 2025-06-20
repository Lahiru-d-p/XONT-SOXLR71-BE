IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RD].[usp_SOXLR71GetReportData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [RD].[usp_SOXLR71GetReportData]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ==================================================================
-- Last Modified Version:   3.0.0.1
-- ==================================================================

CREATE PROCEDURE [RD].[usp_SOXLR71GetReportData]
@BusinessUnit CHAR(4)='',
@FromDate DATETIME=null,
@ToDate DATETIME=null,
@TerritoryCode CHAR(8)='',
@DistributorCode CHAR(8)='',
@VATOnlyFlag CHAR(1) = '1',
@NonVATOnlyFlag CHAR(1) = '1',
@ReportTypeFlag VARCHAR(10)= 'summary',

@ExecutionType CHAR(1)=''

AS BEGIN
	-----------------------------------Getting Data for Excel Report Start---------------------------------------------------

	IF @ExecutionType = '1'
	BEGIN
		SELECT TOP 1 VATRegistrationNo AS DistributorVATRegistrationNo
		FROM RD.Distributor WITH (NOLOCK)
		WHERE BusinessUnit = @BusinessUnit AND DistributorCode = @DistributorCode;
	END

	IF @ExecutionType = '2'
	BEGIN
		--V001 Remove Start
		--SELECT TOP 1 VATRegistrationNo AS DistributorVATRegistrationNo, DistributorCode
		--FROM RD.Distributor WITH (NOLOCK)
		--WHERE BusinessUnit = @BusinessUnit AND TerritoryCode = @TerritoryCode
		--ORDER BY TerritoryCode
		--VR001 Remove End

		--VR001 Add Start
		SELECT TOP 1 @DistributorCode = DistributorCode
		FROM RD.TerritoryControl WITH (NOLOCK)
		WHERE BusinessUnit = @BusinessUnit AND TerritoryCode = @TerritoryCode;

		SELECT TOP 1 VATRegistrationNo AS DistributorVATRegistrationNo, DistributorCode
		FROM RD.Distributor WITH (NOLOCK)
		WHERE BusinessUnit = @BusinessUnit AND DistributorCode = @DistributorCode;
		--VR001 Add Start

	END

	IF @ExecutionType = '3'
	BEGIN
		IF OBJECT_ID('tempdb..#SOXLR71_DATA') IS NOT NULL
		DROP TABLE #SOXLR71_DATA

		BEGIN
			CREATE TABLE #SOXLR71_DATA
			(
				BusinessUnit CHAR(4),
				TerritoryCode CHAR(4),
				TerritoryName VARCHAR(50),

				RetailerCode VARCHAR(15),
				RetailerName VARCHAR(75),
				VATRegistrationNo VARCHAR(20),

				DistributorCode VARCHAR(8),

				InvoiceNo INT,
				InvoiceDate DATETIME,

				TotalVATValue DECIMAL(15,4),
				TotalInvoiceValue DECIMAL(15,4)
			);
			CREATE CLUSTERED INDEX IX_SOXLR71_DATA 
				ON #SOXLR71_DATA (BusinessUnit, TerritoryCode, RetailerCode, DistributorCode, InvoiceNo);

			INSERT INTO #SOXLR71_DATA
				(
					BusinessUnit,
					TerritoryCode,

					RetailerCode,
					RetailerName,
					VATRegistrationNo,

					DistributorCode,
					InvoiceNo,
					InvoiceDate,
					TotalVATValue,
					TotalInvoiceValue
				)
				SELECT 
					SIH.BusinessUnit,
					SIH.TerritoryCode,

					Ret.RetailerCode,
					Ret.RetailerName,
					Ret.VATRegistrationNo,

					SIH.DistributorCode,
					SIH.InvoiceNo,
					SIH.InvoiceDate,

					SIH.TotalVATValue,
					SIH.TotalInvoiceValue

				FROM [RD].[SalesInvoiceHeader] SIH WITH (NOLOCK)

				INNER JOIN RD.Retailer Ret WITH (NOLOCK) ON 
					Ret.BusinessUnit = SIH.BusinessUnit AND 
					Ret.RetailerCode = SIH.RetailerCode

				WHERE 
					SIH.BusinessUnit = @BusinessUnit 
					AND SIH.TerritoryCode = CASE WHEN @TerritoryCode IS NOT NULL AND @TerritoryCode <> '' THEN @TerritoryCode ELSE SIH.TerritoryCode END
					AND SIH.DistributorCode = CASE WHEN @DistributorCode IS NOT NULL AND @DistributorCode <> '' THEN @DistributorCode ELSE SIH.DistributorCode END
					AND SIH.InvoiceDate BETWEEN CONVERT(DATE, @FromDate) AND CONVERT(DATE, @ToDate)

		IF @ReportTypeFlag = 'detail'
			BEGIN

			DECLARE @ZeroVatCountDetail INT;
			DECLARE @NonZeroVATCountDetail INT

			SELECT 			
				@ZeroVatCountDetail = COUNT(CASE WHEN ISNULL(TotalVATValue, 0) = 0 THEN 1 END),
				@NonZeroVATCountDetail = COUNT(CASE WHEN ISNULL(TotalVATValue, 0) != 0 THEN 1 END)
			FROM #SOXLR71_DATA WITH (NOLOCK)
			WHERE
				   (@VATOnlyFlag	= '1' AND TotalVATValue	  != 0  )
				OR (@NonVATOnlyFlag = '1' AND TotalVATValue	   = 0  )
				OR (@VATOnlyFlag	= '1' AND @NonVATOnlyFlag  = '1');

			SELECT 			
				BusinessUnit,
				TerritoryCode,

				RetailerCode,
				RetailerName,
				VATRegistrationNo AS RetailerVatRegistrationNumber,

				DistributorCode,
				InvoiceNo,
				InvoiceDate,
				TotalVATValue AS VATValue,
				TotalInvoiceValue AS TotalSales,
				CASE 
					WHEN TotalVATValue != 0 THEN TotalInvoiceValue
					ELSE 0
				END AS VATSales,

				CASE 
					WHEN TotalVATValue = 0 THEN TotalInvoiceValue
					ELSE 0
				END AS NonVATSales,
				@ZeroVatCountDetail AS ZeroVATCount,
				@NonZeroVATCountDetail AS NonZeroVATCount
			FROM #SOXLR71_DATA WITH (NOLOCK)
			WHERE
				   (@VATOnlyFlag	= '1' AND TotalVATValue	  != 0  )
				OR (@NonVATOnlyFlag = '1' AND TotalVATValue	   = 0  )
				OR (@VATOnlyFlag	= '1' AND @NonVATOnlyFlag  = '1');

			END

		ELSE
			BEGIN

			DECLARE @ZeroVatCountSummary INT;
			DECLARE @NonZeroVATCountSummary INT

			SELECT 			
				@ZeroVatCountSummary = COUNT(CASE WHEN ISNULL(TotalVATValue, 0) = 0 THEN 1 END),
				@NonZeroVATCountSummary = COUNT(CASE WHEN ISNULL(TotalVATValue, 0) != 0 THEN 1 END)
			FROM #SOXLR71_DATA WITH (NOLOCK)
			WHERE
				   (@VATOnlyFlag	= '1' AND TotalVATValue	  != 0  )
				OR (@NonVATOnlyFlag = '1' AND TotalVATValue	   = 0  )
				OR (@VATOnlyFlag	= '1' AND @NonVATOnlyFlag  = '1');

			SELECT 			
				BusinessUnit,
				TerritoryCode,
				RetailerCode,
				RetailerName,
				VATRegistrationNo AS RetailerVatRegistrationNumber,
				SUM(CASE WHEN ISNULL(TotalVATValue, 0) != 0 THEN ISNULL(TotalInvoiceValue, 0) ELSE 0 END) AS VATSales,
				SUM(CASE WHEN ISNULL(TotalVATValue, 0) = 0 THEN ISNULL(TotalInvoiceValue, 0) ELSE 0 END) AS NonVATSales,
				SUM(ISNULL(TotalVATValue, 0)) AS VATValue,
				SUM(ISNULL(TotalInvoiceValue, 0)) AS TotalSales,
				@ZeroVatCountSummary AS ZeroVATCount,
				@NonZeroVATCountSummary AS NonZeroVATCount
			FROM 
				#SOXLR71_DATA WITH (NOLOCK)
			WHERE
				(@VATOnlyFlag = '1' AND ISNULL(TotalVATValue, 0) != 0)
				OR (@NonVATOnlyFlag = '1' AND ISNULL(TotalVATValue, 0) = 0)
				OR (@VATOnlyFlag = '1' AND @NonVATOnlyFlag = '1')
			GROUP BY 
				BusinessUnit,
				TerritoryCode,
				RetailerCode,
				RetailerName,
				VATRegistrationNo;
			END
		END
	END
END