namespace WebApp.Infrastructure.HtmlMarkup

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

/// Variables are defined in the HTML template with the following syntax: ${VariableName}
type Variable = string

/// The content to fill the variable with. It can be either a primitive type or a HTML fragment.
type Value = obj

/// The key/value pairs to fill the template variables with.
type Bindings = Map<Variable, Value>

/// The compiled HTML as a string.
type CompiledHtml = string

type HtmlMarkupException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = HtmlMarkupException(Exception message)

[<RequireQualifiedAccess>]
module internal HtmlContentLoader =

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

type HtmlBindingCollection(antiforgery: IAntiforgery) =

    let mutable bindings: Bindings = Map.empty

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

    /// <exception cref="HtmlMarkupException"></exception>
    member this.Bind(name: Variable, value: Value) : HtmlBindingCollection =
        try
            bindVariables name value true
            this
        with ex ->
            HtmlMarkupException ex |> raise

    /// <exception cref="HtmlMarkupException"></exception>
    member this.BindAntiforgery(name: Variable, httpContext: HttpContext) : HtmlBindingCollection =
        try
            let token = antiforgery.GetAndStoreTokens(httpContext)

            let fragment =
                $"""<input name="{token.FormFieldName}" type="hidden" value="{token.RequestToken}">"""

            bindVariables name fragment false
            this
        with ex ->
            HtmlMarkupException ex |> raise

    /// <exception cref="HtmlMarkupException"></exception>
    member this.BindRaw(name: Variable, value: Value) : HtmlBindingCollection =
        try
            bindVariables name value false
            this
        with ex ->
            HtmlMarkupException ex |> raise

    member this.GetBindings() : Bindings = bindings

    member this.Clear() : unit = bindings <- Map.empty

type HtmlBuilder(environment: IWebHostEnvironment, cache: IMemoryCache) =

    let stringBuilder = StringBuilder()

    member this.LoadContent(fileOrContent: FileOrContent) : unit =
        try
            HtmlContentLoader.loadFileOrContent environment cache fileOrContent
            |> stringBuilder.Append
            |> ignore
        with ex ->
            HtmlMarkupException ex |> raise

    member this.GetBuilder() : StringBuilder = stringBuilder

    member this.Clear() : unit = stringBuilder.Clear() |> ignore

type HtmlMarkup(environment: IWebHostEnvironment, cache: IMemoryCache, antiforgery: IAntiforgery) =

    let bindVariables (stringBuilder: StringBuilder) (bindings: Bindings) =
        bindings
        |> Map.iter (fun name value ->
            let pattern = sprintf "${%s}" name
            let valueToString = value.ToString()
            let content = HtmlContentLoader.loadFileOrContent environment cache valueToString
            stringBuilder.Replace(pattern, content) |> ignore)

    let failOnUnboundedVariables (compiledHtml: string) =
        if environment.IsDevelopment() then
            let unboundedVariables =
                Regex.Matches(compiledHtml, @"\${\b\w+\b}")
                |> Seq.collect (fun match' -> match'.Groups |> Seq.map (fun group -> group.Value))

            let unbounded = String.Join(", ", unboundedVariables)

            if String.IsNullOrWhiteSpace unbounded |> not then
                sprintf "Found unbounded variables in the HTML content: %s" unbounded
                |> failwith

    let render (htmlBuilder: HtmlBuilder) (bindingCollection: HtmlBindingCollection) =
        let builder = htmlBuilder.GetBuilder()
        let bindings = bindingCollection.GetBindings()
        
        bindVariables builder bindings

        let compiledHtml = builder.ToString()

        failOnUnboundedVariables compiledHtml

        bindingCollection.Clear()
        htmlBuilder.Clear()

        compiledHtml

    /// <exception cref="HtmlMarkupException">HTML markup compilation error</exception>
    member this.Render
        (
            fileOrContent: FileOrContent
        ) : CompiledHtml =
        try
            let compiledHtml =
                let htmlBuilder = HtmlBuilder(environment, cache)
                let bindingCollection = HtmlBindingCollection(antiforgery)

                htmlBuilder.LoadContent fileOrContent

                render htmlBuilder bindingCollection

            compiledHtml
        with ex ->
            HtmlMarkupException ex |> raise

    /// <exception cref="HtmlMarkupException">HTML markup compilation error</exception>
    member this.Render
        (
            fileOrContent: FileOrContent,
            mapper: HtmlBindingCollection -> HtmlBindingCollection
        ) : CompiledHtml =
        try
            let compiledHtml =
                let htmlBuilder = HtmlBuilder(environment, cache)
                let bindingCollection = HtmlBindingCollection(antiforgery)

                htmlBuilder.LoadContent fileOrContent

                let bindingCollection = mapper bindingCollection

                render htmlBuilder bindingCollection

            compiledHtml
        with ex ->
            HtmlMarkupException ex |> raise

    /// <exception cref="HtmlMarkupException">HTML markup compilation error</exception>
    member this.Render
        (
            fileOrContent: FileOrContent,
            items: 'T list,
            mapper: HtmlBindingCollection * 'T -> HtmlBindingCollection
        ) : CompiledHtml =
        try
            let compiledHtmls =
                items
                |> List.map (fun item ->
                    let htmlBuilder = HtmlBuilder(environment, cache)
                    let bindingCollection = HtmlBindingCollection(antiforgery)

                    htmlBuilder.LoadContent fileOrContent

                    let bindingCollection = mapper (bindingCollection, item)

                    render htmlBuilder bindingCollection)
                |> List.toArray

            String.Join("\n", compiledHtmls)
        with ex ->
            HtmlMarkupException ex |> raise

    /// <exception cref="HtmlMarkupException">HTML markup compilation error</exception>
    member this.Render
        (
            fileOrContent: FileOrContent,
            item: 'T,
            mapper: HtmlBindingCollection * 'T -> HtmlBindingCollection
        ) : CompiledHtml =
        try
            let compiledHtml =
                let htmlBuilder = HtmlBuilder(environment, cache)
                let bindingCollection = HtmlBindingCollection(antiforgery)

                htmlBuilder.LoadContent fileOrContent

                let bindingCollection = mapper (bindingCollection, item)

                render htmlBuilder bindingCollection

            compiledHtml
        with ex ->
            HtmlMarkupException ex |> raise

[<AutoOpen>]
module ServiceCollectionExtensions =
    open Microsoft.Extensions.DependencyInjection

    type IServiceCollection with

        /// Adds a lightweight HTML markup compiler.
        member this.AddHtmlMarkup() =
            this.AddMemoryCache() |> ignore
            this.AddTransient<HtmlMarkup>()
