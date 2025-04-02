namespace WebApp.Domain.Shared

[<RequireQualifiedAccess>]
module String =

    /// The default value of a string is null.
    let defaultValue = null

/// <summary>
/// Represents a valid email address
/// </summary>
type EmailAddress =
    private
    | EmailAddress of string

    member this.Value =
        let (EmailAddress value) = this
        value
    
    override this.ToString() = this.Value
    
    static member OfString(value: string) =
        if System.String.IsNullOrWhiteSpace value then
            None
        elif System.Text.RegularExpressions.Regex.IsMatch(value, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$") then
            Some(EmailAddress(value.ToLower()))
        else
            None

/// <summary>
/// Represents a non null/empty/white-space string
/// </summary>
type Text =
    private
    | Text of string

    member this.Value =
        let (Text value) = this
        value
    
    override this.ToString() = this.Value

    static member OfString(value: string) =
        if System.String.IsNullOrWhiteSpace value then
            None
        else
            Some(Text value)

[<RequireQualifiedAccess>]
type SortDirection =
    | Ascending
    | Descending

    member this.Value =
        match this with
        | SortDirection.Ascending -> "Ascending"
        | SortDirection.Descending -> "Descending"

    override this.ToString() = this.Value

    static member OfString(value: string) =
        match value with
        | "Ascending" -> Some SortDirection.Ascending
        | "Descending" -> Some SortDirection.Descending
        | _ -> None

[<AutoOpen>]
module Alias =
    open System

    type BigNumber = Int64    
    type EmailAddress = Text
    type Money = Decimal
    type Number = Int32
    type UniqueId = Guid

type Query =
    { SearchCriteria: Text option
      ActiveOnly: bool
      Page: Number
      PageSize: Number
      SortBy: Text option
      SortDirection: SortDirection option }

type PagedData<'a> =
    { Page: Number
      PageSize: Number
      TotalCount: Number
      SortBy: Text option
      SortDirection: SortDirection option
      Data: 'a list }

    member this.TotalPages =
        let pageCount = this.TotalCount / this.PageSize

        let totalPages =
            if this.TotalCount % this.PageSize = 0 then
                pageCount
            else
                pageCount + 1

        totalPages

    member this.HasPreviousPage =
        this.Page > 1

    member this.HasNextPage =
        this.Page * this.PageSize < this.TotalCount