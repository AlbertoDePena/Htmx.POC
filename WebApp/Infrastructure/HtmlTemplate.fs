namespace WebApp.Infrastructure.HtmlTemplate

open System
open System.IO
open System.Net
open System.Text
open System.Text.RegularExpressions

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting

/// Can be either a plain HTML string or a path to a HTML file.
type FileOrContent = string

/// The HTML template defines binding names with the following syntax: ${BindingName}.
type BindingName = string

/// The content to replace the binding name with. It can be either a primitive type or a HTML fragment.
type BindingValue = obj

/// An alias for the binding name/binding value map.
type BindingPairs = Map<BindingName, BindingValue>

/// The HTML content as a string.
type HtmlContent = string

type AntiforgeryToken = { FormFieldName: string; RequestToken: string }

type HtmlTemplateException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = HtmlTemplateException(Exception message)

[<RequireQualifiedAccess>]
module private HtmlContentLoader =

    let loadFileOrContent (environment: IWebHostEnvironment) (fileOrContent: FileOrContent) =
        if isNull fileOrContent then
            nameof fileOrContent |> sprintf "%s cannot be null" |> failwith

        if fileOrContent.EndsWith(".html") then
            Path.Combine(environment.WebRootPath, fileOrContent) |> File.ReadAllText
        else
            fileOrContent

type BindingCollection() =

    let mutable bindings: BindingPairs = Map.empty

    let buildBindings (name: BindingName) (value: BindingValue) (encode: bool) : unit =
        if String.IsNullOrWhiteSpace name then
            failwith "The binding name cannot be null/empty/white-space"

        if isNull value then
            failwith "The binding value cannot be null"

        let isString = value.GetType() = typeof<String>

        let valueToString = value.ToString()

        let isHtml = valueToString.StartsWith("<")

        let sanitizedValue =
            if encode && isString && (not isHtml) then
                valueToString |> WebUtility.HtmlEncode :> obj
            else
                value

        bindings <- bindings |> Map.add name sanitizedValue

    /// <exception cref="HtmlTemplateException"></exception>
    member this.Bind(name: BindingName, value: BindingValue) : BindingCollection =
        try
            buildBindings name value true
            this
        with ex ->
            HtmlTemplateException ex |> raise

    /// <exception cref="HtmlTemplateException"></exception>
    member this.BindAntiforgery(generateAntiforgeyToken: unit -> AntiforgeryToken) : BindingCollection =
        try
            let token = generateAntiforgeyToken ()

            let fragment =
                $"""<input name="{token.FormFieldName}" type="hidden" value="{token.RequestToken}">"""

            buildBindings "Antiforgery" fragment false
            this
        with ex ->
            HtmlTemplateException ex |> raise

    /// <exception cref="HtmlTemplateException"></exception>
    member this.BindRaw(name: BindingName, value: BindingValue) : BindingCollection =
        try
            buildBindings name value false
            this
        with ex ->
            HtmlTemplateException ex |> raise

    member this.GetBindings() : BindingPairs = bindings

    member this.Clear() : unit = bindings <- Map.empty

type HtmlBuilder(environment: IWebHostEnvironment) =

    let stringBuilder = StringBuilder()

    /// <exception cref="HtmlTemplateException"></exception>
    member this.LoadContent(fileOrContent: FileOrContent) : unit =
        try
            HtmlContentLoader.loadFileOrContent environment fileOrContent
            |> stringBuilder.Append
            |> ignore
        with ex ->
            HtmlTemplateException ex |> raise

    member this.GetBuilder() : StringBuilder = stringBuilder

    member this.Clear() : unit = stringBuilder.Clear() |> ignore

type HtmlTemplate(environment: IWebHostEnvironment) =

    let bindVariables (stringBuilder: StringBuilder) (bindings: BindingPairs) : unit =
        bindings
        |> Map.iter (fun name value ->
            let pattern = sprintf "${%s}" name
            stringBuilder.Replace(pattern, value.ToString()) |> ignore)

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
                let htmlBuilder = HtmlBuilder(environment)
                let bindingCollection = BindingCollection()

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
                let htmlBuilder = HtmlBuilder(environment)
                let bindingCollection = BindingCollection()

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
                    let htmlBuilder = HtmlBuilder(environment)
                    let bindingCollection = BindingCollection()

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
            this.AddTransient<HtmlTemplate>()
