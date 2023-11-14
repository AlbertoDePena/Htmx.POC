namespace WebApplication.Controllers

open System
open Microsoft.AspNetCore.Http
open FsToolkit.ErrorHandling

[<AutoOpen>]
module HttpRequestExtensions =

    type HttpRequest with

        member this.TryGetBearerToken() =
            this.Headers
            |> Seq.tryFind (fun q -> q.Key = "Authorization")
            |> Option.bind (fun q -> if Seq.isEmpty q.Value then None else q.Value |> Seq.tryHead)
            |> Option.filter (fun h -> h.Contains("Bearer "))
            |> Option.map (fun h -> h.Substring("Bearer ".Length).Trim())

        member this.TryGetHeaderValue(key: string) =
            this.Headers.TryGetValue key |> Option.ofPair |> Option.map string

        member this.TryGetFormValue(key: string) =
            match this.HasFormContentType with
            | false -> None
            | true -> this.Form.TryGetValue key |> Option.ofPair |> Option.map string

        member this.TryGetQueryStringValue(key: string) =
            this.Query.TryGetValue key |> Option.ofPair |> Option.map string

        member this.IsHtmx() =
            this.TryGetHeaderValue "HX-Request"
            |> Option.exists (String.IsNullOrWhiteSpace >> not)