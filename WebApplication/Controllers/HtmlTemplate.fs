namespace WebApplication.Controllers

open System
open System.Text
open System.IO
open System.Reflection

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

/// Initialize an HTML template. The fileOrContent can be with either a plain HTML string or a path to an HTML file.
type HtmlTemplate(fileOrContent: FileOrContent) =

    let mutable variables: Variables = Map.empty

    let hasVariables () =
        variables.IsEmpty |> not

    let isHtmlString (fileOrContent: string) =
        fileOrContent.Contains("<")

    let getFileContent (fileOrContent: string) =
        if String.IsNullOrWhiteSpace fileOrContent then
            invalidArg (nameof fileOrContent) "cannot be null nor empty"

        if isHtmlString fileOrContent then
            fileOrContent
        else
            let filePath = fileOrContent.Replace("/", ".")
            let assembly = Assembly.GetExecutingAssembly()
            let resourceName = sprintf "WebApplication.%s" filePath
            use stream = assembly.GetManifestResourceStream(resourceName)
            use reader = new StreamReader(stream)
            reader.ReadToEnd()

    member this.Bind(name: VariableName, value: VariableValue) =
        variables <- variables |> Map.add name value
        this

    member this.Compile() : HtmlContent =
        let stringBuilder = StringBuilder(getFileContent fileOrContent)

        if hasVariables () then            
            variables
            |> Map.iter (fun name value ->
                let pattern = sprintf "${%s}" name
                let valueToString = value.ToString()
                stringBuilder.Replace(pattern, valueToString) |> ignore)

        stringBuilder.ToString()
