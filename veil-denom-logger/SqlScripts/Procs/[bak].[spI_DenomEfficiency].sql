SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[spI_DenomEfficiency]
	@BlockID bigint ,
	@BlockRange int
AS
BEGIN
	--DECLARE @BlockRange int = 1;
	--DECLARE @BlockID bigint = 964181;
	DECLARE @iBlockRange int = 1440; -- 1 Days

	IF(@BlockRange = 2)
	BEGIN 
		SET @iBlockRange = 4320; -- 3 Days
	END
	IF(@BlockRange = 3)
	BEGIN 
		SET @iBlockRange = 10080; -- 7 Days
	END	

	Declare @iMaxBlock bigint;
	Declare @dtMaxBlockDate datetime;

	SELECT @iMaxBlock=MAX(BlockID) ,@dtMaxBlockDate = Max(BlockDate)
	FROM [Veil].[bak].[BlockData] with (nolock)
	WHERE BlockID <= @BlockID

	IF @iMaxBlock IS NULL
	BEGIN
		SELECT @iMaxBlock=MAX(BlockID) ,@dtMaxBlockDate = Max(BlockDate)
		FROM [Veil].[bak].[BlockData] with (nolock)		
	END

	DECLARE @iStartBlock bigint = 0;
		SET @iStartBlock = (@iMaxBlock - @iBlockRange);

	IF @iStartBlock <= 0 
	BEGIN
		SET @iStartBlock = 1
	END 
	
	DECLARE @bBlockSaved int = (SELECT COUNT(*)	FROM bak.DenomEfficiency with (nolock) WHERE BlockID = @BlockID AND [BlocksBack] = @iBlockRange);
	IF @bBlockSaved > 0
	BEGIN
		SELECT 0;
		RETURN
	END
	
	DECLARE @dZerocoinSupply decimal(28,0);
	set @dZerocoinSupply = (SELECT AVG(ZeroSupply)
	FROM (
		SELECT SUM(Amount) AS 'ZeroSupply', BlockID
		FROM  bak.ZerocoinSupply  with (nolock) WHERE Denom IN ( 'total') AND BlockID >= @iStartBlock AND BlockID <= @iMaxBlock
		GROUP BY BlockID) AS
	IT)	
	SELECT @dZerocoinSupply;

	DECLARE @iPosBlocks bigint = 0 
	SET @iPosBlocks = (SELECT COUNT_BIG(*)  FROM [Veil].[bak].BlockData (nolock)  WHERE [BlockType]=3 AND BlockID >= @iStartBlock AND BlockID <= @iMaxBlock);
	--SELECT @iPosBlocks

	DECLARE @Amount10s decimal(28,18);
	DECLARE @Amount100s decimal(28,18);
	DECLARE @Amount1000s decimal(28,18);
	DECLARE @Amount10000s decimal(28,18);

	SET @Amount10s = (SELECT AVG(Amount) FROM bak.ZerocoinSupply  with (nolock) WHERE Denom = '10' AND BlockID >= @iStartBlock  AND BlockID <= @iMaxBlock);
	SET @Amount100s = (SELECT AVG(Amount) FROM bak.ZerocoinSupply  with (nolock) WHERE Denom = '100' AND BlockID >= @iStartBlock AND BlockID <= @iMaxBlock);
	SET @Amount1000s = (SELECT AVG(Amount) FROM bak.ZerocoinSupply  with (nolock) WHERE Denom = '1000' AND BlockID >= @iStartBlock AND BlockID <= @iMaxBlock);
	SET @Amount10000s = (SELECT AVG(Amount) FROM bak.ZerocoinSupply  with (nolock) WHERE Denom = '10000' AND BlockID >= @iStartBlock AND BlockID <= @iMaxBlock);
	--SELECT @Amount10s, @Amount1000s, @Amount1000s, @Amount10000s;

	INSERT INTO [bak].[DenomEfficiency]
		([BlockID]
		,[BlockDate]
		,[BlocksBack]
		,[Supply10]
		,[Reward10]
		,[Efficiency10]
		,[Supply100]
		,[Reward100]
		,[Efficiency100]
		,[Supply1000]
		,[Reward1000]
		,[Efficiency1000]
		,[Supply10000]
		,[Reward10000]
		,[Efficiency10000])
		SELECT 
		@iMaxBlock,
		@dtMaxBlockDate,
		@iBlockRange,
		CONVERT(decimal(4,2),Supply10) AS 'Supply10',
		CONVERT(decimal(4,2),Reward10) AS 'Reward10',
		(CASE WHEN Supply10 > 0 THEN CONVERT(decimal(10,2),((Reward10/Supply10)*100)) ELSE 0.00 END) AS 'Efficiency10',
		CONVERT(decimal(4,2),Supply100) AS 'Supply100',
		CONVERT(decimal(4,2),Reward100) AS 'Reward100',
		(CASE WHEN Supply100 > 0 THEN CONVERT(decimal(10,2),((Reward100/Supply100)*100)) ELSE 0.00 END) AS 'Efficiency100',
		CONVERT(decimal(4,2),Supply1000) AS 'Supply1000',
		CONVERT(decimal(4,2),Reward1000) AS 'Reward1000',
		(CASE WHEN Supply1000 > 0 THEN CONVERT(decimal(10,2),((Reward1000/Supply1000)*100)) ELSE 0.00 END) AS 'Efficiency1000',
		CONVERT(decimal(4,2),Supply10000) AS 'Supply10000',
		CONVERT(decimal(4,2),Reward10000) AS 'Reward10000',
		(CASE WHEN Supply10000 > 0 THEN CONVERT(decimal(10,2),((Reward10000/Supply10000)*100)) ELSE 0.00 END) AS 'Efficiency10000'
		FROM (
		SELECT @iMaxBlock AS 'BlockID',
		@dtMaxBlockDate AS 'BlockDate',
		@iBlockRange as 'BlocksBack',
		(@Amount10s/@dZerocoinSupply)*100 AS 'Supply10',
		((SELECT COUNT(*) FROM bak.WinningDenom  with (nolock) WHERE (StakeDenom = '10' or StakeDenom = '10.00') AND BlockID >= @iStartBlock  AND BlockID <= @iMaxBlock) / CONVERT(decimal, @iPosBlocks))*100 AS 'Reward10',
		(@Amount100s/@dZerocoinSupply)*100 AS 'Supply100',
		((SELECT COUNT(*) FROM bak.WinningDenom  with (nolock) WHERE (StakeDenom = '100' or StakeDenom = '100.00') AND BlockID >= @iStartBlock AND BlockID <= @iMaxBlock) / CONVERT(decimal, @iPosBlocks))*100 AS 'Reward100',
		(@Amount1000s/@dZerocoinSupply)*100 AS 'Supply1000',
		((SELECT COUNT(*) FROM bak.WinningDenom  with (nolock) WHERE (StakeDenom = '1000' or StakeDenom = '1000.00') AND BlockID >= @iStartBlock AND BlockID <= @iMaxBlock)  / CONVERT(decimal, @iPosBlocks))*100 AS 'Reward1000',
		(@Amount10000s/@dZerocoinSupply)*100 AS 'Supply10000',
		((SELECT COUNT(*) FROM bak.WinningDenom  with (nolock) WHERE (StakeDenom = '10000' or StakeDenom = '10000.00') AND BlockID >= @iStartBlock AND BlockID <= @iMaxBlock) / CONVERT(decimal, @iPosBlocks))*100 AS 'Reward10000')
		IT
	    
	SELECT 1;
	RETURN
END
GO


