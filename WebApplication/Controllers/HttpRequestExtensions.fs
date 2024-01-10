namespace WebApplication.Controllers

open System
open Microsoft.AspNetCore.Http
open FsToolkit.ErrorHandling

[<AutoOpen>]
module HttpRequestExtensions =

    type HttpRequest with

        member this.GetHeaderValue(key: string) =
            this.Headers.TryGetValue key |> Option.ofPair |> Option.map string

        member this.GetFormValue(key: string) =
            match this.HasFormContentType with
            | false -> None
            | true -> this.Form.TryGetValue key |> Option.ofPair |> Option.map string

        member this.GetQueryStringValue(key: string) =
            this.Query.TryGetValue key |> Option.ofPair |> Option.map string

        member this.GetBearerToken() =            
            this.GetHeaderValue "Authorization"
            |> Option.filter (fun value -> value.Contains("Bearer "))
            |> Option.map (fun value -> value.Substring("Bearer ".Length).Trim())

        /// Determines if the current HTTP Request was invoked by Htmx on the client.
        member this.IsHtmx() =
            this.GetHeaderValue "HX-Request"
            |> Option.exists (String.IsNullOrWhiteSpace >> not)