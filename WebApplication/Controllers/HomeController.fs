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
open WebApplication.Infrastructure.Database
open WebApplication.Infrastructure.UserDatabase
open WebApplication.Infrastructure


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
                  SortDirection = SortDirection.tryCreate "Ascending" }

            let! pagedData = UserDatabase.getPagedData dbConnectionString query

            let buildUserTemplate (index: int) (user: User) =
                let hadNextPage = (query.Page * pagedData.PageSize) < pagedData.TotalCount

                let isLastItem = pagedData.Data.Length = (index + 1)

                let htmx =
                    if hadNextPage && isLastItem then
                        $"""
                            hx-get="/Home/Search?page={query.Page + 1}"
                            hx-trigger="intersect once"
                            hx-swap="afterend"
                            hx-target="this"
                        """
                    else 
                        $"""
                            hx-swap="innerHTML"
                        """

                $"""
                <tr class="is-clickable" hx-include="#user-search-form" {htmx}>
                    <td class="p-2">{user.DisplayName}</td>
                    <td class="p-2">{user.EmailAddress}</td>
                    <td class="p-2">{user.TypeName |> UserType.value}</td>
                    <td class="p-2">
                        <span class="tag {if user.IsActive then "is-success" else ""}">
                            {if user.IsActive then "Yes" else "No"}
                        </span>
                    </td>
                </tr>
                """

            let template =
                pagedData.Data
                |> List.mapi (fun index user -> buildUserTemplate index user)
                |> List.fold (+) String.Empty

            return this.HtmlContent template
        }

    member this.Index() =
        task {            
            let content =
                htmlTemplate
                    .Bind("UserName", "Alberto De Pena")
                    .Bind("PageContent", "templates/user/search-control.html")
                    .Compile("templates/index.html")

            return this.HtmlContent content
        }
