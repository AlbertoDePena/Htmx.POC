namespace WebApplication.Domain.Shared

open FsToolkit.ErrorHandling
open WebApplication.Domain.Invariants

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

    let tryCreate(value: string) =
        match value with
        | "Ascending" -> Some SortDirection.Ascending
        | "Descending" -> Some SortDirection.Descending
        | _ -> None

type Query =
    { SearchCriteria: Text option
      ActiveOnly: bool
      Page: PositiveNumber
      PageSize: PositiveNumber
      SortBy: Text option
      SortDirection: SortDirection option }

type PagedData<'T> =
    { Page: PositiveNumber
      PageSize: PositiveNumber
      TotalCount: WholeNumber
      SortBy: Text option
      SortDirection: SortDirection option
      Data: 'T list }

[<RequireQualifiedAccess>]
module PagedData =

    let calculateNumberOfPages (pagedData: PagedData<'T>) =
        let pageCount =
            WholeNumber.value pagedData.TotalCount / PositiveNumber.value pagedData.PageSize

        let integer =
            if (WholeNumber.value pagedData.TotalCount % PositiveNumber.value pagedData.PageSize) = 0 then
                pageCount
            else
                pageCount + 1

        integer
        |> WholeNumber.tryCreate
        |> Option.defaultValue WholeNumber.defaultValue
