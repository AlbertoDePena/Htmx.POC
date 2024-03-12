﻿namespace WebApp.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApp.Infrastructure.HtmlTemplate

[<AbstractClass>]
type HtmxController(logger: ILogger, templateLoader: HtmlTemplateLoader) =
    inherit Controller()

    [<Literal>]
    let HtmlContentType = "text/html; charset=UTF-8"

    member this.HtmlContent(htmlContent: string) : IActionResult =
        this.Content(htmlContent, HtmlContentType) :> IActionResult

    member this.HtmlContent(userName: string, mainContent: string) : IActionResult =
        let htmlContent =
            templateLoader
                .Load("index.html")
                .Bind("UserName", userName)
                .Bind("MainContent", mainContent)
                .Render()

        this.HtmlContent htmlContent
