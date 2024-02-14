namespace WebApp.Infrastructure.HtmlMarkup

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
type Variable = string

/// The content to fill the variable with. It can be either a primitive type or a HTML fragment.
type Value = obj

/// The key/value pairs to fill the template variables with.
type Variables = Map<Variable, Value>

/// The compiled HTML as a string.
type CompiledHtml = string

type HtmlMarkupException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = HtmlMarkupException(Exception message)

type IHtmlMarkup =
    /// <exception cref="HtmlMarkupException">The variable name or value is null/empty</exception>
    abstract Bind: Variable * Value -> IHtmlMarkup
    /// <exception cref="HtmlMarkupException">The variable name or value is null/empty</exception>
    abstract BindRaw: Variable * Value -> IHtmlMarkup
    /// <exception cref="HtmlMarkupException">The file or content is null/empty</exception>
    abstract Load: FileOrContent -> IHtmlMarkup
    /// <exception cref="HtmlMarkupException">HTML markup compilation error</exception>
    abstract UseAntiforgery: Variable * HttpContext -> IHtmlMarkup
    /// <exception cref="HtmlMarkupException">HTML markup compilation error</exception>
    abstract Join: CompiledHtml list -> CompiledHtml
    /// <exception cref="HtmlMarkupException">HTML markup compilation error</exception>
    abstract Render: unit -> CompiledHtml

type HtmlMarkup(environment: IWebHostEnvironment, cache: IMemoryCache, antiforgery: IAntiforgery) =

    let mutable variables: Variables = Map.empty

    let compiledHtmlBuilder: StringBuilder = StringBuilder()

    let isFile (fileOrContent: FileOrContent) = fileOrContent.EndsWith(".html")

    let isNonHtml (value: string) = value.StartsWith("<") |> not

    let (|EncodedValue|_|) (value: obj) =
        let isString = value.GetType() = typeof<String>

        let valueToString = value.ToString()

        if isString && isNonHtml valueToString then
            valueToString |> WebUtility.HtmlEncode :> obj |> Some
        else
            None

    let loadFileOrContent (fileOrContent: FileOrContent) =
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

    let bindVariables () =
        variables
        |> Map.iter (fun name value ->
            let pattern = sprintf "${%s}" name
            let valueToString = value.ToString() |> loadFileOrContent
            compiledHtmlBuilder.Replace(pattern, valueToString) |> ignore)

    let failOnUnboundedVariables () =
        if environment.IsDevelopment() then
            let compiledHtml = compiledHtmlBuilder.ToString()

            let unboundedVariables =
                Regex.Matches(compiledHtml, @"\${\b\w+\b}")
                |> Seq.collect (fun match' ->
                    match'.Groups
                    |> Seq.map (fun group -> group.Value))

            let unbounded = String.Join(", ", unboundedVariables)

            if String.IsNullOrWhiteSpace unbounded |> not then
                sprintf "Found unbounded variables in the HTML content: %s" unbounded
                |> failwith

    let buildVariables (name: Variable) (value: Value) (encode: bool) =
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

        variables <- variables |> Map.add name sanitizedValue

    interface IHtmlMarkup with

        member this.Bind(name: Variable, value: Value) : IHtmlMarkup =
            try
                buildVariables name value true
                this
            with ex ->
                HtmlMarkupException ex |> raise

        member this.BindRaw(name: Variable, value: Value) : IHtmlMarkup =
            try
                buildVariables name value false
                this
            with ex ->
                HtmlMarkupException ex |> raise

        member this.Load(fileOrContent: FileOrContent) : IHtmlMarkup =
            try
                loadFileOrContent fileOrContent |> compiledHtmlBuilder.Append |> ignore
                this
            with ex ->
                HtmlMarkupException ex |> raise

        member this.UseAntiforgery(name: Variable, httpContext: HttpContext) : IHtmlMarkup =
            try
                let token = antiforgery.GetAndStoreTokens(httpContext)

                let fragment =
                    $"""<input name="{token.FormFieldName}" type="hidden" value="{token.RequestToken}">"""

                buildVariables name fragment false
                this
            with ex ->
                HtmlMarkupException ex |> raise

        member this.Join(items: CompiledHtml list) =
            try
                let htmlContentBuilder = StringBuilder()

                for item in items do
                    htmlContentBuilder.AppendLine(item) |> ignore

                htmlContentBuilder.ToString()
            with ex ->
                HtmlMarkupException ex |> raise

        member this.Render() : CompiledHtml =
            try
                bindVariables ()
                failOnUnboundedVariables ()

                let compiledHtml = compiledHtmlBuilder.ToString()

                variables <- Map.empty
                compiledHtmlBuilder.Clear() |> ignore

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
            this.AddTransient<IHtmlMarkup, HtmlMarkup>()