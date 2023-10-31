namespace WebApplication.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open System.Diagnostics

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApplication.Domain.Extensions

type HomeController(logger: ILogger<HomeController>) =
    inherit Controller()

    member this.Clicked() =

        let htmlContent =
            HtmlTemplate("<div><p>Content retrieved by ${Htmx}, so cool!</p><p>I am ${Age}</p></div>")
                .Bind("Htmx", "HTMX")
                .Bind("Age", 33.5)
                .Compile()

        this.HtmlContent htmlContent

    member this.SayHello() =
        let content = HtmlTemplate("""<div class="has-text-centered has-text-weight-bold">Hello World!</div>""").Compile()

        this.HtmlContent content

    member this.TriggerDelay() =
        let value =
            this.Request.TryGetQueryStringValue "q"
            |> Option.defaultValue String.defaultValue

        let content =
            HtmlTemplate("<div>The value is ${Value}</div>").Bind("Value", value).Compile()

        this.HtmlContent content

    member this.Index() =

        let content =
            HtmlTemplate("templates/index.html").Bind("UserName", "Alberto De Pena").Compile()

        this.HtmlContent content
