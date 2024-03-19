namespace WebApp.Infrastructure.HtmlTemplate

open System
open System.IO
open System.Net
open System.Text
open System.Text.RegularExpressions

/// The path to a HTML file or a HTML string.
type FileOrContent = string

/// The HTML template defines identifiers with the following syntax: ${Identifier.Key}.
type Identifier = string

/// The HTML template defines keys with the following syntax: ${Key}.
type Key = string

/// The content to replace the ${Key} with.
type Value = obj

/// The HTML content.
type HtmlContent = string

/// Token to protect against Cross-Site Request Forgery.
type AntiforgeryToken =
    { FormFieldName: string
      RequestToken: string }

type HtmlTemplateException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = HtmlTemplateException(Exception message)

type HtmlTemplate(htmlContent: string, identifier: string) =

    let stringBuilder = StringBuilder(htmlContent)

    let mutable bindings: Map<string, string> = Map.empty

    let createBinding (key: Key) (value: Value) (encodeValue: bool) : unit =
        if String.IsNullOrWhiteSpace key then
            failwith "The key cannot be null/empty/white-space"

        if isNull value then
            failwith "The value cannot be null"

        let isString = value.GetType() = typeof<String>

        let valueAsString = value.ToString()

        let sanitizedKey =
            if String.IsNullOrWhiteSpace identifier then
                key
            else
                sprintf "%s.%s" identifier key

        let sanitizedValue =
            if encodeValue && isString then
                valueAsString |> WebUtility.HtmlEncode
            else
                valueAsString

        bindings <- bindings |> Map.add sanitizedKey sanitizedValue

    let buildHtmlContent () : string =
        bindings
        |> Map.iter (fun name value ->
            let pattern = sprintf "${%s}" name
            stringBuilder.Replace(pattern, value) |> ignore)

        bindings <- Map.empty
        stringBuilder.ToString()

    let failOnUnboundedValues (htmlContent: string) : unit =
        let unboundedValues =
            Regex.Matches(htmlContent, @"\${\b\w+\.*\w+\b}")
            |> Seq.collect (fun match' -> match'.Groups |> Seq.map (fun group -> group.Value))
            |> fun values -> String.Join(", ", values)

        if String.IsNullOrWhiteSpace unboundedValues |> not then
            sprintf "The HTML content has unbounded values: %s" unboundedValues |> failwith

    /// <exception cref="HtmlTemplateException"></exception>
    member this.WithAntiforgery(getAntiforgeryToken: unit -> AntiforgeryToken) : HtmlTemplate =
        try
            let token = getAntiforgeryToken ()

            let value =
                $"""<input name="{token.FormFieldName}" type="hidden" value="{token.RequestToken}">"""

            createBinding "Antiforgery" value false
            this
        with ex ->
            HtmlTemplateException ex |> raise

    /// <exception cref="HtmlTemplateException"></exception>
    member this.ReplaceRaw(key: Key, value: Value) : HtmlTemplate =
        try
            createBinding key value false
            this
        with ex ->
            HtmlTemplateException ex |> raise

    /// <exception cref="HtmlTemplateException"></exception>
    member this.Replace(key: Key, value: Value) : HtmlTemplate =
        try
            createBinding key value true
            this
        with ex ->
            HtmlTemplateException ex |> raise

    /// <exception cref="HtmlTemplateException"></exception>
    member this.Replace
        (
            identifier: Identifier,
            items: 'a list,
            mapping: ('a * HtmlTemplate) -> HtmlTemplate
        ) : HtmlTemplate =
        try
            if String.IsNullOrWhiteSpace identifier then
                failwith "The identifier cannot be null/empty/white-space"

            let listToken = sprintf "${list %s}" identifier
            let endToken = sprintf "${end %s}" identifier

            let innerHtmlContent = stringBuilder.ToString()

            let listTokenIndex = innerHtmlContent.IndexOf(listToken)
            let endTokenIndex = innerHtmlContent.IndexOf(endToken)

            let templateList =
                innerHtmlContent
                    .Substring(listTokenIndex, endTokenIndex - listTokenIndex)
                    .Replace(listToken, "")
                    .Replace(endToken, "")
                    .TrimStart()
                    .TrimEnd()

            let htmlContents =
                items
                |> List.map (fun item ->
                    let templateWithIdentifier = HtmlTemplate(templateList, identifier)
                    let itemTemplate = mapping (item, templateWithIdentifier)
                    itemTemplate.Render())
                |> String.concat ""

            stringBuilder
                .Replace(templateList, htmlContents)
                .Replace(listToken, "")
                .Replace(endToken, "")
            |> ignore

            this
        with ex ->
            HtmlTemplateException ex |> raise

    /// <exception cref="HtmlTemplateException"></exception>
    member this.Render() : HtmlContent =
        try
            let htmlContent = buildHtmlContent ()

            failOnUnboundedValues htmlContent

            htmlContent
        with ex ->
            HtmlTemplateException ex |> raise

[<RequireQualifiedAccess>]
module Html =

    let private currentDirectory =
        Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location)

    let load (fileOrContent: FileOrContent) =
        if isNull fileOrContent then
            nameof fileOrContent |> sprintf "%s cannot be null" |> failwith

        let htmlContent =
            if fileOrContent.EndsWith(".html") then
                Path.Combine(currentDirectory, fileOrContent) |> File.ReadAllText
            else
                fileOrContent

        HtmlTemplate(htmlContent, "")

    let csrf getToken (template: HtmlTemplate) = template.WithAntiforgery(getToken)

    let replace key value (template: HtmlTemplate) = template.Replace(key, value)

    let replaceRaw key value (template: HtmlTemplate) = template.ReplaceRaw(key, value)

    let replaceList identifier items mapping (template: HtmlTemplate) =
        template.Replace(identifier, items, (fun (x, y) -> mapping x y))

    let render (template: HtmlTemplate) = template.Render()