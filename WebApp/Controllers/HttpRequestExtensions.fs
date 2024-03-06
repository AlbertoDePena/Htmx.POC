namespace WebApp.Controllers

open System
open Microsoft.AspNetCore.Http
open FsToolkit.ErrorHandling

[<AutoOpen>]
module HttpRequestExtensions =

    type HttpRequest with

        member this.TryGetHeaderValue(key: string) =
            this.Headers.TryGetValue key |> Option.ofPair |> Option.map string

        member this.TryGetFormValue(key: string) =
            match this.HasFormContentType with
            | false -> None
            | true -> this.Form.TryGetValue key |> Option.ofPair |> Option.map string

        member this.TryGetQueryStringValue(key: string) =
            this.Query.TryGetValue key |> Option.ofPair |> Option.map string

        member this.TryGetBearerToken() =            
            this.TryGetHeaderValue "Authorization"
            |> Option.filter (fun value -> value.Contains("Bearer "))
            |> Option.map (fun value -> value.Substring("Bearer ".Length).Trim())

        /// Determines if the current HTTP Request was invoked by HTMX on the client.
        member this.IsHtmx() =
            this.TryGetHeaderValue "HX-Request"
            |> Option.exists (String.IsNullOrWhiteSpace >> not)

        /// Determines if the current HTTP Request was invoked by HTMX on the client with the "boosted" attribute.
        member this.IsHtmxBoosted() =
            this.TryGetHeaderValue "HX-Boosted"
            |> Option.exists (String.IsNullOrWhiteSpace >> not)