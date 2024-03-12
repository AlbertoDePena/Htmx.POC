namespace WebApp.Views

[<RequireQualifiedAccess>]
module Htmx =

    [<RequireQualifiedAccess>]
    type Request =
        | FullPage of userName: string
        | MainContent

        static member Create(isBoosted: bool, userName: string) : Request =
            if isBoosted then
                Request.MainContent
            else
                Request.FullPage userName
