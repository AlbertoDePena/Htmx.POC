namespace WebApp.Domain.User

open WebApp.Domain.Shared

[<RequireQualifiedAccess>]
type UserGroup =
    | Viewer
    | Editor
    | Administrator

    member this.Value =
        match this with
        | UserGroup.Viewer -> "Viewer"
        | UserGroup.Editor -> "Editor"
        | UserGroup.Administrator -> "Administrator"

    override this.ToString() = this.Value

    static member OfString (value: string) =
        match value with
        | "Viewer" -> Some UserGroup.Viewer
        | "Editor" -> Some UserGroup.Editor
        | "Administrator" -> Some UserGroup.Administrator
        | _ -> None

[<RequireQualifiedAccess>]
type UserType =
    | Customer
    | Employee

    member this.Value =
        match this with
        | UserType.Customer -> "Customer"
        | UserType.Employee -> "Employee"

    override this.ToString() = this.Value

    static member OfString (value: string) =
        match value with
        | "Customer" -> Some UserType.Customer
        | "Employee" -> Some UserType.Employee
        | _ -> None

[<RequireQualifiedAccess>]
type UserPermission =
    | ViewTransportationData
    | ViewFinancials
    | ExportSearchResults

    member this.Value =
        match this with
        | UserPermission.ViewTransportationData -> "View Transportation Data"
        | UserPermission.ViewFinancials -> "View Financials"
        | UserPermission.ExportSearchResults -> "Export Search Results"

    override this.ToString() = this.Value

    static member OfString (value: string) =
        match value with
        | "View Transportation Data" -> Some UserPermission.ViewTransportationData
        | "View Financials" -> Some UserPermission.ViewFinancials
        | "Export Search Results" -> Some UserPermission.ExportSearchResults
        | _ -> None

type User =
    { UserId: UniqueId
      EmailAddress: EmailAddress
      DisplayName: Text
      UserTypeId: UniqueId
      UserTypeName: UserType
      IsActive: bool }

type UserDetails =
    { User: User
      Permissions: UserPermission list
      Groups: UserGroup list }
