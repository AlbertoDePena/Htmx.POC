namespace WebApp.Infrastructure.Database

[<RequireQualifiedAccess>]
module DbConnection =
    open System
    open System.Data

    open Microsoft.Data.SqlClient

    /// <exception cref="System.ArgumentException"></exception>
    let create (connectionString: string) : IDbConnection =
        if String.IsNullOrWhiteSpace connectionString then
            invalidArg "dbConnectionString" "Database connection string cannot be null/empty/white-space"
        else
            new SqlConnection(connectionString) :> IDbConnection

[<RequireQualifiedAccess>]
module UniqueId =
    open System
    open RT.Comb

    let create () : Guid = Provider.Sql.Create()
