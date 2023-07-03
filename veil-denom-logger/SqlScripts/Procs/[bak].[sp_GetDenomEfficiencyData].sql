SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[sp_GetDenomEfficiencyData] 
	@DateRange int,
	@MovingAverage int
AS
BEGIN

	DECLARE @BlocksBack int = 1440; 
	--DECLARE @MovingAverage int = 10080;

	IF(@DateRange = 2)
	BEGIN 
		SET @BlocksBack = 4320; 
	END
	IF(@DateRange = 3)
	BEGIN 
		SET @BlocksBack = 10080; 
	END
	
	SELECT BlockID, BlockDate,Efficiency10,Efficiency100,Efficiency1000,Efficiency10000,BlocksBack
	FROM [Veil].bak.DenomEfficiency 
	WHERE BlockID >= ((SELECT MAX(BlockID) FROM [Veil].bak.[BlockData]) - @BlocksBack)
	And BlocksBack = @MovingAverage
	ORDER BY BlocksBack, BlockID 

END
GO


