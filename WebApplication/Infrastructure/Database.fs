namespace WebApplication.Infrastructure.Database

open Microsoft.Data.SqlClient
open Microsoft.Extensions.Options

open WebApplication.Infrastructure.Options
open WebApplication.Domain.Shared

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
        
        /// <summary>
        /// Gets the column's value by applying the provided mapper.        
        /// </summary>
        /// <exception cref="Exception">Either the column 'columnName' is missing or the string is not the expected value.</exception>
        member this.GetString<'T>(columnName: string, mapper: string -> 'T option) : 'T =
            this.GetOrdinal(columnName)
            |> this.GetString
            |> mapper
            |> Option.defaultWith (fun () ->
                failwithf "Either the column '%s' is missing or the string is not the expected value" columnName)

type ISqlDatabase =
    abstract CreateConnection: unit -> SqlConnection
    abstract CreateUniqueId: unit -> UniqueId

type SqlDatabase(options: IOptions<Database>) =

    interface ISqlDatabase with

        member this.CreateConnection() =
            new SqlConnection(options.Value.ConnectionString)

        member this.CreateUniqueId() = RT.Comb.Provider.Sql.Create()

[<AutoOpen>]
module ServiceCollectionExtensions =
    open Microsoft.Extensions.Configuration
    open Microsoft.Extensions.DependencyInjection

    type IServiceCollection with

        /// Adds the SQL database
        member this.AddSqlDatabase() =
            this
                .AddOptions<Database>()
                .Configure<IConfiguration>(fun settings configuration ->
                    configuration.GetSection(nameof Database).Bind(settings))
            |> ignore

            this.AddSingleton<ISqlDatabase, SqlDatabase>()
