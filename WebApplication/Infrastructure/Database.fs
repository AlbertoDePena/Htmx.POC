namespace WebApplication.Infrastructure.Database

open Microsoft.Data.SqlClient
open Microsoft.Extensions.Options

open WebApplication.Infrastructure.Options
open WebApplication.Domain.Shared

[<RequireQualifiedAccess>]
module UniqueId =

    let createSqlUniqueId () : UniqueId = RT.Comb.Provider.Sql.Create()

[<AutoOpen>]
module SqlDataReaderExtensions =
    open System.Threading.Tasks

    type SqlDataReader with

        /// Map all records available in the result set
        member this.ReadManyAsync<'T>(mapper: SqlDataReader -> 'T) : Task<'T seq> =
            task {
                let items = ResizeArray<'T>()

                let mutable keepGoing = false
                let! hasNextRecord = this.ReadAsync()
                keepGoing <- hasNextRecord

                while keepGoing do
                    items.Add(mapper this)
                    let! hasMoreItems = this.ReadAsync()
                    keepGoing <- hasMoreItems

                return items.ToArray() |> Seq.ofArray
            }

        /// Map the first record available in the result set
        member this.ReadFirstOrAsync<'T>(mapper: SqlDataReader -> 'T, defaultValue: 'T) : Task<'T> =
            task {
                let! hasMoreItems = this.ReadAsync()

                let item = if hasMoreItems then mapper this else defaultValue

                return item
            }

type IDbConnectionFactory =
    abstract CreateSqlConnection: unit -> SqlConnection

type DbConnectionFactory(options: IOptions<Database>) =

    interface IDbConnectionFactory with

        member this.CreateSqlConnection() =
            new SqlConnection(options.Value.ConnectionString)

[<AutoOpen>]
module ServiceCollectionExtensions =
    open Microsoft.Extensions.Configuration
    open Microsoft.Extensions.DependencyInjection

    type IServiceCollection with

        /// Adds a database connection factory
        member this.AddDbConnectionFactory() =
            this
                .AddOptions<Database>()
                .Configure<IConfiguration>(fun settings configuration ->
                    configuration.GetSection(nameof Database).Bind(settings))
            |> ignore

            this.AddSingleton<IDbConnectionFactory, DbConnectionFactory>()
