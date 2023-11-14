namespace WebApplication.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open System.Diagnostics

open FsToolkit.ErrorHandling

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Options

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
            let query =
                { SearchCriteria = this.Request.TryGetQueryStringValue "search"
                  ActiveOnly =
                    this.Request.TryGetQueryStringValue "view-active-users"
                    |> Option.bind (bool.TryParse >> Option.ofPair)
                    |> Option.defaultValue false
                  Page =
                    this.Request.TryGetQueryStringValue "page"
                    |> Option.bind (Int32.TryParse >> Option.ofPair)
                    |> Option.defaultValue 1
                  PageSize = 15
                  SortBy = None
                  SortDirection = SortDirection.fromString "Ascending" }

            let! pagedData = UserDatabase.getPagedData dbConnectionFactory query

            let toHtmlContent (user: User) =
                htmlTemplate
                    .Bind("DisplayName", user.DisplayName)
                    .Bind("EmailAddress", user.EmailAddress)
                    .Bind("TypeName", user.TypeName |> UserType.value)
                    .Bind("TagClass", (if user.IsActive then "tag is-success" else "tag"))
                    .Bind("IsActive", (if user.IsActive then "Yes" else "No"))
                    .Render("templates/user/search-results.html")

            let searchResultsContent =
                htmlTemplate
                    .Bind("SearchResults", pagedData.Data |> List.map toHtmlContent |> htmlTemplate.Join)
                    .Bind("TotalCount", pagedData.TotalCount)
                    .Bind("Page", pagedData.Page)
                    .Bind("TotalPages", pagedData.NumberOfPage)
                    .Render("templates/user/search-table.html")

            return this.HtmlContent searchResultsContent
        }

    member this.Index() =
        task {
            let content =
                htmlTemplate
                    .Bind("UserName", "Alberto De Pena")
                    .Bind("PageContent", "templates/user/search.html")
                    .Render("templates/index.html")

            return this.HtmlContent content
        }
