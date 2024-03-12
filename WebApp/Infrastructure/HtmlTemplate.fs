namespace WebApp.Infrastructure.HtmlTemplate

open System
open System.IO
open System.Net
open System.Text
open System.Text.RegularExpressions

/// Can be either a plain HTML string or a path to a HTML file.
type FileOrContent = string

/// The HTML template defines keys with the following syntax: ${Key}.
type Key = string

/// The content to replace the ${Key} with.
type Value = obj

/// The HTML content as a string.
type HtmlContent = string

type AntiforgeryToken =
    { FormFieldName: string
      RequestToken: string }

type HtmlTemplateException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = HtmlTemplateException(Exception message)

type HtmlTemplate(htmlContent: string) =

    let mutable bindings: Map<string, string> = Map.empty

    let createBinding (key: Key) (value: Value) (encodeValue: bool) : unit =
        if String.IsNullOrWhiteSpace key then
            failwith "The key cannot be null/empty/white-space"

        if isNull value then
            failwith "The value cannot be null"

        let isString = value.GetType() = typeof<String>

        let valueAsString = value.ToString()

        let isHtml = valueAsString.StartsWith("<")

        let sanitizedValue =
            if encodeValue && isString && (not isHtml) then
                valueAsString |> WebUtility.HtmlEncode
            else
                valueAsString

        bindings <- bindings |> Map.add key sanitizedValue

    let buildHtmlContent () : string =
        let stringBuilder = StringBuilder(htmlContent)

        bindings
        |> Map.iter (fun name value ->
            let pattern = sprintf "${%s}" name
            stringBuilder.Replace(pattern, value) |> ignore)

        bindings <- Map.empty
        stringBuilder.ToString()

    let failOnUnboundedValues (htmlContent: string) : unit =
        let unboundedValues =
            Regex.Matches(htmlContent, @"\${\b\w+\b}")
            |> Seq.collect (fun match' -> match'.Groups |> Seq.map (fun group -> group.Value))
            |> fun values -> String.Join(", ", values)

        if String.IsNullOrWhiteSpace unboundedValues |> not then
            sprintf "The HTML content has unbounded values: %s" unboundedValues |> failwith

    /// <exception cref="HtmlTemplateException"></exception>
    member this.BindAntiforgery(generateAntiforgeryToken: unit -> AntiforgeryToken) : HtmlTemplate =
        try
            let token = generateAntiforgeryToken ()

            let value =
                $"""<input name="{token.FormFieldName}" type="hidden" value="{token.RequestToken}">"""

            createBinding "Antiforgery" value false
            this
        with ex ->
            HtmlTemplateException ex |> raise

    /// <exception cref="HtmlTemplateException"></exception>
    member this.BindRaw(key: Key, value: Value) : HtmlTemplate =
        try
            createBinding key value false
            this
        with ex ->
            HtmlTemplateException ex |> raise

    /// <exception cref="HtmlTemplateException"></exception>
    member this.Bind(key: Key, value: Value) : HtmlTemplate =
        try
            createBinding key value true
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

type HtmlTemplateLoader(templateDirectory: string) =

    member this.Load(fileOrContent: FileOrContent) : HtmlTemplate =
        if isNull templateDirectory then
            nameof templateDirectory |> sprintf "%s cannot be null" |> failwith

        if isNull fileOrContent then
            nameof fileOrContent |> sprintf "%s cannot be null" |> failwith

        let htmlContent =
            if fileOrContent.EndsWith(".html") then
                Path.Combine(templateDirectory, fileOrContent) |> File.ReadAllText
            else
                fileOrContent

        HtmlTemplate(htmlContent)

[<AutoOpen>]
module ServiceCollectionExtensions =
    open Microsoft.Extensions.DependencyInjection
    open Microsoft.AspNetCore.Hosting

    type IServiceCollection with

        /// Adds a lightweight HTML template renderer.
        member this.AddHtmlTemplate() =
            this.AddTransient<HtmlTemplateLoader>(fun services ->
                HtmlTemplateLoader(services.GetRequiredService<IWebHostEnvironment>().WebRootPath))
