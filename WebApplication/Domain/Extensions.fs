namespace WebApplication.Domain.Extensions

[<RequireQualifiedAccess>]
module String =
    open System.Text.RegularExpressions

    /// The default value of a string is null.
    let defaultValue = null

    let ofEmailAddress (value: string) =
        if isNull value then
            None
        elif Regex.IsMatch(value, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$") |> not then
            None
        else
            Some(value.ToLower())