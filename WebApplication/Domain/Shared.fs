namespace WebApplication.Domain.Shared

[<RequireQualifiedAccess>]
type SortDirection =
    | Ascending
    | Descending

[<RequireQualifiedAccess>]
module SortDirection =

    let value this =
        match this with
        | SortDirection.Ascending -> "Ascending"
        | SortDirection.Descending -> "Descending"

    let fromString (value: string) =
        match value with
        | "Ascending" -> Some SortDirection.Ascending
        | "Descending" -> Some SortDirection.Descending
        | _ -> None

type Query =
    { SearchCriteria: string option
      ActiveOnly: bool
      Page: int32
      PageSize: int32
      SortBy: string option
      SortDirection: SortDirection option }

type PagedData<'T> =
    { Page: int32
      PageSize: int32
      TotalCount: int32
      SortBy: string option
      SortDirection: SortDirection option
      Data: 'T list }

    member this.NumberOfPage =
        let pageCount = this.TotalCount / this.PageSize

        let integer =
            if (this.TotalCount % this.PageSize) = 0 then
                pageCount
            else
                pageCount + 1

        integer

[<AutoOpen>]
module Alias =
    open System

    type DbConnectionString = string

    type EmailAddress = string

    type UniqueId = Guid