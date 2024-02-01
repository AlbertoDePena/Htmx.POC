namespace WebApp.Infrastructure.Database

open Microsoft.Data.SqlClient
open Microsoft.Extensions.Options

open WebApp.Infrastructure.Options
open WebApp.Domain.Shared

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
        member this.ReadFirstAsync<'T>(mapper: SqlDataReader -> 'T) : Task<'T option> =
            task {
                let! hasMoreItems = this.ReadAsync()

                let item = if hasMoreItems then mapper this |> Some else None

                return item
            }

        /// <summary>
        /// Tries to get the column's value.
        /// </summary>
        member this.GetString(columnName: string) : string option =
            let ordinal = this.GetOrdinal(columnName)

            match this.IsDBNull ordinal with
            | true -> None
            | false -> this.GetString ordinal |> Some

        /// <summary>
        /// Gets the column's value by applying the provided mapper.
        /// </summary>
        /// <exception cref="Exception">Either the column 'columnName' is missing or the string is not the expected value.</exception>
        member this.GetString<'T>(columnName: string, mapper: string -> 'T option) : 'T =
            let ordinal = this.GetOrdinal(columnName)

            match this.IsDBNull ordinal with
            | true -> failwithf "Either the column '%s' is missing or it has a missing value" columnName
            | false ->
                match this.GetString ordinal |> mapper with
                | None ->
                    failwithf "Either the column '%s' is missing or the string is not the expected value" columnName
                | Some value -> value

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
