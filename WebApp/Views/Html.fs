﻿namespace WebApp.Views

open System
open System.Globalization
open System.Net

[<RequireQualifiedAccess>]
module Html =
    
    type AntiforgeryToken =
        { FormFieldName: string
          RequestToken: string }

    let antiforgery (generateAntiforgeryToken: unit -> AntiforgeryToken) : string =
        let token = generateAntiforgeryToken ()

        $"""<input name="{token.FormFieldName}" type="hidden" value="{token.RequestToken}">"""

    let encode (text: string) : string = WebUtility.HtmlEncode text

    let forEach<'a> (mapping: 'a -> string) (separator: string) (items: 'a list) : string =
        items |> List.map mapping |> String.concat separator

    let formatDateTime (dataTime: DateTime) : string =
        dataTime.ToString("o", CultureInfo.InvariantCulture)

    let formatDateTimeOffset (dateTimeOffset: DateTimeOffset) : string =
        dateTimeOffset.ToString("o", CultureInfo.InvariantCulture)    