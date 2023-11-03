﻿MERGE dbo.Groups AS TARGET
USING (Values
('7EB6472C-FA2C-42C8-9AC0-53D8226910B0','Viewer'),
('7E92940E-8D42-4FB2-8C9F-81178EAB5CE4','Editor'),
('5ECA14E4-6D54-4763-B165-CF261E6D971B','Administrator'))
AS Source([Id],[Name])
ON TARGET.[Id] = Source.[Id]

WHEN NOT MATCHED BY TARGET THEN
	INSERT([Id],[Name])
	VALUES([Id],[Name])
WHEN MATCHED THEN
    UPDATE
    SET Target.[Name] = Source.[Name]
WHEN NOT MATCHED BY SOURCE THEN
    DELETE;
GO