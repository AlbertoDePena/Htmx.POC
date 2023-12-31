﻿MERGE dbo.[Permissions] AS TARGET
USING (Values
('5417B133-69B4-436A-8B77-BFDAB49AAFD6','View Transportation Data'),
('386B4CEC-0A92-4BB8-A39A-42B4B53FAB77','View Financials'),
('6C10AD06-67A8-4996-9DDA-CC253F9465F3','Export Search Results'))
AS Source([PermissionId],[Name])
ON TARGET.[PermissionId] = Source.[PermissionId]

WHEN NOT MATCHED BY TARGET THEN
	INSERT([PermissionId],[Name])
	VALUES([PermissionId],[Name])
WHEN MATCHED THEN
    UPDATE
    SET Target.[Name] = Source.[Name]
WHEN NOT MATCHED BY SOURCE THEN
    DELETE;
GO