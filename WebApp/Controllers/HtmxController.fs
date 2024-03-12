namespace WebApp.Controllers

open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Mvc

open WebApp.Views

[<AbstractClass>]
type HtmxController(antiforgery: IAntiforgery) =
    inherit Controller()

    [<Literal>]
    let HtmlContentType = "text/html; charset=UTF-8"

    member this.HtmlContent(htmlContent: string) : IActionResult =
        this.Content(htmlContent, HtmlContentType) :> IActionResult

    member this.GetAntiforgeryToken() : Html.AntiforgeryToken =
        let token = antiforgery.GetAndStoreTokens(this.HttpContext)

        { FormFieldName = token.FormFieldName
          RequestToken = token.RequestToken }

    member this.GetUserName(): string =
        this.HttpContext.User.Identity.Name