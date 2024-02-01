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
        this.HtmlContent(userName = "Alberto De Pena", mainContent = "demo.html")

    member this.Random() =
        task {
            if this.Request.IsHtmx() then
                do! Task.Delay(3000)

                let randomNumber = random.NextDouble()

                return this.HtmlContent(randomNumber.ToString())
            else
                return this.Index()
        }
