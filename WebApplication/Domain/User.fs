namespace WebApplication.Domain.User

open System

[<RequireQualifiedAccess>]
type UserGroup =
    | Viewer
    | Editor
    | Administrator

[<RequireQualifiedAccess>]
module UserGroup =

    let value this =
        match this with
        | UserGroup.Viewer -> "Viewer"
        | UserGroup.Editor -> "Editor"
        | UserGroup.Administrator -> "Administrator"

    let optional (value: string) =
        match value with
        | "Viewer" -> Some UserGroup.Viewer
        | "Editor" -> Some UserGroup.Editor
        | "Administrator" -> Some UserGroup.Administrator
        | _ -> None

[<RequireQualifiedAccess>]
type UserType =
    | Customer
    | Employee

[<RequireQualifiedAccess>]
module UserType =

    let value this =
        match this with
        | UserType.Customer -> "Customer"
        | UserType.Employee -> "Employee"

    let optional (value: string) =
        match value with
        | "Customer" -> Some UserType.Customer
        | "Employee" -> Some UserType.Employee
        | _ -> None

[<RequireQualifiedAccess>]
type UserPermission =
    | ViewTransportationData
    | ViewFinancials
    | ExportSearchResults

[<RequireQualifiedAccess>]
module UserPermission =

    let value this =
        match this with
        | UserPermission.ViewTransportationData -> "View Tranportation Data"
        | UserPermission.ViewFinancials -> "View Financials"
        | UserPermission.ExportSearchResults -> "Export Search Results"

    let optional (value: string) =
        match value with
        | "View Tranportation Data" -> Some UserPermission.ViewTransportationData
        | "View Financials" -> Some UserPermission.ViewFinancials
        | "Export Search Results" -> Some UserPermission.ExportSearchResults
        | _ -> None

type User =
    { Id: Guid
      EmailAddress: string
      DisplayName: string
      TypeName: UserType
      IsActive: bool }

type UserDetails =
    { User: User
      Permissions: UserPermission list
      Groups: UserGroup list }
