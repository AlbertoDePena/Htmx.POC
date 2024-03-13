namespace WebApp.Views

open System
open System.Globalization
open System.Net

[<RequireQualifiedAccess>]
module Html =

    type AntiforgeryToken =
        { FormFieldName: string
          RequestToken: string }

    [<NoEquality>]
    [<NoComparison>]
    type SharedProps =
        { IsHtmxBoosted: bool
          UserName: string
          GetAntiforgeryToken: unit -> AntiforgeryToken }

    let antiforgery (getAntiforgeryToken: unit -> AntiforgeryToken) : string =
        let token = getAntiforgeryToken ()

        $"""<input name="{token.FormFieldName}" type="hidden" value="{token.RequestToken}">"""

    let encodeString (value: string) : string = WebUtility.HtmlEncode value

    let encodeText (text: WebApp.Domain.Shared.Text) : string = encodeString text.Value

    let forEach<'a> (items: 'a list) (mapping: 'a -> string) (separator: string) : string =
        items |> List.map mapping |> String.concat separator

    let formatDateTime (dataTime: DateTime) : string =
        dataTime.ToString("o", CultureInfo.InvariantCulture)

    let formatDateTimeOffset (dateTimeOffset: DateTimeOffset) : string =
        dateTimeOffset.ToString("o", CultureInfo.InvariantCulture)

[<RequireQualifiedAccess>]
module HtmlAttribute =

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
