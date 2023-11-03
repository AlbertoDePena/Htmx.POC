CREATE PROCEDURE [dbo].[Users_FindById]
	@Id UNIQUEIDENTIFIER
AS

SELECT
	[Users].[Id],
	[Users].[DisplayName],
	[Users].[EmailAddress],
	[Users].[TypeId],
	[UserTypes].[Name] AS [TypeName],
	[Users].[IsActive]
FROM [dbo].[Users]
JOIN [dbo].[UserTypes]
	ON [Users].[TypeId] = [UserTypes].[Id]
WHERE [Users].[Id] = @Id;

SELECT
	[Permissions].[Id],
	[Permissions].[Name]
FROM [dbo].[Permissions]
INNER JOIN [dbo].[UserPermissions]
	ON [Permissions].[Id] = [UserPermissions].[PermissionId]
WHERE [UserPermissions].UserId = @Id;

SELECT
	[Groups].[Id],
	[Groups].[Name]
FROM [dbo].[Groups]
INNER JOIN [dbo].[UserGroups]
	ON [Groups].[Id] = [UserGroups].[GroupId]
WHERE [UserGroups].UserId = @Id;