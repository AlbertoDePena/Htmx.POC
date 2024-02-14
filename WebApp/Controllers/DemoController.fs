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
open WebApp.Infrastructure.HtmlMarkup

type DemoController(logger: ILogger<DemoController>, htmlMarkup: IHtmlMarkup) =
    inherit HtmxController(logger, htmlMarkup)

    let random = Random()

    member this.Index() =
        let htmlContent =
            htmlMarkup
                .Load("demo.html")
                .UseAntiforgery("Antiforgery", this.HttpContext)
                .Render()

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
