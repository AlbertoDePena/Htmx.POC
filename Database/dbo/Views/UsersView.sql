CREATE VIEW [dbo].[UsersView]
AS
SELECT 
	[Users].[Id],
	[Users].[EmailAddress],
	[Users].[DisplayName],
	[Users].[TypeId],
	[UserTypes].[Name] AS [TypeName],
	[Users].[IsActive]
FROM [dbo].[Users]
LEFT JOIN [dbo].[UserTypes]
	ON [Users].[TypeId] = [UserTypes].[Id];
