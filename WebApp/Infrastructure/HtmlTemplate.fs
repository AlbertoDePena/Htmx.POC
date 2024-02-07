﻿namespace WebApp.Infrastructure.HtmlTemplate

open System
open System.IO
open System.Net
open System.Threading.Tasks
open System.Text
open System.Text.RegularExpressions

open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Caching.Memory

/// Can be either a plain HTML string or a path to an HTML file.
type FileOrContent = string

/// Variables are defined in the HTML template with the following syntax: ${VariableName}
type VariableName = string

/// The content to fill the variable with. It can be either a primitive type or a HTML fragment.
type VariableValue = obj

/// The key/value pairs to fill the template variables with.
type Variables = Map<VariableName, VariableValue>

/// The compiled HTML as a string.
type HtmlContent = string

type HtmlTemplateException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = HtmlTemplateException(Exception message)

type IHtmlTemplate =
    /// <exception cref="HtmlTemplateException">The variable name or value is null/empty</exception>
    abstract Bind: VariableName * VariableValue -> IHtmlTemplate
    /// <exception cref="HtmlTemplateException">The variable name or value is null/empty</exception>
    abstract BindRaw: VariableName * VariableValue -> IHtmlTemplate
    /// <exception cref="HtmlTemplateException">HTML template compilation error</exception>
    abstract GenerateAntiforgery: VariableName * HttpContext -> IHtmlTemplate
    /// <exception cref="HtmlTemplateException">HTML template compilation error</exception>
    abstract Join: HtmlContent list -> HtmlContent
    /// <exception cref="HtmlTemplateException">HTML template compilation error</exception>
    abstract Render: FileOrContent -> HtmlContent
    
type HtmlTemplate(environment: IWebHostEnvironment, cache: IMemoryCache, antiforgery: IAntiforgery) =

    let mutable variables: Variables = Map.empty

    let (|EncodedValue|_|) (value: obj) =
        let isString = value.GetType() = typeof<String>

        let valueToString = value.ToString()

        let isNonHtml = valueToString.StartsWith("<") |> not

        if isString && isNonHtml then
            valueToString |> WebUtility.HtmlEncode :> obj |> Some
        else
            None

    let isFile (fileOrContent: FileOrContent) = fileOrContent.EndsWith(".html")

    let getFileOrContent (fileOrContent: FileOrContent) =
        if isNull fileOrContent then
            nameof fileOrContent |> sprintf "%s cannot be null" |> failwith

        if isFile fileOrContent then
            let filePath = Path.Combine(environment.WebRootPath, fileOrContent)

            if environment.IsDevelopment() then
                filePath |> File.ReadAllText
            else
                match cache.TryGetValue<string>(filePath) with
                | true, fileContent -> fileContent
                | _ ->
                    let fileContent = filePath |> File.ReadAllText
                    cache.Set(filePath, fileContent) |> ignore
                    fileContent
        else
            fileOrContent

    let bindVariables (htmlContentBuilder: StringBuilder) =
        variables
        |> Map.iter (fun name value ->
            let pattern = sprintf "${%s}" name
            let valueToString = value.ToString() |> getFileOrContent
            htmlContentBuilder.Replace(pattern, valueToString) |> ignore)

    let failOnUnboundedVariables (htmlContent: string) =
        if environment.IsDevelopment() then
            let stringBuilder = StringBuilder()

            Regex.Matches(htmlContent, @"\${\b\w+\b}")
            |> Seq.iter (fun match' ->
                match'.Groups
                |> Seq.iter (fun group -> stringBuilder.AppendLine(group.Value) |> ignore))

            let unbounded = stringBuilder.ToString()

            if String.IsNullOrWhiteSpace unbounded |> not then
                sprintf "Found unbounded variables in the HTML content: %s" unbounded
                |> failwith

    let buildVariables (name: VariableName) (value: VariableValue) (encode: bool) =
        if String.IsNullOrWhiteSpace name then
            failwith "The variable name cannot be null/empty"

        if isNull value then
            failwith "The variable value cannot be null"

        let sanitizedValue =
            if encode then
                match value with
                | EncodedValue encodedValue -> encodedValue
                | _ -> value
            else
                value

        variables <- variables |> Map.add name sanitizedValue

    interface IHtmlTemplate with

        member this.Bind(name: VariableName, value: VariableValue) =
            try
                buildVariables name value true
                this
            with ex ->
                HtmlTemplateException ex |> raise

        member this.BindRaw(name: VariableName, value: VariableValue) =
            try
                buildVariables name value false
                this
            with ex ->
                HtmlTemplateException ex |> raise

        member this.GenerateAntiforgery(name: VariableName, httpContext: HttpContext) =
            try
                let token = antiforgery.GetAndStoreTokens(httpContext)
                let fragment = $"""<input name="{token.FormFieldName}" type="hidden" value="{token.RequestToken}">"""

                buildVariables name fragment false
                this
            with ex ->
                HtmlTemplateException ex |> raise

        member this.Join(items: HtmlContent list) =
            try
                let htmlContentBuilder = StringBuilder()

                for item in items do
                    htmlContentBuilder.AppendLine(item) |> ignore

                htmlContentBuilder.ToString()
            with ex ->
                HtmlTemplateException ex |> raise

        member this.Render(fileOrContent: FileOrContent) : HtmlContent =
            try
                let htmlContentBuilder = getFileOrContent fileOrContent |> StringBuilder

                bindVariables htmlContentBuilder

                let htmlContent = htmlContentBuilder.ToString()

                failOnUnboundedVariables htmlContent

                variables <- Map.empty

                htmlContent
            with ex ->
                HtmlTemplateException ex |> raise
        
[<AutoOpen>]
module ServiceCollectionExtensions =
    open Microsoft.Extensions.DependencyInjection

    type IServiceCollection with

        /// Adds a lightweight HTML template compiler.
        member this.AddHtmlTemplate() =
            this.AddMemoryCache() |> ignore
            this.AddTransient<IHtmlTemplate, HtmlTemplate>()