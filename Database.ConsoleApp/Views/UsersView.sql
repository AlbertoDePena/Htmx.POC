DROP VIEW IF EXISTS [dbo].[UsersView];
GO

CREATE VIEW [dbo].[UsersView]
AS
SELECT 
	[Users].[UserId],
	[Users].[EmailAddress],
	[Users].[DisplayName],
	[Users].[UserTypeId],
	[UserTypes].[Name] AS [UserTypeName],
	[Users].[IsActive]
FROM [dbo].[Users]
INNER JOIN [dbo].[UserTypes]
	ON [Users].[UserTypeId] = [UserTypes].[UserTypeId];
