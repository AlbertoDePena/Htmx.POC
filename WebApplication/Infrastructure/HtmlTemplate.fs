namespace WebApplication.Infrastructure

open System
open System.Text
open System.IO
open Microsoft.AspNetCore.Hosting
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

type IHtmlTemplate =
    abstract Bind: name: VariableName * value: VariableValue -> IHtmlTemplate
    abstract Compile: fileOrContent: FileOrContent -> HtmlContent

type HtmlTemplate(environment: IWebHostEnvironment, cache: IMemoryCache) =

    let mutable _variables: Variables = Map.empty

    let getFileContent (fileOrContent: FileOrContent) =
        if String.IsNullOrWhiteSpace fileOrContent then
            invalidArg (nameof fileOrContent) "cannot be null nor empty"

        if fileOrContent.Contains("<") then
            fileOrContent
        else
            let filePath = Path.Combine(environment.WebRootPath, fileOrContent)

            match cache.TryGetValue<string>(filePath) with
            | true, fileContent -> fileContent
            | _ ->
                let fileContent = File.ReadAllText filePath
                cache.Set(filePath, fileContent) |> ignore
                fileContent

    interface IHtmlTemplate with

        member this.Bind(name: VariableName, value: VariableValue) =
            _variables <- _variables |> Map.add name value
            this

        member this.Compile(fileOrContent: FileOrContent) : HtmlContent =
            let stringBuilder = StringBuilder(getFileContent fileOrContent)

            _variables
            |> Map.iter (fun name value ->
                let pattern = sprintf "${%s}" name
                let valueToString = value.ToString()
                stringBuilder.Replace(pattern, valueToString) |> ignore)

            stringBuilder.ToString()
