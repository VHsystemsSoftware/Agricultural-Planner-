begin transaction

  delete from jobtrays
  delete from jobs
  delete from [batches]
  delete from batchPlans


  declare @farmid uniqueidentifier ='7EF78648-BCE7-4AD2-BF74-E0F39287CCF1'
  declare @startdate datetime ='20250512'

  INSERT INTO [dbo].[BatchPlans]([Id],[FarmId] ,[TraysPerDay],[RecipeId],[TotalTrays],[EndTime],[StartTime],[Name],[AddedDateTime],[ModifiedDateTime],[DeletedDateTime])
	values 
	('ecc8b2ca-e66c-4444-8357-ee1e43ea3c91',@farmid,131,'2858a657-4ddc-4964-970b-50d940720073',131,@startdate,@startdate,'Petite Gen 1 g4',getdate(),getdate(),null)

  INSERT INTO [dbo].[Batches]([Id],[FarmId],[BatchPlanId],[HarvestDate],[SeedDate],[BatchName],[StatusId],[TrayCount],[AddedDateTime],[ModifiedDateTime],[DeletedDateTime])
	values
	('7f3f597f-b652-4d81-bc27-e662f7f18854',@farmid,'ecc8b2ca-e66c-4444-8357-ee1e43ea3c91',dateadd(day,22,@startdate),@startdate,'BATCH Petite Gen 1 g4','afca571c-fa99-4040-becb-8412889b097f',131,getdate(),getdate(),null)

  INSERT INTO [dbo].[Jobs]([Id],[OrderOnDay],[Name],[ScheduledDate],[BatchId],[JobLocationTypeId],[StatusId],[TrayCount],[AddedDateTime],[ModifiedDateTime],[DeletedDateTime])
  values
	('70d8b0e2-a292-4d1b-a25f-dcce07052d28',1,'Seeding',@startdate,'7f3f597f-b652-4d81-bc27-e662f7f18854','29e3acf3-c85f-4fc7-9aa5-d5fe94fb4606','a592ab99-3141-44c9-83ed-6fabb9534100',135,getdate(),getdate(),null)
	,('13825dc1-3934-4668-b68c-9f6802c05d03',1,'Germ->Grow',dateadd(day,3,@startdate),'7f3f597f-b652-4d81-bc27-e662f7f18854','ef7bf499-fbf3-4cf3-9057-1c39b22a1c36','a592ab99-3141-44c9-83ed-6fabb9534100',131,getdate(),getdate(),null)
	--,('8a419191-a8b4-42af-b1ac-61f6e8730d41',1,'Empty reroute',dateadd(day,3,@startdate),'7f3f597f-b652-4d81-bc27-e662f7f18854','ef7bf499-fbf3-4cf3-9057-1c39b22a1c36','a592ab99-3141-44c9-83ed-6fabb9534100',4 ,getdate(),getdate(),null)

	declare @germinationrackid as uniqueidentifier,@layer71 as uniqueidentifier,@layer72 as uniqueidentifier,@layer73 as uniqueidentifier,@layer74 as uniqueidentifier,@layer70 as uniqueidentifier
	select @germinationrackid= racks.id from racks inner join floors fl on fl.id  =FloorId and fl.Name='SK1' where racks.name='1'
	select @layer70 = id from layers where RackId=@germinationrackid and LayerNumber=70
	select @layer71 = id from layers where RackId=@germinationrackid and LayerNumber=71
	select @layer72 = id from layers where RackId=@germinationrackid and LayerNumber=72
	select @layer73 = id from layers where RackId=@germinationrackid and LayerNumber=73
	select @layer74 = id from layers where RackId=@germinationrackid and LayerNumber=74

	declare @growrackid as uniqueidentifier, @growlayer7 as uniqueidentifier
	select @growrackid= racks.id from racks inner join floors fl on fl.id  =FloorId and fl.Name='SK2' where racks.name='6'
	select @growlayer7 = id from layers where RackId=@growrackid and LayerNumber=7
	
	declare @parentid uniqueidentifier
	DECLARE @Counter INT,@Counter2 INT 
	SET @Counter=1
	SET @Counter2=1
	WHILE ( @Counter <= 27)
	BEGIN
		set @parentid =newid()
		INSERT INTO [dbo].[JobTrays]([Id],[JobId],[TrayId],[DestinationLocation],[DestinationLayerId],OrderOnLayer,ParentJobTrayId, [AddedDateTime])
		values (@parentid,'70d8b0e2-a292-4d1b-a25f-dcce07052d28',nulL,'30ed163a-400c-4749-96ae-b805c1851b19',@layer70,@counter, null, getdate())

		INSERT INTO [dbo].[JobTrays]([Id],[JobId],[TrayId],[DestinationLocation],[DestinationLayerId],OrderOnLayer,ParentJobTrayId,[AddedDateTime])
		values (newid(),'13825dc1-3934-4668-b68c-9f6802c05d03',nulL,'2723daf2-aa55-4520-8379-485dbce94626',@growlayer7,@counter2, @parentid, getdate())

		SET @Counter  = @Counter  + 1
		SET @Counter2  = @Counter2  + 1
	END
	
	SET @Counter=1
	WHILE ( @Counter <= 27)
	BEGIN
		set @parentid =newid()
		INSERT INTO [dbo].[JobTrays]([Id],[JobId],[TrayId],[DestinationLocation],[DestinationLayerId],OrderOnLayer,ParentJobTrayId,[AddedDateTime])
		values (newid(),'70d8b0e2-a292-4d1b-a25f-dcce07052d28',nulL,'30ed163a-400c-4749-96ae-b805c1851b19',@layer71,@counter, null, getdate())
		
		INSERT INTO [dbo].[JobTrays]([Id],[JobId],[TrayId],[DestinationLocation],[DestinationLayerId],OrderOnLayer,ParentJobTrayId,[AddedDateTime])
		values (newid(),'13825dc1-3934-4668-b68c-9f6802c05d03',nulL,'2723daf2-aa55-4520-8379-485dbce94626',@growlayer7,@counter2, @parentid, getdate())
		SET @Counter  = @Counter  + 1
		SET @Counter2  = @Counter2  + 1
	END
		SET @Counter=1
	WHILE ( @Counter <= 27)
	BEGIN
		set @parentid =newid()
		INSERT INTO [dbo].[JobTrays]([Id],[JobId],[TrayId],[DestinationLocation],[DestinationLayerId],OrderOnLayer,ParentJobTrayId,[AddedDateTime])
		values (newid(),'70d8b0e2-a292-4d1b-a25f-dcce07052d28',nulL,'30ed163a-400c-4749-96ae-b805c1851b19',@layer72,@counter, null, getdate())

		INSERT INTO [dbo].[JobTrays]([Id],[JobId],[TrayId],[DestinationLocation],[DestinationLayerId],OrderOnLayer,ParentJobTrayId,[AddedDateTime])
		values (newid(),'13825dc1-3934-4668-b68c-9f6802c05d03',nulL,'2723daf2-aa55-4520-8379-485dbce94626',@growlayer7,@counter2, @parentid, getdate())

		SET @Counter  = @Counter  + 1
		SET @Counter2  = @Counter2  + 1
	END
		SET @Counter=1
	WHILE ( @Counter <= 27)
	BEGIN
		set @parentid =newid()
		INSERT INTO [dbo].[JobTrays]([Id],[JobId],[TrayId],[DestinationLocation],[DestinationLayerId],OrderOnLayer,ParentJobTrayId,[AddedDateTime])
		values (newid(),'70d8b0e2-a292-4d1b-a25f-dcce07052d28',nulL,'30ed163a-400c-4749-96ae-b805c1851b19',@layer73,@counter, null, getdate())

		INSERT INTO [dbo].[JobTrays]([Id],[JobId],[TrayId],[DestinationLocation],[DestinationLayerId],OrderOnLayer,ParentJobTrayId,[AddedDateTime])
		values (newid(),'13825dc1-3934-4668-b68c-9f6802c05d03',nulL,'2723daf2-aa55-4520-8379-485dbce94626',@growlayer7,@counter2, @parentid, getdate())

		SET @Counter  = @Counter  + 1
		SET @Counter2  = @Counter2  + 1
	END
		SET @Counter=1
	WHILE ( @Counter <= 23)
	BEGIN
	set @parentid =newid()
		INSERT INTO [dbo].[JobTrays]([Id],[JobId],[TrayId],[DestinationLocation],[DestinationLayerId],OrderOnLayer,ParentJobTrayId,[AddedDateTime])
		values (newid(),'70d8b0e2-a292-4d1b-a25f-dcce07052d28',nulL,'30ed163a-400c-4749-96ae-b805c1851b19',@layer74,@counter, null, getdate())
		
		INSERT INTO [dbo].[JobTrays]([Id],[JobId],[TrayId],[DestinationLocation],[DestinationLayerId],OrderOnLayer,ParentJobTrayId,[AddedDateTime])
		values (newid(),'13825dc1-3934-4668-b68c-9f6802c05d03',nulL,'2723daf2-aa55-4520-8379-485dbce94626',@growlayer7,@counter2, @parentid, getdate())

		SET @Counter  = @Counter  + 1
		SET @Counter2  = @Counter2  + 1
	END
	--4 empty
	INSERT INTO [dbo].[JobTrays]([Id],[JobId],[TrayId],[DestinationLocation],[DestinationLayerId],OrderOnLayer,[AddedDateTime])
		values 
			(newid(),'70d8b0e2-a292-4d1b-a25f-dcce07052d28',nulL,'95e503d0-32b7-4601-954b-e3212d2adaa0',@layer74,24, getdate()),
			(newid(),'70d8b0e2-a292-4d1b-a25f-dcce07052d28',nulL,'95e503d0-32b7-4601-954b-e3212d2adaa0',@layer74,25, getdate()),
			(newid(),'70d8b0e2-a292-4d1b-a25f-dcce07052d28',nulL,'95e503d0-32b7-4601-954b-e3212d2adaa0',@layer74,26, getdate()),
			(newid(),'70d8b0e2-a292-4d1b-a25f-dcce07052d28',nulL,'95e503d0-32b7-4601-954b-e3212d2adaa0',@layer74,27, getdate())

	select *
	from JobTrays order by OrderOnLayer



commit transaction





--
--
--5231bb4c-d745-4ec1-88a4-4706e564fa25
--dc7ef600-3ee7-477f-a755-4bc5f3a08f30
--f40ef5c3-e1fb-4baa-bae2-45d284d0d8a7
--f25c4726-cd75-4be2-9cf8-5e267d0a2348
--f816e3ed-80b9-4ed2-a470-3e31372b7f1b
--6ce88335-830f-4352-bd37-811a65fe621f
--6a44314f-54a9-4b32-8bd7-42b86a56e04d
--47bdf85f-64c4-44a7-8225-9c72472fe3e1
--339db8e2-c4f8-453a-be25-02ed819493fc
--67042d3b-1251-4fac-bf0f-2131f27f5358
--f8b7eff1-5153-4d6b-aca5-f36878bd70c8
--1a3064d4-c08d-4f2b-ba61-31b90a6d1457
--cd16fde6-d938-4702-b8d4-bb695f39b8e5
--f4ab0f67-efda-45ab-adb3-53b3953b9f0f
--b9b042ae-9f84-446b-85d8-34a720b2e45f