namespace WebApplication.Controllers

open Microsoft.AspNetCore.Mvc
open Feliz.ViewEngine

[<AutoOpen>]
module ControllerExtensions =

    [<Literal>]
    let ContentType = "text/html"

    type Controller with

        /// Render HTML fragment
        member this.HtmlFragment(html: ReactElement list) =
            this.Content(Render.htmlView (React.fragment html), ContentType)

        /// Render HTML document with <!DOCTYPE html>
        member this.HtmlDocument(html: ReactElement list) =
            
            let navbar =
                Html.nav [
                    prop.classes [ "navbar"; "is-fixed-top"; "is-white"; "has-shadow" ]
                    prop.role.navigation
                    prop.ariaLabel "main navigation"
                    prop.children [
                        Html.div [
                            prop.classes [ "navbar-brand" ]
                            prop.children [
                                Html.a [
                                    prop.classes [ "navbar-item" ]
                                    prop.href "/"
                                    prop.children [
                                        Html.img [ prop.src "https://craneww-assets.azureedge.net/assets/crane-logo.svg" ]
                                    ]
                                ]

                                Html.a [
                                    prop.ariaExpanded false
                                    prop.ariaLabel "menu"
                                    prop.classes [
                                        "navbar-burger"
                                    ]
                                    prop.role.button
                                    prop.hyperscript "on click toggle .is-active on me then toggle .is-active on #MainNavbar"
                                    prop.children [
                                        Html.span [ prop.ariaHidden true ]
                                        Html.span [ prop.ariaHidden true ]
                                        Html.span [ prop.ariaHidden true ]
                                    ]
                                ]
                            ]
                        ]

                        Html.div [
                            prop.id "MainNavbar"
                            prop.classes [
                                "navbar-menu"
                            ]
                            prop.children [
                                Html.div [
                                    prop.className "navbar-start"
                                    prop.children [
                                        Html.a [
                                            prop.classes [
                                                "navbar-item"
                                                "is-size-7"
                                                "has-text-link"
                                                "has-text-weight-semibold"
                                                //if state.CurrentUrl = Url.Dashboard then "is-active"
                                            ]
                                            prop.href "/"
                                            prop.text "Dashboard"
                                        ]
                                        Html.a [
                                            prop.classes [
                                                "navbar-item"
                                                "is-size-7"
                                                "has-text-link"
                                                "has-text-weight-semibold"
                                                //if state.CurrentUrl = Url.Inventory then "is-active"
                                            ]
                                            prop.href "/inventory"
                                            prop.text "Inventory"
                                        ]
                                        Html.a [
                                            prop.classes [
                                                "navbar-item"
                                                "is-size-7"
                                                "has-text-link"
                                                "has-text-weight-semibold"
                                                //if state.CurrentUrl = Url.TrackAndTrace then "is-active"
                                            ]
                                            prop.href "/shipments"
                                            prop.text "Shipments"
                                        ]
                                        Html.a [
                                            prop.classes [
                                                "navbar-item"
                                                "is-size-7"
                                                "has-text-link"
                                                "has-text-weight-semibold"
                                                //if state.CurrentUrl = Url.Documents then "is-active"
                                            ]
                                            prop.href "/documents"
                                            prop.text "Documents"
                                        ]
                                        //match state.CurrentUser with
                                        //| Deferred.Resolved(Ok user) ->
                                        //    if user.CanViewAnalytics then
                                        //        Html.a [
                                        //            prop.classes [
                                        //                "navbar-item"
                                        //                "is-size-7"
                                        //                "has-text-link"
                                        //                "has-text-weight-semibold"
                                        //                if state.CurrentUrl = Url.AnalyticsDashboard then
                                        //                    "is-active"
                                        //            ]
                                        //            prop.href (Router.format ("/analytics-dashboard"))
                                        //            prop.text "Analytics"
                                        //        ]
                                        //| _ -> Html.none
                                    ]
                                ]

                                Html.div [
                                    prop.className "navbar-end"
                                    prop.children [
                                        Html.div [
                                            prop.classes [ "navbar-item" ]
                                            //prop.children [
                                            //    AccountSearch.render state.AccountSearch (AccountSearchMsg >> dispatch)
                                            //]
                                        ]
                                        Html.div [
                                            prop.classes [ "navbar-item"; "has-dropdown"; "is-hoverable" ]
                                            prop.children [
                                                Html.a [
                                                    prop.classes [
                                                        "navbar-link"
                                                        "is-size-7"
                                                        "has-text-link"
                                                        "has-text-weight-semibold"
                                                    ]
                                                    //prop.text (
                                                    //    match state.CurrentUser with
                                                    //    | Deferred.HasNotStartedYet -> String.Empty
                                                    //    | Deferred.InProgress -> "Loading..."
                                                    //    | Deferred.Resolved(Error _) -> String.Empty
                                                    //    | Deferred.Resolved(Ok user) -> user.DisplayName
                                                    //)
                                                ]
                                                Html.div [
                                                    prop.className "navbar-dropdown"
                                                    prop.children [
                                                        Html.a [
                                                            prop.classes [ "navbar-item"; "is-size-7" ]
                                                            prop.href "/user-preferences"
                                                            prop.text "Preferences"
                                                        ]
                                                        Html.a [
                                                            prop.classes [ "navbar-item"; "is-size-7" ]
                                                            //prop.onClick (fun _ -> dispatch SignOut)
                                                            prop.text "Log Out"
                                                        ]
                                                    ]
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]

            let main =
                Html.main [
                    prop.classes [ "container-fluid"; "mb-6"; "p-6"; "is-size-7" ]
                    prop.children [
                        Html.div html
                    ]
                ]

            let footer =
                Html.footer [
                    prop.classes [ "footer"; "is-fixed-bottom" ]
                    prop.children [
                        Html.div [
                            prop.classes [ "content"; "has-text-centered"; "is-size-7" ]
                            prop.children [
                                Html.div [
                                    prop.classes [ "columns" ]
                                    prop.children [
                                        Html.div [
                                            prop.classes [ "column" ]
                                            prop.children [
                                                Html.a [
                                                    prop.classes [ "mx-4"; "has-text-light" ]
                                                    prop.href "https://craneww.com/privacy-policy/"
                                                    prop.target "_blank"
                                                    prop.text "Privacy Policy"
                                                ]

                                                Html.a [
                                                    prop.classes [ "mx-4"; "has-text-light" ]
                                                    prop.href "https://craneww.com/terms-of-service/"
                                                    prop.target "_blank"
                                                    prop.text "Terms of Service"
                                                ]
                                            ]
                                        ]

                                        Html.div [
                                            prop.classes [ "column"; "is-hidden-mobile" ]
                                            prop.children [
                                                Html.a [
                                                    prop.classes [ "has-text-light" ]
                                                    prop.href "https://craneww.com/"
                                                    prop.target "_blank"
                                                    prop.text "Crane Worldwide Logistics"
                                                ]
                                            ]
                                        ]
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            
            let document =
                Html.html [
                    prop.lang "en"
                    prop.children [
                        Html.head [
                            prop.children [
                                Html.meta [ 
                                    prop.charset.utf8 
                                ]
                                Html.meta [ 
                                    prop.name "viewport"
                                    prop.content "width=device-width, initial-scale=1"
                                ]
                                Html.title "Htmx POC"
                                Html.link [
                                    prop.href "https://craneww-assets.azureedge.net/assets/craneww.min.css"
                                    prop.rel.stylesheet
                                ]
                                Html.link [
                                    prop.href "https://craneww-assets.azureedge.net/assets/favicon.ico"
                                    prop.rel.icon
                                ]
                            ]
                        ]

                        Html.body [
                            prop.classes [ "has-navbar-fixed-top" ]
                            prop.children [
                                navbar
                                main
                                footer
                                Html.script [ prop.src "https://unpkg.com/htmx.org@1.9.6" ]
                                Html.script [ prop.src "https://unpkg.com/hyperscript.org@0.9.12" ]
                            ]
                        ]
                    ]
                ]

            this.Content(Render.htmlDocument document, ContentType)
