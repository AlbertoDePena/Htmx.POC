namespace WebApp.Controllers

open System

open FsToolkit.ErrorHandling

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApp.Domain.User
open WebApp.Domain.Shared
open WebApp.Infrastructure.Constants
open WebApp.Infrastructure.UserDatabase
open WebApp.Infrastructure.HtmlMarkup

type UsersController(logger: ILogger<UsersController>, htmlMarkup: HtmlMarkup, userDatabase: IUserDatabase) =
    inherit HtmxController(logger, htmlMarkup)

    [<HttpGet>]
    member this.Index() =
        let currentUserName = this.HttpContext.User.Identity.Name

        let htmlContent =
            htmlMarkup.Render(
                "user/search-section.html",
                fun binder -> binder.BindAntiforgery("Antiforgery", this.HttpContext)
            )

        this.HtmlContent(userName = currentUserName, mainContent = htmlContent)

    [<HttpGet>]
    [<HttpPost>]
    [<ActionName("Search")>]
    member this.RenderPagedData() =
        task {
            if this.Request.IsHtmx() then
                let getValue =
                    if this.Request.Method = "GET" then this.Request.TryGetQueryStringValue
                    elif this.Request.Method = "POST" then this.Request.TryGetFormValue
                    else failwith "Unsupported HTTP method"

                let query =
                    { SearchCriteria = getValue QueryName.Search |> Option.bind Text.OfString
                      ActiveOnly =
                        getValue QueryName.ActiveOnly
                        |> Option.bind (bool.TryParse >> Option.ofPair)
                        |> Option.defaultValue false
                      Page =
                        getValue QueryName.Page
                        |> Option.bind (Int32.TryParse >> Option.ofPair)
                        |> Option.filter (fun page -> page > 0)
                        |> Option.defaultValue 1
                      PageSize = 20
                      SortBy = None
                      SortDirection = None }

                let! pagedData = userDatabase.GetPagedData query

                let searchResults =
                    htmlMarkup.Render(
                        "user/search-table-row.html",
                        pagedData.Data,
                        fun (binder, user) ->
                            let typeNameClass =
                                match user.UserTypeName with
                                | UserType.Customer -> "tag is-light is-info"
                                | UserType.Employee -> "tag is-light is-success"

                            let isActiveClass = if user.IsActive then "tag is-success" else "tag"

                            let isActiveText = if user.IsActive then "Yes" else "No"

                            binder
                                .Bind("DisplayName", user.DisplayName)
                                .Bind("EmailAddress", user.EmailAddress)
                                .Bind("TypeNameClass", typeNameClass)
                                .Bind("TypeName", user.UserTypeName)
                                .Bind("IsActiveClass", isActiveClass)
                                .Bind("IsActive", isActiveText)
                    )

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
                    htmlMarkup.Render(
                        "user/search-table.html",
                        fun binder ->
                            binder
                                .Bind("SearchResults", searchResults)
                                .Bind("SearchResultSummary", searchResultSummary)
                                .Bind("PreviousButtonDisabled", previousButtonDisabled)
                                .Bind("PreviousPage", pagedData.Page - 1)
                                .Bind("NextButtonDisabled", nextButtonDisabled)
                                .Bind("NextPage", pagedData.Page + 1)
                    )

                return this.HtmlContent tableContent
            else
                return this.Index()
        }
