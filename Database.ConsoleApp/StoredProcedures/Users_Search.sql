DROP PROCEDURE IF EXISTS [dbo].[Users_Search];
GO

CREATE PROCEDURE [dbo].[Users_Search]
	@SearchCriteria NVARCHAR(256) = NULL,
	@ActiveOnly  BIT = 0,
	@Page INT = 1,
	@PageSize INT = 25,
	@SortBy NVARCHAR(256) = 'DisplayName',
	@SortDirection NVARCHAR(5) = 'ASC'
AS
SELECT 
	[UserId],
	[EmailAddress],
	[DisplayName],
	[UserTypeId],
	[UserTypeName],
	[IsActive]
FROM [dbo].[UsersView]
WHERE (@SearchCriteria IS NULL OR 
		[DisplayName] LIKE '%' + @SearchCriteria + '%' OR 
		[EmailAddress] LIKE '%' + @SearchCriteria + '%')
AND (@ActiveOnly = 0 OR (@ActiveOnly = 1 AND [IsActive] = 1))
ORDER BY 
	CASE WHEN @SortBy = 'DisplayName' AND @SortDirection = 'ASC'
		THEN [DisplayName] END ASC,
	CASE WHEN @SortBy = 'DisplayName' AND @SortDirection = 'DESC'
		THEN [DisplayName] END DESC,
	CASE WHEN @SortBy = 'EmailAddress' AND @SortDirection = 'ASC'
		THEN [EmailAddress] END ASC,
	CASE WHEN @SortBy = 'EmailAddress' AND @SortDirection = 'DESC'
		THEN [EmailAddress] END DESC
OFFSET(@Page - 1) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY
OPTION (RECOMPILE);

-- Total Count
SELECT Count(1) AS TotalCount
FROM [dbo].[UsersView]
WHERE (@SearchCriteria IS NULL OR 
		[DisplayName] LIKE '%' + @SearchCriteria + '%' OR 
		[EmailAddress] LIKE '%' + @SearchCriteria + '%')
AND (@ActiveOnly = 0 OR (@ActiveOnly = 1 AND [IsActive] = 1))
OPTION (RECOMPILE);
