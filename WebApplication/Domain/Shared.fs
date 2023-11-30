namespace WebApplication.Domain.Shared

[<AutoOpen>]
module Alias =
    open System

    type BigInteger = Int64
    type EmailAddress = String    
    type Integer = Int32
    type Text = String
    type UniqueId = Guid

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

    let ofString (value: string) =
        match value with
        | "Ascending" -> Some SortDirection.Ascending
        | "Descending" -> Some SortDirection.Descending
        | _ -> None

type Query =
    { SearchCriteria: Text option
      ActiveOnly: bool
      Page: Integer
      PageSize: Integer
      SortBy: Text option
      SortDirection: SortDirection option }

type PagedData<'T> =
    { Page: Integer
      PageSize: Integer
      TotalCount: Integer
      SortBy: Text option
      SortDirection: SortDirection option
      Data: 'T list }

    member this.TotalPages =
        let pageCount = this.TotalCount / this.PageSize

        let integer =
            if (this.TotalCount % this.PageSize) = 0 then
                pageCount
            else
                pageCount + 1

        integer

