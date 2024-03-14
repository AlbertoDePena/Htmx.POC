namespace WebApp.Data

open System.Data
open System.Threading.Tasks

open Dapper
open FsToolkit.ErrorHandling

open WebApp.Domain.Shared
open WebApp.Domain.User
open WebApp.Infrastructure.Exceptions

[<RequireQualifiedAccess>]
module UserRepository =

    /// <exception cref="DataAccessException"></exception>
    let getPagedData (createDbConnection: unit -> IDbConnection) (query: Query) : Task<PagedData<User>> =
        task {
            try
                use connection = createDbConnection ()

                let! reader =
                    connection.QueryMultipleAsync(
                        "dbo.Users_Search",
                        param =
                            {| SearchCriteria =
                                query.SearchCriteria
                                |> Option.either (fun x -> x.Value) (fun () -> String.defaultValue)
                               ActiveOnly = query.ActiveOnly
                               Page = query.Page
                               PageSize = query.PageSize
                               SortBy = query.SortBy |> Option.either (fun x -> x.Value) (fun () -> String.defaultValue)
                               SortDirection =
                                query.SortDirection
                                |> Option.map (fun x -> x.Value)
                                |> Option.defaultValue String.defaultValue |},
                        commandType = CommandType.StoredProcedure
                    )

                let! users = reader.ReadAsync<User>() |> Task.map Seq.toList

                let! totalCount = reader.ReadAsync<int>() |> Task.map (Seq.tryHead >> Option.defaultValue 0)

                return
                    { Page = query.Page
                      PageSize = query.PageSize
                      TotalCount = totalCount
                      SortBy = query.SortBy
                      SortDirection = query.SortDirection
                      Data = users }

            with ex ->
                return (DataAccessException ex |> raise)
        }

    /// <exception cref="DataAccessException"></exception>
    let tryFindById (createDbConnection: unit -> IDbConnection) (userId: UniqueId) : Task<UserDetails option> =
        task {
            try
                use connection = createDbConnection ()

                let! reader =
                    connection.QueryMultipleAsync(
                        "dbo.Users_FindById",
                        param = {| UserId = userId |},
                        commandType = CommandType.StoredProcedure
                    )

                let! user = reader.ReadFirstOrDefaultAsync<User>() |> Task.map Option.ofNull

                let! permissions = reader.ReadAsync<UserPermission>() |> Task.map List.ofSeq

                let! groups = reader.ReadAsync<UserGroup>() |> Task.map List.ofSeq

                let result =
                    user
                    |> Option.map (fun user ->
                        { User = user
                          Permissions = permissions
                          Groups = groups })

                return result
            with ex ->
                return (DataAccessException ex |> raise)
        }

    /// <exception cref="DataAccessException"></exception>
    let tryFindByEmailAddress
        (createDbConnection: unit -> IDbConnection)
        (emailAddress: EmailAddress)
        : Task<UserDetails option> =
        task {
            try
                use connection = createDbConnection ()

                let! reader =
                    connection.QueryMultipleAsync(
                        "dbo.Users_FindByEmailAddress",
                        param = {| EmailAddress = emailAddress |},
                        commandType = CommandType.StoredProcedure
                    )

                let! user = reader.ReadFirstOrDefaultAsync<User>() |> Task.map Option.ofNull

                let! permissions = reader.ReadAsync<UserPermission>() |> Task.map List.ofSeq

                let! groups = reader.ReadAsync<UserGroup>() |> Task.map List.ofSeq

                let result =
                    user
                    |> Option.map (fun user ->
                        { User = user
                          Permissions = permissions
                          Groups = groups })

                return result
            with ex ->
                return (DataAccessException ex |> raise)
        }
