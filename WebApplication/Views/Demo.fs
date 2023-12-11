namespace WebApplication.Views

[<RequireQualifiedAccess>]
module DemoView =
    open Giraffe.ViewEngine
    open Giraffe.ViewEngine.Accessibility

    let render () =
        div [
            _class "container"
            attr "hx-indicator" ".htmx-indicator"
            attr "hx-push-url" "true"
            attr "hx-swap" "innerHTML"
            attr "hx-target" "#output"
        ] [
            h3 [ _class "title is-size-1" ] [ encodedText "Random Number Generator" ]
            button [
                _class "button is-primary block"
                attr "hx-get" "/Demo/Random"
                _type "button"
            ] [ encodedText "Get Random Number" ]
            h2 [ _class "title is-size-2"; _id "output" ] [ encodedText "N/A" ]
            div [ _class "box" ] [ p [] [ encodedText "This is a test" ] ]
            div [ _class "loader-container htmx-indicator" ] [ div [ _class "loader" ] [] ]
        ]
