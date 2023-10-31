namespace WebApplication.Controllers

open Microsoft.AspNetCore.Mvc

[<AutoOpen>]
module ControllerExtensions =

    [<Literal>]
    let HtmlContentType = "text/html"

    type Controller with
        
        member this.HtmlContent(content: string) =
            this.Content(content, HtmlContentType)
