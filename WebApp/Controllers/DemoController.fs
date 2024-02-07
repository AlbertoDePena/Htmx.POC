namespace WebApp.Controllers

open System
open System.Threading.Tasks

open FsToolkit.ErrorHandling

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApp.Domain.User
open WebApp.Domain.Shared
open WebApp.Infrastructure.Constants
open WebApp.Infrastructure.Database
open WebApp.Infrastructure.UserDatabase
open WebApp.Infrastructure.HtmlTemplate

type DemoController(logger: ILogger<DemoController>, htmlTemplate: IHtmlTemplate) =
    inherit HtmxController(logger, htmlTemplate)

    let random = Random()

    member this.Index() =
        let htmlContent =
            htmlTemplate
                .GenerateAntiforgery("Antiforgery", this.HttpContext)
                .Render("demo.html")

        this.HtmlContent(userName = "Alberto De Pena", mainContent = htmlContent)

    member this.Random() =
        task {
            if this.Request.IsHtmx() then
                do! Task.Delay(3000)

                let randomNumber = random.NextDouble()

                return this.HtmlContent(randomNumber.ToString())
            else
                return this.Index()
        }

    member this.Save() =
        task {
            if this.Request.IsHtmx() then

                return this.HtmlContent("Saved!")
            else
                return this.Index()
        }
