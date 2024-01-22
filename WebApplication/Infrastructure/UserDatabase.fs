namespace WebApplication.Infrastructure.UserDatabase

open System.Data
open System.Threading.Tasks

open Microsoft.Data.SqlClient
open FsToolkit.ErrorHandling

open WebApplication.Infrastructure.Database
open WebApplication.Infrastructure.Exceptions
open WebApplication.Domain.Shared
open WebApplication.Domain.User

type IUserDatabase =
    /// <exception cref="DatabaseException"></exception>
    abstract GetPagedData: Query -> Task<PagedData<User>>
    /// <exception cref="DatabaseException"></exception>
    abstract FindById: UniqueId -> Task<UserDetails option>
    /// <exception cref="DatabaseException"></exception>
    abstract FindByEmailAddress: EmailAddress -> Task<UserDetails option>

type UserDatabase(database: ISqlDatabase) =

    let nullUser = Unchecked.defaultof<User>

    let readUser (reader: SqlDataReader) : User =
        { UserId = reader.GetOrdinal(nameof nullUser.UserId) |> reader.GetGuid
          EmailAddress = reader.GetString(nameof nullUser.EmailAddress, EmailAddress.OfString)
          DisplayName = reader.GetString(nameof nullUser.DisplayName, Text.OfString)
          UserTypeId = reader.GetOrdinal(nameof nullUser.UserTypeId) |> reader.GetGuid
          UserTypeName = reader.GetString(nameof nullUser.UserTypeName, UserType.OfString)
          IsActive = reader.GetOrdinal(nameof nullUser.IsActive) |> reader.GetBoolean }

    let getUserDetails (connection: SqlConnection) (command: SqlCommand) : Task<UserDetails Option> =
        task {
            do! connection.OpenAsync()

            use! reader = command.ExecuteReaderAsync()

            let! users = reader.ReadManyAsync readUser
            
            let! hasNextResult = reader.NextResultAsync()

            let! userPermissions =
                if hasNextResult then
                    reader.ReadManyAsync(fun reader -> reader.GetString("PermissionName", UserPermission.OfString))
                else
                    Task.singleton []

            let! hasNextResult = reader.NextResultAsync()

            let! userGroups =
                if hasNextResult then
                    reader.ReadManyAsync(fun reader -> reader.GetString("GroupName", UserGroup.OfString))
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
                    use connection = database.CreateConnection()
                    use command = new SqlCommand("dbo.Users_Search", connection)

                    command.CommandType <- CommandType.StoredProcedure

                    command.Parameters.AddWithValue(
                        "@SearchCriteria",
                        query.SearchCriteria |> Option.either (fun x -> x.ToString()) (fun () -> String.defaultValue)
                    )
                    |> ignore

                    command.Parameters.AddWithValue("@ActiveOnly", query.ActiveOnly) |> ignore

                    command.Parameters.AddWithValue("@Page", query.Page) |> ignore

                    command.Parameters.AddWithValue("@PageSize", query.PageSize) |> ignore

                    command.Parameters.AddWithValue("@SortBy", query.SortBy |> Option.either (fun x -> x.ToString()) (fun () -> String.defaultValue))
                    |> ignore

                    command.Parameters.AddWithValue(
                        "@SortDirection",
                        query.SortDirection
                        |> Option.map (fun x -> x.ToString())
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
                    return (DatabaseException ex |> raise)
            }

        member this.FindById(userId: UniqueId) : Task<UserDetails option> =
            task {
                try
                    use connection = database.CreateConnection()
                    use command = new SqlCommand("dbo.Users_FindById", connection)

                    command.CommandType <- CommandType.StoredProcedure

                    command.Parameters.AddWithValue("@UserId", userId) |> ignore

                    let! result = getUserDetails connection command

                    return result
                with ex ->
                    return (DatabaseException ex |> raise)
            }

        member this.FindByEmailAddress(emailAddress: EmailAddress) : Task<UserDetails option> =
            task {
                try
                    use connection = database.CreateConnection()
                    use command = new SqlCommand("dbo.Users_FindByEmailAddress", connection)

                    command.CommandType <- CommandType.StoredProcedure

                    command.Parameters.AddWithValue("@EmailAddress", emailAddress) |> ignore

                    let! result = getUserDetails connection command

                    return result
                with ex ->
                    return (DatabaseException ex |> raise)
            }

[<AutoOpen>]
module ServiceCollectionExtensions =
    open Microsoft.Extensions.DependencyInjection

    type IServiceCollection with

        /// Adds a user database
        member this.AddUserDatabase() =
            this.AddSingleton<IUserDatabase, UserDatabase>()
