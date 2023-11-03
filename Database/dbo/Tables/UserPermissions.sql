CREATE TABLE [dbo].[UserPermissions]
(
	[UserId] UNIQUEIDENTIFIER NOT NULL,
	[PermissionId] UNIQUEIDENTIFIER NOT NULL,
	CONSTRAINT PK_UserPermissions PRIMARY KEY CLUSTERED ([UserId],[PermissionId]),
	CONSTRAINT FK_UserPermissions_User FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] (Id) ON DELETE CASCADE,
	CONSTRAINT FK_UserPermissions_Permission FOREIGN KEY ([PermissionId]) REFERENCES [dbo].[Permissions] (Id) ON DELETE CASCADE
);
GO
