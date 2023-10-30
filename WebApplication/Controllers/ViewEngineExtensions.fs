namespace WebApplication.Controllers

open Feliz.ViewEngine

[<AutoOpen>]
module ViewEngineExtensions =
    
    type htmx private() =
        
        static let instance = new htmx()
        
        static member Instance = instance

        member this.confirm (value: string) =
            prop.custom ("hx-confirm", value)

        member this.delete (value: string) =
            prop.custom ("hx-delete", value)

        member this.disable (value: string) =
            prop.custom ("hx-disable", value)

        member this.disableElt (value: string) =
            prop.custom ("hx-disable-elt", value)

        member this.get (value: string) =
            prop.custom ("hx-get", value) 

        member this.swap (value: string) =
            prop.custom ("hx-swap", value)

        member this.target (value: string) =
            prop.custom ("hx-target", value) 

        member this.trigger (value: string) =
            prop.custom ("hx-trigger", value) 

    type prop with
        
        static member hyperscript (value: string) =
            prop.custom ("_", value)  

        static member htmx =
            htmx.Instance