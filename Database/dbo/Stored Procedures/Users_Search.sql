CREATE PROCEDURE [dbo].[Users_Search]
	@SearchCriteria NVARCHAR(256) = NULL,
	@ActiveOnly  BIT = 0,
	@Page INT = 1,
	@PageSize INT = 25,
	@SortBy NVARCHAR(256) = 'DisplayName',
	@SortDirection NVARCHAR(5) = 'ASC'
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
	ON [Users].[TypeId] = [UserTypes].[Id]
WHERE (@SearchCriteria IS NULL OR 
		[Users].[DisplayName] LIKE '%' + @SearchCriteria + '%' OR 
		[Users].[EmailAddress] LIKE '%' + @SearchCriteria + '%')
AND (@ActiveOnly = 0 OR (@ActiveOnly = 1 AND [Users].[IsActive] = 1))
ORDER BY 
	CASE WHEN @SortBy = 'DisplayName' AND @SortDirection = 'ASC'
		THEN [Users].[DisplayName] END ASC,
	CASE WHEN @SortBy = 'DisplayName' AND @SortDirection = 'DESC'
		THEN [Users].[DisplayName] END DESC,
	CASE WHEN @SortBy = 'EmailAddress' AND @SortDirection = 'ASC'
		THEN [Users].[EmailAddress] END ASC,
	CASE WHEN @SortBy = 'EmailAddress' AND @SortDirection = 'DESC'
		THEN [Users].[EmailAddress] END DESC
OFFSET(@Page - 1) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY
OPTION (RECOMPILE);

-- Total Count
SELECT Count(1) AS TotalCount
FROM [dbo].[Users]
WHERE (@SearchCriteria IS NULL OR 
		[Users].[DisplayName] LIKE '%' + @SearchCriteria + '%' OR 
		[Users].[EmailAddress] LIKE '%' + @SearchCriteria + '%')
AND (@ActiveOnly = 0 OR (@ActiveOnly = 1 AND [Users].[IsActive] = 1))
OPTION (RECOMPILE);
