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
open WebApplication.Infrastructure.Options
open WebApplication.Infrastructure.UserDatabase
open WebApplication.Infrastructure.HtmlTemplate

type HomeController(logger: ILogger<HomeController>, htmlTemplate: IHtmlTemplate, databaseOptions: IOptions<Database>) =
    inherit Controller()

    let dbConnectionString =
        databaseOptions.Value.ConnectionString
        |> String.valueOrThrow "The database connection string is missing"

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
                  SortDirection = SortDirection.optional "Ascending" }

            let! pagedData = UserDatabase.getPagedData dbConnectionString query

            let searchResultsContent (index: int) (user: User) =
                let hadNextPage = (query.Page * pagedData.PageSize) < pagedData.TotalCount

                let isLastItem = pagedData.Data.Length = (index + 1)

                if hadNextPage && isLastItem then
                    htmlTemplate
                        .Bind("DisplayName", user.DisplayName)
                        .Bind("EmailAddress", user.EmailAddress)
                        .Bind("TypeName", user.TypeName |> UserType.value)
                        .Bind("TagClass", (if user.IsActive then "tag is-success" else "tag"))
                        .Bind("IsActive", (if user.IsActive then "Yes" else "No"))
                        .Bind("Page", query.Page + 1)
                        .Render("templates/user/search-results-infinite-scroll.html")
                else
                    htmlTemplate
                        .Bind("DisplayName", user.DisplayName)
                        .Bind("EmailAddress", user.EmailAddress)
                        .Bind("TypeName", user.TypeName |> UserType.value)
                        .Bind("TagClass", (if user.IsActive then "tag is-success" else "tag"))
                        .Bind("IsActive", (if user.IsActive then "Yes" else "No"))
                        .Render("templates/user/search-results.html")

            let content = htmlTemplate.Reduce(pagedData.Data, searchResultsContent)

            return this.HtmlContent content
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
