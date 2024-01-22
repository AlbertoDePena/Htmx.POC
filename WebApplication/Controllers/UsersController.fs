namespace WebApplication.Controllers

open System

open FsToolkit.ErrorHandling

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApplication.Domain.User
open WebApplication.Domain.Shared
open WebApplication.Infrastructure.Constants
open WebApplication.Infrastructure.UserDatabase
open WebApplication.Infrastructure.HtmlTemplate

type UsersController(logger: ILogger<UsersController>, htmlTemplate: IHtmlTemplate, userDatabase: IUserDatabase) =
    inherit HtmxController(logger, htmlTemplate)

    member this.Index() =
        task { return! this.HtmlContent(userName = "Alberto De Pena", mainContent = "user/search-section.html") }

    member this.Search() =
        task {
            if this.Request.IsHtmx() then
                let query =
                    { SearchCriteria = this.Request.GetQueryStringValue QueryName.Search |> Option.bind Text.OfString
                      ActiveOnly =
                        this.Request.GetQueryStringValue QueryName.ActiveOnly
                        |> Option.bind (bool.TryParse >> Option.ofPair)
                        |> Option.defaultValue false
                      Page =
                        this.Request.GetQueryStringValue QueryName.Page
                        |> Option.bind (Int32.TryParse >> Option.ofPair)
                        |> Option.filter (fun page -> page > 0)
                        |> Option.defaultValue 1
                      PageSize = 20
                      SortBy = None
                      SortDirection = None }

                let! pagedData = userDatabase.GetPagedData query

                let toHtmlContent (user: User) =
                    let typeNameClass =
                        match user.UserTypeName with
                        | UserType.Customer -> "tag is-light is-info"
                        | UserType.Employee -> "tag is-light is-success"

                    let isActiveClass = if user.IsActive then "tag is-success" else "tag"

                    htmlTemplate
                        .Bind("DisplayName", user.DisplayName)
                        .Bind("EmailAddress", user.EmailAddress)
                        .Bind("TypeNameClass", typeNameClass)
                        .Bind("TypeName", user.UserTypeName)
                        .Bind("IsActiveClass", isActiveClass)
                        .Bind("IsActive", (if user.IsActive then "Yes" else "No"))
                        .Render("user/search-table-row.html")

                let searchResults = pagedData.Data |> List.map toHtmlContent |> htmlTemplate.Join

                let searchResultSummary =
                    sprintf
                        "%i users found | showing page %i of %i"
                        pagedData.TotalCount
                        pagedData.Page
                        pagedData.TotalPages

                let previousButtonDisabled = if pagedData.Page > 1 then String.Empty else "disabled"

                let nextButtonDisabled =
                    if (pagedData.Page * pagedData.PageSize) < pagedData.TotalCount then
                        String.Empty
                    else
                        "disabled"

                let tableContent =
                    htmlTemplate
                        .Bind("SearchResults", searchResults)
                        .Bind("SearchResultSummary", searchResultSummary)
                        .Bind("PreviousButtonDisabled", previousButtonDisabled)
                        .Bind("PreviousPage", pagedData.Page - 1)
                        .Bind("NextButtonDisabled", nextButtonDisabled)
                        .Bind("NextPage", pagedData.Page + 1)
                        .Render("user/search-table.html")

                return this.HtmlContent tableContent
            else
                return! this.Index()
        }
