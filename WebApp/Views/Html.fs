namespace WebApp.Views

open System
open System.Globalization
open System.Net

[<RequireQualifiedAccess>]
module Html =

    [<Literal>]
    let NonBreakingSpace = "&nbsp;"

    let disabled (value: bool) : string =
        match value with
        | true -> "disabled"
        | false -> ""

    let readonly (value: bool) : string =
        match value with
        | true -> "readonly"
        | false -> ""

    let required (value: bool) : string =
        match value with
        | true -> "required"
        | false -> ""

    let antiforgery (formFieldName: string) (requestToken: string) : string =
        $"""<input name="{formFieldName}" type="hidden" value="{requestToken}">"""

    let encode (value: string) : string = WebUtility.HtmlEncode value

    let forEach<'a> (items: 'a list) (mapping: 'a -> string) (separator: string) : string =
        items |> List.map mapping |> String.concat separator

    let formatDate (date: DateOnly) : string =
        date.ToString("o", CultureInfo.InvariantCulture)

    let formatDateTime (dateTime: DateTime) : string =
        dateTime.ToString("o", CultureInfo.InvariantCulture)

    let formatDateTimeOffset (dateTimeOffset: DateTimeOffset) : string =
        dateTimeOffset.ToString("o", CultureInfo.InvariantCulture)
