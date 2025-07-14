  begin transaction

  delete from jobtrays
  delete from jobs
  delete from [batches]
  delete from BatchPlanRows
  delete from batchPlans
  delete from recipes
  delete from products

  declare @farmid uniqueidentifier 
  SELECT TOP 1 @FarmId = Farms.Id FROM dbo.Farms

  INSERT INTO [dbo].[Products] ([Id],[ProductCategoryId],[Name],[Description],[SeedIdentifier],[AddedDateTime] ,[ModifiedDateTime],[DeletedDateTime],[ImageData] ,[FarmId],[SeedSupplier])
  values 
	('cb12d08e-64a5-45df-adcd-957489684f91','90E1EE66-1949-4FFD-88A4-3809C32739DE','1. Lalique','',1,getdate(),getdate(),null,null,@farmid,''),
	('3013c8c5-1c5b-402f-8c8a-14ea4397b869','90E1EE66-1949-4FFD-88A4-3809C32739DE','2. Red Gem','',1,getdate(),getdate(),null,null,@farmid,''),
	('6ac85cb0-3970-4025-8b6c-c90fe43b9390','90E1EE66-1949-4FFD-88A4-3809C32739DE','3. Green Butter','',1,getdate(),getdate(),null,null,@farmid,''),
	('c9a96ff8-c8bb-49d3-941c-2454c34789bd','90E1EE66-1949-4FFD-88A4-3809C32739DE','4. Tendita','',1,getdate(),getdate(),null,null,@farmid,'')

  INSERT INTO [dbo].[Products] ([Id],[ProductCategoryId],[Name],[Description],[SeedIdentifier],[AddedDateTime] ,[ModifiedDateTime],[DeletedDateTime],[ImageData] ,[FarmId],[SeedSupplier])
  values 
	('c518883a-7649-4a10-8630-b464be4c65b0','173FC70B-3475-4303-B9BD-C49870AF13BF','1. Green Toscano Petite','',1,getdate(),getdate(),null,null,@farmid,''),
	('d4030969-7123-4db3-90b7-ca3ca0d00d7a','173FC70B-3475-4303-B9BD-C49870AF13BF','2. Purple Kale Petite','',1,getdate(),getdate(),null,null,@farmid,''),
	('816e1b60-8e14-4591-87c3-d1ca9ecbb8a2','173FC70B-3475-4303-B9BD-C49870AF13BF','3. French Sorrel Petite','',1,getdate(),getdate(),null,null,@farmid,''),
	('3fb39a79-db23-4848-b278-49f65267f33b','173FC70B-3475-4303-B9BD-C49870AF13BF','4. Mizuna Petite','',1,getdate(),getdate(),null,null,@farmid,''),
	('ffdab100-8c87-4157-858f-a9340569ff6f','173FC70B-3475-4303-B9BD-C49870AF13BF','5. Red Vein Sorrel Petite','',1,getdate(),getdate(),null,null,@farmid,''),
	('e3c59e29-4bbb-4763-a53c-4630415d29e2','173FC70B-3475-4303-B9BD-C49870AF13BF','6. Red Vein Sorrel Microgreen','',1,getdate(),getdate(),null,null,@farmid,''),
	('aa570315-663b-44b8-9cbe-31e1a6144f26','173FC70B-3475-4303-B9BD-C49870AF13BF','7. Cilantro Microgreen','',1,getdate(),getdate(),null,null,@farmid,'')

  INSERT INTO [dbo].[Products] ([Id],[ProductCategoryId],[Name],[Description],[SeedIdentifier],[AddedDateTime] ,[ModifiedDateTime],[DeletedDateTime],[ImageData] ,[FarmId],[SeedSupplier])
  values 
	('3afaef18-0c03-47a3-b377-f870c1908b31','3A0C8CC4-18A8-41E2-AFC9-4B51FC288F1C','1. Red Stem Radish Microgreen','',1,getdate(),getdate(),null,null,@farmid,''),
	('938f2ac1-8635-4303-aaa9-158735e17b29','3A0C8CC4-18A8-41E2-AFC9-4B51FC288F1C','2. Daikon Radish Microgreen','',1,getdate(),getdate(),null,null,@farmid,''),
	('79df0214-e451-484e-b2b8-9117ccebd617','3A0C8CC4-18A8-41E2-AFC9-4B51FC288F1C','3. Rambo Radish Microgreen','',1,getdate(),getdate(),null,null,@farmid,''),
	('c65e99da-99da-4855-9d6f-7c7dfa49c5bd','3A0C8CC4-18A8-41E2-AFC9-4B51FC288F1C','4. Mustard Microgreen','',1,getdate(),getdate(),null,null,@farmid,''),
	('beb201ea-0e54-47f3-89b5-bc22af75fa9d','3A0C8CC4-18A8-41E2-AFC9-4B51FC288F1C','5. Kohlrabi Microgreen','',1,getdate(),getdate(),null,null,@farmid,''),
	('3c6992a0-04b6-4598-aafb-704de1549c57','3A0C8CC4-18A8-41E2-AFC9-4B51FC288F1C','6. Pea Shoots','',1,getdate(),getdate(),null,null,@farmid,''),
	('9f776d43-c493-47c5-abdb-9e025841ce18','3A0C8CC4-18A8-41E2-AFC9-4B51FC288F1C','7. Purple Kale','',1,getdate(),getdate(),null,null,@farmid,'')

