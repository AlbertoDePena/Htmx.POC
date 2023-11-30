namespace WebApplication.Domain.Extensions

[<RequireQualifiedAccess>]
module String =
    open System.Text.RegularExpressions

    /// The default value of a string is null.
    let defaultValue = null

    let ofEmailAddress (value: string) =        
        value
        |> Option.ofObj
        |> Option.filter (fun text -> Regex.IsMatch(text, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"))
        |> Option.map (fun text -> text.ToLower())