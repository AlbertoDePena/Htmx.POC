namespace WebApp.Controllers

open System
open System.Threading.Tasks

open FsToolkit.ErrorHandling

open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApp.Domain.User
open WebApp.Domain.Shared
open WebApp.Infrastructure.Constants
open WebApp.Infrastructure.Database
open WebApp.Infrastructure.UserDatabase
open WebApp.Infrastructure.HtmlTemplate

type DemoController(logger: ILogger<DemoController>, antiforgery: IAntiforgery, htmlTemplate: HtmlTemplate) =
    inherit HtmxController(logger, htmlTemplate)

    let random = Random()

    [<HttpGet>]
    member this.Index() : Task<IActionResult> =
        task {
            let htmlContent =
                htmlTemplate.Render(
                    "demo.html",
                    (fun binder ->
                        binder.BindAntiforgery(fun () ->
                            let token = antiforgery.GetAndStoreTokens(this.HttpContext)

                            { FormFieldName = token.FormFieldName
                              RequestToken = token.RequestToken }))
                )

            if this.Request.IsHtmxBoosted() then
                return this.HtmlContent(htmlContent)
            else
                return this.HtmlContent(userName = this.HttpContext.User.Identity.Name, mainContent = htmlContent)
        }

    [<HttpGet>]
    member this.Random() : Task<IActionResult> =
        task {
            if this.Request.IsHtmx() then
                do! Task.Delay(3000)

                let randomNumber = random.NextDouble()

                return this.HtmlContent(randomNumber.ToString())
            else
                return! this.Index()
        }

    [<HttpPost>]
    member this.Save() : Task<IActionResult> =
        task {
            if this.Request.IsHtmx() then
                return this.HtmlContent("Saved!")
            else
                return! this.Index()
        }
