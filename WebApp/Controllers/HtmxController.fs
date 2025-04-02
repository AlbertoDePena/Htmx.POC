namespace WebApp.Controllers

open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Mvc

[<AbstractClass>]
type HtmxController(antiforgery: IAntiforgery) =
    inherit Controller()

    [<Literal>]
    let HtmlContentType = "text/html; charset=UTF-8"

    member this.HtmlContent(htmlContent: string) : IActionResult =
        this.Content(htmlContent, HtmlContentType) :> IActionResult

    member this.GetUserName() : string = this.HttpContext.User.Identity.Name

    member this.GetAntiforgeryToken() : AntiforgeryTokenSet =
        antiforgery.GetAndStoreTokens this.HttpContext
