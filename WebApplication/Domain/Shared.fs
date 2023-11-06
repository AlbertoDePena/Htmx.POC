namespace WebApplication.Domain.Shared

/// Represents a non-null string
type Text =
    private
    | Text of string

    member this.Value =
        match this with
        | Text value -> value

    override this.ToString() = this.Value

    /// The default value of a Text is the empty string.
    static member DefaultValue = Text System.String.Empty

    static member op_Implicit(value: string) =
        if isNull value then Text.DefaultValue else Text value

    static member TryCreate(value: string) =
        if isNull value then
            Error "Text cannot be null"
        else
            Ok(Text value)

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

    let optional (value: string) =
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
