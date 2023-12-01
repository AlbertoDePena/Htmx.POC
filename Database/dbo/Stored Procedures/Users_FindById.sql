CREATE PROCEDURE [dbo].[Users_FindById]
	@UserId UNIQUEIDENTIFIER
AS

SELECT
	[UserId],
	[DisplayName],
	[EmailAddress],
	[UserTypeId],
	[UserTypeName],
	[IsActive]
FROM [dbo].[UsersView]
WHERE [UserId] = @UserId;

SELECT
	[UserId],
	[PermissionId],
	[PermissionName]
FROM [dbo].[UserPermissionsView]
WHERE [UserId] = @UserId;

SELECT
	[UserId],
	[GroupId],
	[GroupName]
FROM [dbo].[UserGroupsView]
WHERE [UserId] = @UserId;