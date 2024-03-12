namespace WebApp.Controllers

open System
open System.Threading.Tasks

open FsToolkit.ErrorHandling

open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApp.Domain.Shared
open WebApp.Infrastructure.Constants
open WebApp.Infrastructure.UserDatabase
open WebApp.Views

type UsersController(logger: ILogger<UsersController>, antiforgery: IAntiforgery, userDatabase: IUserDatabase) =
    inherit HtmxController(antiforgery)

    [<HttpGet>]
    member this.Index() : Task<IActionResult> =
        task {
            let props: UserView.MainProps =
                { HtmxRequest = Htmx.Request.Create(this.Request.IsHtmxBoosted(), this.GetUserName())
                  GetAntiforgeryToken = this.GetAntiforgeryToken }

            let htmlContent = UserView.renderMain props

            return this.HtmlContent(htmlContent)
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

                let props: UserView.TableProps = { PagedData = pagedData }

                let htmlContent = UserView.renderTable props

                return this.HtmlContent htmlContent
            else
                return! this.Index()
        }
