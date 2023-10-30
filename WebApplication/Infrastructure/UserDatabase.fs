namespace WebApplication.Infrastructure.UserDatabase

open System.Data
open System.Threading.Tasks

open Microsoft.Data.SqlClient
open FsToolkit.ErrorHandling

open WebApplication.Infrastructure.Database
open WebApplication.Infrastructure.Exceptions
open WebApplication.Domain.Extensions
open WebApplication.Domain.Invariants
open WebApplication.Domain.Shared
open WebApplication.Domain.User

[<RequireQualifiedAccess>]
module UserDatabase =

    type DbConnectionString = Text

    let private readUserGroup (reader: SqlDataReader) : UserGroup =
        reader.GetOrdinal("GroupName")
        |> reader.GetString
        |> UserGroup.tryCreate
        |> Option.defaultWith (fun () -> failwith "Missing GroupName column")

    let private readUserPermission (reader: SqlDataReader) : UserPermission =
        reader.GetOrdinal("PermissionName")
        |> reader.GetString
        |> UserPermission.tryCreate
        |> Option.defaultWith (fun () -> failwith "Missing PermissionName column")

    let private readUser (reader: SqlDataReader) : User =
        { Id =
            reader.GetOrdinal("Id")
            |> reader.GetGuid
            |> UniqueId.tryCreate
            |> Option.defaultWith (fun () -> failwith "Missing Id column")
          EmailAddress =
            reader.GetOrdinal("EmailAddress")
            |> reader.GetString
            |> EmailAddress.tryCreate
            |> Option.defaultWith (fun () -> failwith "Missing EmailAddress column")
          DisplayName =
            reader.GetOrdinal("DisplayName")
            |> reader.GetString
            |> Text.tryCreate
            |> Option.defaultWith (fun () -> failwith "Missing DisplayName column")
          Type =
            reader.GetOrdinal("Type")
            |> reader.GetString
            |> UserType.tryCreate
            |> Option.defaultWith (fun () -> failwith "Missing Type column") }

    /// <exception cref="DataStorageException"></exception>
    let getPagedData (dbConnectionString: DbConnectionString) (query: Query) : Task<PagedData<User>> =
        task {
            try
                use connection = new SqlConnection(Text.value dbConnectionString)
                use command = new SqlCommand("dbo.Users_Search", connection)

                command.CommandType <- CommandType.StoredProcedure

                command.Parameters.AddWithValue(
                    "@SearchCriteria",
                    query.SearchCriteria
                    |> Option.map Text.value
                    |> Option.defaultValue String.defaultValue
                )
                |> ignore

                command.Parameters.AddWithValue("@ActiveOnly", query.ActiveOnly) |> ignore

                command.Parameters.AddWithValue("@Page", query.Page |> PositiveNumber.value)
                |> ignore

                command.Parameters.AddWithValue("@PageSize", query.PageSize |> PositiveNumber.value)
                |> ignore

                command.Parameters.AddWithValue(
                    "@SortBy",
                    query.SortBy |> Option.map Text.value |> Option.defaultValue String.defaultValue
                )
                |> ignore

                command.Parameters.AddWithValue(
                    "@SortDirection",
                    query.SortDirection
                    |> Option.map SortDirection.value
                    |> Option.defaultValue String.defaultValue
                )
                |> ignore

                do! connection.OpenAsync()

                use! reader = command.ExecuteReaderAsync()

                let! users = reader.ReadAllAsync readUser

                let! hasNextResult = reader.NextResultAsync()

                let totalCount =
                    if hasNextResult then
                        reader.GetInt32(0)
                        |> WholeNumber.tryCreate
                        |> Option.defaultValue WholeNumber.defaultValue
                    else
                        WholeNumber.defaultValue

                return
                    { Page = query.Page
                      PageSize = query.PageSize
                      TotalCount = totalCount
                      SortBy = query.SortBy
                      SortDirection = query.SortDirection
                      Data = users |> Seq.toList }
            with ex ->
                return (DataStorageException ex |> raise)
        }

    /// <exception cref="DataStorageException"></exception>
    let tryFindByEmailAddress
        (dbConnectionString: DbConnectionString)
        (emailAddress: EmailAddress)
        : Task<UserDetails option> =
        task {
            try
                use connection = new SqlConnection(Text.value dbConnectionString)
                use command = new SqlCommand("dbo.Users_FindByEmailAddress", connection)

                command.CommandType <- CommandType.StoredProcedure

                command.Parameters.AddWithValue("@EmailAddress", EmailAddress.value emailAddress)
                |> ignore

                do! connection.OpenAsync()

                use! reader = command.ExecuteReaderAsync()

                let! users = reader.ReadAllAsync readUser

                let! hasNextResult = reader.NextResultAsync()

                let! userPermissions =
                    if hasNextResult then
                        reader.ReadAllAsync readUserPermission
                    else
                        Task.singleton []

                let! hasNextResult = reader.NextResultAsync()

                let! userGroups =
                    if hasNextResult then
                        reader.ReadAllAsync readUserGroup
                    else
                        Task.singleton []

                let userDetailsOption =
                    users
                    |> Seq.tryHead
                    |> Option.map (fun user ->
                        { User = user
                          Permissions = userPermissions |> Seq.toList
                          Groups = userGroups |> Seq.toList })

                return userDetailsOption
            with ex ->
                return (DataStorageException ex |> raise)
        }