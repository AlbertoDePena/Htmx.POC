namespace WebApplication.Domain.Invariants

open System

type EmailAddress = private EmailAddress of string

[<RequireQualifiedAccess>]
module EmailAddress =

    let value (EmailAddress x) = x

    let tryCreate (value: string) =
        if isNull value then
            None
        elif
            System.Text.RegularExpressions.Regex.IsMatch(value, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$")
            |> not
        then
            None
        else
            let emailAddress = value.ToLower()
            Some(EmailAddress emailAddress)

type PositiveNumber = private PositiveNumber of int

[<RequireQualifiedAccess>]
module PositiveNumber =

    let defaultValue = PositiveNumber 1

    let value (PositiveNumber x) = x

    let tryCreate value =
        if value < 1 then None else Some(PositiveNumber value)

type Text = private Text of string

[<RequireQualifiedAccess>]
module Text =
   
    /// <summary>Text's default value is the empty string</summary>
    let defaultValue = Text String.Empty

    /// <summary>Unwrap the primitive string from Text</summary>
    let value (Text x) = x

    /// <summary>Return the primitive string when Some string otherwise return null when None</summary>
    let valueOrNull (text: Text option) =
        text |> Option.map value |> Option.defaultValue null

    /// <summary>Try to convert a potentially null string to Text</summary>
    let tryCreate (value: string) =
        if isNull value then None else Some(Text value)

    /// <summary>Convert a potentially null string to Text</summary>
    /// <exception cref="System.Exception">Throw exception when the string is null</exception>
    let tryCreateOrThrow exceptionMessage value =
        value |> tryCreate |> Option.defaultWith (fun () -> failwith exceptionMessage)

    /// <summary>Apply the function to the Text</summary>
    /// <exception cref="System.Exception">Throw exception when the transformed string is null</exception>
    let transform (transformer: string -> string) (Text value) =
        match transformer value with
        | null -> failwith "The transformed string cannot be null"
        | transformed -> Text transformed

type UniqueId = private UniqueId of Guid

[<RequireQualifiedAccess>]
module UniqueId =

    let value (UniqueId x) = x

    let tryCreate (value: Guid) =
        if value = Guid.Empty then None else Some(UniqueId value)

    let create () =
        RT.Comb.Provider.Sql.Create() |> UniqueId

type WholeNumber = private WholeNumber of int

[<RequireQualifiedAccess>]
module WholeNumber =

    let defaultValue = WholeNumber 0

    let value (WholeNumber x) = x

    let tryCreate value =
        if value < 0 then None else Some(WholeNumber value)