CREATE VIEW [dbo].[UserPermissionsView]
AS
SELECT
	[Users].[UserId],
	[Permissions].[PermissionId],
	[Permissions].[Name] AS [PermissionName]
FROM [dbo].[Users]
INNER JOIN [dbo].[UserPermissions]
	ON [Users].[UserId] = [UserPermissions].[UserId]
INNER JOIN [dbo].[Permissions]
	ON [Permissions].[PermissionId] = [UserPermissions].[PermissionId];
