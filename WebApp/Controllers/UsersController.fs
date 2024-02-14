namespace WebApp.Controllers

open System

open FsToolkit.ErrorHandling

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApp.Domain.User
open WebApp.Domain.Shared
open WebApp.Infrastructure.Constants
open WebApp.Infrastructure.UserDatabase
open WebApp.Infrastructure.HtmlMarkup

type UsersController(logger: ILogger<UsersController>, htmlMarkup: IHtmlMarkup, userDatabase: IUserDatabase) =
    inherit HtmxController(logger, htmlMarkup)

    member this.Index() =
        let htmlContent =
            htmlMarkup
                .Load("user/search-section.html")
                .BindAntiforgery("Antiforgery", this.HttpContext)
                .Render()

        this.HtmlContent(userName = "Alberto De Pena", mainContent = htmlContent)

    member this.Search() =
        task {
            if this.Request.IsHtmx() then
                let query =
                    { SearchCriteria = this.Request.GetFormValue QueryName.Search |> Option.bind Text.OfString
                      ActiveOnly =
                        this.Request.GetFormValue QueryName.ActiveOnly
                        |> Option.bind (bool.TryParse >> Option.ofPair)
                        |> Option.defaultValue false
                      Page =
                        this.Request.GetFormValue QueryName.Page
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

                    htmlMarkup
                        .Load("user/search-table-row.html")
                        .Bind("DisplayName", user.DisplayName)
                        .Bind("EmailAddress", user.EmailAddress)
                        .Bind("TypeNameClass", typeNameClass)
                        .Bind("TypeName", user.UserTypeName)
                        .Bind("IsActiveClass", isActiveClass)
                        .Bind("IsActive", (if user.IsActive then "Yes" else "No"))
                        .Render()

                let searchResults = pagedData.Data |> List.map toHtmlContent |> htmlMarkup.Join

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
                    htmlMarkup
                        .Load("user/search-table.html")
                        .Bind("SearchResults", searchResults)
                        .Bind("SearchResultSummary", searchResultSummary)
                        .Bind("PreviousButtonDisabled", previousButtonDisabled)
                        .Bind("PreviousPage", pagedData.Page - 1)
                        .Bind("NextButtonDisabled", nextButtonDisabled)
                        .Bind("NextPage", pagedData.Page + 1)
                        .Render()

                return this.HtmlContent tableContent
            else
                return this.Index()
        }
