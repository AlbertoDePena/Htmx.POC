CREATE VIEW [dbo].[UserGroupsView]
AS
SELECT
	[Users].[UserId],
	[Groups].[GroupId],
	[Groups].[Name] AS [GroupName]
FROM [dbo].[Users]
INNER JOIN [dbo].[UserGroups]
	ON [Users].[UserId] = [UserGroups].[UserId]
INNER JOIN [dbo].[Groups]
	ON [Groups].[GroupId] = [UserGroups].[GroupId];
