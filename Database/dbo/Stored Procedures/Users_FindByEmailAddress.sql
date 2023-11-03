CREATE PROCEDURE [dbo].[Users_FindByEmailAddress]
    @EmailAddress NVARCHAR(256)			
AS

DECLARE
    @Id UNIQUEIDENTIFIER = (SELECT [Id]
                            FROM [dbo].[Users]
                            WHERE [EmailAddress] = @EmailAddress)

    EXEC dbo.Users_FindById @Id
RETURN
