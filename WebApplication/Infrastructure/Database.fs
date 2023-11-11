namespace WebApplication.Infrastructure.Database

open System
open System.Data

open Microsoft.Data.SqlClient
open Microsoft.Extensions.Options

open WebApplication.Infrastructure.Options
open WebApplication.Domain.Shared

[<RequireQualifiedAccess>]
module UniqueId =

    let create () : UniqueId =
        RT.Comb.Provider.Sql.Create()

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

type ISqlConnectionFactory =
    abstract Create : unit -> SqlConnection

type SqlConnectionFactory(options: IOptions<Database>) =

    interface ISqlConnectionFactory with

        member this.Create () =
            new SqlConnection(options.Value.ConnectionString)