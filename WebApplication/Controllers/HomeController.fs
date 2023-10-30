namespace WebApplication.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open System.Diagnostics

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open Feliz.ViewEngine

open WebApplication.Domain.Extensions

type HomeController(logger: ILogger<HomeController>) =
    inherit Controller()

    member this.Clicked() =
        this.HtmlFragment [ Html.p "Content retrieved by HTMX, so cool!" ]

    member this.SayHello() =
        this.HtmlFragment [ Html.div "Hello World!" ]

    member this.TriggerDelay() =
        let value =
            this.Request.TryGetQueryStringValue "q"
            |> Option.defaultValue String.defaultValue

        this.HtmlFragment [ Html.div [ prop.text value ] ]

    member this.Index() =
        this.HtmlDocument [
            Html.h1 "Home"
            Html.button [
                prop.classes [ "button"; "is-small" ]
                prop.type'.button
                prop.htmx.get "/Home/Clicked"
                prop.htmx.swap "innerHTML"
                prop.text "Click me!"
            ]

            Html.button [
                prop.classes [ "button"; "is-small"; "is-primary" ]
                prop.type'.button
                prop.text "Say Hello"
                prop.htmx.trigger "click delay:1s"
                prop.htmx.get "/Home/SayHello"
                prop.htmx.target "#SayHelloContainer"
                prop.htmx.swap "innerHTML"
            ]

            Html.div [ prop.id "SayHelloContainer" ]

            Html.div [
                Html.input [
                    prop.placeholder "Search..."
                    prop.type'.text
                    prop.name "q"
                    prop.htmx.trigger "keyup changed delay:500ms"
                    prop.htmx.get "/Home/TriggerDelay"
                    prop.htmx.target "#search-results"
                    prop.htmx.swap "innerHTML"
                ]
                Html.div [ prop.id "search-results" ]
            ]
        ]
