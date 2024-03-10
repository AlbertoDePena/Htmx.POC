namespace WebApp.Infrastructure.HtmlTemplate

open System
open System.IO
open System.Net
open System.Text
open System.Text.RegularExpressions

open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Caching.Memory

/// Can be either a plain HTML string or a path to an HTML file.
type FileOrContent = string

/// The HTML template defines variables with the following syntax: ${VariableName}.
type Variable = string

/// The content to fill the variable with. It can be either a primitive type or a HTML fragment.
type Value = obj

/// The key/value pairs to fill the template variables with.
type BindingPairs = Map<Variable, Value>

/// The HTML content as a string.
type HtmlContent = string

type HtmlTemplateException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = HtmlTemplateException(Exception message)

[<RequireQualifiedAccess>]
module private HtmlContentLoader =

    let loadFileOrContent (environment: IWebHostEnvironment) (cache: IMemoryCache) (fileOrContent: FileOrContent) =
        if isNull fileOrContent then
            nameof fileOrContent |> sprintf "%s cannot be null" |> failwith

        if fileOrContent.EndsWith(".html") then
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

type BindingCollection(antiforgery: IAntiforgery) =

    let mutable bindings: BindingPairs = Map.empty

    let (|EncodedValue|_|) (value: Value) : Value option =
        let isString = value.GetType() = typeof<String>

        let valueToString = value.ToString()

        let isNonHtml = valueToString.StartsWith("<") |> not

        if isString && isNonHtml then
            valueToString |> WebUtility.HtmlEncode :> obj |> Some
        else
            None

    let bindVariables (name: Variable) (value: Value) (encode: bool) : unit =
        if String.IsNullOrWhiteSpace name then
            failwith "The variable cannot be null/empty"

        if isNull value then
            failwith "The value cannot be null"

        let sanitizedValue =
            if encode then
                match value with
                | EncodedValue encodedValue -> encodedValue
                | _ -> value
            else
                value

        bindings <- bindings |> Map.add name sanitizedValue

    /// <exception cref="HtmlTemplateException"></exception>
    member this.Bind(name: Variable, value: Value) : BindingCollection =
        try
            bindVariables name value true
            this
        with ex ->
            HtmlTemplateException ex |> raise

    /// <exception cref="HtmlTemplateException"></exception>
    member this.BindAntiforgery(name: Variable, httpContext: HttpContext) : BindingCollection =
        try
            let token = antiforgery.GetAndStoreTokens(httpContext)

            let fragment =
                $"""<input name="{token.FormFieldName}" type="hidden" value="{token.RequestToken}">"""

            bindVariables name fragment false
            this
        with ex ->
            HtmlTemplateException ex |> raise

    /// <exception cref="HtmlTemplateException"></exception>
    member this.BindRaw(name: Variable, value: Value) : BindingCollection =
        try
            bindVariables name value false
            this
        with ex ->
            HtmlTemplateException ex |> raise

    member this.GetBindings() : BindingPairs = bindings

    member this.Clear() : unit = bindings <- Map.empty

type HtmlBuilder(environment: IWebHostEnvironment, cache: IMemoryCache) =

    let stringBuilder = StringBuilder()

    /// <exception cref="HtmlTemplateException"></exception>
    member this.LoadContent(fileOrContent: FileOrContent) : unit =
        try
            HtmlContentLoader.loadFileOrContent environment cache fileOrContent
            |> stringBuilder.Append
            |> ignore
        with ex ->
            HtmlTemplateException ex |> raise

    member this.GetBuilder() : StringBuilder = stringBuilder

    member this.Clear() : unit = stringBuilder.Clear() |> ignore

type HtmlTemplate(environment: IWebHostEnvironment, cache: IMemoryCache, antiforgery: IAntiforgery) =

    let bindVariables (stringBuilder: StringBuilder) (bindings: BindingPairs) : unit =
        bindings
        |> Map.iter (fun name value ->
            let pattern = sprintf "${%s}" name
            let valueToString = value.ToString()
            let content = HtmlContentLoader.loadFileOrContent environment cache valueToString
            stringBuilder.Replace(pattern, content) |> ignore)

    let failOnUnboundedVariables (htmlContent: string) : unit =
        if environment.IsDevelopment() then
            let unboundedVariables =
                Regex.Matches(htmlContent, @"\${\b\w+\b}")
                |> Seq.collect (fun match' -> match'.Groups |> Seq.map (fun group -> group.Value))

            let unbounded = String.Join(", ", unboundedVariables)

            if String.IsNullOrWhiteSpace unbounded |> not then
                sprintf "The HTML content has unbounded variables: %s" unbounded
                |> failwith

    let render (htmlBuilder: HtmlBuilder) (bindingCollection: BindingCollection) : HtmlContent =
        let builder = htmlBuilder.GetBuilder()
        let bindings = bindingCollection.GetBindings()
        
        bindVariables builder bindings

        let htmlContent = builder.ToString()

        failOnUnboundedVariables htmlContent

        bindingCollection.Clear()
        htmlBuilder.Clear()

        htmlContent

    /// <exception cref="HtmlTemplateException"></exception>
    member this.Render
        (
            fileOrContent: FileOrContent
        ) : HtmlContent =
        try
            let htmlContent =
                let htmlBuilder = HtmlBuilder(environment, cache)
                let bindingCollection = BindingCollection(antiforgery)

                htmlBuilder.LoadContent fileOrContent

                render htmlBuilder bindingCollection

            htmlContent
        with ex ->
            HtmlTemplateException ex |> raise

    /// <exception cref="HtmlTemplateException"></exception>
    member this.Render
        (
            fileOrContent: FileOrContent,
            mapper: BindingCollection -> BindingCollection
        ) : HtmlContent =
        try
            let htmlContent =
                let htmlBuilder = HtmlBuilder(environment, cache)
                let bindingCollection = BindingCollection(antiforgery)

                htmlBuilder.LoadContent fileOrContent

                let bindingCollection = mapper bindingCollection

                render htmlBuilder bindingCollection

            htmlContent
        with ex ->
            HtmlTemplateException ex |> raise

    /// <exception cref="HtmlTemplateException"></exception>
    member this.Render
        (
            fileOrContent: FileOrContent,
            items: 'T list,
            mapper: BindingCollection * 'T -> BindingCollection
        ) : HtmlContent =
        try
            let htmlContents =
                items
                |> List.map (fun item ->
                    let htmlBuilder = HtmlBuilder(environment, cache)
                    let bindingCollection = BindingCollection(antiforgery)

                    htmlBuilder.LoadContent fileOrContent

                    let bindingCollection = mapper (bindingCollection, item)

                    render htmlBuilder bindingCollection)
                |> List.toArray

            String.Join("\n", htmlContents)
        with ex ->
            HtmlTemplateException ex |> raise

[<AutoOpen>]
module ServiceCollectionExtensions =
    open Microsoft.Extensions.DependencyInjection

    type IServiceCollection with

        /// Adds a lightweight HTML template renderer.
        member this.AddHtmlTemplate() =
            this.AddMemoryCache() |> ignore
            this.AddTransient<HtmlTemplate>()
