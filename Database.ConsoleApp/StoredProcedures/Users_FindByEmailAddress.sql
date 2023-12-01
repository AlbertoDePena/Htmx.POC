DROP PROCEDURE IF EXISTS [dbo].[Users_FindByEmailAddress];
GO

CREATE PROCEDURE [dbo].[Users_FindByEmailAddress]
    @EmailAddress NVARCHAR(256)			
AS

DECLARE
    @UserId UNIQUEIDENTIFIER = (SELECT [UserId]
                            FROM [dbo].[Users]
                            WHERE [EmailAddress] = @EmailAddress)

    EXEC dbo.Users_FindById @UserId
RETURN
