SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[sp_GetBlocksWithoutDenomEfficiency]
AS
BEGIN
	SELECT TOP 10 DB.BlockID
	FROM [Veil].bak.BlockData DB 
	LEFT JOIN [Veil].bak.DenomEfficiency DE ON DB.BlockID=DE.BlockID
	WHERE DE.BlockDate IS NULL
	ORDER BY DB.BlockID DESC

END
GO