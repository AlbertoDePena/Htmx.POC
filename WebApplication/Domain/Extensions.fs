namespace WebApplication.Domain.Extensions

[<RequireQualifiedAccess>]
module Array =

    /// Convert a potentially null value to an empty array.
    let ofNull (items: 'T array) =
        if isNull items then Array.empty<'T> else items

[<RequireQualifiedAccess>]
module Seq =

    /// Convert a potentially null value to an empty sequence.
    let ofNull (items: 'T seq) =
        if isNull items then Seq.empty<'T> else items

[<RequireQualifiedAccess>]
module String =

    /// The default value of a string is null.
    let defaultValue = null

    let toEmailAddress (value: string) =
        if isNull value then
            None
        elif
            System.Text.RegularExpressions.Regex.IsMatch(value, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$")
            |> not
        then
            None
        else
            Some(value.ToLower())

    /// <summary>Check for null on a string</summary>
    /// <exception cref="System.Exception">Throw exception when the string is null</exception>
    let valueOrThrow exceptionMessage (value: string) =
        match value with
        | null -> failwith exceptionMessage
        | _ -> value

[<AutoOpen>]
module StringExtensions =

    type System.String with

        /// Convert a potentially null string to an empty string.
        member this.NonNull =
            match this with
            | null -> System.String.Empty
            | _ -> this
