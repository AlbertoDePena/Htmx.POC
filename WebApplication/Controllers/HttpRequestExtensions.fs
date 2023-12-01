namespace WebApplication.Controllers

open System
open Microsoft.AspNetCore.Http
open FsToolkit.ErrorHandling

[<AutoOpen>]
module HttpRequestExtensions =

    type HttpRequest with

        member this.GetBearerToken() =
            this.Headers
            |> Seq.tryFind (fun q -> q.Key = "Authorization")
            |> Option.bind (fun q -> if Seq.isEmpty q.Value then None else q.Value |> Seq.tryHead)
            |> Option.filter (fun h -> h.Contains("Bearer "))
            |> Option.map (fun h -> h.Substring("Bearer ".Length).Trim())

        member this.GetHeaderValue(key: string) =
            this.Headers.TryGetValue key |> Option.ofPair |> Option.map string

        member this.GetFormValue(key: string) =
            match this.HasFormContentType with
            | false -> None
            | true -> this.Form.TryGetValue key |> Option.ofPair |> Option.map string

        member this.GetQueryStringValue(key: string) =
            this.Query.TryGetValue key |> Option.ofPair |> Option.map string

        /// Determines if the current HTTP Request was invoked by Htmx on the client.
        member this.IsHtmx() =
            this.GetHeaderValue "HX-Request"
            |> Option.exists (String.IsNullOrWhiteSpace >> not)