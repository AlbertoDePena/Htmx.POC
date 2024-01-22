namespace WebApplication.Domain.Shared

[<AutoOpen>]
module Alias =
    open System

    type BigNumber = Int64
    type Money = Decimal
    type Number = Int32
    type UniqueId = Guid

/// Represents a non null/empty email address
type EmailAddress =
    private
    | EmailAddress of string

    member this.Value =
        let (EmailAddress value) = this
        value

    override this.ToString() = this.Value

    static member OfString(value: string) =
        if System.String.IsNullOrEmpty value then
            None
        elif System.Text.RegularExpressions.Regex.IsMatch(value, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$") then
            Some(EmailAddress(value.ToLower()))
        else
            None

/// Represents a non null/empty string
type Text =
    private
    | Text of string

    member this.Value =
        let (Text value) = this
        value

    override this.ToString() = this.Value

    static member OfString(value: string) =
        if System.String.IsNullOrEmpty value then
            None
        else
            Some(Text value)

[<RequireQualifiedAccess>]
type SortDirection =
    | Ascending
    | Descending

    override this.ToString() =
        match this with
        | SortDirection.Ascending -> "Ascending"
        | SortDirection.Descending -> "Descending"

    static member OfString(value: string) =
        match value with
        | "Ascending" -> Some SortDirection.Ascending
        | "Descending" -> Some SortDirection.Descending
        | _ -> None

type Query =
    { SearchCriteria: Text option
      ActiveOnly: bool
      Page: Number
      PageSize: Number
      SortBy: Text option
      SortDirection: SortDirection option }

type PagedData<'T> =
    { Page: Number
      PageSize: Number
      TotalCount: Number
      SortBy: Text option
      SortDirection: SortDirection option
      Data: 'T list }

    member this.TotalPages =
        let pageCount = this.TotalCount / this.PageSize

        let totalPages =
            if (this.TotalCount % this.PageSize) = 0 then
                pageCount
            else
                pageCount + 1

        totalPages
