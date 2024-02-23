namespace WebApp.Domain.Shared

[<AutoOpen>]
module Alias =
    open System

    type BigNumber = Int64
    type Money = Decimal
    type Number = Int32
    type UniqueId = Guid

[<RequireQualifiedAccess>]
module String =

    /// The default value of a string is null.
    let defaultValue = null

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

/// <summary>
/// Represents a non null/empty/white-space string
/// </summary>
type Text =
    private
    | Text of string

    /// <summary>Unwrap the Text to it's primitive value</summary>
    member this.Value =
        let (Text value) = this
        value
    
    /// <summary>Apply a function to  the Text's primitive value</summary>
    member this.Apply (f: string -> 'a) =
        this.Value |> f

    override this.ToString() = this.Value

    /// <summary>The Text's default value is the empty string</summary>
    static member DefaultValue = Text System.String.Empty

    /// <summary>Try to convert a potentially null/empty/white-space string to a Text</summary>
    static member OfString(value: string) =
        if System.String.IsNullOrWhiteSpace value then
            None
        else
            Some(Text value)

    /// <summary>Return the Text's primitive value when Some Text otherwise return null when None</summary>
    static member ValueOrNull(textOption: Text option) =        
        match textOption with
        | None -> String.defaultValue
        | Some text when System.String.IsNullOrWhiteSpace text.Value -> String.defaultValue
        | Some text -> text.Value

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
