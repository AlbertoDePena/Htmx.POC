namespace WebApplication.Domain.User

open WebApplication.Domain.Invariants

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

    let tryCreate (value: string) =
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

    let tryCreate (value: string) =
        match value with
        | "Customer" -> Some UserType.Customer
        | "Employee" -> Some UserType.Employee
        | _ -> None

[<RequireQualifiedAccess>]
type UserPermission =
    | ViewAirShipments
    | ViewGroundShipments
    | ViewOceanShipments
    | ViewFinancials
    | ViewBookings
    | ExportSearchResults
    | ViewInventory
    | ViewAnalytics

[<RequireQualifiedAccess>]
module UserPermission =

    let value this =
        match this with
        | UserPermission.ViewAirShipments -> "View Air Shipments"
        | UserPermission.ViewGroundShipments -> "View Ground Shipments"
        | UserPermission.ViewOceanShipments -> "View Ocean Shipments"
        | UserPermission.ViewFinancials -> "View Financials"
        | UserPermission.ViewBookings -> "View Bookings"
        | UserPermission.ExportSearchResults -> "Export Search Results"
        | UserPermission.ViewInventory -> "View Inventory"
        | UserPermission.ViewAnalytics -> "View Analytics"

    let tryCreate (value: string) =
        None

type User =
    { Id: UniqueId
      EmailAddress: EmailAddress
      DisplayName: Text
      Type: UserType }

type UserDetails =
    { User: User
      Permissions: UserPermission list
      Groups: UserGroup list }
