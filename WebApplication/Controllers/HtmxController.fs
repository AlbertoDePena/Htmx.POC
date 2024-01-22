namespace WebApp.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApp.Infrastructure.HtmlTemplate

[<AbstractClass>]
type HtmxController(logger: ILogger, htmlTemplate: IHtmlTemplate) =
    inherit Controller()

    [<Literal>]
    let HtmlContentType = "text/html; charset=UTF-8"

    member this.HtmlContent(content: string) =
        this.Content(content, HtmlContentType)

    member this.HtmlContent(userName: string, mainContent: string) =
        task {
            let content =
                htmlTemplate
                    .Bind("UserName", userName)
                    .Bind("MainContent", mainContent)
                    .Render("index.html")

            return this.HtmlContent content
        }
