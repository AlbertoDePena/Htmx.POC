CREATE TABLE [dbo].[Permissions]
(
	[PermissionId] UNIQUEIDENTIFIER NOT NULL,
	[Name] NVARCHAR(256) NOT NULL,
	CONSTRAINT PK_Permissions PRIMARY KEY CLUSTERED ([PermissionId])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [UX_Permissions_Name] ON [dbo].[Permissions] ([Name]);
GO