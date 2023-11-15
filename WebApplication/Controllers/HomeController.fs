namespace WebApplication.Controllers

open System

open FsToolkit.ErrorHandling

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApplication.Domain.User
open WebApplication.Domain.Shared
open WebApplication.Domain.Extensions
open WebApplication.Infrastructure.Database
open WebApplication.Infrastructure.UserDatabase
open WebApplication.Infrastructure.HtmlTemplate

type HomeController
    (logger: ILogger<HomeController>, htmlTemplate: IHtmlTemplate, dbConnectionFactory: IDbConnectionFactory) =
    inherit Controller()

    member this.Search() =
        task {
            if this.Request.IsHtmx() then
                let query =
                    { SearchCriteria = this.Request.TryGetQueryStringValue "search"
                      ActiveOnly =
                        this.Request.TryGetQueryStringValue "active-only"
                        |> Option.bind (bool.TryParse >> Option.ofPair)
                        |> Option.defaultValue false
                      Page =
                        this.Request.TryGetQueryStringValue "page"
                        |> Option.bind (Int32.TryParse >> Option.ofPair)
                        |> Option.filter (fun page -> page > 0)
                        |> Option.defaultValue 1
                      PageSize = 15
                      SortBy = None
                      SortDirection = SortDirection.ofString "Ascending" }

                let! pagedData = UserDatabase.getPagedData dbConnectionFactory query

                let toHtmlContent (user: User) =
                    htmlTemplate
                        .Bind("DisplayName", user.DisplayName)
                        .Bind("EmailAddress", user.EmailAddress)
                        .Bind("TypeName", user.TypeName |> UserType.value)
                        .Bind("TagClass", (if user.IsActive then "tag is-success" else "tag"))
                        .Bind("IsActive", (if user.IsActive then "Yes" else "No"))
                        .Render("templates/user/search-table-row.html")

                let searchResultSummary =
                    sprintf
                        "%i users found | showing page %i of %i"
                        pagedData.TotalCount
                        pagedData.Page
                        pagedData.TotalPages

                let previousButtonDisabled =
                    if pagedData.Page > 1 then String.Empty else "disabled"

                let nextButtonDisabled =
                    if (pagedData.Page * pagedData.PageSize) < pagedData.TotalCount then String.Empty else "disabled"

                let tableContent =
                    htmlTemplate
                        .Bind("SearchResults", pagedData.Data |> List.map toHtmlContent |> htmlTemplate.Join)
                        .Bind("SearchResultSummary", searchResultSummary)
                        .Bind("PreviousButtonDisabled", previousButtonDisabled)
                        .Bind("PreviousPage", pagedData.Page - 1)
                        .Bind("NextButtonDisabled", nextButtonDisabled)
                        .Bind("NextPage", pagedData.Page + 1)
                        .Render("templates/user/search-table.html")

                return this.HtmlContent tableContent
            else
                return! this.Index()
        }

    member this.Index() =
        task {
            let content =
                htmlTemplate
                    .Bind("CurrentUserName", "Alberto De Pena")
                    .Bind("MainContent", "templates/user/search-section.html")
                    .Render("templates/index.html")

            return this.HtmlContent content
        }
