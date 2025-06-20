IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RD].[usp_SOXLR71GetPromptData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [RD].[usp_SOXLR71GetPromptData]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ==================================================================
-- Last Modified Version:   3.0.0.0
-- ==================================================================

CREATE PROCEDURE [RD].[usp_SOXLR71GetPromptData]
@BusinessUnit CHAR(4)='',
@Username VARCHAR(40)  = '',
@PowerUser CHAR(1)  = '',

@FromDate DATETIME=null,
@ToDate DATETIME=null,

@ExecutionType CHAR(2)='',

@DistributorCode CHAR(8)='',
@DistributorName VARCHAR(50) = '',

@TerritoryCode CHAR(8)='',
@TerritoryName VARCHAR(50) = ''


AS BEGIN
	DECLARE @ApplyUserMasterValueForTETY char(1)
	SET @ApplyUserMasterValueForTETY='0'
			IF (@PowerUser='0')
			BEGIN		
				BEGIN
					SET @ApplyUserMasterValueForTETY='1'
				END 
			END

	--Create temporary table for Territory
		CREATE TABLE #SOXLR71_TERRITORY 
				(
					BusinessUnit CHAR(4) NOT NULL,
					TerritoryCode VARCHAR(10) PRIMARY KEY,
					TerritoryName VARCHAR(50)
				)
	
		IF @TerritoryCode = ''
		BEGIN
			IF @ApplyUserMasterValueForTETY = '1'
				INSERT INTO #SOXLR71_TERRITORY  
				SELECT TETY.BusinessUnit,TETY.TerritoryCode,TETY.Description 
				FROM XA.UserTerritory TETY WITH(NOLOCK)
				WHERE TETY.BusinessUnit=@BusinessUnit AND TETY.UserName=@Username			 
			ELSE
				INSERT INTO #SOXLR71_TERRITORY  
				SELECT BusinessUnit,MasterGroupValue,MasterGroupValueDescription 
				FROM XA.MasterDefinitionValue WITH(NOLOCK) 
				WHERE (GroupType = '00') AND (MasterGroup = 'TETY') AND BusinessUnit = @BusinessUnit		
		END
		ELSE
		BEGIN
			IF @ApplyUserMasterValueForTETY = '1'
				INSERT INTO #SOXLR71_TERRITORY  SELECT TETY.BusinessUnit,TETY.TerritoryCode,TETY.Description 
				FROM XA.UserTerritory TETY WITH(NOLOCK)
				INNER JOIN 	XA.MasterDefinitionValue AS Definition WITH(NOLOCK) ON 
				Definition.BusinessUnit= TETY.BusinessUnit
				AND Definition.MasterGroup='TETY' 
				AND Definition.MasterGroupValue=TETY.TerritoryCode AND TETY.UserName=@Username
				WHERE Definition.MasterGroupValue IN(SELECT MasterGroupValue FROM [XA].[fn_MasterGroupChildValuesWithInactive](@BusinessUnit, '00', @TerritoryCode,'') WHERE MasterGroup = 'TETY')			
			ELSE
				INSERT INTO #SOXLR71_TERRITORY  
				SELECT BusinessUnit,MasterGroupValue AS TerritoryCode,MasterGroupValueDescription 
				FROM XA.MasterDefinitionValue WITH(NOLOCK) 
				WHERE (GroupType = '00') AND (MasterGroup = 'TETY') 
				AND BusinessUnit = @BusinessUnit 
				AND MasterGroupValue IN(SELECT MasterGroupValue FROM [XA].[fn_MasterGroupChildValuesWithInactive](@BusinessUnit, '00', @TerritoryCode,'') WHERE MasterGroup = 'TETY')
		END
		DECLARE @ApplyUserMasterValueForProduct char(1)
		SET @ApplyUserMasterValueForProduct='0'
				IF (@PowerUser='0')
				BEGIN
				IF Exists (SELECT ApplyUserMasterValue FROM XA.MasterDefinition 
				WHERE BusinessUnit=@BusinessUnit AND GroupType='02'	AND ApplyUserMasterValue='1')
					BEGIN
						SET @ApplyUserMasterValueForProduct='1'
					END 
				END
	
	IF @ExecutionType = '1' -- Get Territory Prompt
		BEGIN
			SELECT TerritoryCode,TerritoryName
			FROM #SOXLR71_TERRITORY
			WHERE BusinessUnit = @BusinessUnit
		END

	IF @ExecutionType = '2' -- Get Distributor Prompt
		BEGIN
			SELECT DistributorCode, DistributorName
			FROM RD.Distributor WITH (NOLOCK)
			WHERE BusinessUnit = @BusinessUnit
		END

	--IF @ExecutionType = '3' -- Get DistributorVATRegistrationNo
	--	BEGIN
	--		SELECT VATRegistrationNo AS DistributorVATRegistrationNo
	--		FROM RD.Distributor
	--		WHERE BusinessUnit = @BusinessUnit AND DistributorCode = @DistributorCode
	--	END

END