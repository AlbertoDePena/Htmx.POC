namespace WebApp.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApp.Infrastructure.HtmlMarkup

[<AbstractClass>]
type HtmxController(logger: ILogger, htmlMarkup: IHtmlMarkup) =
    inherit Controller()

    [<Literal>]
    let HtmlContentType = "text/html; charset=UTF-8"

    member this.HtmlContent(content: string) : IActionResult =
        this.Content(content, HtmlContentType) :> IActionResult

    member this.HtmlContent(userName: string, mainContent: string) : IActionResult =
        let content =
            htmlMarkup
                .Load("index.html")
                .Bind("UserName", userName)
                .Bind("MainContent", mainContent)
                .Render()

        this.HtmlContent content
