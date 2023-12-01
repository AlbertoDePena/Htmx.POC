namespace WebApplication.Infrastructure.UserDatabase

open System.Data
open System.Threading.Tasks

open Microsoft.Data.SqlClient
open FsToolkit.ErrorHandling

open WebApplication.Infrastructure.Database
open WebApplication.Infrastructure.Exceptions
open WebApplication.Domain.Extensions
open WebApplication.Domain.Shared
open WebApplication.Domain.User

type IUserDatabase =
    /// <exception cref="DataStorageException"></exception>
    abstract GetPagedData: Query -> Task<PagedData<User>>
    /// <exception cref="DataStorageException"></exception>
    abstract FindById: UniqueId -> Task<UserDetails option>
    /// <exception cref="DataStorageException"></exception>
    abstract FindByEmailAddress: EmailAddress -> Task<UserDetails option>

type UserDatabase(dbConnectionFactory: IDbConnectionFactory) =

    let readUserGroup (reader: SqlDataReader) : UserGroup =
        reader.GetOrdinal("Name")
        |> reader.GetString
        |> UserGroup.ofString
        |> Option.defaultWith (fun () -> failwith "Missing Name column")

    let readUserPermission (reader: SqlDataReader) : UserPermission =
        reader.GetOrdinal("Name")
        |> reader.GetString
        |> UserPermission.ofString
        |> Option.defaultWith (fun () -> failwith "Missing Name column")

    let readUserType (reader: SqlDataReader) : UserType =
        reader.GetOrdinal("TypeName")
        |> reader.GetString
        |> UserType.ofString
        |> Option.defaultWith (fun () -> failwith "Missing TypeName column")

    let readUser (reader: SqlDataReader) : User =
        { Id = reader.GetOrdinal("Id") |> reader.GetGuid
          EmailAddress = reader.GetOrdinal("EmailAddress") |> reader.GetString
          DisplayName = reader.GetOrdinal("DisplayName") |> reader.GetString
          TypeName = readUserType reader
          IsActive = reader.GetOrdinal("IsActive") |> reader.GetBoolean }

    let getUserDetails (connection: SqlConnection) (command: SqlCommand) : Task<UserDetails Option> =
        task {
            do! connection.OpenAsync()

            use! reader = command.ExecuteReaderAsync()

            let! users = reader.ReadManyAsync readUser

            let! hasNextResult = reader.NextResultAsync()

            let! userPermissions =
                if hasNextResult then
                    reader.ReadManyAsync readUserPermission
                else
                    Task.singleton []

            let! hasNextResult = reader.NextResultAsync()

            let! userGroups =
                if hasNextResult then
                    reader.ReadManyAsync readUserGroup
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
        }

    interface IUserDatabase with

        member this.GetPagedData(query: Query) : Task<PagedData<User>> =
            task {
                try
                    use connection = dbConnectionFactory.CreateSqlConnection()
                    use command = new SqlCommand("dbo.Users_Search", connection)

                    command.CommandType <- CommandType.StoredProcedure

                    command.Parameters.AddWithValue(
                        "@SearchCriteria",
                        query.SearchCriteria |> Option.defaultValue String.defaultValue
                    )
                    |> ignore

                    command.Parameters.AddWithValue("@ActiveOnly", query.ActiveOnly) |> ignore

                    command.Parameters.AddWithValue("@Page", query.Page) |> ignore

                    command.Parameters.AddWithValue("@PageSize", query.PageSize) |> ignore

                    command.Parameters.AddWithValue("@SortBy", query.SortBy |> Option.defaultValue String.defaultValue)
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

                    let! users = reader.ReadManyAsync readUser

                    let! totalCount =
                        task {
                            let! hasNextResult = reader.NextResultAsync()

                            if hasNextResult then
                                let! _ = reader.ReadAsync()
                                return reader.GetInt32(0)
                            else
                                return 0
                        }

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

        member this.FindById(id: UniqueId) : Task<UserDetails option> =
            task {
                try
                    use connection = dbConnectionFactory.CreateSqlConnection()
                    use command = new SqlCommand("dbo.Users_FindById", connection)

                    command.CommandType <- CommandType.StoredProcedure

                    command.Parameters.AddWithValue("@Id", id) |> ignore

                    let! result = getUserDetails connection command

                    return result
                with ex ->
                    return (DataStorageException ex |> raise)
            }

        member this.FindByEmailAddress(emailAddress: EmailAddress) : Task<UserDetails option> =
            task {
                try
                    use connection = dbConnectionFactory.CreateSqlConnection()
                    use command = new SqlCommand("dbo.Users_FindByEmailAddress", connection)

                    command.CommandType <- CommandType.StoredProcedure

                    command.Parameters.AddWithValue("@EmailAddress", emailAddress) |> ignore

                    let! result = getUserDetails connection command

                    return result
                with ex ->
                    return (DataStorageException ex |> raise)
            }

[<AutoOpen>]
module ServiceCollectionExtensions =
    open Microsoft.Extensions.DependencyInjection

    type IServiceCollection with

        /// Adds a user database
        member this.AddUserDatabase() =
            this.AddSingleton<IUserDatabase, UserDatabase>()
