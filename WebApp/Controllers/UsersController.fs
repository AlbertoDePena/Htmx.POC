namespace WebApp.Controllers

open System
open System.Threading.Tasks

open FsToolkit.ErrorHandling

open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options

open WebApp.Domain.Shared
open WebApp.Domain.User
open WebApp.Infrastructure.Constants
open WebApp.Infrastructure.Database
open WebApp.Infrastructure.HtmlTemplate
open WebApp.Infrastructure.Options
open WebApp.Data
open WebApp.Views

type UsersController
    (logger: ILogger<UsersController>, databaseOptions: IOptions<DatabaseOptions>, antiforgery: IAntiforgery) =
    inherit HtmxController(antiforgery)

    [<HttpGet>]
    member this.IndexAlt() : Task<IActionResult> =
        task {
            let antiforgeryToken = this.GetAntiforgeryToken()

            let props: UserView.MainProps =
                { FormFieldName = antiforgeryToken.FormFieldName
                  RequestToken = antiforgeryToken.RequestToken }

            let pageProps: IndexView.PageProps =
                { PageName = "Users"
                  UserName = this.GetUserName()
                  MainContent = UserView.renderMain props }

            let htmlContent = IndexView.renderPage pageProps

            return this.HtmlContent htmlContent
        }

    [<HttpGet>]
    member this.Index() : Task<IActionResult> =
        task {
            let getAntiforgeryToken () =
                let token = antiforgery.GetAndStoreTokens(this.HttpContext)

                { FormFieldName = token.FormFieldName
                  RequestToken = token.RequestToken }

            let mainContent =
                Html.load "templates/search-section.html"
                |> Html.csrf getAntiforgeryToken
                |> Html.replace "UserSearchTableElementId" "UserSearchTable"
                |> Html.replace "UserSearchFormElementId" "UserSearchForm"
                |> Html.render

            if this.Request.IsHtmxBoosted() then
                return this.HtmlContent(mainContent)
            else
                let userName = this.GetUserName()

                let indexContent =
                    Html.load "templates/index.html"
                    |> Html.replace "NavbarBurgerElementId" "NavbarBurger"
                    |> Html.replace "MainNavbarElementId" "MainNavbar"
                    |> Html.replace "MainContentElementId" "MainContent"
                    |> Html.replace "UserName" userName
                    |> Html.replace "MainContent" mainContent
                    |> Html.render

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

                let! pagedData =
                    UserRepository.getPagedData
                        (fun () -> DbConnection.create databaseOptions.Value.ConnectionString)
                        query

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

                let! pagedData =
                    UserRepository.getPagedData
                        (fun () -> DbConnection.create databaseOptions.Value.ConnectionString)
                        query

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

                let userTableContent (user: User) (template: HtmlTemplate) =
                    let typeNameClass =
                        match user.UserTypeName with
                        | UserType.Customer -> "tag is-light is-info"
                        | UserType.Employee -> "tag is-light is-success"

                    let isActiveClass = if user.IsActive then "tag is-success" else "tag"

                    let isActiveText = if user.IsActive then "Yes" else "No"

                    template
                    |> Html.replace "DisplayName" user.DisplayName
                    |> Html.replace "EmailAddress" user.EmailAddress
                    |> Html.replace "TypeNameClass" typeNameClass
                    |> Html.replace "UserTypeName" user.UserTypeName
                    |> Html.replace "IsActiveClass" isActiveClass
                    |> Html.replace "IsActiveText" isActiveText

                let htmlContent =
                    Html.load "templates/search-table.html"
                    |> Html.csrf getAntiforgeryToken
                    |> Html.replace "UserSearchTableElementId" "UserSearchTable"
                    |> Html.replace "UserSearchFormElementId" "UserSearchForm"
                    |> Html.replace "SearchResultSummary" searchResultSummary
                    |> Html.replace "PreviousPageDisabled" (Html.disabled (not pagedData.HasPreviousPage))
                    |> Html.replace "NextPageDisabled" (Html.disabled (not pagedData.HasNextPage))
                    |> Html.replace "PreviousPage" (pagedData.Page - 1)
                    |> Html.replace "NextPage" (pagedData.Page + 1)
                    |> Html.replaceList "User" pagedData.Data userTableContent
                    |> Html.render

                return this.HtmlContent htmlContent
            else
                return! this.Index()
        }
