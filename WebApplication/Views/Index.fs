namespace WebApplication.Views

[<RequireQualifiedAccess>]
module IndexView =
    open Giraffe.ViewEngine
    open Giraffe.ViewEngine.Accessibility

    let render (userName: string) (mainContent: XmlNode) =
        let headNode =
            head [] [
                meta [
                    _charset "utf-8"
                    _name "viewport"
                    _content "width=device-width, initial-scale=1"
                ]
                title [] [ encodedText "HTM POC" ]
                link [ _rel "stylesheet"; _href "https://fonts.googleapis.com/css?family=Poppins" ]
                link [
                    _rel "stylesheet"
                    _href "https://fonts.googleapis.com/icon?family=Material+Icons"
                ]
                link [
                    _rel "stylesheet"
                    _href "https://cdn.jsdelivr.net/npm/bulma@0.9.4/css/bulma.min.css"
                ]
                link [ _rel "stylesheet"; _href "../index.css" ]
                link [ _rel "icon shortcut"; _href "../favicon.ico"; _type "image/x-icon" ]
            ]

        let navNode =
            nav [
                _class "navbar is-fixed-top is-white has-shadow"
                _roleNavigation
                _ariaLabel "main navigation"
            ] [
                div [ _class "navbar-brand" ] [
                    a [ _class "navbar-item"; _href "/" ] [ img [ _src "https://bulma.io/images/bulma-logo.png" ] ]
                    a [
                        _ariaExpanded "false"
                        _ariaLabel "menu"
                        _class "navbar-burger"
                        _roleButton
                        _data "target" "MainNavbar"
                    ] [
                        span [ _ariaHidden "true" ] []
                        span [ _ariaHidden "true" ] []
                        span [ _ariaHidden "true" ] []
                    ]
                ]
                div [ _class "navbar-menu"; _id "MainNavbar" ] [
                    div [ _class "navbar-start" ] [
                        a [ _class "navbar-item"; _href "/Home" ] [ encodedText "Home" ]
                        a [ _class "navbar-item"; _href "/Demo" ] [ encodedText "Demo" ]
                    ]
                    div [ _class "navbar-end" ] [
                        div [ _class "navbar-item has-dropdown is-hoverable" ] [
                            a [ _class "navbar-link is-size-7 has-text-link has-text-weight-semibold" ] [
                                encodedText userName
                            ]
                            div [ _class "navbar-dropdown" ] [
                                a [ _class "navbar-item is-size-7"; _href "#/logout" ] [ encodedText "Log Out" ]
                            ]
                        ]
                    ]
                ]
            ]

        let mainNode = main [ _class "p-5" ] [ mainContent ]

        let footerNode =
            footer [ _class "footer is-fixed-bottom" ] [
                p [ _class "has-text-centered" ] [ encodedText "This is a test" ]
            ]

        html [ _lang "en" ] [
            headNode
            body [ _class "has-navbar-fixed-top has-footer-fixed-bottom is-size-7" ] [
                navNode
                mainNode
                footerNode
                script [ _src "https://unpkg.com/htmx.org@1.9.6" ] []
                script [ _src "../index.js" ] []
            ]
        ]
        |> RenderView.AsString.htmlDocument
