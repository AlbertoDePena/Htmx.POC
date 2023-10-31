namespace WebApplication.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open System.Diagnostics

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApplication.Domain.Extensions
open WebApplication.Infrastructure

type HomeController(logger: ILogger<HomeController>, htmlTemplate: IHtmlTemplate) =
    inherit Controller()

    member this.Clicked() =

        let htmlContent =
            htmlTemplate
                .Bind("Htmx", "HTMX")
                .Bind("Age", 33.5)
                .Compile("<div><p>Content retrieved by ${Htmx}, so cool!</p><p>I am ${Age}</p></div>")

        this.HtmlContent htmlContent

    member this.SayHello() =
        let content =
            htmlTemplate.Compile("""<div class="has-text-centered has-text-weight-bold">Hello World!</div>""")

        this.HtmlContent content

    member this.TriggerDelay() =
        let value =
            this.Request.TryGetQueryStringValue "q"
            |> Option.defaultValue String.defaultValue

        let content =
            htmlTemplate.Bind("Value", value).Compile("<div>The value is ${Value}</div>")

        this.HtmlContent content

    member this.Index() =

        let content =
            htmlTemplate.Bind("UserName", "Alberto De Pena").Compile("templates/index.html")

        this.HtmlContent content
