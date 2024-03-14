namespace WebApp.Controllers

open System
open System.Threading.Tasks

open FsToolkit.ErrorHandling

open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApp.Domain.Shared
open WebApp.Domain.User
open WebApp.Infrastructure.Constants
open WebApp.Infrastructure.UserDatabase
open WebApp.Infrastructure.HtmlTemplate
open WebApp.Views

type UsersController
    (
        logger: ILogger<UsersController>,
        antiforgery: IAntiforgery,
        userDatabase: IUserDatabase,
        templateLoader: HtmlTemplateLoader
    ) =
    inherit HtmxController(antiforgery)

    [<HttpGet>]
    member this.IndexAlt() : Task<IActionResult> =
        task {
            let props: UserView.MainProps = { Shared = this.GetSharedProps() }

            let htmlContent = UserView.renderMain props

            return this.HtmlContent(htmlContent)
        }

    [<HttpGet>]
    member this.Index() : Task<IActionResult> =
        task {
            let getAntiforgeryToken () =
                let token = antiforgery.GetAndStoreTokens(this.HttpContext)

                { FormFieldName = token.FormFieldName
                  RequestToken = token.RequestToken }

            let mainContent =
                templateLoader
                    .Load("user/search-section.html")
                    .BindAntiforgery(getAntiforgeryToken)
                    .Bind("UserSearchTableElementId", "UserSearchTable")
                    .Bind("UserSearchFormElementId", "UserSearchForm")
                    .Render()

            if this.Request.IsHtmxBoosted() then
                return this.HtmlContent(mainContent)
            else
                let indexContent =
                    templateLoader
                        .Load("index.html")
                        .Bind("NavbarBurgerElementId", "NavbarBurger")
                        .Bind("MainNavbarElementId", "MainNavbar")
                        .Bind("MainContentElementId", "MainContent")
                        .Bind("UserName", this.GetUserName())
                        .Bind("MainContent", mainContent)
                        .Render()

                return this.HtmlContent(indexContent)
        }

    //[<HttpGet>]
    //[<HttpPost>]
    //[<ActionName("Search")>]
    member this.RenderPagedDataAlt() : Task<IActionResult> =
        task {
            if this.Request.IsHtmx() then
                let getValue =
                    if this.Request.Method = "GET" then
                        this.Request.TryGetQueryStringValue
                    elif this.Request.Method = "POST" then
                        this.Request.TryGetFormValue
                    else
                        failwith "Unsupported HTTP method"

                let query =
                    { SearchCriteria = getValue FieldName.Search |> Option.bind Text.OfString
                      ActiveOnly =
                        getValue FieldName.ActiveOnly
                        |> Option.bind (bool.TryParse >> Option.ofPair)
                        |> Option.defaultValue false
                      Page =
                        getValue FieldName.Page
                        |> Option.bind (Int32.TryParse >> Option.ofPair)
                        |> Option.filter (fun page -> page > 0)
                        |> Option.defaultValue 1
                      PageSize = 20
                      SortBy = None
                      SortDirection = None }

                let! pagedData = userDatabase.GetPagedData query

                let props: UserView.TableProps = { PagedData = pagedData }

                let htmlContent = UserView.renderTable props

                return this.HtmlContent htmlContent
            else
                return! this.Index()
        }

    [<HttpGet>]
    [<HttpPost>]
    [<ActionName("Search")>]
    member this.RenderPagedData() : Task<IActionResult> =
        task {
            if this.Request.IsHtmx() then
                let getValue =
                    if this.Request.Method = "GET" then
                        this.Request.TryGetQueryStringValue
                    elif this.Request.Method = "POST" then
                        this.Request.TryGetFormValue
                    else
                        failwith "Unsupported HTTP method"

                let query =
                    { SearchCriteria = getValue FieldName.Search |> Option.bind Text.OfString
                      ActiveOnly =
                        getValue FieldName.ActiveOnly
                        |> Option.bind (bool.TryParse >> Option.ofPair)
                        |> Option.defaultValue false
                      Page =
                        getValue FieldName.Page
                        |> Option.bind (Int32.TryParse >> Option.ofPair)
                        |> Option.filter (fun page -> page > 0)
                        |> Option.defaultValue 1
                      PageSize = 20
                      SortBy = None
                      SortDirection = None }

                let! pagedData = userDatabase.GetPagedData query

                let searchResultSummary =
                    sprintf
                        "%i users found | showing page %i of %i"
                        pagedData.TotalCount
                        pagedData.Page
                        pagedData.TotalPages

                let getAntiforgeryToken () =
                    let token = antiforgery.GetAndStoreTokens(this.HttpContext)

                    { FormFieldName = token.FormFieldName
                      RequestToken = token.RequestToken }

                let userTableContent (user: User, template: HtmlTemplate) =
                    let typeNameClass =
                        match user.UserTypeName with
                        | UserType.Customer -> "tag is-light is-info"
                        | UserType.Employee -> "tag is-light is-success"

                    let isActiveClass = if user.IsActive then "tag is-success" else "tag"

                    let isActiveText = if user.IsActive then "Yes" else "No"

                    template
                        .Bind("DisplayName", user.DisplayName)
                        .Bind("EmailAddress", user.EmailAddress)
                        .Bind("TypeNameClass", typeNameClass)
                        .Bind("UserTypeName", user.UserTypeName)
                        .Bind("IsActiveClass", isActiveClass)
                        .Bind("IsActiveText", isActiveText)

                let htmlContent =
                    templateLoader
                        .Load("user/search-table.html")
                        .BindAntiforgery(getAntiforgeryToken)
                        .Bind("UserSearchTableElementId", "UserSearchTable")
                        .Bind("UserSearchFormElementId", "UserSearchForm")
                        .Bind("SearchResultSummary", searchResultSummary)
                        .Bind("PreviousPageDisabled", HtmlAttribute.disabled (not pagedData.HasPreviousPage))
                        .Bind("NextPageDisabled", HtmlAttribute.disabled (not pagedData.HasNextPage))
                        .Bind("PreviousPage", pagedData.Page - 1)
                        .Bind("NextPage", pagedData.Page + 1)
                        .Bind("User", pagedData.Data, userTableContent)
                        .Render()

                return this.HtmlContent htmlContent
            else
                return! this.Index()
        }