INSERT INTO [dbo].[Recipes]([Id],[Name],[Description],[ProductId],[GerminationDays],[PropagationDays],[GrowDays],[AddedDateTime],[ModifiedDateTime],[DeletedDateTime])
values
	('2858a657-4ddc-4964-970b-50d940720073','1. Lalique Recipe','','cb12d08e-64a5-45df-adcd-957489684f91',0,14,21, getdate(),getdate(),null),
	('d937a096-0540-4ffa-8af3-794a30c47a01','2. Red Gem Recipe','','3013c8c5-1c5b-402f-8c8a-14ea4397b869',0,14,21, getdate(),getdate(),null),
	('8d5714a6-add9-458c-b36c-355ede166f64','3. Green Butter Recipe','','6ac85cb0-3970-4025-8b6c-c90fe43b9390',0,14,21, getdate(),getdate(),null),
	('dff67a04-c970-4db4-b9b0-027b679cab29','4. Tendita Recipe','','c9a96ff8-c8bb-49d3-941c-2454c34789bd',0,14,21, getdate(),getdate(),null)
INSERT INTO [dbo].[Recipes]([Id],[Name],[Description],[ProductId],[GerminationDays],[PropagationDays],[GrowDays],[AddedDateTime],[ModifiedDateTime],[DeletedDateTime])
values
	('dcb51942-62aa-4e85-b393-5feac3e1782b','1. Green Toscano Petite Recipe','','c518883a-7649-4a10-8630-b464be4c65b0',3,0,18, getdate(),getdate(),null),
	('664b46f5-1c1f-4538-b7ab-d468e22f4ced','2. Purple Kale Petite Recipe','','d4030969-7123-4db3-90b7-ca3ca0d00d7a',3,0,18, getdate(),getdate(),null),
	('6cc831da-b29b-49d3-aeda-2f0bc3039dbf','3. French Sorrel Petite Recipe','','816e1b60-8e14-4591-87c3-d1ca9ecbb8a2',3,0,18, getdate(),getdate(),null),
	('4ca2ec05-7e8a-49b9-b6ed-1b826412eb7d','4. Mizuna Petite Recipe','','3fb39a79-db23-4848-b278-49f65267f33b',3,0,18, getdate(),getdate(),null),
	('c55dfb8c-fc53-41a2-b29f-d71673dc3643','5. Red Vein Sorrel Petite Recipe','','ffdab100-8c87-4157-858f-a9340569ff6f',3,0,18, getdate(),getdate(),null),
	('361ae49f-0110-46a0-9b5c-16f5108c35c9','6. Red Vein Sorrel Microgreen Recipe','','e3c59e29-4bbb-4763-a53c-4630415d29e2',3,0,18, getdate(),getdate(),null),
	('0db22f92-862d-4cc8-86a3-d92d5421a3c5','7. Cilantro Microgreen Recipe','','aa570315-663b-44b8-9cbe-31e1a6144f26',3,0,18, getdate(),getdate(),null)

INSERT INTO [dbo].[Recipes]([Id],[Name],[Description],[ProductId],[GerminationDays],[PropagationDays],[GrowDays],[AddedDateTime],[ModifiedDateTime],[DeletedDateTime])
values
	('096bb1c2-3c98-4e24-b3ff-5a8eac2e9f37','1. Red Stem Radish Microgreen Recipe','','3afaef18-0c03-47a3-b377-f870c1908b31',3,0,4, getdate(),getdate(),null),
	('8099c2b0-6e5b-46f7-8755-fc4a20d86b0f','2. Daikon Radish Microgreen Recipe','','938f2ac1-8635-4303-aaa9-158735e17b29',3,0,4, getdate(),getdate(),null),
	('c5dda269-0d11-472b-bf6a-fb1dec998de3','3. Rambo Radish Microgreen Recipe','','79df0214-e451-484e-b2b8-9117ccebd617',3,0,4, getdate(),getdate(),null),
	('948746d6-7b61-4920-8f6b-f4c99eb15744','4. Mustard Microgreen Recipe','','c65e99da-99da-4855-9d6f-7c7dfa49c5bd',3,0,4, getdate(),getdate(),null),
	('ffd2a94e-9fec-4b45-b91f-43277df62185','5. Kohlrabi Microgreen Recipe','','beb201ea-0e54-47f3-89b5-bc22af75fa9d',3,0,4, getdate(),getdate(),null),
	('e3e44bc3-73e0-42ba-8043-e60a5abbf287','6. Pea Shoots Recipe','','3c6992a0-04b6-4598-aafb-704de1549c57',3,0,4, getdate(),getdate(),null),
	('2905b895-36c2-406d-9a34-a7fd5321ba7c','7. Purple Kale Recipe','','9f776d43-c493-47c5-abdb-9e025841ce18',3,0,4, getdate(),getdate(),null)

commit transaction


