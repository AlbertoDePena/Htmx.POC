namespace WebApp.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApp.Infrastructure.HtmlTemplate

[<AbstractClass>]
type HtmxController(logger: ILogger, htmlTemplate: IHtmlTemplate) =
    inherit Controller()

    [<Literal>]
    let HtmlContentType = "text/html; charset=UTF-8"

    member this.HtmlContent(content: string) : IActionResult =
        this.Content(content, HtmlContentType) :> IActionResult

    member this.HtmlContent(userName: string, mainContent: string) : IActionResult =
        let content =
            htmlTemplate
                .Bind("UserName", userName)
                .Bind("MainContent", mainContent)
                .Render("index.html")

        this.HtmlContent content
