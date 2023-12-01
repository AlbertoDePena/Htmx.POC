MERGE dbo.UserTypes AS TARGET
USING (Values
('79687582-2d02-4856-bbcc-2826d5d7e4e2','Customer'),
('E3654E10-96F6-4B97-BC79-F7C9C254C9E7','Employee'))
AS Source([UserTypeId],[Name])
ON TARGET.[UserTypeId] = Source.[UserTypeId]

WHEN NOT MATCHED BY TARGET THEN
	INSERT([UserTypeId],[Name])
	VALUES([UserTypeId],[Name])
WHEN MATCHED THEN
    UPDATE
    SET Target.[Name] = Source.[Name]
WHEN NOT MATCHED BY SOURCE THEN
    DELETE;
GO